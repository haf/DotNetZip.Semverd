// PasswordDialog.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009 Dino Chiesa
// All rights reserved.
//
// This code module is part of DotNetZip, a zipfile class library.
//
// ------------------------------------------------------------------
//
// This code is licensed under the Microsoft Public License. 
// See the file License.txt for the license details.
// More info on: http://dotnetzip.codeplex.com
//
// ------------------------------------------------------------------
//

namespace Ionic.Zip.Forms
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    public partial class PasswordDialog : Form
    {
        public enum PasswordDialogResult { OK, Skip, Cancel };
        
        public PasswordDialog()
        {
            InitializeComponent();
            this.textBox1.Focus();
        }

        public PasswordDialogResult Result
        {
            get
            {
                return _result;
            }
        }
        
        public string EntryName
        {
            set
            {
                prompt.Text = "Enter the password for " + value;
            }
        }
        public string Password
        {
            get
            {
                return textBox1.Text;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            _result = PasswordDialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _result = PasswordDialogResult.Cancel;
            this.Close();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            _result = PasswordDialogResult.Skip;
            this.Close();
        }


        private PasswordDialogResult _result;


    }
}
