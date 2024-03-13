namespace Ionic.Zip
{
    partial class WinFormsSelfExtractorStub
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WinFormsSelfExtractorStub));
            this.btnExtract = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.txtExtractDirectory = new System.Windows.Forms.TextBox();
            this.lblExtractDir = new System.Windows.Forms.Label();
            this.btnDirBrowse = new System.Windows.Forms.Button();
            this.chk_OpenExplorer = new System.Windows.Forms.CheckBox();
            this.chk_ExeAfterUnpack = new System.Windows.Forms.CheckBox();
            this.txtPostUnpackCmdLine = new System.Windows.Forms.TextBox();
            this.chk_Remove = new System.Windows.Forms.CheckBox();
            this.lblComment = new System.Windows.Forms.Label();
            this.txtComment = new System.Windows.Forms.TextBox();
            this.btnContents = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.comboExistingFileAction = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // btnExtract
            //
            this.btnExtract.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExtract.Location = new System.Drawing.Point(331, 268);
            this.btnExtract.Name = "btnExtract";
            this.btnExtract.Size = new System.Drawing.Size(60, 23);
            this.btnExtract.TabIndex = 0;
            this.btnExtract.Text = "Extract";
            this.btnExtract.UseVisualStyleBackColor = true;
            this.btnExtract.Click += new System.EventHandler(this.btnExtract_Click);
            //
            // btnCancel
            //
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(397, 268);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(60, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            // txtExtractDirectory
            //
            this.txtExtractDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtExtractDirectory.Location = new System.Drawing.Point(8, 125);
            this.txtExtractDirectory.Name = "txtExtractDirectory";
            this.txtExtractDirectory.Size = new System.Drawing.Size(417, 20);
            this.txtExtractDirectory.TabIndex = 2;
            //
            // lblExtractDir
            //
            this.lblExtractDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblExtractDir.AutoSize = true;
            this.lblExtractDir.Location = new System.Drawing.Point(5, 109);
            this.lblExtractDir.Name = "lblExtractDir";
            this.lblExtractDir.Size = new System.Drawing.Size(100, 13);
            this.lblExtractDir.TabIndex = 3;
            this.lblExtractDir.Text = "Extract to Directory:";
            //
            // btnDirBrowse
            //
            this.btnDirBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDirBrowse.Location = new System.Drawing.Point(431, 122);
            this.btnDirBrowse.Name = "btnDirBrowse";
            this.btnDirBrowse.Size = new System.Drawing.Size(25, 23);
            this.btnDirBrowse.TabIndex = 4;
            this.btnDirBrowse.Text = "...";
            this.btnDirBrowse.UseVisualStyleBackColor = true;
            this.btnDirBrowse.Click += new System.EventHandler(this.btnDirBrowse_Click);
            //
            // chk_OpenExplorer
            //
            this.chk_OpenExplorer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chk_OpenExplorer.AutoSize = true;
            this.chk_OpenExplorer.Checked = true;
            this.chk_OpenExplorer.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_OpenExplorer.Location = new System.Drawing.Point(8, 151);
            this.chk_OpenExplorer.Name = "chk_OpenExplorer";
            this.chk_OpenExplorer.Size = new System.Drawing.Size(152, 17);
            this.chk_OpenExplorer.TabIndex = 13;
            this.chk_OpenExplorer.Text = "Open Explorer after extract";
            this.chk_OpenExplorer.UseVisualStyleBackColor = true;
            //
            // chk_ExeAfterUnpack
            //
            this.chk_ExeAfterUnpack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chk_ExeAfterUnpack.AutoSize = true;
            this.chk_ExeAfterUnpack.Checked = true;
            this.chk_ExeAfterUnpack.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_ExeAfterUnpack.Location = new System.Drawing.Point(8, 192);
            this.chk_ExeAfterUnpack.Name = "chk_ExeAfterUnpack";
            this.chk_ExeAfterUnpack.Size = new System.Drawing.Size(131, 17);
            this.chk_ExeAfterUnpack.TabIndex = 15;
            this.chk_ExeAfterUnpack.Text = "Execute after unpack:";
            this.chk_ExeAfterUnpack.UseVisualStyleBackColor = true;
            this.chk_ExeAfterUnpack.CheckedChanged += new System.EventHandler(this.chk_ExeAfterUnpack_CheckedChanged);
            //
            // txtPostUnpackCmdLine
            //
            this.txtPostUnpackCmdLine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPostUnpackCmdLine.Location = new System.Drawing.Point(145, 190);
            this.txtPostUnpackCmdLine.Name = "txtPostUnpackCmdLine";
            this.txtPostUnpackCmdLine.ReadOnly = true;
            this.txtPostUnpackCmdLine.Size = new System.Drawing.Size(312, 20);
            this.txtPostUnpackCmdLine.TabIndex = 16;
            //
            // chk_Remove
            //
            this.chk_Remove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chk_Remove.AutoSize = true;
            this.chk_Remove.Location = new System.Drawing.Point(8, 212);
            this.chk_Remove.Name = "chk_Remove";
            this.chk_Remove.Size = new System.Drawing.Size(271, 17);
            this.chk_Remove.TabIndex = 17;
            this.chk_Remove.Text = "Remove files after executing post-unpack command";
            this.chk_Remove.UseVisualStyleBackColor = true;
            //
            // lblComment
            //
            this.lblComment.AutoSize = true;
            this.lblComment.Location = new System.Drawing.Point(5, 6);
            this.lblComment.Name = "lblComment";
            this.lblComment.Size = new System.Drawing.Size(75, 13);
            this.lblComment.TabIndex = 8;
            this.lblComment.Text = "Zip Comment: ";
            //
            // txtComment
            //
            this.txtComment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtComment.Location = new System.Drawing.Point(8, 22);
            this.txtComment.Multiline = true;
            this.txtComment.Name = "txtComment";
            this.txtComment.ReadOnly = true;
            this.txtComment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtComment.Size = new System.Drawing.Size(448, 82);
            this.txtComment.TabIndex = 9;
            //
            // btnContents
            //
            this.btnContents.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnContents.Location = new System.Drawing.Point(235, 268);
            this.btnContents.Name = "btnContents";
            this.btnContents.Size = new System.Drawing.Size(90, 23);
            this.btnContents.TabIndex = 20;
            this.btnContents.Text = "Show Contents";
            this.btnContents.UseVisualStyleBackColor = true;
            this.btnContents.Click += new System.EventHandler(this.btnContents_Click);
            //
            // progressBar1
            //
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(8, 237);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(449, 10);
            this.progressBar1.TabIndex = 11;
            this.progressBar1.TabStop = false;
            //
            // progressBar2
            //
            this.progressBar2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar2.Location = new System.Drawing.Point(8, 252);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(449, 11);
            this.progressBar2.TabIndex = 12;
            this.progressBar2.TabStop = false;
            //
            // lblStatus
            //
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(9, 273);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 13);
            this.lblStatus.TabIndex = 13;
            //
            // comboExistingFileAction
            //
            this.comboExistingFileAction.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboExistingFileAction.FormattingEnabled = true;
            this.comboExistingFileAction.Location = new System.Drawing.Point(116, 167);
            this.comboExistingFileAction.Name = "comboExistingFileAction";
            this.comboExistingFileAction.Size = new System.Drawing.Size(104, 21);
            this.comboExistingFileAction.TabIndex = 21;
            //
            // label1
            //
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 172);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 13);
            this.label1.TabIndex = 22;
            this.label1.Text = "Existing File Action:";
            //
            // WinFormsSelfExtractorStub
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(468, 300);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboExistingFileAction);
            this.Controls.Add(this.chk_Remove);
            this.Controls.Add(this.chk_ExeAfterUnpack);
            this.Controls.Add(this.txtPostUnpackCmdLine);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.progressBar2);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.btnContents);
            this.Controls.Add(this.txtComment);
            this.Controls.Add(this.lblComment);
            this.Controls.Add(this.chk_OpenExplorer);
            this.Controls.Add(this.btnDirBrowse);
            this.Controls.Add(this.lblExtractDir);
            this.Controls.Add(this.txtExtractDirectory);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnExtract);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1024, 400);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(484, 336);
            this.Name = "WinFormsSelfExtractorStub";
            this.Text = "This text will be replaced at runtime";
            this.Shown += new System.EventHandler(this.Form_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnExtract;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnDirBrowse;
        private System.Windows.Forms.Button btnContents;
        private System.Windows.Forms.TextBox txtExtractDirectory;
        private System.Windows.Forms.Label lblExtractDir;
        private System.Windows.Forms.CheckBox chk_OpenExplorer;
        private System.Windows.Forms.Label lblComment;
        private System.Windows.Forms.TextBox txtComment;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.ProgressBar progressBar2;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.TextBox txtPostUnpackCmdLine;
        private System.Windows.Forms.CheckBox chk_ExeAfterUnpack;
        private System.Windows.Forms.CheckBox chk_Remove;
        private System.Windows.Forms.ComboBox comboExistingFileAction;
        private System.Windows.Forms.Label label1;
    }
}