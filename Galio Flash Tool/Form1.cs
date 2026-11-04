using System;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Galio_Flash_Tool
{
    public partial class Form1 : Form
    {
        private string firmwareFullPath = "";
        private readonly object serialGate = new object();
        private volatile bool busy;
        private bool closing;
        private int session;
        private string detectedBanner;
        private BoardMap boardMap;
        private NativeLoaderPort programPort;
        private CancellationTokenSource monitorStop;
        private Task programMonitor;
        private CancellationTokenSource cancellation;
        private Task operation;
        private readonly object captureGate = new object();
        private StreamWriter capture;

        public Form1()
        {
            InitializeComponent();
            chkTimestamp.Checked = true;
            chkAutoscroll.Checked = true;
            cb_baud.Text = "115200";
            cb_baud.Enabled = false;
            txtFirmwareFile.ReadOnly = true;
           
            tabControl1.HandleCreated += delegate { ClipTabBorder(); };
            tabControl1.Layout += delegate { ClipTabBorder(); };
            ClipTabBorder();
            lbl_uploadStatus.TextChanged += delegate { UpdateStatusColor(); };
            UpdateStatusColor();
            FormClosing += OnClosing;
            RefreshControls();
        }

        private void ClipTabBorder()
        {
            if (!tabControl1.IsHandleCreated || tabControl1.TabCount == 0) return;
            Rectangle page = tabControl1.DisplayRectangle;
            if (page.Width <= 0 || page.Height <= 0) return;

            // Keep the native header and page, excluding the raised frame around the page.
            var region = new Region(page);
            region.Union(new Rectangle(0, 0, tabControl1.ClientSize.Width, tabControl1.GetTabRect(0).Bottom));
            tabControl1.Region = region;
        }

        private void RefreshControls()
        {
            btn_terminalLog.Enabled = !busy && sp1.IsOpen;
            btn_search.Enabled = !busy;
            bool identified = detectedBanner != null;
            groupBox_fwFile.Enabled = identified;
            groupBox_uploadProgress.Enabled = identified;
            groupBox_hwInfo.Enabled = identified;
            groupBox_port.Enabled = !busy;
            cb_port.Enabled = !busy && !sp1.IsOpen && programPort == null;
            groupBox_terminalRX.Enabled = true;
            btn_connect.Text = sp1.IsOpen || programPort != null ? "Disconnect" : "Connect";
            btn_upload.Enabled = !busy && firmwareFullPath.Length > 0 &&
                programPort != null && boardMap != null && detectedBanner != null;
        }
        private void Ui(Action action)
        {
            if (!IsDisposed && IsHandleCreated) BeginInvoke(action);
        }
        private void UpdateStatusColor()
        {
            switch (lbl_uploadStatus.Text)
            {
                case "Bootloader ready":
                case "Upload accepted":
                case "Application running":
                case "Terminal connected":
                    lbl_uploadStatus.ForeColor = Color.DarkGreen;
                    break;
                case "Error":
                case "Configuration error":
                case "Canceled":
                case "Disconnected":
                case "Port closed":
                    lbl_uploadStatus.ForeColor = Color.Firebrick;
                    break;
                case "--":
                    lbl_uploadStatus.ForeColor = SystemColors.GrayText;
                    break;
                default:
                    lbl_uploadStatus.ForeColor = Color.FromArgb(135, 85, 0);
                    break;
            }
        }

        private void ShowError(Exception ex)
        {
            lbl_uploadStatus.Text = "Error";
            MessageBox.Show(this, ex.Message, "Galio Flash Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void DisplayFirmwareMetadata(FirmwareImage image)
        {
            lbl_microcontroller.Text = image?.Microcontroller ?? "--";
            lbl_created.Text = image?.Created ?? "--";
        }

        private void btn_search_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { Filter = "Intel HEX (*.hex)|*.hex", CheckFileExists = true })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    var image = boardMap == null ? FirmwareImage.ReadMetadata(dialog.FileName) : FirmwareImage.Load(dialog.FileName, boardMap.Start, boardMap.End);
                    firmwareFullPath = dialog.FileName;
                    txtFirmwareFile.Text = Path.GetFileName(firmwareFullPath);
                    DisplayFirmwareMetadata(image);
                    lbl_totalLines.Text = boardMap == null ? "--" : image.Records.Count.ToString();
                    lbl_uploadStatus.Text = detectedBanner != null ? BootStatus :
                        (programPort != null ? "Waiting for bootloader" : "File selected");
                    progressBar1.Value = 0;
                    lbl_percent.Text = "0 %";
                    lbl_transferLines.Text = "0";
                }
                catch (Exception ex)
                {
                    firmwareFullPath = "";
                    txtFirmwareFile.Clear();
                    DisplayFirmwareMetadata(null);
                    ShowError(ex);
                }
                RefreshControls();
            }
        }
        private void cb_port_Click(object sender, EventArgs e)
        {
            string selected = cb_port.Text;
            cb_port.Items.Clear();
            cb_port.Items.AddRange(SerialPort.GetPortNames());
            cb_port.Text = selected;
        }
        private void OpenPort(string name)
        {
            if (sp1.IsOpen)
            {
                if (sp1.PortName != name) throw new InvalidOperationException("Desconecte el puerto actual antes de cambiar COM.");
                return;
            }
            if (string.IsNullOrWhiteSpace(name)) throw new InvalidOperationException("Seleccione el puerto COM.");
            sp1.PortName = name;
            sp1.BaudRate = 115200;
            sp1.Parity = Parity.None; sp1.DataBits = 8; sp1.StopBits = StopBits.One;
            sp1.Handshake = Handshake.None;
            sp1.DtrEnable = false; sp1.RtsEnable = false;
            sp1.ReadTimeout = 10; sp1.WriteTimeout = 2000;
            sp1.Encoding = Encoding.ASCII;
            sp1.Open();
        }
        private void ResetIdentification()
        {
            Interlocked.Increment(ref session);
            detectedBanner = null;
            boardMap = null;
            lbl_hwInfoName.Text = lbl_hwInfoVersion.Text = lbl_hwInfoDate.Text = lbl_hwInfoTime.Text = "--";
        }

        private string BootStatus { get { return boardMap != null ? "Bootloader ready" : "Configuration error"; } }

        private void OpenProgram(string name)
        {
            programPort = new NativeLoaderPort(name);
            monitorStop = new CancellationTokenSource();
            var token = monitorStop.Token;
            var loader = new StationLoader(programPort, token);
            int currentSession = session;
            Action<string> receive = line => Ui(() =>
            {
                if (session != currentSession) return;
                AppendTerminal(line + Environment.NewLine, Color.Black);
                string identification = StationLoader.BootIdentification(line);
                if (identification != null)
                {
                    detectedBanner = identification;
                    DisplayBanner(identification);
                    boardMap = null;
                    try
                    {
                        boardMap = BoardMap.Read(Path.Combine(Application.StartupPath, "placas.ini"), identification);
                    }
                    catch (Exception ex) { ShowError(ex); }
                    lbl_uploadStatus.Text = BootStatus;
                    if (boardMap != null && firmwareFullPath.Length > 0)
                    {
                        try
                        {
                            var image = FirmwareImage.Load(firmwareFullPath, boardMap.Start, boardMap.End);
                            lbl_totalLines.Text = image.Records.Count.ToString();
                            DisplayFirmwareMetadata(image);
                        }
                        catch (Exception ex)
                        {
                            firmwareFullPath = "";
                            txtFirmwareFile.Clear();
                            DisplayFirmwareMetadata(null);
                            lbl_totalLines.Text = "--";
                            ShowError(ex);
                        }
                    }
                }
                else if (line.StartsWith("INFO app=", StringComparison.Ordinal) && line.Contains(" fw="))
                {
                    detectedBanner = null;
                    boardMap = null;
                    lbl_hwInfoName.Text = lbl_hwInfoVersion.Text = lbl_hwInfoDate.Text = lbl_hwInfoTime.Text = "--";
                    lbl_uploadStatus.Text = "Application running";
                }
                RefreshControls();
            });
            lbl_uploadStatus.Text = "Waiting for bootloader";
            programMonitor = Task.Run(() =>
            {
                try
                {
                    loader.PulseBoot(receive);
                    while (!token.IsCancellationRequested)
                    {
                        string line = loader.ReadLineByte();
                        if (line != null) receive(line);
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    Ui(() =>
                    {
                        if (session != currentSession) return;
                        CloseProgram();
                        ShowError(ex);
                        RefreshControls();
                    });
                }
            });
        }

        private void StopMonitor()
        {
            if (monitorStop == null) return;
            monitorStop.Cancel();
            programMonitor.GetAwaiter().GetResult();
            monitorStop.Dispose();
            monitorStop = null;
        }

        private void CloseProgram()
        {
            StopMonitor();
            if (programPort != null) programPort.Dispose();
            programPort = null;
            ResetIdentification();
        }

        private void ClosePort()
        {
            lock (serialGate)
            {
                StopCapture();
                if (!sp1.IsOpen) return;
                try { sp1.DtrEnable = false; sp1.RtsEnable = false; }
                finally { sp1.Close(); }
            }
        }
        private void btn_connect_Click(object sender, EventArgs e)
        {
            if (busy) return;
            try
            {
                StopCapture();
                ResetIdentification();
                if (programPort != null) { CloseProgram(); lbl_uploadStatus.Text = "Port closed"; }
                else if (sp1.IsOpen) { ClosePort(); lbl_uploadStatus.Text = "Port closed"; }
                else if (tabControl1.SelectedTab == tabPageProgram) OpenProgram(cb_port.Text);
                else { OpenPort(cb_port.Text); lbl_uploadStatus.Text = "Terminal connected"; }
            }
            catch (Exception ex) { ShowError(ex); }
            RefreshControls();
        }
        private async void btn_upload_Click(object sender, EventArgs e) { await RunUpload(); }

        private async Task RunUpload()
        {
            if (busy) return;
            FirmwareImage image = null;
            if (boardMap == null) return;
            var map = boardMap;
            try
            {
                image = FirmwareImage.Load(firmwareFullPath, map.Start, map.End);
                DisplayFirmwareMetadata(image);
            }
            catch (Exception ex) { DisplayFirmwareMetadata(null); ShowError(ex); return; }
            string portName = cb_port.Text, path = firmwareFullPath;
            string bannerAtStart = detectedBanner;
            if (programPort == null || !StationLoader.IsBanner(bannerAtStart)) return;
            StopMonitor();
            busy = true;
            Interlocked.Increment(ref session); // Ignore queued monitor events during transfer.
            cancellation = new CancellationTokenSource();
            RefreshControls();
            lbl_uploadStatus.Text = "Preparing";
            progressBar1.Value = 0;
            progressBar1.Maximum = image.Records.Count;
            lbl_totalLines.Text = boardMap == null ? "--" : image.Records.Count.ToString();
            lbl_transferLines.Text = "0"; lbl_percent.Text = "0 %";
            string result = "", logPath = "";
            Exception failure = null;
            try
            {
                operation = Task.Run(() =>
                {
                    string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Galio", "FlashTool", "logs");
                    Directory.CreateDirectory(folder);
                    logPath = Path.Combine(folder, DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") + "-" + Guid.NewGuid().ToString("N") + ".log");
                    using (var log = new StreamWriter(logPath, false, Encoding.UTF8) { AutoFlush = true })
                    {
                        Action<string> writeLog = message => log.WriteLine(DateTimeOffset.Now.ToString("o") + " " + message);
                        writeLog("Perfil=" + map.Section + " COM=" + portName + " Archivo=" + path);
                        if (image != null) writeLog("SHA256=" + image.Sha256 + " Bytes=" + image.PayloadBytes + " Fuses excluidos=" + image.SkippedFuseBytes);
                        try
                        {
                            writeLog(bannerAtStart);
                            var loader = new StationLoader(programPort, cancellation.Token);
                            loader.Log = message => { writeLog(message); Ui(() => AppendTerminal(message + Environment.NewLine, Color.Black)); };
                            bool started = loader.Upload(image,
                                count => Ui(() => { progressBar1.Value = count; lbl_transferLines.Text = count.ToString(); lbl_percent.Text = (count * 100 / image.Records.Count) + " %"; }),
                                stage => { writeLog(stage); Ui(() => lbl_uploadStatus.Text = stage); }, bannerAtStart);
                            result = started ? "Carga aceptada y aplicación iniciada." : "Carga aceptada; no se recibió identificación de inicio de aplicación.";
                          
                            writeLog(result);
                        }
                        catch (Exception ex) { writeLog("ERROR " + ex.Message); throw; }
                    }
                });
                await operation;
                lbl_uploadStatus.Text = "Upload accepted";
            }
            catch (OperationCanceledException) { lbl_uploadStatus.Text = "Canceled"; }
            catch (Exception ex)
            {
                failure = new IOException(ex.Message + "\r\nSi la escritura comenzó, repita la carga completa.\r\nRegistro: " + logPath, ex);
            }
            finally
            {
                CloseProgram();
                progressBar1.Value = 0;
                lbl_percent.Text = "0 %";
                lbl_transferLines.Text = "0";
                lbl_totalLines.Text = "0";
                busy = false;
                cancellation.Dispose(); cancellation = null;
                RefreshControls();
            }
            if (closing) return;
            if (failure != null) ShowError(failure);
            else if (result.Length > 0) MessageBox.Show(this, result + "\r\n\r\nRegistro: " + logPath, "Galio Flash Tool", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private async void OnClosing(object sender, FormClosingEventArgs e)
        {
            if (busy)
            {
                e.Cancel = true;
                if (closing) return;
                closing = true;
                cancellation.Cancel();
                try { if (operation != null) await operation; } catch { }
                // Let RunOperation finish its UI cleanup before disposing controls.
                BeginInvoke(new Action(Close));
                return;
            }
            try { CloseProgram(); ClosePort(); } catch { }
        }
        private void DisplayBanner(string banner)
        {
            if (banner == null) return;
            foreach (string part in banner.Split(','))
            {
                string[] pair = part.Split(new[] { '=' }, 2);
                if (pair.Length != 2) continue;
                switch (pair[0])
                {
                    case "name": lbl_hwInfoName.Text = pair[1]; break;
                    case "version": lbl_hwInfoVersion.Text = pair[1]; break;
                    case "date": lbl_hwInfoDate.Text = pair[1]; break;
                    case "time": lbl_hwInfoTime.Text = pair[1]; break;
                }
            }
        }
        private void sp1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int receivedSession = Volatile.Read(ref session);
            if (busy) return;
            if (!Monitor.TryEnter(serialGate)) return;
            try
            {
                if (busy || !sp1.IsOpen) return;
                try
                {
                    byte[] buffer = new byte[sp1.BytesToRead];
                    if (buffer.Length == 0) return;
                    int count = sp1.Read(buffer, 0, buffer.Length);
                    string received = Encoding.ASCII.GetString(buffer, 0, count);
                    CaptureReceived(received);
                    Ui(() => { if (busy || receivedSession != session) return;
                        AppendTerminal(received, Color.Black); });
                }
                catch (IOException ex) { Ui(() => { if (busy || receivedSession != session) return; ResetIdentification(); try { ClosePort(); } catch (IOException) { } RefreshControls(); lbl_uploadStatus.Text = "Disconnected"; AppendTerminal(ex.Message + Environment.NewLine, Color.Red); }); }
                catch (InvalidOperationException) { }
                catch (TimeoutException) { }
            }
            finally { Monitor.Exit(serialGate); }
        }
        private void AppendTerminal(string text, Color color)
        {
            if (richTerminal.TextLength > 100000) richTerminal.Clear();
            if (chkTimestamp.Checked) text = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + text;
            richTerminal.SelectionStart = richTerminal.TextLength;
            richTerminal.SelectionColor = color;
            richTerminal.AppendText(text);
            if (chkAutoscroll.Checked) { richTerminal.SelectionStart = richTerminal.TextLength; richTerminal.ScrollToCaret(); }
        }
        private void btn_terminalLog_Click(object sender, EventArgs e)
        {
            lock (captureGate)
            {
                if (capture != null) { StopCapture(); return; }
            }
            if (!sp1.IsOpen || busy) return;
            using (var dialog = new SaveFileDialog {
                Filter = "Registro de terminal (*.log)|*.log", DefaultExt = "log",
                FileName = "Galio-" + sp1.PortName + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".log"
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK || !sp1.IsOpen) return;
                try
                {
                    StartCapture(dialog.FileName, sp1.PortName);
                    btn_terminalLog.Text = "Stop log";
                }
                catch (Exception ex) { ShowError(ex); }
            }
        }

        private void StartCapture(string path, string port)
        {
            lock (captureGate)
            {
                var writer = new StreamWriter(path, false, new UTF8Encoding(false));
                try
                {
                    writer.AutoFlush = true;
                    writer.WriteLine("Inicio {0:o} | {1} | 115200 8N1 | Texto", DateTimeOffset.Now, port);
                    capture = writer;
                }
                catch { writer.Dispose(); throw; }
            }
        }

        private void CaptureReceived(string received)
        {
            lock (captureGate)
            {
                if (capture == null) return;
                try
                {
                    capture.Write(received);
                }
                catch (IOException ex)
                {
                    try { capture.Dispose(); } catch (IOException) { }
                    capture = null;
                    Ui(() =>
                    {
                        lock (captureGate) { if (capture == null) btn_terminalLog.Text = "Start log"; }
                        ShowError(new IOException("Se interrumpió la grabación del log: " + ex.Message, ex));
                    });
                }
            }
        }

        private void StopCapture()
        {
            lock (captureGate)
            {
                if (capture == null) return;
                try
                {
                    using (capture) capture.WriteLine("\r\nFin {0:o}", DateTimeOffset.Now);
                }
                catch (IOException ex) { ShowError(ex); }
                finally { capture = null; btn_terminalLog.Text = "Start log"; }
            }
        }

        private void btn_terminalClear_Click(object sender, EventArgs e) { richTerminal.Clear(); }
        private void tabControl1_Selected(object sender, TabControlEventArgs e) { }
        private void Form1_Load(object sender, EventArgs e) { cb_port_Click(sender, e); }
        private void label4_Click(object sender, EventArgs e) { }
        private void linkLabel_website_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) { System.Diagnostics.Process.Start("https://www.galio.dev"); }
    }
}
