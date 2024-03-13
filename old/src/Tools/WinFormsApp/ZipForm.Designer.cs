namespace Ionic.Zip.Forms
{
    partial class ZipForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ZipForm));
            this.tbDirectoryToZip = new System.Windows.Forms.TextBox();
            this.tbZipToCreate = new System.Windows.Forms.TextBox();
            this.btnZipupDirBrowse = new System.Windows.Forms.Button();
            this.btnZipUp = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.comboEncoding = new System.Windows.Forms.ComboBox();
            this.tbComment = new System.Windows.Forms.TextBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.comboCompLevel = new System.Windows.Forms.ComboBox();
            this.comboEncryption = new System.Windows.Forms.ComboBox();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.chkHidePassword = new System.Windows.Forms.CheckBox();
            this.listView1 = new System.Windows.Forms.ListView();
            this.btnOpenZip = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label20 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.comboExistingFileAction = new System.Windows.Forms.ComboBox();
            this.tbSelectionToExtract = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.chkOpenExplorer = new System.Windows.Forms.CheckBox();
            this.btnExtractDirBrowse = new System.Windows.Forms.Button();
            this.tbExtractDir = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.btnExtract = new System.Windows.Forms.Button();
            this.btnReadZipBrowse = new System.Windows.Forms.Button();
            this.tbZipToOpen = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label21 = new System.Windows.Forms.Label();
            this.comboEncodingUsage = new System.Windows.Forms.ComboBox();
            this.label18 = new System.Windows.Forms.Label();
            this.chkRemoveFiles = new System.Windows.Forms.CheckBox();
            this.label17 = new System.Windows.Forms.Label();
            this.comboSplit = new System.Windows.Forms.ComboBox();
            this.chkUnixTime = new System.Windows.Forms.CheckBox();
            this.chkWindowsTime = new System.Windows.Forms.CheckBox();
            this.label16 = new System.Windows.Forms.Label();
            this.tbExeOnUnpack = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.tbDefaultExtractDirectory = new System.Windows.Forms.TextBox();
            this.comboZip64 = new System.Windows.Forms.ComboBox();
            this.comboCompMethod = new System.Windows.Forms.ComboBox();
            this.comboFlavor = new System.Windows.Forms.ComboBox();
            this.btnCreateZipBrowse = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkRecurse = new System.Windows.Forms.CheckBox();
            this.chkTraverseJunctions = new System.Windows.Forms.CheckBox();
            this.tbDirectoryInArchive = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label14 = new System.Windows.Forms.Label();
            this.tbSelectionToZip = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.btnClearItemsToZip = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.listView2 = new ListViewEx.ListViewEx();
            this.chCheckbox = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            //
            // tbDirectoryToZip
            //
            this.tbDirectoryToZip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDirectoryToZip.Location = new System.Drawing.Point(104, 13);
            this.tbDirectoryToZip.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.tbDirectoryToZip.Name = "tbDirectoryToZip";
            this.tbDirectoryToZip.Size = new System.Drawing.Size(289, 20);
            this.tbDirectoryToZip.TabIndex = 10;
            this.tbDirectoryToZip.Leave += new System.EventHandler(this.tbDirectoryToZip_Leave);
            //
            // tbZipToCreate
            //
            this.tbZipToCreate.AcceptsReturn = true;
            this.tbZipToCreate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbZipToCreate.Location = new System.Drawing.Point(104, 11);
            this.tbZipToCreate.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.tbZipToCreate.Name = "tbZipToCreate";
            this.tbZipToCreate.Size = new System.Drawing.Size(432, 20);
            this.tbZipToCreate.TabIndex = 30;
            //
            // btnZipupDirBrowse
            //
            this.btnZipupDirBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnZipupDirBrowse.Location = new System.Drawing.Point(399, 13);
            this.btnZipupDirBrowse.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.btnZipupDirBrowse.Name = "btnZipupDirBrowse";
            this.btnZipupDirBrowse.Size = new System.Drawing.Size(24, 20);
            this.btnZipupDirBrowse.TabIndex = 11;
            this.btnZipupDirBrowse.Text = "...";
            this.toolTip1.SetToolTip(this.btnZipupDirBrowse, "Browse for a directory to search in");
            this.btnZipupDirBrowse.UseVisualStyleBackColor = true;
            this.btnZipupDirBrowse.Click += new System.EventHandler(this.btnDirBrowse_Click);
            //
            // btnZipUp
            //
            this.btnZipUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnZipUp.Location = new System.Drawing.Point(512, 460);
            this.btnZipUp.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.btnZipUp.Name = "btnZipUp";
            this.btnZipUp.Size = new System.Drawing.Size(66, 26);
            this.btnZipUp.TabIndex = 140;
            this.btnZipUp.Text = "Zip All";
            this.toolTip1.SetToolTip(this.btnZipUp, "actually save the Zip file. ");
            this.btnZipUp.UseVisualStyleBackColor = true;
            this.btnZipUp.Click += new System.EventHandler(this.btnZipup_Click);
            //
            // btnCancel
            //
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Enabled = false;
            this.btnCancel.Location = new System.Drawing.Point(512, 489);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(66, 26);
            this.btnCancel.TabIndex = 90;
            this.btnCancel.Text = "Cancel";
            this.toolTip1.SetToolTip(this.btnCancel, "cancel the currently running operation");
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            // progressBar1
            //
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(6, 490);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(496, 10);
            this.progressBar1.TabIndex = 4;
            //
            // progressBar2
            //
            this.progressBar2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar2.Location = new System.Drawing.Point(6, 503);
            this.progressBar2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(496, 10);
            this.progressBar2.TabIndex = 17;
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 17);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "directory to add: ";
            //
            // label2
            //
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 15);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "file to save to:";
            //
            // label3
            //
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 39);
            this.label3.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(36, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "flavor:";
            //
            // label4
            //
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(250, 64);
            this.label4.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(45, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "level:";
            //
            // label5
            //
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 89);
            this.label5.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(54, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "encoding:";
            //
            // label6
            //
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 139);
            this.label6.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "comment:";
            //
            // label7
            //
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 64);
            this.label7.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(55, 13);
            this.label7.TabIndex = 5;
            this.label7.Text = "ZIP64?:";
            //
            // label8
            //
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(250, 89);
            this.label8.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(59, 13);
            this.label8.TabIndex = 91;
            this.label8.Text = "encryption:";
            //
            // label9
            //
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(250, 114);
            this.label9.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(55, 13);
            this.label9.TabIndex = 93;
            this.label9.Text = "password:";
            //
            // label22
            //
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(250, 39);
            this.label22.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(59, 13);
            this.label22.TabIndex = 94;
            this.label22.Text = "method:";
            //
            // comboEncoding
            //
            this.comboEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboEncoding.FormattingEnabled = true;
            this.comboEncoding.Location = new System.Drawing.Point(104, 85);
            this.comboEncoding.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.comboEncoding.Name = "comboEncoding";
            this.comboEncoding.Size = new System.Drawing.Size(128, 21);
            this.comboEncoding.TabIndex = 60;
            this.toolTip1.SetToolTip(this.comboEncoding, "use this encoding when saving the file");
            this.comboEncoding.SelectedIndexChanged += new System.EventHandler(this.comboEncoding_SelectedIndexChanged);
            //
            // tbComment
            //
            this.tbComment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbComment.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbComment.ForeColor = System.Drawing.SystemColors.InactiveCaption;
            this.tbComment.Location = new System.Drawing.Point(104, 133);
            this.tbComment.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.tbComment.Name = "tbComment";
            this.tbComment.Size = new System.Drawing.Size(432, 20);
            this.tbComment.TabIndex = 100;
            this.tbComment.Text = "-zip file comment here-";
            this.toolTip1.SetToolTip(this.tbComment, "a comment to embed in the zip file");
            this.tbComment.Enter += new System.EventHandler(this.tbComment_Enter);
            this.tbComment.Leave += new System.EventHandler(this.tbComment_Leave);
            //
            // lblStatus
            //
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 471);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 13);
            this.lblStatus.TabIndex = 8;
            //
            // comboCompLevel
            //
            this.comboCompLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboCompLevel.FormattingEnabled = true;
            this.comboCompLevel.Location = new System.Drawing.Point(310, 60);
            this.comboCompLevel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.comboCompLevel.Name = "comboCompLevel";
            this.comboCompLevel.Size = new System.Drawing.Size(122, 21);
            this.comboCompLevel.TabIndex = 50;
            this.toolTip1.SetToolTip(this.comboCompLevel, "The compression level to use when creating the zip.");
            //
            // comboEncryption
            //
            this.comboEncryption.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboEncryption.FormattingEnabled = true;
            this.comboEncryption.Location = new System.Drawing.Point(310, 85);
            this.comboEncryption.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.comboEncryption.Name = "comboEncryption";
            this.comboEncryption.Size = new System.Drawing.Size(122, 21);
            this.comboEncryption.TabIndex = 80;
            this.toolTip1.SetToolTip(this.comboEncryption, "AES is not compatible with some other zip tools.");
            this.comboEncryption.SelectedIndexChanged += new System.EventHandler(this.comboEncryption_SelectedIndexChanged);
            //
            // tbPassword
            //
            this.tbPassword.AcceptsReturn = true;
            this.tbPassword.Location = new System.Drawing.Point(310, 110);
            this.tbPassword.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.PasswordChar = '*';
            this.tbPassword.Size = new System.Drawing.Size(104, 20);
            this.tbPassword.TabIndex = 90;
            this.tbPassword.Text = "c:\\dinoch\\dev\\dotnet\\zip\\test\\U.zip";
            this.tbPassword.TextChanged += new System.EventHandler(this.tbPassword_TextChanged);
            //
            // chkHidePassword
            //
            this.chkHidePassword.AutoSize = true;
            this.chkHidePassword.Checked = true;
            this.chkHidePassword.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkHidePassword.Location = new System.Drawing.Point(418, 113);
            this.chkHidePassword.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.chkHidePassword.Name = "chkHidePassword";
            this.chkHidePassword.Size = new System.Drawing.Size(15, 14);
            this.chkHidePassword.TabIndex = 91;
            this.toolTip1.SetToolTip(this.chkHidePassword, "check to hide password characters");
            this.chkHidePassword.UseVisualStyleBackColor = true;
            this.chkHidePassword.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            //
            // listView1
            //
            this.listView1.AllowDrop = true;
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.GridLines = true;
            this.listView1.Location = new System.Drawing.Point(6, 137);
            this.listView1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(572, 327);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView1_ColumnClick);
            this.listView1.DragDrop += new System.Windows.Forms.DragEventHandler(this.listView1_DragDrop);
            this.listView1.DragEnter += new System.Windows.Forms.DragEventHandler(this.listView_DragEnter);
            //
            // btnOpenZip
            //
            this.btnOpenZip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenZip.Location = new System.Drawing.Point(518, 27);
            this.btnOpenZip.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.btnOpenZip.Name = "btnOpenZip";
            this.btnOpenZip.Size = new System.Drawing.Size(60, 24);
            this.btnOpenZip.TabIndex = 14;
            this.btnOpenZip.Text = "Open";
            this.toolTip1.SetToolTip(this.btnOpenZip, "open and read the zip file");
            this.btnOpenZip.UseVisualStyleBackColor = true;
            this.btnOpenZip.Click += new System.EventHandler(this.btnOpen_Click);
            //
            // tabControl1
            //
            this.tabControl1.AllowDrop = true;
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(592, 548);
            this.tabControl1.TabIndex = 96;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            this.tabControl1.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.tabControl1_Selecting);
            //
            // tabPage1
            //
            this.tabPage1.Controls.Add(this.label20);
            this.tabPage1.Controls.Add(this.label19);
            this.tabPage1.Controls.Add(this.comboExistingFileAction);
            this.tabPage1.Controls.Add(this.tbSelectionToExtract);
            this.tabPage1.Controls.Add(this.label13);
            this.tabPage1.Controls.Add(this.chkOpenExplorer);
            this.tabPage1.Controls.Add(this.btnExtractDirBrowse);
            this.tabPage1.Controls.Add(this.tbExtractDir);
            this.tabPage1.Controls.Add(this.label11);
            this.tabPage1.Controls.Add(this.label10);
            this.tabPage1.Controls.Add(this.btnExtract);
            this.tabPage1.Controls.Add(this.btnReadZipBrowse);
            this.tabPage1.Controls.Add(this.tbZipToOpen);
            this.tabPage1.Controls.Add(this.listView1);
            this.tabPage1.Controls.Add(this.btnOpenZip);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.tabPage1.Size = new System.Drawing.Size(584, 522);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Read/Extract";
            this.tabPage1.UseVisualStyleBackColor = true;
            //
            // label20
            //
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(5, 33);
            this.label20.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(54, 13);
            this.label20.TabIndex = 43;
            this.label20.Text = "encoding:";
            //
            // label19
            //
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(6, 114);
            this.label19.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(108, 13);
            this.label19.TabIndex = 42;
            this.label19.Text = "action for existing file:";
            //
            // comboExistingFileAction
            //
            this.comboExistingFileAction.FormattingEnabled = true;
            this.comboExistingFileAction.Location = new System.Drawing.Point(114, 109);
            this.comboExistingFileAction.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.comboExistingFileAction.Name = "comboExistingFileAction";
            this.comboExistingFileAction.Size = new System.Drawing.Size(104, 21);
            this.comboExistingFileAction.TabIndex = 41;
            this.toolTip1.SetToolTip(this.comboExistingFileAction, "What to do when extracting a file that already exists");
            //
            // tbSelectionToExtract
            //
            this.tbSelectionToExtract.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSelectionToExtract.Location = new System.Drawing.Point(60, 83);
            this.tbSelectionToExtract.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.tbSelectionToExtract.Name = "tbSelectionToExtract";
            this.tbSelectionToExtract.Size = new System.Drawing.Size(489, 20);
            this.tbSelectionToExtract.TabIndex = 24;
            this.tbSelectionToExtract.Text = "*.*";
            this.toolTip1.SetToolTip(this.tbSelectionToExtract, "Selection criteria.  eg, (name = *.* and size> 1000) etc.  Also use atime/mtime/c" +
        "time and attributes. (HRSA)");
            //
            // label13
            //
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(6, 87);
            this.label13.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(52, 13);
            this.label13.TabIndex = 27;
            this.label13.Text = "selection:";
            //
            // chkOpenExplorer
            //
            this.chkOpenExplorer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkOpenExplorer.AutoSize = true;
            this.chkOpenExplorer.Location = new System.Drawing.Point(236, 111);
            this.chkOpenExplorer.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.chkOpenExplorer.Name = "chkOpenExplorer";
            this.chkOpenExplorer.Size = new System.Drawing.Size(91, 17);
            this.chkOpenExplorer.TabIndex = 28;
            this.chkOpenExplorer.Text = "open Explorer";
            this.toolTip1.SetToolTip(this.chkOpenExplorer, "open explorer after extraction");
            this.chkOpenExplorer.UseVisualStyleBackColor = true;
            //
            // btnExtractDirBrowse
            //
            this.btnExtractDirBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExtractDirBrowse.Location = new System.Drawing.Point(554, 54);
            this.btnExtractDirBrowse.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.btnExtractDirBrowse.Name = "btnExtractDirBrowse";
            this.btnExtractDirBrowse.Size = new System.Drawing.Size(24, 24);
            this.btnExtractDirBrowse.TabIndex = 23;
            this.btnExtractDirBrowse.Text = "...";
            this.btnExtractDirBrowse.UseVisualStyleBackColor = true;
            this.btnExtractDirBrowse.Click += new System.EventHandler(this.btnExtractDirBrowse_Click);
            //
            // tbExtractDir
            //
            this.tbExtractDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbExtractDir.Location = new System.Drawing.Point(60, 57);
            this.tbExtractDir.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.tbExtractDir.Name = "tbExtractDir";
            this.tbExtractDir.Size = new System.Drawing.Size(489, 20);
            this.tbExtractDir.TabIndex = 22;
            //
            // label11
            //
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 61);
            this.label11.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(54, 13);
            this.label11.TabIndex = 16;
            this.label11.Text = "extract to:";
            //
            // label10
            //
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 9);
            this.label10.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(45, 13);
            this.label10.TabIndex = 15;
            this.label10.Text = "archive:";
            //
            // btnExtract
            //
            this.btnExtract.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExtract.Enabled = false;
            this.btnExtract.Location = new System.Drawing.Point(518, 106);
            this.btnExtract.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.btnExtract.Name = "btnExtract";
            this.btnExtract.Size = new System.Drawing.Size(60, 24);
            this.btnExtract.TabIndex = 24;
            this.btnExtract.Text = "Extract";
            this.btnExtract.UseVisualStyleBackColor = true;
            this.btnExtract.Click += new System.EventHandler(this.btnExtract_Click);
            //
            // btnReadZipBrowse
            //
            this.btnReadZipBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReadZipBrowse.Location = new System.Drawing.Point(554, 3);
            this.btnReadZipBrowse.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.btnReadZipBrowse.Name = "btnReadZipBrowse";
            this.btnReadZipBrowse.Size = new System.Drawing.Size(24, 24);
            this.btnReadZipBrowse.TabIndex = 13;
            this.btnReadZipBrowse.Text = "...";
            this.btnReadZipBrowse.UseVisualStyleBackColor = true;
            this.btnReadZipBrowse.Click += new System.EventHandler(this.btnZipBrowse_Click);
            //
            // tbZipToOpen
            //
            this.tbZipToOpen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbZipToOpen.Location = new System.Drawing.Point(60, 5);
            this.tbZipToOpen.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.tbZipToOpen.Name = "tbZipToOpen";
            this.tbZipToOpen.Size = new System.Drawing.Size(489, 20);
            this.tbZipToOpen.TabIndex = 12;
            //
            // tabPage2
            //
            this.tabPage2.Controls.Add(this.groupBox2);
            this.tabPage2.Controls.Add(this.groupBox1);
            this.tabPage2.Controls.Add(this.checkBox1);
            this.tabPage2.Controls.Add(this.btnClearItemsToZip);
            this.tabPage2.Controls.Add(this.textBox1);
            this.tabPage2.Controls.Add(this.listView2);
            this.tabPage2.Controls.Add(this.progressBar2);
            this.tabPage2.Controls.Add(this.lblStatus);
            this.tabPage2.Controls.Add(this.btnCancel);
            this.tabPage2.Controls.Add(this.progressBar1);
            this.tabPage2.Controls.Add(this.btnZipUp);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.tabPage2.Size = new System.Drawing.Size(584, 522);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Create";
            this.tabPage2.UseVisualStyleBackColor = true;
            //
            // groupBox2
            //
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label21);
            this.groupBox2.Controls.Add(this.comboEncodingUsage);
            this.groupBox2.Controls.Add(this.label18);
            this.groupBox2.Controls.Add(this.chkRemoveFiles);
            this.groupBox2.Controls.Add(this.label17);
            this.groupBox2.Controls.Add(this.comboSplit);
            this.groupBox2.Controls.Add(this.chkUnixTime);
            this.groupBox2.Controls.Add(this.chkWindowsTime);
            this.groupBox2.Controls.Add(this.label16);
            this.groupBox2.Controls.Add(this.tbExeOnUnpack);
            this.groupBox2.Controls.Add(this.label15);
            this.groupBox2.Controls.Add(this.tbDefaultExtractDirectory);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.tbZipToCreate);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.comboEncoding);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.tbComment);
            this.groupBox2.Controls.Add(this.comboZip64);
            this.groupBox2.Controls.Add(this.comboCompLevel);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.comboFlavor);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.btnCreateZipBrowse);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.chkHidePassword);
            this.groupBox2.Controls.Add(this.comboCompMethod);
            this.groupBox2.Controls.Add(this.tbPassword);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label22);
            this.groupBox2.Controls.Add(this.comboEncryption);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(6, 84);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.groupBox2.Size = new System.Drawing.Size(572, 230);
            this.groupBox2.TabIndex = 104;
            this.groupBox2.TabStop = false;
            //
            // label21
            //
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(6, 112);
            this.label21.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(86, 13);
            this.label21.TabIndex = 127;
            this.label21.Text = "encoding usage:";
            //
            // comboEncodingUsage
            //
            this.comboEncodingUsage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboEncodingUsage.FormattingEnabled = true;
            this.comboEncodingUsage.Location = new System.Drawing.Point(104, 108);
            this.comboEncodingUsage.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.comboEncodingUsage.Name = "comboEncodingUsage";
            this.comboEncodingUsage.Size = new System.Drawing.Size(128, 21);
            this.comboEncodingUsage.TabIndex = 128;
            this.toolTip1.SetToolTip(this.comboEncodingUsage, "use this encoding when saving the file");
            //
            // label18
            //
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(6, 209);
            this.label18.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(66, 13);
            this.label18.TabIndex = 126;
            this.label18.Text = "remove files:";
            //
            // chkRemoveFiles
            //
            this.chkRemoveFiles.AutoSize = true;
            this.chkRemoveFiles.Location = new System.Drawing.Point(104, 208);
            this.chkRemoveFiles.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.chkRemoveFiles.Name = "chkRemoveFiles";
            this.chkRemoveFiles.Size = new System.Drawing.Size(15, 14);
            this.chkRemoveFiles.TabIndex = 125;
            this.toolTip1.SetToolTip(this.chkRemoveFiles, "remove files after running post-extract command");
            this.chkRemoveFiles.UseVisualStyleBackColor = true;
            //
            // label17
            //
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(447, 89);
            this.label17.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(28, 13);
            this.label17.TabIndex = 124;
            this.label17.Text = "split:";
            //
            // comboSplit
            //
            this.comboSplit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSplit.FormattingEnabled = true;
            this.comboSplit.Location = new System.Drawing.Point(477, 85);
            this.comboSplit.Name = "comboSplit";
            this.comboSplit.Size = new System.Drawing.Size(85, 21);
            this.comboSplit.TabIndex = 123;
            this.toolTip1.SetToolTip(this.comboSplit, "segment size when creating a spanned archive");
            this.comboSplit.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.comboBox1_DrawItem);
            //
            // chkUnixTime
            //
            this.chkUnixTime.AutoSize = true;
            this.chkUnixTime.Location = new System.Drawing.Point(450, 60);
            this.chkUnixTime.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.chkUnixTime.Name = "chkUnixTime";
            this.chkUnixTime.Size = new System.Drawing.Size(77, 17);
            this.chkUnixTime.TabIndex = 122;
            this.chkUnixTime.Text = "times: Unix";
            this.toolTip1.SetToolTip(this.chkUnixTime, "store extended times in Unix format");
            this.chkUnixTime.UseVisualStyleBackColor = true;
            //
            // chkWindowsTime
            //
            this.chkWindowsTime.AutoSize = true;
            this.chkWindowsTime.Checked = true;
            this.chkWindowsTime.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkWindowsTime.Location = new System.Drawing.Point(450, 39);
            this.chkWindowsTime.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.chkWindowsTime.Name = "chkWindowsTime";
            this.chkWindowsTime.Size = new System.Drawing.Size(100, 17);
            this.chkWindowsTime.TabIndex = 121;
            this.chkWindowsTime.Text = "times: Windows";
            this.toolTip1.SetToolTip(this.chkWindowsTime, "store extended times in Windows format");
            this.chkWindowsTime.UseVisualStyleBackColor = true;
            //
            // label16
            //
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(6, 185);
            this.label16.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(81, 13);
            this.label16.TabIndex = 100;
            this.label16.Text = "exe on unpack:";
            //
            // tbExeOnUnpack
            //
            this.tbExeOnUnpack.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbExeOnUnpack.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbExeOnUnpack.ForeColor = System.Drawing.SystemColors.InactiveCaption;
            this.tbExeOnUnpack.Location = new System.Drawing.Point(104, 181);
            this.tbExeOnUnpack.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.tbExeOnUnpack.Name = "tbExeOnUnpack";
            this.tbExeOnUnpack.Size = new System.Drawing.Size(432, 20);
            this.tbExeOnUnpack.TabIndex = 120;
            this.tbExeOnUnpack.Text = "-command line to execute here-";
            this.toolTip1.SetToolTip(this.tbExeOnUnpack, "command to run upon successful extract of SFX");
            this.tbExeOnUnpack.TextChanged += new System.EventHandler(this.tbExeOnUnpack_TextChanged);
            this.tbExeOnUnpack.Enter += new System.EventHandler(this.tbExeOnUnpack_Enter);
            this.tbExeOnUnpack.Leave += new System.EventHandler(this.tbExeOnUnpack_Leave);
            //
            // label15
            //
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(6, 161);
            this.label15.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(91, 13);
            this.label15.TabIndex = 98;
            this.label15.Text = "default extract dir:";
            //
            // tbDefaultExtractDirectory
            //
            this.tbDefaultExtractDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDefaultExtractDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbDefaultExtractDirectory.ForeColor = System.Drawing.SystemColors.InactiveCaption;
            this.tbDefaultExtractDirectory.Location = new System.Drawing.Point(104, 157);
            this.tbDefaultExtractDirectory.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.tbDefaultExtractDirectory.Name = "tbDefaultExtractDirectory";
            this.tbDefaultExtractDirectory.Size = new System.Drawing.Size(432, 20);
            this.tbDefaultExtractDirectory.TabIndex = 110;
            this.tbDefaultExtractDirectory.Tag = "b";
            this.tbDefaultExtractDirectory.Text = "-default extract directory-";
            this.toolTip1.SetToolTip(this.tbDefaultExtractDirectory, "optional default extraction directory for SFX");
            this.tbDefaultExtractDirectory.Enter += new System.EventHandler(this.tbDefaultExtractDirectory_Enter);
            this.tbDefaultExtractDirectory.Leave += new System.EventHandler(this.tbDefaultExtractDirectory_Leave);
            //
            // comboZip64
            //
            this.comboZip64.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboZip64.FormattingEnabled = true;
            this.comboZip64.Location = new System.Drawing.Point(104, 60);
            this.comboZip64.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.comboZip64.Name = "comboZip64";
            this.comboZip64.Size = new System.Drawing.Size(128, 21);
            this.comboZip64.TabIndex = 70;
            this.toolTip1.SetToolTip(this.comboZip64, "ZIP64 is not compatible with some other zip tools.");
            //
            // comboCompMethod
            //
            this.comboCompMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboCompMethod.FormattingEnabled = true;
            this.comboCompMethod.Location = new System.Drawing.Point(310, 35);
            this.comboCompMethod.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.comboCompMethod.Name = "comboCompMethod";
            this.comboCompMethod.Size = new System.Drawing.Size(122, 21);
            this.comboCompMethod.TabIndex = 71;
            this.comboCompMethod.SelectedIndexChanged += new System.EventHandler(this.comboCompMethod_SelectedIndexChanged);

            this.toolTip1.SetToolTip(this.comboCompMethod, "BZip2 is not compatible with some other zip tools.");
            //
            // comboFlavor
            //
            this.comboFlavor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboFlavor.FormattingEnabled = true;
            this.comboFlavor.Location = new System.Drawing.Point(104, 35);
            this.comboFlavor.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.comboFlavor.Name = "comboFlavor";
            this.comboFlavor.Size = new System.Drawing.Size(128, 21);
            this.comboFlavor.TabIndex = 40;
            this.comboFlavor.SelectedIndexChanged += new System.EventHandler(this.comboFlavor_SelectedIndexChanged);
            //
            // btnCreateZipBrowse
            //
            this.btnCreateZipBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreateZipBrowse.Location = new System.Drawing.Point(542, 10);
            this.btnCreateZipBrowse.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.btnCreateZipBrowse.Name = "btnCreateZipBrowse";
            this.btnCreateZipBrowse.Size = new System.Drawing.Size(24, 20);
            this.btnCreateZipBrowse.TabIndex = 31;
            this.btnCreateZipBrowse.Text = "...";
            this.toolTip1.SetToolTip(this.btnCreateZipBrowse, "browse for a file to save to");
            this.btnCreateZipBrowse.UseVisualStyleBackColor = true;
            this.btnCreateZipBrowse.Click += new System.EventHandler(this.btnCreateZipBrowse_Click);
            //
            // groupBox1
            //
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.chkRecurse);
            this.groupBox1.Controls.Add(this.chkTraverseJunctions);
            this.groupBox1.Controls.Add(this.tbDirectoryToZip);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.btnZipupDirBrowse);
            this.groupBox1.Controls.Add(this.tbDirectoryInArchive);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.tbSelectionToZip);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Location = new System.Drawing.Point(6, -2);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.groupBox1.Size = new System.Drawing.Size(572, 87);
            this.groupBox1.TabIndex = 103;
            this.groupBox1.TabStop = false;
            //
            // chkRecurse
            //
            this.chkRecurse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkRecurse.AutoSize = true;
            this.chkRecurse.Checked = true;
            this.chkRecurse.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRecurse.Location = new System.Drawing.Point(435, 15);
            this.chkRecurse.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.chkRecurse.Name = "chkRecurse";
            this.chkRecurse.Size = new System.Drawing.Size(61, 17);
            this.chkRecurse.TabIndex = 126;
            this.chkRecurse.Text = "recurse";
            this.toolTip1.SetToolTip(this.chkRecurse, "recurse directories when adding");
            this.chkRecurse.UseVisualStyleBackColor = true;
            //
            // chkTraverseJunctions
            //
            this.chkTraverseJunctions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkTraverseJunctions.AutoSize = true;
            this.chkTraverseJunctions.Checked = true;
            this.chkTraverseJunctions.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTraverseJunctions.Location = new System.Drawing.Point(505, 15);
            this.chkTraverseJunctions.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.chkTraverseJunctions.Name = "chkTraverseJunctions";
            this.chkTraverseJunctions.Size = new System.Drawing.Size(68, 18);
            this.chkTraverseJunctions.TabIndex = 125;
            this.chkTraverseJunctions.Text = "junctions";
            this.toolTip1.SetToolTip(this.chkTraverseJunctions, "traverse directory junctions and \r\nsymlinks when adding");
            this.chkTraverseJunctions.UseCompatibleTextRendering = true;
            this.chkTraverseJunctions.UseVisualStyleBackColor = true;
            //
            // tbDirectoryInArchive
            //
            this.tbDirectoryInArchive.AcceptsReturn = true;
            this.tbDirectoryInArchive.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDirectoryInArchive.Location = new System.Drawing.Point(104, 38);
            this.tbDirectoryInArchive.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.tbDirectoryInArchive.Name = "tbDirectoryInArchive";
            this.tbDirectoryInArchive.Size = new System.Drawing.Size(432, 20);
            this.tbDirectoryInArchive.TabIndex = 14;
            this.toolTip1.SetToolTip(this.tbDirectoryInArchive, "the directory to use within the archive.");
            //
            // button1
            //
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(542, 63);
            this.button1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(24, 20);
            this.button1.TabIndex = 21;
            this.button1.Text = "+";
            this.toolTip1.SetToolTip(this.button1, "Add Selected files to the list of files to Save in the Zip");
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            //
            // label14
            //
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(6, 42);
            this.label14.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(99, 13);
            this.label14.TabIndex = 16;
            this.label14.Text = "directory in archive:";
            //
            // tbSelectionToZip
            //
            this.tbSelectionToZip.AcceptsReturn = true;
            this.tbSelectionToZip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSelectionToZip.Location = new System.Drawing.Point(104, 63);
            this.tbSelectionToZip.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.tbSelectionToZip.Name = "tbSelectionToZip";
            this.tbSelectionToZip.Size = new System.Drawing.Size(432, 20);
            this.tbSelectionToZip.TabIndex = 20;
            this.tbSelectionToZip.Text = "*.*";
            this.toolTip1.SetToolTip(this.tbSelectionToZip, resources.GetString("tbSelectionToZip.ToolTip"));
            //
            // label12
            //
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 67);
            this.label12.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(52, 13);
            this.label12.TabIndex = 95;
            this.label12.Text = "selection:";
            //
            // checkBox1
            //
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(12, 322);
            this.checkBox1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(15, 14);
            this.checkBox1.TabIndex = 102;
            this.checkBox1.TabStop = false;
            this.toolTip1.SetToolTip(this.checkBox1, "select ALL");
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged_1);
            //
            // btnClearItemsToZip
            //
            this.btnClearItemsToZip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearItemsToZip.Location = new System.Drawing.Point(398, 460);
            this.btnClearItemsToZip.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.btnClearItemsToZip.Name = "btnClearItemsToZip";
            this.btnClearItemsToZip.Size = new System.Drawing.Size(102, 26);
            this.btnClearItemsToZip.TabIndex = 130;
            this.btnClearItemsToZip.Text = "Remove Checked";
            this.toolTip1.SetToolTip(this.btnClearItemsToZip, "remove any checked files from the list of files to save in the zip");
            this.btnClearItemsToZip.UseVisualStyleBackColor = true;
            this.btnClearItemsToZip.Click += new System.EventHandler(this.btnClearItemsToZip_Click);
            //
            // textBox1
            //
            this.textBox1.AcceptsReturn = true;
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox1.Location = new System.Drawing.Point(362, 463);
            this.textBox1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 100;
            this.toolTip1.SetToolTip(this.textBox1, "edit the value");
            this.textBox1.Visible = false;
            this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            //
            // listView2
            //
            this.listView2.AllowColumnReorder = true;
            this.listView2.AllowDrop = true;
            this.listView2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView2.CheckBoxes = true;
            this.listView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chCheckbox,
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listView2.DoubleClickActivation = false;
            this.listView2.FullRowSelect = true;
            this.listView2.GridLines = true;
            this.listView2.Location = new System.Drawing.Point(6, 316);
            this.listView2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.listView2.MultiSelect = false;
            this.listView2.Name = "listView2";
            this.listView2.Size = new System.Drawing.Size(572, 142);
            this.listView2.TabIndex = 98;
            this.listView2.UseCompatibleStateImageBehavior = false;
            this.listView2.View = System.Windows.Forms.View.Details;
            this.listView2.BeforeLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.listView2_BeforeLabelEdit);
            this.listView2.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listView2_ItemChecked);
            this.listView2.DragDrop += new System.Windows.Forms.DragEventHandler(this.listView2_DragDrop);
            this.listView2.DragEnter += new System.Windows.Forms.DragEventHandler(this.listView_DragEnter);
            //
            // chCheckbox
            //
            this.chCheckbox.Text = "?";
            this.chCheckbox.Width = 24;
            //
            // columnHeader1
            //
            this.columnHeader1.Text = "File Name";
            //
            // columnHeader2
            //
            this.columnHeader2.Text = "Directory In Archive";
            //
            // columnHeader3
            //
            this.columnHeader3.Text = "File name in Archive";
            //
            // tabPage3
            //
            this.tabPage3.Controls.Add(this.richTextBox1);
            this.tabPage3.Controls.Add(this.pictureBox1);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.tabPage3.Size = new System.Drawing.Size(584, 522);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "About";
            this.tabPage3.UseVisualStyleBackColor = true;
            //
            // richTextBox1
            //
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox1.Location = new System.Drawing.Point(54, 20);
            this.richTextBox1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(518, 497);
            this.richTextBox1.TabIndex = 2;
            this.richTextBox1.Text = "Placeholder only. ";
            this.richTextBox1.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.richTextBox1_LinkClicked);
            //
            // pictureBox1
            //
            this.pictureBox1.Image = global::Ionic.Zip.Forms.Properties.Resources.zippedFile;
            this.pictureBox1.Location = new System.Drawing.Point(6, 20);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(42, 52);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            //
            // ZipForm
            //
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(592, 548);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.MinimumSize = new System.Drawing.Size(598, 458);
            this.Name = "ZipForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "DotNetZip Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ZipForm_FormClosing);
            this.Load += new System.EventHandler(this.ZipForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Button btnZipupDirBrowse;
        private System.Windows.Forms.Button btnZipUp;
        private System.Windows.Forms.Button btnOpenZip;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.ProgressBar progressBar2;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.TextBox tbDirectoryToZip;
        private System.Windows.Forms.TextBox tbZipToCreate;
        private System.Windows.Forms.TextBox tbComment;
        private System.Windows.Forms.ComboBox comboEncoding;
        private System.Windows.Forms.ComboBox comboCompLevel;
        private System.Windows.Forms.ComboBox comboEncryption;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.CheckBox chkHidePassword;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button btnReadZipBrowse;
        private System.Windows.Forms.TextBox tbZipToOpen;
        private System.Windows.Forms.Button btnExtract;
        private System.Windows.Forms.Button btnCreateZipBrowse;
        private System.Windows.Forms.Button btnExtractDirBrowse;
        private System.Windows.Forms.TextBox tbExtractDir;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.CheckBox chkOpenExplorer;
        private System.Windows.Forms.TextBox tbSelectionToZip;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TextBox tbSelectionToExtract;
        private System.Windows.Forms.TextBox tbDirectoryInArchive;
        //private System.Windows.Forms.ListViewEx listView2;
        //this.listView2 = new System.Windows.Forms.ListView();
        private ListViewEx.ListViewEx listView2;
        private System.Windows.Forms.ComboBox comboZip64;
        private System.Windows.Forms.ComboBox comboCompMethod;
        private System.Windows.Forms.ComboBox comboFlavor;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnClearItemsToZip;
        private System.Windows.Forms.ColumnHeader chCheckbox;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox tbDefaultExtractDirectory;
        private System.Windows.Forms.TextBox tbExeOnUnpack;
        private System.Windows.Forms.CheckBox chkUnixTime;
        private System.Windows.Forms.CheckBox chkWindowsTime;
        private System.Windows.Forms.ComboBox comboSplit;
        private System.Windows.Forms.CheckBox chkTraverseJunctions;
        private System.Windows.Forms.CheckBox chkRecurse;
        private System.Windows.Forms.CheckBox chkRemoveFiles;
        private System.Windows.Forms.ComboBox comboExistingFileAction;
        private System.Windows.Forms.ComboBox comboEncodingUsage;
    }
}

