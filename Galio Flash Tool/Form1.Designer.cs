namespace Galio_Flash_Tool
{
    partial class Form1
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.sp1 = new System.IO.Ports.SerialPort(this.components);
            this.groupBox_port = new System.Windows.Forms.GroupBox();
            this.checkBox_autoReset = new System.Windows.Forms.CheckBox();
            this.btn_connect = new System.Windows.Forms.Button();
            this.cb_baud = new System.Windows.Forms.ComboBox();
            this.cb_port = new System.Windows.Forms.ComboBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageProgram = new System.Windows.Forms.TabPage();
            this.groupBox_hwInfo = new System.Windows.Forms.GroupBox();
            this.btn_boot = new System.Windows.Forms.Button();
            this.lbl_hwInfoTime = new System.Windows.Forms.Label();
            this.lbl_hwInfoDate = new System.Windows.Forms.Label();
            this.lbl_hwInfoName = new System.Windows.Forms.Label();
            this.lbl_hwInfoVersion = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox_uploadProgress = new System.Windows.Forms.GroupBox();
            this.lbl_percent = new System.Windows.Forms.Label();
            this.lbl_totalLines = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lbl_transferLines = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.groupBox_fwFile = new System.Windows.Forms.GroupBox();
            this.lbl_uploadStatus = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.btn_upload = new System.Windows.Forms.Button();
            this.lbl_created = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lbl_microcontroller = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_search = new System.Windows.Forms.Button();
            this.txtFirmwareFile = new System.Windows.Forms.TextBox();
            this.tabPageTerminal = new System.Windows.Forms.TabPage();
            this.groupBoxTerminalTx = new System.Windows.Forms.GroupBox();
            this.chkNlCr = new System.Windows.Forms.CheckBox();
            this.radioSendHex = new System.Windows.Forms.RadioButton();
            this.radioSendAscii = new System.Windows.Forms.RadioButton();
            this.txtSend = new System.Windows.Forms.TextBox();
            this.btn_TerminalSend = new System.Windows.Forms.Button();
            this.groupBox_terminalRX = new System.Windows.Forms.GroupBox();
            this.chkAutoscroll = new System.Windows.Forms.CheckBox();
            this.chkTimestamp = new System.Windows.Forms.CheckBox();
            this.radioRcvText = new System.Windows.Forms.RadioButton();
            this.radioRcvRaw = new System.Windows.Forms.RadioButton();
            this.btn_terminalClear = new System.Windows.Forms.Button();
            this.richTerminal = new System.Windows.Forms.RichTextBox();
            this.openFile = new System.Windows.Forms.OpenFileDialog();
            this.linkLabel_website = new System.Windows.Forms.LinkLabel();
            this.label10 = new System.Windows.Forms.Label();
            this.groupBox_port.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPageProgram.SuspendLayout();
            this.groupBox_hwInfo.SuspendLayout();
            this.groupBox_uploadProgress.SuspendLayout();
            this.groupBox_fwFile.SuspendLayout();
            this.tabPageTerminal.SuspendLayout();
            this.groupBoxTerminalTx.SuspendLayout();
            this.groupBox_terminalRX.SuspendLayout();
            this.SuspendLayout();
            // 
            // sp1
            // 
            this.sp1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.sp1_DataReceived);
            // 
            // groupBox_port
            // 
            this.groupBox_port.Controls.Add(this.checkBox_autoReset);
            this.groupBox_port.Controls.Add(this.btn_connect);
            this.groupBox_port.Controls.Add(this.cb_baud);
            this.groupBox_port.Controls.Add(this.cb_port);
            this.groupBox_port.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.groupBox_port.Location = new System.Drawing.Point(12, 13);
            this.groupBox_port.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox_port.Name = "groupBox_port";
            this.groupBox_port.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox_port.Size = new System.Drawing.Size(249, 79);
            this.groupBox_port.TabIndex = 0;
            this.groupBox_port.TabStop = false;
            this.groupBox_port.Text = "COM Port";
            // 
            // checkBox_autoReset
            // 
            this.checkBox_autoReset.AutoSize = true;
            this.checkBox_autoReset.Checked = true;
            this.checkBox_autoReset.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_autoReset.Location = new System.Drawing.Point(6, 53);
            this.checkBox_autoReset.Name = "checkBox_autoReset";
            this.checkBox_autoReset.Size = new System.Drawing.Size(80, 19);
            this.checkBox_autoReset.TabIndex = 3;
            this.checkBox_autoReset.Text = "Auto boot";
            this.checkBox_autoReset.UseVisualStyleBackColor = true;
            // 
            // btn_connect
            // 
            this.btn_connect.Location = new System.Drawing.Point(140, 24);
            this.btn_connect.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btn_connect.Name = "btn_connect";
            this.btn_connect.Size = new System.Drawing.Size(95, 33);
            this.btn_connect.TabIndex = 1;
            this.btn_connect.Text = "Connect";
            this.btn_connect.UseVisualStyleBackColor = true;
            this.btn_connect.Click += new System.EventHandler(this.btn_connect_Click);
            // 
            // cb_baud
            // 
            this.cb_baud.FormattingEnabled = true;
            this.cb_baud.Items.AddRange(new object[] {
            "9600",
            "19200",
            "38400",
            "57600",
            "115200"});
            this.cb_baud.Location = new System.Drawing.Point(73, 28);
            this.cb_baud.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cb_baud.Name = "cb_baud";
            this.cb_baud.Size = new System.Drawing.Size(61, 23);
            this.cb_baud.TabIndex = 2;
            this.cb_baud.Text = "115200";
            // 
            // cb_port
            // 
            this.cb_port.FormattingEnabled = true;
            this.cb_port.Location = new System.Drawing.Point(6, 28);
            this.cb_port.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cb_port.Name = "cb_port";
            this.cb_port.Size = new System.Drawing.Size(66, 23);
            this.cb_port.TabIndex = 1;
            this.cb_port.Click += new System.EventHandler(this.cb_port_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(253, 6);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(180, 86);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageProgram);
            this.tabControl1.Controls.Add(this.tabPageTerminal);
            this.tabControl1.Location = new System.Drawing.Point(12, 99);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(431, 334);
            this.tabControl1.TabIndex = 2;
            this.tabControl1.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControl1_Selected);
            // 
            // tabPageProgram
            // 
            this.tabPageProgram.Controls.Add(this.groupBox_hwInfo);
            this.tabPageProgram.Controls.Add(this.groupBox_uploadProgress);
            this.tabPageProgram.Controls.Add(this.groupBox_fwFile);
            this.tabPageProgram.Location = new System.Drawing.Point(4, 24);
            this.tabPageProgram.Name = "tabPageProgram";
            this.tabPageProgram.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageProgram.Size = new System.Drawing.Size(423, 306);
            this.tabPageProgram.TabIndex = 0;
            this.tabPageProgram.Text = "Program";
            this.tabPageProgram.UseVisualStyleBackColor = true;
            // 
            // groupBox_hwInfo
            // 
            this.groupBox_hwInfo.Controls.Add(this.btn_boot);
            this.groupBox_hwInfo.Controls.Add(this.lbl_hwInfoTime);
            this.groupBox_hwInfo.Controls.Add(this.lbl_hwInfoDate);
            this.groupBox_hwInfo.Controls.Add(this.lbl_hwInfoName);
            this.groupBox_hwInfo.Controls.Add(this.lbl_hwInfoVersion);
            this.groupBox_hwInfo.Controls.Add(this.label9);
            this.groupBox_hwInfo.Controls.Add(this.label8);
            this.groupBox_hwInfo.Controls.Add(this.label6);
            this.groupBox_hwInfo.Controls.Add(this.label4);
            this.groupBox_hwInfo.Enabled = false;
            this.groupBox_hwInfo.Location = new System.Drawing.Point(11, 6);
            this.groupBox_hwInfo.Name = "groupBox_hwInfo";
            this.groupBox_hwInfo.Size = new System.Drawing.Size(399, 76);
            this.groupBox_hwInfo.TabIndex = 3;
            this.groupBox_hwInfo.TabStop = false;
            this.groupBox_hwInfo.Text = "Hardware Info";
            // 
            // btn_boot
            // 
            this.btn_boot.Location = new System.Drawing.Point(298, 28);
            this.btn_boot.Name = "btn_boot";
            this.btn_boot.Size = new System.Drawing.Size(95, 33);
            this.btn_boot.TabIndex = 11;
            this.btn_boot.Text = "Boot";
            this.btn_boot.UseVisualStyleBackColor = true;
            this.btn_boot.Click += new System.EventHandler(this.btn_boot_Click);
            // 
            // lbl_hwInfoTime
            // 
            this.lbl_hwInfoTime.AutoSize = true;
            this.lbl_hwInfoTime.Location = new System.Drawing.Point(223, 46);
            this.lbl_hwInfoTime.Name = "lbl_hwInfoTime";
            this.lbl_hwInfoTime.Size = new System.Drawing.Size(17, 15);
            this.lbl_hwInfoTime.TabIndex = 10;
            this.lbl_hwInfoTime.Text = "--";
            // 
            // lbl_hwInfoDate
            // 
            this.lbl_hwInfoDate.AutoSize = true;
            this.lbl_hwInfoDate.Location = new System.Drawing.Point(223, 22);
            this.lbl_hwInfoDate.Name = "lbl_hwInfoDate";
            this.lbl_hwInfoDate.Size = new System.Drawing.Size(17, 15);
            this.lbl_hwInfoDate.TabIndex = 9;
            this.lbl_hwInfoDate.Text = "--";
            // 
            // lbl_hwInfoName
            // 
            this.lbl_hwInfoName.AutoSize = true;
            this.lbl_hwInfoName.Location = new System.Drawing.Point(71, 22);
            this.lbl_hwInfoName.Name = "lbl_hwInfoName";
            this.lbl_hwInfoName.Size = new System.Drawing.Size(17, 15);
            this.lbl_hwInfoName.TabIndex = 8;
            this.lbl_hwInfoName.Text = "--";
            // 
            // lbl_hwInfoVersion
            // 
            this.lbl_hwInfoVersion.AutoSize = true;
            this.lbl_hwInfoVersion.Location = new System.Drawing.Point(71, 46);
            this.lbl_hwInfoVersion.Name = "lbl_hwInfoVersion";
            this.lbl_hwInfoVersion.Size = new System.Drawing.Size(17, 15);
            this.lbl_hwInfoVersion.TabIndex = 7;
            this.lbl_hwInfoVersion.Text = "--";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(183, 46);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(37, 15);
            this.label9.TabIndex = 6;
            this.label9.Text = "Time:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(183, 22);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(34, 15);
            this.label8.TabIndex = 5;
            this.label8.Text = "Date:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(23, 46);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(48, 15);
            this.label6.TabIndex = 4;
            this.label6.Text = "Version:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(23, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(42, 15);
            this.label4.TabIndex = 3;
            this.label4.Text = "Name:";
            // 
            // groupBox_uploadProgress
            // 
            this.groupBox_uploadProgress.Controls.Add(this.lbl_percent);
            this.groupBox_uploadProgress.Controls.Add(this.lbl_totalLines);
            this.groupBox_uploadProgress.Controls.Add(this.label5);
            this.groupBox_uploadProgress.Controls.Add(this.lbl_transferLines);
            this.groupBox_uploadProgress.Controls.Add(this.label2);
            this.groupBox_uploadProgress.Controls.Add(this.progressBar1);
            this.groupBox_uploadProgress.Enabled = false;
            this.groupBox_uploadProgress.Location = new System.Drawing.Point(11, 211);
            this.groupBox_uploadProgress.Name = "groupBox_uploadProgress";
            this.groupBox_uploadProgress.Size = new System.Drawing.Size(406, 80);
            this.groupBox_uploadProgress.TabIndex = 2;
            this.groupBox_uploadProgress.TabStop = false;
            this.groupBox_uploadProgress.Text = "Upload Progress";
            // 
            // lbl_percent
            // 
            this.lbl_percent.AutoSize = true;
            this.lbl_percent.Location = new System.Drawing.Point(371, 52);
            this.lbl_percent.Name = "lbl_percent";
            this.lbl_percent.Size = new System.Drawing.Size(23, 15);
            this.lbl_percent.TabIndex = 5;
            this.lbl_percent.Text = "0%";
            this.lbl_percent.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_totalLines
            // 
            this.lbl_totalLines.AutoSize = true;
            this.lbl_totalLines.Location = new System.Drawing.Point(108, 52);
            this.lbl_totalLines.Name = "lbl_totalLines";
            this.lbl_totalLines.Size = new System.Drawing.Size(13, 15);
            this.lbl_totalLines.TabIndex = 4;
            this.lbl_totalLines.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(85, 52);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(12, 15);
            this.label5.TabIndex = 3;
            this.label5.Text = "/";
            // 
            // lbl_transferLines
            // 
            this.lbl_transferLines.AutoSize = true;
            this.lbl_transferLines.Location = new System.Drawing.Point(61, 52);
            this.lbl_transferLines.Name = "lbl_transferLines";
            this.lbl_transferLines.Size = new System.Drawing.Size(13, 15);
            this.lbl_transferLines.TabIndex = 2;
            this.lbl_transferLines.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "Lines:";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(6, 22);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(388, 16);
            this.progressBar1.TabIndex = 0;
            // 
            // groupBox_fwFile
            // 
            this.groupBox_fwFile.Controls.Add(this.lbl_uploadStatus);
            this.groupBox_fwFile.Controls.Add(this.label7);
            this.groupBox_fwFile.Controls.Add(this.btn_upload);
            this.groupBox_fwFile.Controls.Add(this.lbl_created);
            this.groupBox_fwFile.Controls.Add(this.label3);
            this.groupBox_fwFile.Controls.Add(this.lbl_microcontroller);
            this.groupBox_fwFile.Controls.Add(this.label1);
            this.groupBox_fwFile.Controls.Add(this.btn_search);
            this.groupBox_fwFile.Controls.Add(this.txtFirmwareFile);
            this.groupBox_fwFile.Enabled = false;
            this.groupBox_fwFile.Location = new System.Drawing.Point(11, 88);
            this.groupBox_fwFile.Name = "groupBox_fwFile";
            this.groupBox_fwFile.Size = new System.Drawing.Size(406, 117);
            this.groupBox_fwFile.TabIndex = 1;
            this.groupBox_fwFile.TabStop = false;
            this.groupBox_fwFile.Text = "Firmware File";
            // 
            // lbl_uploadStatus
            // 
            this.lbl_uploadStatus.AutoSize = true;
            this.lbl_uploadStatus.Location = new System.Drawing.Point(223, 87);
            this.lbl_uploadStatus.Name = "lbl_uploadStatus";
            this.lbl_uploadStatus.Size = new System.Drawing.Size(17, 15);
            this.lbl_uploadStatus.TabIndex = 6;
            this.lbl_uploadStatus.Text = "--";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(155, 87);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(42, 15);
            this.label7.TabIndex = 5;
            this.label7.Text = "Status:";
            // 
            // btn_upload
            // 
            this.btn_upload.Location = new System.Drawing.Point(299, 78);
            this.btn_upload.Name = "btn_upload";
            this.btn_upload.Size = new System.Drawing.Size(95, 33);
            this.btn_upload.TabIndex = 7;
            this.btn_upload.Text = "Upload";
            this.btn_upload.UseVisualStyleBackColor = true;
            this.btn_upload.Click += new System.EventHandler(this.btn_upload_Click);
            // 
            // lbl_created
            // 
            this.lbl_created.AutoSize = true;
            this.lbl_created.Location = new System.Drawing.Point(223, 58);
            this.lbl_created.Name = "lbl_created";
            this.lbl_created.Size = new System.Drawing.Size(17, 15);
            this.lbl_created.TabIndex = 5;
            this.lbl_created.Text = "--";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(155, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "Compiled:";
            // 
            // lbl_microcontroller
            // 
            this.lbl_microcontroller.AutoSize = true;
            this.lbl_microcontroller.Location = new System.Drawing.Point(71, 58);
            this.lbl_microcontroller.Name = "lbl_microcontroller";
            this.lbl_microcontroller.Size = new System.Drawing.Size(17, 15);
            this.lbl_microcontroller.TabIndex = 3;
            this.lbl_microcontroller.Text = "--";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Device:";
            // 
            // btn_search
            // 
            this.btn_search.Location = new System.Drawing.Point(299, 16);
            this.btn_search.Name = "btn_search";
            this.btn_search.Size = new System.Drawing.Size(95, 33);
            this.btn_search.TabIndex = 1;
            this.btn_search.Text = "Search";
            this.btn_search.UseVisualStyleBackColor = true;
            this.btn_search.Click += new System.EventHandler(this.btn_search_Click);
            // 
            // txtFirmwareFile
            // 
            this.txtFirmwareFile.Location = new System.Drawing.Point(26, 22);
            this.txtFirmwareFile.Name = "txtFirmwareFile";
            this.txtFirmwareFile.Size = new System.Drawing.Size(267, 23);
            this.txtFirmwareFile.TabIndex = 0;
            // 
            // tabPageTerminal
            // 
            this.tabPageTerminal.Controls.Add(this.groupBoxTerminalTx);
            this.tabPageTerminal.Controls.Add(this.groupBox_terminalRX);
            this.tabPageTerminal.Location = new System.Drawing.Point(4, 24);
            this.tabPageTerminal.Name = "tabPageTerminal";
            this.tabPageTerminal.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTerminal.Size = new System.Drawing.Size(423, 306);
            this.tabPageTerminal.TabIndex = 1;
            this.tabPageTerminal.Text = "Terminal";
            this.tabPageTerminal.UseVisualStyleBackColor = true;
            // 
            // groupBoxTerminalTx
            // 
            this.groupBoxTerminalTx.Controls.Add(this.chkNlCr);
            this.groupBoxTerminalTx.Controls.Add(this.radioSendHex);
            this.groupBoxTerminalTx.Controls.Add(this.radioSendAscii);
            this.groupBoxTerminalTx.Controls.Add(this.txtSend);
            this.groupBoxTerminalTx.Controls.Add(this.btn_TerminalSend);
            this.groupBoxTerminalTx.Enabled = false;
            this.groupBoxTerminalTx.Location = new System.Drawing.Point(3, 230);
            this.groupBoxTerminalTx.Name = "groupBoxTerminalTx";
            this.groupBoxTerminalTx.Size = new System.Drawing.Size(414, 73);
            this.groupBoxTerminalTx.TabIndex = 1;
            this.groupBoxTerminalTx.TabStop = false;
            this.groupBoxTerminalTx.Text = "Send Data";
            // 
            // chkNlCr
            // 
            this.chkNlCr.AutoSize = true;
            this.chkNlCr.Location = new System.Drawing.Point(259, 51);
            this.chkNlCr.Name = "chkNlCr";
            this.chkNlCr.Size = new System.Drawing.Size(61, 19);
            this.chkNlCr.TabIndex = 7;
            this.chkNlCr.Text = "NL/CR";
            this.chkNlCr.UseVisualStyleBackColor = true;
            // 
            // radioSendHex
            // 
            this.radioSendHex.AutoSize = true;
            this.radioSendHex.Location = new System.Drawing.Point(66, 51);
            this.radioSendHex.Name = "radioSendHex";
            this.radioSendHex.Size = new System.Drawing.Size(47, 19);
            this.radioSendHex.TabIndex = 6;
            this.radioSendHex.TabStop = true;
            this.radioSendHex.Text = "HEX";
            this.radioSendHex.UseVisualStyleBackColor = true;
            // 
            // radioSendAscii
            // 
            this.radioSendAscii.AutoSize = true;
            this.radioSendAscii.Location = new System.Drawing.Point(7, 51);
            this.radioSendAscii.Name = "radioSendAscii";
            this.radioSendAscii.Size = new System.Drawing.Size(53, 19);
            this.radioSendAscii.TabIndex = 5;
            this.radioSendAscii.TabStop = true;
            this.radioSendAscii.Text = "ASCII";
            this.radioSendAscii.UseVisualStyleBackColor = true;
            // 
            // txtSend
            // 
            this.txtSend.Location = new System.Drawing.Point(9, 22);
            this.txtSend.Name = "txtSend";
            this.txtSend.Size = new System.Drawing.Size(311, 23);
            this.txtSend.TabIndex = 3;
            // 
            // btn_TerminalSend
            // 
            this.btn_TerminalSend.Location = new System.Drawing.Point(329, 22);
            this.btn_TerminalSend.Name = "btn_TerminalSend";
            this.btn_TerminalSend.Size = new System.Drawing.Size(75, 23);
            this.btn_TerminalSend.TabIndex = 2;
            this.btn_TerminalSend.Text = "Send";
            this.btn_TerminalSend.UseVisualStyleBackColor = true;
            this.btn_TerminalSend.Click += new System.EventHandler(this.btn_TerminalSend_Click);
            // 
            // groupBox_terminalRX
            // 
            this.groupBox_terminalRX.Controls.Add(this.chkAutoscroll);
            this.groupBox_terminalRX.Controls.Add(this.chkTimestamp);
            this.groupBox_terminalRX.Controls.Add(this.radioRcvText);
            this.groupBox_terminalRX.Controls.Add(this.radioRcvRaw);
            this.groupBox_terminalRX.Controls.Add(this.btn_terminalClear);
            this.groupBox_terminalRX.Controls.Add(this.richTerminal);
            this.groupBox_terminalRX.Enabled = false;
            this.groupBox_terminalRX.Location = new System.Drawing.Point(3, 6);
            this.groupBox_terminalRX.Name = "groupBox_terminalRX";
            this.groupBox_terminalRX.Size = new System.Drawing.Size(414, 218);
            this.groupBox_terminalRX.TabIndex = 0;
            this.groupBox_terminalRX.TabStop = false;
            this.groupBox_terminalRX.Text = "Received Data";
            // 
            // chkAutoscroll
            // 
            this.chkAutoscroll.AutoSize = true;
            this.chkAutoscroll.Location = new System.Drawing.Point(214, 13);
            this.chkAutoscroll.Name = "chkAutoscroll";
            this.chkAutoscroll.Size = new System.Drawing.Size(80, 19);
            this.chkAutoscroll.TabIndex = 6;
            this.chkAutoscroll.Text = "Autoscroll";
            this.chkAutoscroll.UseVisualStyleBackColor = true;
            // 
            // chkTimestamp
            // 
            this.chkTimestamp.AutoSize = true;
            this.chkTimestamp.Location = new System.Drawing.Point(122, 13);
            this.chkTimestamp.Name = "chkTimestamp";
            this.chkTimestamp.Size = new System.Drawing.Size(86, 19);
            this.chkTimestamp.TabIndex = 5;
            this.chkTimestamp.Text = "Timestamp";
            this.chkTimestamp.UseVisualStyleBackColor = true;
            // 
            // radioRcvText
            // 
            this.radioRcvText.AutoSize = true;
            this.radioRcvText.Location = new System.Drawing.Point(6, 15);
            this.radioRcvText.Name = "radioRcvText";
            this.radioRcvText.Size = new System.Drawing.Size(46, 19);
            this.radioRcvText.TabIndex = 4;
            this.radioRcvText.TabStop = true;
            this.radioRcvText.Text = "Text";
            this.radioRcvText.UseVisualStyleBackColor = true;
            // 
            // radioRcvRaw
            // 
            this.radioRcvRaw.AutoSize = true;
            this.radioRcvRaw.Location = new System.Drawing.Point(58, 15);
            this.radioRcvRaw.Name = "radioRcvRaw";
            this.radioRcvRaw.Size = new System.Drawing.Size(47, 19);
            this.radioRcvRaw.TabIndex = 3;
            this.radioRcvRaw.TabStop = true;
            this.radioRcvRaw.Text = "Raw";
            this.radioRcvRaw.UseVisualStyleBackColor = true;
            // 
            // btn_terminalClear
            // 
            this.btn_terminalClear.Location = new System.Drawing.Point(329, 13);
            this.btn_terminalClear.Name = "btn_terminalClear";
            this.btn_terminalClear.Size = new System.Drawing.Size(75, 23);
            this.btn_terminalClear.TabIndex = 1;
            this.btn_terminalClear.Text = "Clear";
            this.btn_terminalClear.UseVisualStyleBackColor = true;
            this.btn_terminalClear.Click += new System.EventHandler(this.btn_terminalClear_Click);
            // 
            // richTerminal
            // 
            this.richTerminal.Location = new System.Drawing.Point(6, 42);
            this.richTerminal.Name = "richTerminal";
            this.richTerminal.Size = new System.Drawing.Size(402, 169);
            this.richTerminal.TabIndex = 0;
            this.richTerminal.Text = "";
            // 
            // openFile
            // 
            this.openFile.FileName = "openFileDialog1";
            // 
            // linkLabel_website
            // 
            this.linkLabel_website.AutoSize = true;
            this.linkLabel_website.Location = new System.Drawing.Point(345, 436);
            this.linkLabel_website.Name = "linkLabel_website";
            this.linkLabel_website.Size = new System.Drawing.Size(94, 15);
            this.linkLabel_website.TabIndex = 3;
            this.linkLabel_website.TabStop = true;
            this.linkLabel_website.Text = "Galio Electronics";
            this.linkLabel_website.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_website_LinkClicked);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(16, 436);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(118, 15);
            this.label10.TabIndex = 4;
            this.label10.Text = "desarrollo@galio.dev";
            this.label10.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(451, 461);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.linkLabel_website);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.groupBox_port);
            this.Controls.Add(this.pictureBox1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Galio Flash Tool v1.0.3";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox_port.ResumeLayout(false);
            this.groupBox_port.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPageProgram.ResumeLayout(false);
            this.groupBox_hwInfo.ResumeLayout(false);
            this.groupBox_hwInfo.PerformLayout();
            this.groupBox_uploadProgress.ResumeLayout(false);
            this.groupBox_uploadProgress.PerformLayout();
            this.groupBox_fwFile.ResumeLayout(false);
            this.groupBox_fwFile.PerformLayout();
            this.tabPageTerminal.ResumeLayout(false);
            this.groupBoxTerminalTx.ResumeLayout(false);
            this.groupBoxTerminalTx.PerformLayout();
            this.groupBox_terminalRX.ResumeLayout(false);
            this.groupBox_terminalRX.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.IO.Ports.SerialPort sp1;
        private System.Windows.Forms.GroupBox groupBox_port;
        private System.Windows.Forms.Button btn_connect;
        private System.Windows.Forms.ComboBox cb_baud;
        private System.Windows.Forms.ComboBox cb_port;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageProgram;
        private System.Windows.Forms.GroupBox groupBox_fwFile;
        private System.Windows.Forms.Button btn_search;
        private System.Windows.Forms.TextBox txtFirmwareFile;
        private System.Windows.Forms.TabPage tabPageTerminal;
        private System.Windows.Forms.Label lbl_microcontroller;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbl_created;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox_uploadProgress;
        private System.Windows.Forms.Label lbl_uploadStatus;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lbl_totalLines;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lbl_transferLines;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button btn_upload;
        private System.Windows.Forms.GroupBox groupBox_hwInfo;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkBox_autoReset;
        private System.Windows.Forms.Label lbl_hwInfoTime;
        private System.Windows.Forms.Label lbl_hwInfoDate;
        private System.Windows.Forms.Label lbl_hwInfoName;
        private System.Windows.Forms.Label lbl_hwInfoVersion;
        private System.Windows.Forms.OpenFileDialog openFile;
        private System.Windows.Forms.Label lbl_percent;
        private System.Windows.Forms.Button btn_boot;
        private System.Windows.Forms.LinkLabel linkLabel_website;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.GroupBox groupBoxTerminalTx;
        private System.Windows.Forms.GroupBox groupBox_terminalRX;
        private System.Windows.Forms.Button btn_terminalClear;
        private System.Windows.Forms.RichTextBox richTerminal;
        private System.Windows.Forms.Button btn_TerminalSend;
        private System.Windows.Forms.CheckBox chkAutoscroll;
        private System.Windows.Forms.CheckBox chkTimestamp;
        private System.Windows.Forms.RadioButton radioRcvText;
        private System.Windows.Forms.RadioButton radioRcvRaw;
        private System.Windows.Forms.TextBox txtSend;
        private System.Windows.Forms.RadioButton radioSendAscii;
        private System.Windows.Forms.RadioButton radioSendHex;
        private System.Windows.Forms.CheckBox chkNlCr;
    }
}

