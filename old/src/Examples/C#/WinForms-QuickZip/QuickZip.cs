// QuickZip.cs
// ------------------------------------------------------------------
//
// A simple app that creates a zip file, zipping up a files and
// directories specified on the command line.  This application needs to
// be run from the console, in order to specify arguments, but it is
// also a winforms app.
//
// It correctly does the multi-threading to allow smooth UI update.
//
// compile it with:
//      c:\.net3.5\csc.exe /t:exe /debug:full /optimize- /R:System.dll /R:Ionic.Zip.dll
//                                /out:QuickZip.exe QuickZip.cs
//
// last saved:
// Time-stamp: <2010-March-16 14:11:42>
// ------------------------------------------------------------------
//
// Copyright (c) 2010 by Dino Chiesa
// All rights reserved!
//
// Licensed under the Microsoft Public License.
// see http://www.opensource.org/licenses/ms-pl.html
//
// ------------------------------------------------------------------

using System;
//using System.Reflection;
using System.Windows.Forms;                    // Form, Label, ProgressBar
using Ionic.Zip;                               // ZipFile, ZipEntry
using System.ComponentModel;                   // BackgroundWorker
using Interop=System.Runtime.InteropServices;  // DllImport

namespace Ionic.Zip.Examples.CS
{
    public class QuickZip
    {
        [Interop.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [Interop.DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int pid);

        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                // open a new window so we can write to it.
                QuickZip.AllocConsole();
                QuickZip.Usage(args);
                return;
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var f = new QuickZipForm(args[0], args[1]);
                Application.Run(f);
            }
            return;
        }

        private static int Usage(string[] args)
        {
            Console.WriteLine("QuickUnzip.  Usage:  QuickZip <zipfile> <selection criteria>");
            Console.WriteLine("\n<ENTER> to continue...");
            Console.ReadLine();
            return 1;
        }

    }



    public class QuickZipForm : Form
    {

        private QuickZipForm()
        {
            this.components = null;
            this.InitializeComponent();
        }

        public QuickZipForm(string zipfile, string selectionCriteria) : this()
        {
            this.zipfileName = zipfile;
            this.selectionCriteria = selectionCriteria;
        }

        protected override void Dispose(Boolean disposing)
        {
            if (disposing && (this.components != null))
                this.components.Dispose();

            base.Dispose(disposing);
        }

        private void FixTitle()
        {
            this.Text = String.Format("Quick Zip {0}", this.zipfileName);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new Label();
            this.progressBar1 = new ProgressBar();
            base.SuspendLayout();
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Zipping...";
            this.progressBar1.Anchor = (AnchorStyles.Right | (AnchorStyles.Left | AnchorStyles.Top));
            this.progressBar1.Location = new System.Drawing.Point(12, 36);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(436, 18);
            this.progressBar1.Step = 1;
            this.progressBar1.TabIndex = 7;
            base.AutoScaleDimensions = new System.Drawing.SizeF(6, 13);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new System.Drawing.Size(460, 80);
            base.Controls.Add(this.label1);
            base.Controls.Add(this.progressBar1);
            base.Name = "QuickUnzipForm";
            this.Text = "QuickUnzip";
            this.Load += this.QuickZipForm_Load;
            this.Shown += this.QuickZipForm_Shown;
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        public void OnTimerEvent(Object source,  EventArgs e)
        {
            base.Close();
        }

        private void QuickZipForm_Load(Object sender, EventArgs e)
        {
            this.FixTitle();
        }

        private void QuickZipForm_Shown(Object sender, EventArgs e)
        {
            // For info on running long-running tasks in response to button clicks,
            // in  VB.NET WinForms, see
            // http://msdn.microsoft.com/en-us/library/ms951089.aspx

            var backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            backgroundWorker1.WorkerSupportsCancellation = false;
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.DoWork += this.Zipit;
            backgroundWorker1.RunWorkerAsync();
        }


        public void SaveProgress(object sender, SaveProgressEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<Object, SaveProgressEventArgs>(SaveProgress),  new Object[] { sender, e });
            }
            else
            {
                switch (e.EventType)
                {
                    case ZipProgressEventType.Saving_Started:
                        //Console.WriteLine("pb max {0}", e.EntriesTotal);
                        this.progressBar1.Maximum = e.EntriesTotal;
                        this.progressBar1.Value = 0;
                        this.progressBar1.Minimum = 0;
                        this.progressBar1.Step = 1;
                        break;

                    case ZipProgressEventType.Saving_BeforeWriteEntry:
                        //Console.WriteLine("entry {0}", e.CurrentEntry.FileName);
                        this.label1.Text = e.CurrentEntry.FileName;
                        break;

                    case ZipProgressEventType.Saving_AfterWriteEntry:
                        this.progressBar1.PerformStep();
                        break;
                }
                this.Update();
                Application.DoEvents();
            }
        }


        private void Zipit(object sender, DoWorkEventArgs e)
        {
            int delay = 1200; // ms to keep form open after completion
            try
            {
                using (var zip = new ZipFile())
                {
                    zip.AddSelectedFiles(selectionCriteria, ".", "", true);
                    zip.SaveProgress += this.SaveProgress;
                    zip.Save(zipfileName);
                }
            }
            catch (Exception ex1)
            {
                this.label1.Text = "Exception: " +  ex1.ToString();
                delay = 4000;
            }

            var timer1 =  new System.Timers.Timer(delay);
            timer1.Enabled = true;
            timer1.AutoReset = false;
            timer1.Elapsed += this.OnTimerEvent;
        }


        // Fields
        private String selectionCriteria;
        private String zipfileName;
        private System.ComponentModel.IContainer components ;
        private Label label1;
        private ProgressBar  progressBar1;
    }

}

