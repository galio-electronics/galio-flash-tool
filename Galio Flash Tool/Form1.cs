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
        private CancellationTokenSource cancellation;
        private Task operation;

        public Form1()
        {
            InitializeComponent();
            Text = "Galio Flash Tool — Station/Monitor";
            radioRcvText.Checked = radioSendAscii.Checked = chkTimestamp.Checked = chkNlCr.Checked = true;
            chkAutoscroll.Checked = true;
            checkBox_autoReset.Checked = true;
            checkBox_autoReset.Enabled = false;
            cb_baud.Text = "115200";
            cb_baud.Enabled = false;
            txtFirmwareFile.ReadOnly = true;
            btn_upload.Text = "Cargar";
            btn_search.Text = "Seleccionar";
            tabPageTerminal.Text = "Terminal avanzada";
            lbl_uploadStatus.AutoSize = false;
            lbl_uploadStatus.Size = new Size(76, 20);
            txtSend.KeyDown += txtSend_KeyDown;
            FormClosing += OnClosing;
            RefreshControls();
        }

        private void RefreshControls()
        {
            groupBox_fwFile.Enabled = !busy;
            groupBox_uploadProgress.Enabled = true;
            groupBox_hwInfo.Enabled = true;
            groupBox_port.Enabled = !busy;
            btn_boot.Enabled = !busy;
            groupBoxTerminalTx.Enabled = !busy && sp1.IsOpen;
            groupBox_terminalRX.Enabled = true;
            btn_connect.Text = sp1.IsOpen ? "Disconnect" : "Connect";
            btn_upload.Enabled = !busy && firmwareFullPath.Length > 0;
        }
        private void Ui(Action action)
        {
            if (!IsDisposed && IsHandleCreated) BeginInvoke(action);
        }
        private void ShowError(Exception ex)
        {
            lbl_uploadStatus.Text = "Error";
            MessageBox.Show(this, ex.Message, "Galio Flash Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void btn_search_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { Filter = "Intel HEX (*.hex)|*.hex", CheckFileExists = true })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    var image = FirmwareImage.Load(dialog.FileName);
                    firmwareFullPath = dialog.FileName;
                    txtFirmwareFile.Text = Path.GetFileName(firmwareFullPath);
                    lbl_microcontroller.Text = "18F67K40";
                    lbl_created.Text = "--";
                    lbl_totalLines.Text = image.Records.Count.ToString();
                    lbl_uploadStatus.Text = "Listo";
                    progressBar1.Value = 0;
                    lbl_percent.Text = "0 %";
                    lbl_transferLines.Text = "0";
                }
                catch (Exception ex)
                {
                    firmwareFullPath = "";
                    txtFirmwareFile.Clear();
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
        private void ClosePort()
        {
            lock (serialGate)
            {
                if (!sp1.IsOpen) return;
                try { sp1.DtrEnable = false; sp1.RtsEnable = false; }
                finally { sp1.Close(); }
            }
        }
        private void btn_connect_Click(object sender, EventArgs e)
        {
            if (busy) return;
            try { if (sp1.IsOpen) ClosePort(); else OpenPort(cb_port.Text); }
            catch (Exception ex) { ShowError(ex); }
            RefreshControls();
        }
        private async void btn_upload_Click(object sender, EventArgs e) { await RunOperation(true); }
        private async void btn_boot_Click(object sender, EventArgs e) { await RunOperation(false); }

        private async Task RunOperation(bool upload)
        {
            if (busy) return;
            FirmwareImage image = null;
            try { if (upload) image = FirmwareImage.Load(firmwareFullPath); }
            catch (Exception ex) { ShowError(ex); return; }
            string portName = cb_port.Text, path = firmwareFullPath;
            busy = true;
            cancellation = new CancellationTokenSource();
            RefreshControls();
            lbl_uploadStatus.Text = "Preparando";
            if (upload)
            {
                progressBar1.Value = 0;
                progressBar1.Maximum = image.Records.Count;
                lbl_totalLines.Text = image.Records.Count.ToString();
                lbl_transferLines.Text = "0"; lbl_percent.Text = "0 %";
            }
            string result = "", logPath = "";
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
                        writeLog("Perfil=Station/Monitor BOOT:1.2 COM=" + portName + " Archivo=" + path);
                        if (image != null) writeLog("SHA256=" + image.Sha256 + " Bytes=" + image.PayloadBytes + " Fuses excluidos=" + image.SkippedFuseBytes);
                        try
                        {
                            lock (serialGate)
                            {
                                ClosePort();
                                using (var transport = new NativeLoaderPort(portName))
                                {
                                    var loader = new StationLoader(transport, cancellation.Token);
                                    loader.Log = message => { writeLog(message); Ui(() => AppendTerminal(message + Environment.NewLine, Color.Black)); };
                                    if (upload)
                                    {
                                        bool started = loader.Upload(image,
                                            count => Ui(() => { progressBar1.Value = count; lbl_transferLines.Text = count.ToString(); lbl_percent.Text = (count * 100 / image.Records.Count) + " %"; }),
                                            stage => Ui(() => lbl_uploadStatus.Text = stage));
                                        result = started ? "Carga aceptada y aplicación iniciada." : "Carga aceptada; no se recibió identificación de inicio de aplicación.";
                                        result += "\r\nEl protocolo no permite verificar la flash por lectura.";
                                    }
                                    else { loader.Enter(); result = "Bootloader Station/Monitor identificado y listo."; }
                                    string banner = loader.Banner;
                                    Ui(() => DisplayBanner(banner));
                                    writeLog(result);
                                    }
                            }
                        }
                        catch (Exception ex) { writeLog("ERROR " + ex.Message); throw; }
                        finally { ClosePort(); }
                    }
                });
                await operation;
                lbl_uploadStatus.Text = upload ? "Aceptada" : "Boot listo";
                if (!closing) MessageBox.Show(this, result + "\r\n\r\nRegistro: " + logPath, "Galio Flash Tool", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException) { lbl_uploadStatus.Text = "Cancelada"; }
            catch (Exception ex)
            {
                if (!closing) ShowError(new IOException(ex.Message + "\r\nSi la escritura comenzó, repita la carga completa.\r\nRegistro: " + logPath, ex));
            }
            finally
            {
                busy = false;
                cancellation.Dispose(); cancellation = null;
                RefreshControls();
            }
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
            try { ClosePort(); } catch { }
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
                    Ui(() => AppendTerminal(radioRcvText.Checked ? Encoding.ASCII.GetString(buffer, 0, count) : BitConverter.ToString(buffer, 0, count).Replace('-', ' ') + Environment.NewLine, Color.Black));
                }
                catch (IOException ex) { Ui(() => { lbl_uploadStatus.Text = "Desconectado"; AppendTerminal(ex.Message + Environment.NewLine, Color.Red); }); }
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
        private void SendTerminalData()
        {
            if (busy || !sp1.IsOpen || string.IsNullOrWhiteSpace(txtSend.Text)) return;
            try
            {
                string text = txtSend.Text;
                if (radioSendAscii.Checked) sp1.Write(text);
                else
                {
                    string hex = text.Replace(" ", "");
                    if (hex.Length % 2 != 0) throw new FormatException("Hexadecimal incompleto.");
                    byte[] bytes = new byte[hex.Length / 2];
                    for (int i = 0; i < bytes.Length; i++) bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
                    sp1.Write(bytes, 0, bytes.Length);
                }
                if (chkNlCr.Checked) sp1.Write("\r\n");
                AppendTerminal("> " + text + Environment.NewLine, Color.Red);
                txtSend.Clear();
            }
            catch (Exception ex) { ShowError(ex); }
        }
        private void btn_TerminalSend_Click(object sender, EventArgs e) { SendTerminalData(); }
        private void txtSend_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; SendTerminalData(); } }
        private void btn_terminalClear_Click(object sender, EventArgs e) { richTerminal.Clear(); }
        private void tabControl1_Selected(object sender, TabControlEventArgs e) { }
        private void Form1_Load(object sender, EventArgs e) { cb_port_Click(sender, e); }
        private void label4_Click(object sender, EventArgs e) { }
        private void linkLabel_website_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) { System.Diagnostics.Process.Start("https://www.galio.dev"); }
    }
}
