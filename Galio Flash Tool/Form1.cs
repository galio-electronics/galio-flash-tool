using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace Galio_Flash_Tool
{
   

    public partial class Form1 : Form
    {
        public bool nextLine = false; //bandera para enviar siguiente linea
        public bool terminal = false;
        public bool textMode = true;
        public bool bootmode = false;

        private string firmwareFullPath = "";
        System.Threading.Thread firmwareSender;
        private readonly ManualResetEventSlim _lineAckEvent = new ManualResetEventSlim(false);

        public delegate void updateDebugCallback(string str);

        

// ...

        private readonly StringBuilder _hwInfoBuffer = new StringBuilder();
    public Form1()
        {
            InitializeComponent();

            // Modo terminal por defecto
            radioRcvText.Checked = true;
            radioRcvRaw.Checked = false;
            chkTimestamp.Checked = true;
            chkAutoscroll.Checked = false;

            radioSendAscii.Checked = true;
            radioSendHex.Checked = false;
            chkNlCr.Checked = true; // si quieres enviar CR por defecto
        }

        private void btn_search_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "HEX/BIN Files (*.hex;*.bin)|*.hex;*.bin";

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                lbl_uploadStatus.Text = "Ready";
                // Guardas la ruta completa para uso interno
                firmwareFullPath = openFile.FileName;

                // Solo muestras el nombre del archivo
                txtFirmwareFile.Text = Path.GetFileName(openFile.FileName);

                // Pasas la ruta completa a tu parser actual
                readHexInfo(firmwareFullPath);
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void cb_port_Click(object sender, EventArgs e)
        {
            cb_port.Items.Clear();
            foreach (string SPort in System.IO.Ports.SerialPort.GetPortNames())
            {
                cb_port.Items.Add(SPort.Trim());
            }
        }

        private void btn_connect_Click(object sender, EventArgs e)
        {
            if (btn_connect.Text == "Disconnect")
            {
                if (sp1.IsOpen)
                {
                    sp1.Close();
                }
                btn_connect.Text = "Connect";
                //pictureBox_off.Visible = true;
                //pictureBox_On.Visible = false;
                //groupBox.Enabled = false;
                //groupBoxUpload.Enabled = false;
                //groupBox_Received.Enabled = false;
                //groupBox_SendData.Enabled = false;
                richTerminal.Clear();
                groupBox_terminalRX.Enabled = false;
                groupBoxTerminalTx.Enabled = false;
                groupBox_hwInfo.Enabled = false;
                groupBox_fwFile.Enabled = false;
                groupBox_uploadProgress.Enabled = false;
                initApp();
            }
            else
            {
                try
                {
                    if (cb_port.Text == "")
                    {

                        MessageBox.Show("COM not selected", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                    else
                    {
                        sp1.BaudRate = Int32.Parse(cb_baud.Text);
                        sp1.PortName = cb_port.Text;
                        sp1.Parity = Parity.None;
                        sp1.DataBits = 8;
                        sp1.ReadTimeout = 500;       // en btn_conectar_Click
                        sp1.NewLine = "\r\n";        // si usas CR+LF desde el PIC
                        sp1.Open();

                        btn_connect.Text = "Disconnect";
                        CheckForIllegalCrossThreadCalls = false;
                        //pictureBox_off.Visible = false;
                        //pictureBox_On.Visible = true;
                        //groupBox_Firmware.Enabled = true;
                        //groupBoxUpload.Enabled = true;
                        //groupBox_Received.Enabled = true;
                        //groupBox_SendData.Enabled = true;
                        //groupBox_info.Enabled = true;
                        groupBoxTerminalTx.Enabled = true;
                        groupBox_terminalRX.Enabled = true;                        
                        groupBox_hwInfo.Enabled = true;
                        groupBox_fwFile.Enabled = true;
                        groupBox_uploadProgress.Enabled = true;
                        if (checkBox_autoReset.Checked && terminal==false)
                        {
                            // 1) Forzar modo boot
                            EnterBootloaderMode(sp1);
                        }
                        
                    }


                }
                catch (Exception)
                {

                    MessageBox.Show(cb_port.Text + " is already open", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            }
        }

        private void btn_upload_Click(object sender, EventArgs e)
        {
            if (txtFirmwareFile.Text == "")
            {
                MessageBox.Show("Firmware not selected", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            else
            {

                if (sp1.IsOpen)
                {
                    try
                    {

                        lbl_uploadStatus.Text = "Uploading";
                        bootmode = true;
                        firmwareSender = new System.Threading.Thread(sendFirmware);
                        firmwareSender.IsBackground = true;
                        firmwareSender.Start();
                        //sendFirmware();
                    }
                    catch (Exception)
                    {

                        lbl_uploadStatus.Text = "Error";
                        MessageBox.Show("Serial Port Error", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        sp1.Close();
                        initApp();

                    }
                }
                else
                {

                    MessageBox.Show("Serial Port Error", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    initApp();
                }


                //new Thread(serialFirmware).Start();
            }

        }


        private void stopSendingFirmware()
        {
            //issending = false;
            if (firmwareSender.Join(200) == false)
            {
                firmwareSender.Abort();
            }
            firmwareSender = null;
        }

        private void sendFirmware()
        {
            const int ackTimeoutMs = 2000; // 2 s por línea, ajusta a gusto
            int counter = 0;

            // Validación rápida: que sí haya archivo seleccionado
            if (string.IsNullOrEmpty(firmwareFullPath))
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show("Firmware file not selected.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
                return;
            }

            try
            {
                // Deshabilitar UI mientras se envía
                Invoke(new Action(() =>
                {
                    tabControl1.Enabled = false;
                    progressBar1.Value = 0;
                    lbl_transferLines.Text = "0";
                    lbl_percent.Text = "0 %";
                }));

                // ⬅️ AQUÍ es donde cambiamos: usamos firmwareFullPath en lugar de txtFirmwareFile.Text
                using (var fileToSend = new StreamReader(firmwareFullPath))
                {
                    string line;

                    while ((line = fileToSend.ReadLine()) != null)
                    {
                        // Solo líneas Intel HEX que empiezan con ':'
                        if (string.IsNullOrEmpty(line) || line[0] != ':')
                            continue;

                        // Enviar línea + CR
                        byte[] fin = { 0x0D };
                        sp1.Write(line);
                        sp1.Write(fin, 0, 1);

                        // Preparar espera de ACK
                        nextLine = false;
                        _lineAckEvent.Reset();

                        // Esperar ACK del PIC
                        if (!_lineAckEvent.Wait(ackTimeoutMs))
                        {
                            // Timeout
                            Invoke(new Action(() =>
                            {
                                MessageBox.Show("Not response from PIC", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                cleanVariablesUpload();
                            }));
                            return; // salimos de la función
                        }

                        // ACK recibido, actualizar progreso
                        counter++;
                        Invoke(new Action(() =>
                        {
                            progressBar1.Value = counter;
                            lbl_transferLines.Text = counter.ToString();
                            int percent = 0;
                            if (progressBar1.Maximum > 0)
                                percent = (int)(progressBar1.Value * 100.0 / progressBar1.Maximum);

                            lbl_percent.Text = percent.ToString() + " %";
                        }));
                    }
                }

                // Si llegamos aquí, no hubo timeout
                Invoke(new Action(() =>
                {
                    lbl_percent.Text = "100 %";   // marcamos completo antes de limpiar
                    lbl_uploadStatus.Text = "Success";
                    MessageBox.Show("Firmware Uploaded", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    bootmode = false;
                    cleanVariablesUpload();
                }));
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show("Error sending firmware:\r\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cleanVariablesUpload();
                }));
            }
            finally
            {
                Invoke(new Action(() =>
                {
                    tabControl1.Enabled = true;
                }));

                stopSendingFirmware();
            }
        }

        //private void sendFirmware()
        //{
        //    tabControl1.Enabled = false;
        //    string line;
        //    int counter = 0;
        //    //Se abre el archivo de nuevo para escribirlo en el serial
        //    System.IO.StreamReader fileToSend = new System.IO.StreamReader(txtFirmwareFile.Text);
        //    uint j = 0;
        //    uint limite = 829496729;

        //    while ((line = fileToSend.ReadLine()) != null)
        //    {
        //        if (line[0] == ':')
        //        {
        //            byte[] fin = { 0x0D };
        //            sp1.Write(line);
        //            sp1.Write(fin, 0, 1);
        //            j = 0;
        //            while (!nextLine && j != limite) j++; //Se cicla esperando bandera o timeout (contador j)

        //            if (j == limite)
        //            {
        //                MessageBox.Show("Not response from PIC", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //                cleanVariablesUpload();

        //                break;
        //            }
        //            progressBar1.Value = ++counter; // Se va sumando el contador de la barra
        //            nextLine = false;
        //            lbl_transferLines.Text = counter.ToString();

        //        }
        //    }
        //    if (j != limite) //Significa que no hubo timeout
        //    {

        //        MessageBox.Show("Firmware Uploaded", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        bootmode = false;
        //    }


        //    fileToSend.Close(); //cerramos el archivo
        //    cleanVariablesUpload(); //limpiamos variables
        //    tabControl1.Enabled = true;
        //    stopSendingFirmware();



        //}
        private void initApp()
        {
            btn_connect.Text = "Connect";
            txtFirmwareFile.Text = "";
            lbl_created.Text = "-";
            lbl_microcontroller.Text = "-";
            lbl_totalLines.Text = "0";
            lbl_transferLines.Text = "0";
            groupBox_fwFile.Enabled = false;
            groupBox_uploadProgress.Enabled = false;
         
            //groupBox_SendData.Enabled = false;
            //groupBox_info.Enabled = false;
            //pictureBox_off.Visible = true;
            //pictureBox_On.Visible = false;

            lbl_hwInfoDate.Text = "--";
            lbl_hwInfoName.Text = "--";
            lbl_hwInfoTime.Text = "--";
            lbl_hwInfoVersion.Text = "--";

            //richTextBox_received.Clear();
            //txtbox_dataSend.Clear();




        }

        private void EnterBootloaderMode(SerialPort port)
        {
            if (!port.IsOpen)
                port.Open();

            // ✅ Limpiar cualquier debug pendiente del firmware de aplicación
            try { port.DiscardInBuffer(); } catch { }

            // ser.setDTR(True)
            // ser.setRTS(True)
            //port.DtrEnable = true;
            //port.RtsEnable = true;
            //Thread.Sleep(500);   // time.sleep(2)

            // ser.setDTR(False)   # RESET en HIGH
            // ser.setRTS(True)    # BOOT en LOW
            port.DtrEnable = false;
            port.RtsEnable = true;
            Thread.Sleep(1200);    // time.sleep(0.1)

            // ser.setDTR(True)    # RESET en LOW
            // ser.setRTS(False)   # BOOT en HIGH
            port.DtrEnable = true;
            port.RtsEnable = false;
            Thread.Sleep(100);    // time.sleep(0.2)

            // ser.setDTR(False)   # BOOT EN HIGH, end
            port.DtrEnable = false;
            // RTS lo dejamos como esté según tu diseño
        }


        private void cleanVariablesUpload()
        {
            progressBar1.Value = 0;
            lbl_transferLines.Text = "0";
        }

        private void readHexInfo(string fullPath)
        {
            int counter = 0;
            int maxlines;
            string line;
            //string device;
            //string created;
            int found = 0;
            System.IO.StreamReader file = new StreamReader(fullPath);
            // lbl_total_lines.Text = "0%";
            progressBar1.Value = 0;
            while ((line = file.ReadLine()) != null)
                if (line[0] == ':')
                {
                    counter++;
                }
                else if (line[0] == ';')
                {
                    if (line.Contains("CREATED"))
                    {
                        found = line.IndexOf("CREATED=");
                        lbl_created.Text = line.Substring(found + 8);
                        //created = line;
                        //lbl_created.Text = line.Substring("CREATED=");
                    }
                    else
                    {

                        lbl_microcontroller.Text = line.Substring(1);
                    }
                }



            file.Close();
            //lbl_microcontroller.Text = device;
            //lbl_created.Text = created;

            progressBar1.Maximum = counter;
            maxlines = counter;
            lbl_totalLines.Text = counter.ToString();
            counter = 0;
        }

        private void UpdateHardwareInfoFromLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return;

            // Normalizamos (por si venían \r\n en medio)
            line = line.Replace("\r", "").Replace("\n", "");

            try
            {
                var parts = line.Split(',');

                var kv = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var part in parts)
                {
                    var pair = part.Split(new[] { '=' }, 2);
                    if (pair.Length == 2)
                    {
                        var key = pair[0].Trim();
                        var value = pair[1].Trim();
                        kv[key] = value;
                    }
                }

                BeginInvoke(new Action(() =>
                {
                    if (kv.TryGetValue("name", out var name))
                    {
                        lbl_hwInfoName.Text = name;
                        lbl_uploadStatus.Text = "Ready";
                    }
                        

                    if (kv.TryGetValue("version", out var version))
                        lbl_hwInfoVersion.Text = version;

                    if (kv.TryGetValue("date", out var date))
                        lbl_hwInfoDate.Text = date;

                    if (kv.TryGetValue("time", out var time))
                        lbl_hwInfoTime.Text = time;
                    if(checkBox_autoReset.Checked)
                    MessageBox.Show(name, "HW Found",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }));
            }
            catch
            {
                // Si viene algo raro, lo ignoramos para no matar la app
            }
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void AppendTerminal(string text, Color color)
        {
            if (richTerminal.InvokeRequired)
            {
                richTerminal.BeginInvoke(new Action<string, Color>(AppendTerminal), text, color);
                return;
            }

            // Timestamp si aplica
            if (chkTimestamp.Checked)
            {
                string ts = DateTime.Now.ToString("HH:mm:ss");
                text = $"[{ts}] {text}";
            }

            int start = richTerminal.TextLength;
            richTerminal.SelectionStart = start;
            richTerminal.SelectionLength = 0;
            richTerminal.SelectionColor = color;
            richTerminal.AppendText(text);
            richTerminal.SelectionColor = richTerminal.ForeColor;

            if (chkAutoscroll.Checked)
            {
                richTerminal.SelectionStart = richTerminal.TextLength;
                richTerminal.ScrollToCaret();
            }
        }

        private string ByteArrayToHexString(byte[] data)
        {
            var sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
            return sb.ToString().ToUpper();
        }


        private void sp1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!sp1.IsOpen)
                return;

            try
            {
                if (terminal) // MODO TERMINAL
                {
                 
                    int bytesReceived = sp1.BytesToRead;
                    if (bytesReceived <= 0) return;

                    byte[] bufferReceived = new byte[bytesReceived];
                    sp1.Read(bufferReceived, 0, bytesReceived);

                    if (radioRcvText.Checked)   // Text
                    {
                        string str = Encoding.ASCII.GetString(bufferReceived, 0, bytesReceived);
                        AppendTerminal(str, Color.Black);
                    }
                    else                        // Raw (HEX)
                    {
                        string hex = ByteArrayToHexString(bufferReceived) + Environment.NewLine;
                        AppendTerminal(hex, Color.Black);
                    }

                    return; // salimos del handler, no seguimos con boot/hwinfo
                }
                else if (bootmode) // MODO BOOTLOADER
                {
                    int bytes = sp1.BytesToRead;
                    if (bytes <= 0) return;

                    byte[] buffer = new byte[bytes];
                    sp1.Read(buffer, 0, bytes);

                    for (int i = 0; i < bytes; i++)
                    {
                        if (buffer[i] == 0x06) // ACK
                        {
                            nextLine = true;
                            _lineAckEvent.Set();   // despierta al hilo de envío
                            break;
                        }
                    }
                }
                else // MODO INFO HW (name, version, date, time)
                {
                    string chunk = sp1.ReadExisting();  // no bloqueante
                    if (string.IsNullOrEmpty(chunk))
                        return;

                    // Acumulamos todo
                    _hwInfoBuffer.Append(chunk);

                    string bufferStr = _hwInfoBuffer.ToString();

                    // Quitamos CR/LF para analizar
                    string normalized = bufferStr.Replace("\r", "").Replace("\n", "");

                    Console.WriteLine("HW RAW: " + bufferStr);

                    // ✅ Buscar el inicio REAL de la info de HW
                    int startIndex = normalized.IndexOf("name=");
                    if (startIndex < 0)
                    {
                        // Aún no ha llegado 'name=', solo hay basura/debug → si se hace gigante, reseteamos
                        if (_hwInfoBuffer.Length > 512)
                            _hwInfoBuffer.Clear();

                        return;
                    }

                    // Nos quedamos solo con la sección desde "name=" hacia adelante
                    string infoSection = normalized.Substring(startIndex);

                    // Verificamos que ya está completa (tiene todos los campos)
                    if (infoSection.Contains("name=") &&
                        infoSection.Contains("version=") &&
                        infoSection.Contains("date=") &&
                        infoSection.Contains("time="))
                    {
                        // ✅ Parseamos SOLO la parte limpia
                        UpdateHardwareInfoFromLine(infoSection);

                        // Limpiamos para el siguiente mensaje
                        _hwInfoBuffer.Clear();
                    }
                    else
                    {
                        // Aún falta parte de la línea (truncada), seguimos acumulando
                        // Safety net para no crecer infinito
                        if (_hwInfoBuffer.Length > 512)
                            _hwInfoBuffer.Clear();
                    }
                }



            }
            catch (TimeoutException)
            {
                // si tienes ReadTimeout > 0, puedes loguearlo, pero no revientes la UI
                // Console.WriteLine("Serial timeout");
            }
            catch (IOException)
            {
                // Aquí caes cuando cierras el puerto mientras había un Read en progreso.
                // Lo normal es simplemente ignorar y salir.
                // Console.WriteLine("Serial IO aborted (port closed/disposed)");
            }

        }

        private void btn_boot_Click(object sender, EventArgs e)
        {
            EnterBootloaderMode(sp1);
        }

        private void linkLabel_website_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.galio.dev");
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPage == tabPageTerminal)   // tu Tab "Terminal"
                terminal = true;
            else
                terminal = false;
        }

        private void btn_TerminalSend_Click(object sender, EventArgs e)
        {
            SendTerminalData();
        }
        private void txtSend_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                SendTerminalData();
            }
        }
        private void SendTerminalData()
        {
            if (!sp1.IsOpen)
                return;

            string text = txtSend.Text;
            if (string.IsNullOrWhiteSpace(text))
                return;

            try
            {
                if (radioSendAscii.Checked)
                {
                    // ASCII
                    sp1.Write(text);

                    if (chkNlCr.Checked)
                    {
                        sp1.Write("\r\n"); // NL/CR
                    }

                    // Eco en terminal (en rojo)
                    AppendTerminal("> " + text + (chkNlCr.Checked ? "\r\n" : ""), Color.Red);
                }
                else // HEX
                {
                    // Quitamos espacios
                    string hexStr = text.Replace(" ", "");
                    if (hexStr.Length % 2 != 0)
                    {
                        MessageBox.Show("Hexadecimal malformed", "Warning",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    byte[] buffer = new byte[hexStr.Length / 2];
                    for (int i = 0; i < hexStr.Length; i += 2)
                    {
                        buffer[i / 2] = Convert.ToByte(hexStr.Substring(i, 2), 16);
                    }

                    sp1.Write(buffer, 0, buffer.Length);

                    if (chkNlCr.Checked)
                    {
                        sp1.Write("\r\n");
                    }

                    AppendTerminal("> " + ByteArrayToHexString(buffer) + Environment.NewLine, Color.Red);
                }

                txtSend.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending data:\r\n" + ex.Message,
                    "Serial Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_terminalClear_Click(object sender, EventArgs e)
        {
            richTerminal.Clear();
        }
    }
}
