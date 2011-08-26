namespace SmartDeviceProject3
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MainMenu mainMenu1;

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
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.menuItemUnzip = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuItemDelete = new System.Windows.Forms.MenuItem();
            this.menuItemAbout = new System.Windows.Forms.MenuItem();
            this.menuItemQuit = new System.Windows.Forms.MenuItem();
            this.tvFolders = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.menuItemUnzip);
            this.mainMenu1.MenuItems.Add(this.menuItem2);
            // 
            // menuItemUnzip
            // 
            this.menuItemUnzip.Text = "Unzip";
            this.menuItemUnzip.Click += new System.EventHandler(this.menuItemUnzip_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.MenuItems.Add(this.menuItemDelete);
            this.menuItem2.MenuItems.Add(this.menuItemAbout);
            this.menuItem2.MenuItems.Add(this.menuItemQuit);
            this.menuItem2.Text = "Menu";
            // 
            // menuItemDelete
            // 
            this.menuItemDelete.Text = "Delete";
            this.menuItemDelete.Click += new System.EventHandler(this.menuItemDelete_Click);
            // 
            // menuItemAbout
            // 
            this.menuItemAbout.Text = "About";
            this.menuItemAbout.Click += new System.EventHandler(this.menuItemAbout_Click);
            // 
            // menuItemQuit
            // 
            this.menuItemQuit.Text = "Quit";
            this.menuItemQuit.Click += new System.EventHandler(this.menuItemQuit_Click);
            // 
            // tvFolders
            // 
            this.tvFolders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvFolders.Location = new System.Drawing.Point(0, 0);
            this.tvFolders.Name = "tvFolders";
            this.tvFolders.Size = new System.Drawing.Size(320, 186);
            this.tvFolders.TabIndex = 0;
            this.tvFolders.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvFolders_AfterSelect);
            // 
            // imageList1
            // 
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(131F, 131F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(320, 186);
            this.Controls.Add(this.tvFolders);
            this.Font = new System.Drawing.Font("Segoe Condensed", 9F, System.Drawing.FontStyle.Bold);
            this.Menu = this.mainMenu1;
            this.Name = "Form1";
            this.Text = "Unzipper - browse and unzip";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView tvFolders;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.MenuItem menuItemUnzip;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem menuItemDelete;
        private System.Windows.Forms.MenuItem menuItemAbout;
        private System.Windows.Forms.MenuItem menuItemQuit;
    }
}

