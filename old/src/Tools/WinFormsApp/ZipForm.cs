// ZipForm.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009-2011 Dino Chiesa
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

using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Ionic.Zip;

namespace Ionic.Zip.Forms
{
    public partial class ZipForm : Form
    {
        delegate void ZipProgress(ZipProgressEventArgs e);
        delegate void ButtonClick(object sender, EventArgs e);
        HiResTimer _hrt;

        public ZipForm()
        {
            InitializeComponent();

            InitializeListboxes();
            FixTitle();
            FillFormFromRegistry();
            AdoptProgressBars();
            SetListView2();
            SetTextBoxes();

            // first run only
            if (numRuns == 0)
                this.tabControl1.SelectedIndex = 2; // help/about tab
        }

        // This constructor works to load zips from the command line.
        // It also works to allow "open With..." from Windows Explorer.
        public ZipForm(string[] args)
            : this()
        {
            if (args != null && args.Length >= 1 && args[0] != null)
                _initialFileToLoad = args[0];
        }

        // in ZipForm.DragDrop.cs
        partial void SetDragDrop();

        private void SetTextBoxes()
        {
            this.tbComment.Text= TB_COMMENT_NOTE;
            this.tbDefaultExtractDirectory.Text= TB_EXTRACT_DIR_NOTE;
            this.tbExeOnUnpack.Text= TB_EXE_ON_UNPACK_NOTE;

            // set autocomplete on a few textboxes
            this.tbDirectoryToZip.AutoCompleteMode = AutoCompleteMode.Suggest;
            this.tbDirectoryToZip.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;

            this.tbExtractDir.AutoCompleteMode = AutoCompleteMode.Suggest;
            this.tbExtractDir.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;

            this.tbZipToOpen.AutoCompleteMode = AutoCompleteMode.Suggest;
            this.tbZipToOpen.AutoCompleteSource = AutoCompleteSource.FileSystem;

            this.tbZipToCreate.AutoCompleteMode = AutoCompleteMode.Suggest;
            this.tbZipToCreate.AutoCompleteSource = AutoCompleteSource.FileSystem;

            this.tbSelectionToExtract.AutoCompleteMode = AutoCompleteMode.Suggest;
            this.tbSelectionToExtract.AutoCompleteSource = AutoCompleteSource.CustomSource;
            this.tbSelectionToExtract.AutoCompleteCustomSource = _selectionCompletions;

            this.tbSelectionToZip.AutoCompleteMode = AutoCompleteMode.Suggest;
            this.tbSelectionToZip.AutoCompleteSource = AutoCompleteSource.CustomSource;
            this.tbSelectionToZip.AutoCompleteCustomSource = _selectionCompletions;

            tbExeOnUnpack_TextChanged(null, null);
        }

        private void SetListView2()
        {
            // The listview2 is a ListViewEx control, an extension of
            // System.Windows.Forms.ListView that allows editing of subitems using arbitrary
            // controls.  You can have a textbox, a datepicker, or other controls.
            // I want the user to be able to modify the directory-in-archive value in the
            // list, which is why ListViewEx is interesting.

            // I also want a checkbox associated to each list item, but I don't want the
            // checkbox attached to the first subitem, as it normally is in a ListView. So I
            // put an empty string as the main ListView item (subitem #0), and then the
            // filename and directory-in-archive value in the 2nd and 3rd columns (subitems 1
            // and 2).  This way, the checkbox gets its own column.

            // Next twist is I want the checkbox label to be uneditable, which is easy
            // by just installing a  listView2_BeforeLabelEdit method and always cancelling
            // it. (e.CancelEdit = true).

            // And then there is a "master checkbox" at the column header that indicates
            // whether the state of all checkboxes in the list is the same. With the "check
            // change" of any item in the list I want to see if the master should be checked
            // or unchecked.

            // The final thing is I want the checkbox for each item to change state only if I
            // click on the checkbox itself, rather than "anywhere in the item row".

            this.listView2.SubItemClicked += new ListViewEx.SubItemEventHandler(listView2_SubItemClicked);
            this.listView2.SubItemEndEditing += new ListViewEx.SubItemEndEditingEventHandler(listView2_SubItemEndEditing);
            this.listView2.DoubleClickActivation = true;
            SetDragDrop();
        }



        private void AdoptProgressBars()
        {
            tabControl1_SelectedIndexChanged(null, null);
        }

        private void FixTitle()
        {
            this.Text = String.Format("DotNetZip Tool v{0}",
                                      System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }

        private void InitializeListboxes()
        {
            InitEncodingsList();
            InitFlavorList();
            InitZip64List();
            InitCompMethodList();
            InitCompressionLevelList();
            InitEncryptionList();
            InitSplitBox();
            InitExtractExistingFileList();
        }

        private void InitSplitBox()
        {
            string[] values = { "-none-", "64kb", "128kb", "256kb", "512kb", "1mb", "2mb", "4mb", "8mb", "16mb", "32mb", "64mb", "128mb", "256mb", "512mb", "1gb" };
            foreach (var value in values)
                this.comboSplit.Items.Add(value);
            this.comboSplit.SelectedIndex = 0;

            this.comboSplit.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            //this.comboSplit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        }

        private void InitEncryptionList()
        {
            List<String> _EncryptionNames = new List<string>(Enum.GetNames(typeof(Ionic.Zip.EncryptionAlgorithm)));
            foreach (var name in _EncryptionNames)
            {
                if (name != "Unsupported")
                    comboEncryption.Items.Add(name);
            }

            // select the first item:
            comboEncryption.SelectedIndex = 0;
            this.tbPassword.Text = "";
        }


        private void InitExtractExistingFileList()
        {
            List<String> _ExtractActionNames = new List<string>(Enum.GetNames(typeof(Ionic.Zip.ExtractExistingFileAction)));
            foreach (var name in _ExtractActionNames)
            {
                if (!name.StartsWith("Invoke"))
                {
                    if (name.StartsWith("Throw"))
                        comboExistingFileAction.Items.Add("Stop");
                    else
                        comboExistingFileAction.Items.Add(name);
                }
            }

            // select the first item:
            comboExistingFileAction.SelectedIndex = 0;
        }


        private void InitEncodingsList()
        {
            List<String> _EncodingNames = new List<string>();
            var e = System.Text.Encoding.GetEncodings();
            foreach (var e1 in e)
            {
                if (!_EncodingNames.Contains(e1.Name))
                    if (!_EncodingNames.Contains(e1.Name.ToUpper()))
                        if (!_EncodingNames.Contains(e1.Name.ToLower()))
                            if (e1.Name != "IBM437" && e1.Name != "utf-8")
                                _EncodingNames.Add(e1.Name);
            }
            _EncodingNames.Sort();
            comboEncoding.Items.Add("zip default (IBM437)");
            comboEncoding.Items.Add("utf-8");
            foreach (var name in _EncodingNames)
            {
                comboEncoding.Items.Add(name);
            }

            // select the first item:
            comboEncoding.SelectedIndex = 0;

            // also set the encoding usage
            List<String> _UsageNames = new List<string>(Enum.GetNames(typeof(Ionic.Zip.ZipOption)));
            _UsageNames.Sort();
            foreach (var name in _UsageNames)
            {
                if (!name.StartsWith("Default"))
                    comboEncodingUsage.Items.Add(name);
            }

            // select the first item:
            comboEncodingUsage.SelectedIndex = 0;
        }


        private void InitCompressionLevelList()
        {
            List<String> _CompressionLevelNames = new List<string>(Enum.GetNames(typeof(Ionic.Zlib.CompressionLevel)));
            _CompressionLevelNames.Sort();
            foreach (var name in _CompressionLevelNames)
            {
                if (!name.StartsWith("Level"))
                {
                    comboCompLevel.Items.Add(name);
                }
            }

            // select the 2nd item, "Default":
            comboCompLevel.SelectedIndex = 2;
        }

        private void InitFlavorList()
        {
            this.comboFlavor.Items.Add("traditional Zip");
            this.comboFlavor.Items.Add("Self-extractor (GUI)");
            this.comboFlavor.Items.Add("Self-extractor (CMD)");
            // select the first item:
            comboFlavor.SelectedIndex = 0;
        }

        private void InitZip64List()
        {
            var _Names = new List<string>(Enum.GetNames(typeof(Ionic.Zip.Zip64Option)));
            _Names.Sort();
            foreach (var name in _Names)
            {
                if (!name.StartsWith("Default"))
                    comboZip64.Items.Add(name);
            }

            // select the first item:
            comboZip64.SelectedIndex = 0;
        }


        private void InitCompMethodList()
        {
            var _Names = new List<string>(Enum.GetNames(typeof(Ionic.Zip.CompressionMethod)));
            _Names.Sort();
            foreach (var name in _Names)
            {
                if (!name.StartsWith("Default"))
                    comboCompMethod.Items.Add(name);
            }

            // select DEFLATE:
            comboCompMethod.SelectedIndex = 1;
        }



        private void KickoffZipup()
        {
            if (String.IsNullOrEmpty(this.tbDirectoryToZip.Text)) return;
            if (!System.IO.Directory.Exists(this.tbDirectoryToZip.Text))
            {
                var dlgResult = MessageBox.Show(String.Format("The directory you have specified ({0}) does not exist.", this.tbZipToCreate.Text),
                                                "Not gonna happen", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (this.tbZipToCreate.Text == null || this.tbZipToCreate.Text == "") return;

            // check for existence of the zip file:
            if (System.IO.File.Exists(this.tbZipToCreate.Text))
            {
                var dlgResult = MessageBox.Show(String.Format("The file you have specified ({0}) already exists.  Do you want to overwrite this file?", this.tbZipToCreate.Text), "Confirmation is Required", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dlgResult != DialogResult.Yes) return;
                System.IO.File.Delete(this.tbZipToCreate.Text);
            }


            // check for a valid zip file name:
            string extension = System.IO.Path.GetExtension(this.tbZipToCreate.Text);
            if ((extension != ".exe" && (this.comboFlavor.SelectedIndex == 1 || this.comboFlavor.SelectedIndex == 2)) ||
                (extension != ".zip" && this.comboFlavor.SelectedIndex == 0))
            {
                var dlgResult = MessageBox.Show(String.Format("The file you have specified ({0}) has a non-standard extension ({1}) for this zip flavor.  Do you want to continue anyway?",
                                                              this.tbZipToCreate.Text, extension), "Hold on there, pardner!", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dlgResult != DialogResult.Yes) return;
                System.IO.File.Delete(this.tbZipToCreate.Text);
            }

            _hrt = new HiResTimer();
            _hrt.Start();

            _working = true;
            _operationCanceled = false;
            _nFilesCompleted = 0;
            _totalBytesAfterCompress = 0;
            _totalBytesBeforeCompress = 0;
            PauseUI("Zipping...");
            lblStatus.Text = "Zipping...";

            var options = new SaveWorkerOptions
                {
                    ZipName = this.tbZipToCreate.Text,
                    Selection = this.tbSelectionToZip.Text,
                    TraverseJunctions = this.chkTraverseJunctions.Checked,
                    Encoding = "ibm437",
                    //EncodingUsage = ZipOption.Always,
                    ZipFlavor = this.comboFlavor.SelectedIndex,
                    Password = this.tbPassword.Text,
                    WindowsTimes = this.chkWindowsTime.Checked,
                    UnixTimes = this.chkUnixTime.Checked,
                    RemoveFilesAfterExe = this.chkRemoveFiles.Checked,
                    ExtractExistingFile = this.comboExistingFileAction.SelectedIndex,
                    };

            if (this.comboEncoding.SelectedIndex != 0)
                options.Encoding = this.comboEncoding.SelectedItem.ToString();

            options.Encryption = (EncryptionAlgorithm) Enum.Parse(typeof(EncryptionAlgorithm),
                                                                 this.comboEncryption.SelectedItem.ToString());

            options.CompressionLevel = (Ionic.Zlib.CompressionLevel) Enum.Parse(typeof(Ionic.Zlib.CompressionLevel),
                                                                               this.comboCompLevel.SelectedItem.ToString());
            options.CompressionMethod = (Ionic.Zip.CompressionMethod) Enum.Parse(typeof(Ionic.Zip.CompressionMethod),
                                                                               this.comboCompMethod.SelectedItem.ToString());

            options.EncodingUsage = (Ionic.Zip.ZipOption) Enum.Parse(typeof(Ionic.Zip.ZipOption),
                                                                     this.comboEncodingUsage.SelectedItem.ToString());

            string arg = this.comboSplit.SelectedItem.ToString().ToUpper();

            try
            {
                int multiplier = 1;
                int suffixLength = 0;
                if (arg.EndsWith("KB"))
                {
                    multiplier = 1024; suffixLength = 2;
                }
                else if (arg.EndsWith("K"))
                {
                    multiplier = 1024; suffixLength = 1;
                }
                else if (arg.EndsWith("MB"))
                {
                    multiplier = 1024*1024; suffixLength = 2;
                }
                else if (arg.EndsWith("M"))
                {
                    multiplier = 1024*1024; suffixLength = 1;
                }
                else if (arg.EndsWith("GB"))
                {
                    multiplier = 1024*1024*1024; suffixLength = 2;
                }
                else if (arg.EndsWith("G"))
                {
                    multiplier = 1024*1024*1024; suffixLength = 1;
                }

                if (suffixLength > 0)
                {
                    options.MaxSegmentSize =
                        Int32.Parse(arg.Substring(0,arg.Length-suffixLength)) * multiplier;
                }
                else
                    options.MaxSegmentSize = Int32.Parse(arg);
            }
            catch
            {
                // just reset to "none"
                this.comboSplit.SelectedIndex = 0;
                options.MaxSegmentSize = 0;
            }

            options.Zip64 = (Zip64Option)Enum.Parse(typeof(Zip64Option),
                                                    this.comboZip64.SelectedItem.ToString());

            //this.listView2.Items.ToList();
            var entriesList = new System.Collections.ArrayList(this.listView2.Items);
            options.Entries = System.Array.ConvertAll((ListViewItem[])entriesList.ToArray(typeof(ListViewItem)), (item) =>
                {
                    return new ItemToAdd
                        {
                            LocalFileName = item.SubItems[1].Text,
                            DirectoryInArchive = item.SubItems[2].Text,
                            FileNameInArchive = item.SubItems[3].Text,
                        };
                }
                );

            string compress = (options.CompressionMethod == Ionic.Zip.CompressionMethod.Deflate)
                ? "Deflate level" + options.CompressionLevel.ToString()
                : options.CompressionMethod.ToString();

            options.Comment = String.Format("Encoding:{0} || Compression:{1} || Encrypt:{2} || ZIP64:{3}\r\nCreated at {4} || {5}\r\n",
                                            options.Encoding,
                                            compress,
                                            (this.tbPassword.Text == "") ? "None" : options.Encryption.ToString(),
                                            options.Zip64.ToString(),
                                            System.DateTime.Now.ToString("yyyy-MMM-dd HH:mm:ss"),
                                            this.Text);

            if (this.tbComment.Text != TB_COMMENT_NOTE)
                options.Comment += this.tbComment.Text;

            if (this.tbExeOnUnpack.Text != TB_EXE_ON_UNPACK_NOTE)
                options.ExeOnUnpack = this.tbExeOnUnpack.Text;

            if (this.tbDefaultExtractDirectory.Text != TB_EXTRACT_DIR_NOTE)
                options.ExtractDirectory = this.tbDefaultExtractDirectory.Text;

            _workerThread = new Thread(this.DoSave);
            _workerThread.Name = "Zip Saver thread";
            _workerThread.Start(options);
            this.Cursor = Cursors.WaitCursor;
        }


        private string FlavorToString(int p)
        {
            if (p == 2) return "SFX-CMD";
            if (p == 1) return "SFX-GUI";
            return "ZIP";
        }


        private bool _firstFocusInCommentTextBox = true;
        private void tbComment_Enter(object sender, EventArgs e)
        {
            if (_firstFocusInCommentTextBox)
            {
                tbComment.Text = "";
                tbComment.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                tbComment.ForeColor = System.Drawing.SystemColors.WindowText;
                _firstFocusInCommentTextBox = false;
            }
        }

        private void tbComment_Leave(object sender, EventArgs e)
        {
            string TextInTheBox = tbComment.Text;

            if ((TextInTheBox == null) || (TextInTheBox == ""))
            {
                this.tbComment.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.tbComment.ForeColor = System.Drawing.SystemColors.InactiveCaption;
                _firstFocusInCommentTextBox = true;
                this.tbComment.Text = TB_COMMENT_NOTE;
            }
        }


        private bool _firstFocusInExtractDirTextBox = true;
        private void tbDefaultExtractDirectory_Enter(object sender, EventArgs e)
        {
            if (_firstFocusInExtractDirTextBox)
            {
                tbDefaultExtractDirectory.Text = "";
                tbDefaultExtractDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                tbDefaultExtractDirectory.ForeColor = System.Drawing.SystemColors.WindowText;
                _firstFocusInExtractDirTextBox = false;
            }
        }

        private void tbDefaultExtractDirectory_Leave(object sender, EventArgs e)
        {
            string TextInTheBox = tbDefaultExtractDirectory.Text;

            if ((TextInTheBox == null) || (TextInTheBox == ""))
            {
                this.tbDefaultExtractDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.tbDefaultExtractDirectory.ForeColor = System.Drawing.SystemColors.InactiveCaption;
                _firstFocusInExtractDirTextBox = true;
                this.tbDefaultExtractDirectory.Text = TB_EXTRACT_DIR_NOTE;
            }
        }


        // I want the values in the combobox to be right-aligned.
        private int delta = 0;
        private void comboBox1_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            var rc = new System.Drawing.Rectangle(e.Bounds.X + delta, e.Bounds.Y + delta,
                                         e.Bounds.Width - delta, e.Bounds.Height - delta);

            var sf = new System.Drawing.StringFormat
            {
                Alignment = System.Drawing.StringAlignment.Far
            };

            string str = (string)comboSplit.Items[e.Index];

            if (e.State == (DrawItemState.Selected | DrawItemState.NoAccelerator
                              | DrawItemState.NoFocusRect) ||
                 e.State == DrawItemState.Selected)
            {
                e.Graphics.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.CornflowerBlue), rc);
                e.Graphics.DrawString(str, this.comboSplit.Font, new System.Drawing.SolidBrush(System.Drawing.Color.Cyan), rc, sf);
            }
            else
            {
                e.Graphics.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.White), rc);
                e.Graphics.DrawString(str, this.comboSplit.Font, new System.Drawing.SolidBrush(System.Drawing.Color.Black), rc, sf);
            }
        }


        private bool _firstFocusInExeTextBox = true;
        private void tbExeOnUnpack_Enter(object sender, EventArgs e)
        {
            if (_firstFocusInExeTextBox)
            {
                tbExeOnUnpack.Text = "";
                tbExeOnUnpack.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                tbExeOnUnpack.ForeColor = System.Drawing.SystemColors.WindowText;
                _firstFocusInExeTextBox = false;
            }
        }

        private void tbExeOnUnpack_Leave(object sender, EventArgs e)
        {
            string TextInTheBox = tbExeOnUnpack.Text;

            if ((TextInTheBox == null) || (TextInTheBox == ""))
            {
                this.tbExeOnUnpack.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.tbExeOnUnpack.ForeColor = System.Drawing.SystemColors.InactiveCaption;
                _firstFocusInExeTextBox = true;
                this.tbExeOnUnpack.Text = TB_EXE_ON_UNPACK_NOTE;
                this.label18.Enabled = false;
                this.chkRemoveFiles.Enabled = false;
            }
            else
            {
                this.label18.Enabled = true;
                this.chkRemoveFiles.Enabled = true;
            }
        }


        private void tbExeOnUnpack_TextChanged(object sender, EventArgs e)
        {
            if (this.tbExeOnUnpack.Text != TB_EXE_ON_UNPACK_NOTE && this.tbExeOnUnpack.Text != "")
            {
                this.label18.Enabled = true;
                this.chkRemoveFiles.Enabled = true;
            }
            else
            {
                this.label18.Enabled = false;
                this.chkRemoveFiles.Enabled = false;
            }
        }



        private void SetProgressBars()
        {
            if (this.progressBar1.InvokeRequired)
            {
                this.progressBar1.Invoke(new MethodInvoker(this.SetProgressBars));
                //this.progressBar1.Invoke(new ProgressBarSetup(this.SetProgressBars), new object[] { count });
            }
            else
            {
                this.progressBar1.Value = 0;
                this.progressBar1.Maximum = _totalEntriesToProcess;
                this.progressBar1.Minimum = 0;
                this.progressBar1.Step = 1;
                this.progressBar2.Value = 0;
                this.progressBar2.Minimum = 0;
                this.progressBar2.Maximum = 1; // will be set later, for each entry.
                this.progressBar2.Step = 1;
            }
        }


        private void DoSave(Object p)
        {
            SaveWorkerOptions options = p as SaveWorkerOptions;
            try
            {
                using (var zip1 = new ZipFile())
                {
                    zip1.CompressionMethod = options.CompressionMethod;
                    zip1.CompressionLevel = options.CompressionLevel;
                    zip1.AlternateEncoding = System.Text.Encoding.GetEncoding(options.Encoding);
                    zip1.AlternateEncodingUsage = options.EncodingUsage;
                    zip1.Comment = options.Comment;
                    zip1.MaxOutputSegmentSize = options.MaxSegmentSize;
                    zip1.Password = (options.Password != "") ? options.Password : null;
                    zip1.Encryption = options.Encryption;
                    zip1.EmitTimesInWindowsFormatWhenSaving = options.WindowsTimes;
                    zip1.EmitTimesInUnixFormatWhenSaving = options.UnixTimes;
                    zip1.AddDirectoryWillTraverseReparsePoints = options.TraverseJunctions;
                    foreach (ItemToAdd item in options.Entries)
                    {
                        var e = zip1.AddItem(item.LocalFileName, item.DirectoryInArchive);
                        // use a different name in the archive if appropriate
                        if (!String.IsNullOrEmpty(item.FileNameInArchive) && item.FileNameInArchive != System.IO.Path.GetFileName(item.LocalFileName))
                            e.FileName = item.FileNameInArchive;
                    }

                    _totalEntriesToProcess = zip1.EntryFileNames.Count;
                    SetProgressBars();
                    zip1.TempFileFolder = System.IO.Path.GetDirectoryName(options.ZipName);
                    zip1.SaveProgress += this.zip1_SaveProgress;

                    zip1.UseZip64WhenSaving = options.Zip64;

                    if (options.ZipFlavor == 2 || options.ZipFlavor == 1)
                    {
                        SelfExtractorSaveOptions sfxOptions = new SelfExtractorSaveOptions()
                        {
                            Flavor =   (options.ZipFlavor == 1)?SelfExtractorFlavor.WinFormsApplication:
                                SelfExtractorFlavor.ConsoleApplication,
                            DefaultExtractDirectory = options.ExtractDirectory,
                            PostExtractCommandLine = options.ExeOnUnpack,
                            RemoveUnpackedFilesAfterExecute = options.RemoveFilesAfterExe,
                            ExtractExistingFile = (ExtractExistingFileAction) options.ExtractExistingFile,
                        };

                        zip1.SaveSelfExtractor(options.ZipName, sfxOptions);
                    }
                    else
                        zip1.Save(options.ZipName);
                }
            }
            catch (System.Exception exc1)
            {
                MessageBox.Show(String.Format("Exception while zipping:\n{0}\n\n{1}", exc1.Message, exc1.StackTrace.ToString()));
                btnCancel_Click(null, null);
            }
        }



        void zip1_SaveProgress(object sender, SaveProgressEventArgs e)
        {
            switch (e.EventType)
            {
                case ZipProgressEventType.Saving_AfterWriteEntry:
                    StepArchiveProgress(e);
                    break;
                case ZipProgressEventType.Saving_EntryBytesRead:
                    StepEntryProgress(e);
                    break;
                case ZipProgressEventType.Saving_Completed:
                    SaveCompleted();
                    break;
                case ZipProgressEventType.Saving_AfterSaveTempArchive:
                    TempArchiveSaved();
                    break;
            }
            if (_operationCanceled)
                e.Cancel = true;
        }

        private void TempArchiveSaved()
        {
            if (this.lblStatus.InvokeRequired)
            {
                this.lblStatus.Invoke(new MethodInvoker(this.TempArchiveSaved));
            }
            else
            {
                System.TimeSpan ts = new System.TimeSpan(0, 0, (int)_hrt.Seconds);

                lblStatus.Text = String.Format("Temp archive saved ({0})...{1}...",
                    ts.ToString(),
                    (this.comboFlavor.SelectedIndex == 0)
                    ? "finishing"
                    : "compiling SFX");
            }
        }



        private void SaveCompleted()
        {
            if (this.lblStatus.InvokeRequired)
            {
                this.lblStatus.Invoke(new MethodInvoker(this.SaveCompleted));
            }
            else
            {
                _hrt.Stop();
                System.TimeSpan ts = new System.TimeSpan(0, 0, (int)_hrt.Seconds);
                lblStatus.Text = String.Format("Done, Compressed {0} files, {1:N0}% of original, time: {2}",
                           _nFilesCompleted, (100.00 * _totalBytesAfterCompress) / _totalBytesBeforeCompress,
                           ts.ToString());
                ResetUiState();
            }
        }



        private void StepArchiveProgress(ZipProgressEventArgs e)
        {
            if (this.progressBar1.InvokeRequired)
            {
                this.progressBar1.Invoke(new ZipProgress(this.StepArchiveProgress), new object[] { e });
            }
            else
            {
                if (!_operationCanceled)
                {
                    _nFilesCompleted++;
                    this.progressBar1.PerformStep();
                    _totalBytesAfterCompress += e.CurrentEntry.CompressedSize;
                    _totalBytesBeforeCompress += e.CurrentEntry.UncompressedSize;

                    // reset the progress bar for the entry:
                    this.progressBar2.Value = this.progressBar2.Maximum = 1;

                    this.Update();

#if NOT_SPEEDY
                        // Sleep here just to show the progress bar, when the number of files is small,
                        // or when all done.
                        // You may not want this for actual use!
                        if (this.progressBar2.Value == this.progressBar2.Maximum)
                            Thread.Sleep(350);
                        else if (_entriesToZip < 10)
                            Thread.Sleep(350);
                        else if (_entriesToZip < 20)
                            Thread.Sleep(200);
                        else if (_entriesToZip < 30)
                            Thread.Sleep(100);
                        else if (_entriesToZip < 45)
                            Thread.Sleep(80);
                        else if (_entriesToZip < 75)
                            Thread.Sleep(40);
                    // more than 75 entries, don't sleep at all.
#endif

                }
            }
        }


        private void StepEntryProgress(ZipProgressEventArgs e)
        {
            if (this.progressBar2.InvokeRequired)
            {
                this.progressBar2.Invoke(new ZipProgress(this.StepEntryProgress), new object[] { e });
            }
            else
            {
                if (!_operationCanceled)
                {
                    if (this.progressBar2.Maximum == 1)
                    {
                        // reset
                        Int64 entryMax = e.TotalBytesToTransfer;
                        Int64 absoluteMax = System.Int32.MaxValue;
                        _progress2MaxFactor = 0;
                        while (entryMax > absoluteMax)
                        {
                            entryMax /= 2;
                            _progress2MaxFactor++;
                        }
                        if ((int)entryMax < 0) entryMax *= -1;
                        this.progressBar2.Maximum = (int)entryMax;
                        lblStatus.Text = String.Format("{0} of {1} files...({2})",
                               _nFilesCompleted + 1, _totalEntriesToProcess, e.CurrentEntry.FileName);
                    }

                    // downcast is safe here because we have shifted e.BytesTransferred
                    int xferred = (int)(e.BytesTransferred >> _progress2MaxFactor);

                    this.progressBar2.Value = (xferred >= this.progressBar2.Maximum)
                        ? this.progressBar2.Maximum
                        : xferred;

                    this.Update();
                }
            }
        }



        private void btnDirBrowse_Click(object sender, EventArgs e)
        {
            var dlg1 = new Ionic.Utils.FolderBrowserDialogEx
            {
                Description = "Select a folder to zip up:",
                ShowNewFolderButton = false,
                ShowEditBox = true,
                //NewStyle = false,
                SelectedPath = this.tbDirectoryToZip.Text,
                ShowFullPathInEditBox = true,
            };
            dlg1.RootFolder = System.Environment.SpecialFolder.MyComputer;

            var result = dlg1.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.tbDirectoryToZip.Text = dlg1.SelectedPath;
                this.tbDirectoryInArchive.Text = System.IO.Path.GetFileName(this.tbDirectoryToZip.Text);
            }
        }

        private void btnCreateZipBrowse_Click(object sender, EventArgs e)
        {
            var dlg1 = new SaveFileDialog
            {
                OverwritePrompt = false,
                Title = "Where would you like to save the generated Zip file?",
                Filter = "ZIP files|*.zip",
            };

            try
            {
                dlg1.FileName = System.IO.Path.GetFileName(this.tbZipToCreate.Text);
            }
            catch
            {
                dlg1.FileName = "";
            }

            try
            {
                dlg1.InitialDirectory = System.IO.Path.GetDirectoryName(this.tbZipToCreate.Text);
            }
            catch
            {
                dlg1.InitialDirectory = "";
            }

            var result = dlg1.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.tbZipToCreate.Text = dlg1.FileName;
            }
        }


        private void btnZipup_Click(object sender, EventArgs e)
        {
            // Do not start zipping while editing a textbox
            // in listView2.
            if (!textBox1.Visible)
                KickoffZipup();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (this.lblStatus.InvokeRequired)
            {
                this.lblStatus.Invoke(new ButtonClick(this.btnCancel_Click), new object[] { sender, e });
            }
            else
            {
                _operationCanceled = true;
                lblStatus.Text = "Canceled...";
                ResetUiState();
            }
        }

        private void comboFlavor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.comboFlavor.SelectedIndex == 1 || this.comboFlavor.SelectedIndex == 2)
            {
                // intelligently change the name of the thing to create
                // It's an SFX, swap out ZIP and insert EXE
                if (this.tbZipToCreate.Text.ToUpper().EndsWith(".ZIP"))
                {
                    tbZipToCreate.Text = System.Text.RegularExpressions.Regex.Replace(tbZipToCreate.Text, "(?i:)\\.zip$", ".exe");
                }
                // enable/disable other dependent UI elements
                this.label17.Enabled = false;
                this.comboSplit.Enabled = false;
                this.label15.Enabled = true;
                this.tbDefaultExtractDirectory.Enabled = true;
                this.label16.Enabled = true;
                this.tbExeOnUnpack.Enabled = true;

                this.label18.Enabled = true;
                this.chkRemoveFiles.Enabled = true;

            }
            else if (this.comboFlavor.SelectedIndex == 0)
            {
                // intelligently change the name of the thing to create
                // It's a regular ZIP, so swap out EXE and insert ZIP
                if (this.tbZipToCreate.Text.ToUpper().EndsWith(".EXE"))
                {
                    tbZipToCreate.Text = System.Text.RegularExpressions.Regex.Replace(tbZipToCreate.Text, "(?i:)\\.exe$", ".zip");
                }

                // enable/disable other dependent UI elements
                this.label17.Enabled = true;
                this.comboSplit.Enabled = true;
                this.label15.Enabled = false;
                this.tbDefaultExtractDirectory.Enabled = false;
                this.label16.Enabled = false;
                this.tbExeOnUnpack.Enabled = false;
                this.label18.Enabled = false;
                this.chkRemoveFiles.Enabled = false;
            }
        }


        private void comboEncryption_SelectedIndexChanged(object sender, EventArgs e)
        {
            //this.tbPassword.Enabled = (this.comboBox3.SelectedItem.ToString() != "None");
            if (this.comboEncryption.SelectedIndex == 0)
            {
                this.label9.Enabled = false;
                this.tbPassword.Enabled = false;
                this.chkHidePassword.Enabled = false;
            }
            else
            {
                this.label9.Enabled = true;
                this.tbPassword.Enabled = true;
                this.chkHidePassword.Enabled = true;
            }
        }

        private void comboCompMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.comboCompMethod.SelectedIndex == 1)  // DEFLATE
            {
                this.label4.Enabled = true;
                this.comboCompLevel.Enabled = true;
            }
            else
            {
                this.label4.Enabled = false;
                this.comboCompLevel.Enabled = false;
            }
        }

        private void comboEncoding_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.comboEncodingUsage.Enabled = (this.comboEncoding.SelectedIndex != 0);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            this.tbPassword.PasswordChar = (this.chkHidePassword.Checked) ? '*' : '\0';
        }

        private void ResetUiState()
        {
            this.btnZipUp.Text = "Zip it!";
            this.btnZipUp.Enabled = true;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Enabled = false;
            this.btnExtract.Text = "Extract";
            this.btnExtractDirBrowse.Enabled = true;
            this.btnZipupDirBrowse.Enabled = true;
            this.btnReadZipBrowse.Enabled = true;
            this.btnClearItemsToZip.Enabled = true;
            this.btnCreateZipBrowse.Enabled = true;

            this.progressBar1.Value = 0;
            this.progressBar2.Value = 0;
            this.Cursor = Cursors.Default;
            if (_workerThread != null)
                if (!_workerThread.IsAlive)
                    _workerThread.Join();

            _working = false;
        }


        private void SelectNamedEncoding(string s)
        {
            _SelectComboBoxItem(this.comboEncoding, s);
        }

        private void SelectNamedEncodingUsage(string s)
        {
            _SelectComboBoxItem(this.comboEncodingUsage, s);
        }

        private void SelectNamedCompressionLevel(string s)
        {
            _SelectComboBoxItem(this.comboCompLevel, s);
        }

        private void SelectNamedCompressionMethod(string s)
        {
            _SelectComboBoxItem(this.comboCompMethod, s);
        }

        private void SelectNamedEncryption(string s)
        {
            _SelectComboBoxItem(this.comboEncryption, s);
            //tbPassword.Text = "";
            comboEncryption_SelectedIndexChanged(null, null);
        }

        private void _SelectComboBoxItem(ComboBox c, string s)
        {
            for (int i = 0; i < c.Items.Count; i++)
            {
                if (c.Items[i].ToString() == s)
                {
                    c.SelectedIndex = i;
                    break;
                }
            }
        }


        private void ZipForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveFormToRegistry();
        }



        private void btnZipBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = (System.IO.File.Exists(this.tbZipToOpen.Text) ? System.IO.Path.GetDirectoryName(this.tbZipToOpen.Text) : this.tbZipToOpen.Text);
            openFileDialog1.Filter = "zip files|*.zip|EXE files|*.exe|All Files|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.tbZipToOpen.Text = openFileDialog1.FileName;
                if (System.IO.File.Exists(this.tbZipToOpen.Text))
                    btnOpen_Click(sender, e);
            }
        }



        string _DisplayedZip = null;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (!System.IO.File.Exists(this.tbZipToOpen.Text)) return;
            DisplayZipFile(this.tbZipToOpen.Text);
        }


        private void DisplayZipFile(string zipFile)
        {
            try
            {
                listView1.Clear();
                listView1.BeginUpdate();

                string[] columnHeaders = new string[] { "n", "name", "lastmod", "original", "ratio", "compressed", "enc?", "CRC" };
                foreach (string label in columnHeaders)
                {
                    SortableColumnHeader ch = new SortableColumnHeader(label);
                    if (label != "name" && label != "lastmod")
                        ch.TextAlign = HorizontalAlignment.Right;
                    listView1.Columns.Add(ch);
                }

                int n = 1;
                var readOptions = new ReadOptions
                    {
                        Encoding = (comboEncoding.SelectedIndex > 0)
                            ? System.Text.Encoding.GetEncoding(comboEncoding.SelectedItem.ToString())
                            : System.Text.Encoding.GetEncoding("IBM437")
                    };

                using (ZipFile zip = ZipFile.Read(zipFile, readOptions))
                {
                    foreach (ZipEntry entry in zip.EntriesSorted)
                    {
                        ListViewItem item = new ListViewItem(n.ToString());
                        n++;
                        string[] subitems = new string[] {
                            entry.FileName.Replace("/","\\"),
                            entry.LastModified.ToString("yyyy-MM-dd HH:mm:ss"),
                            entry.UncompressedSize.ToString(),
                            String.Format("{0,5:F0}%", entry.CompressionRatio),
                            entry.CompressedSize.ToString(),
                            (entry.UsesEncryption) ? "Y" : "N",
                            String.Format("{0:X8}", entry.Crc)};

                        foreach (String s in subitems)
                        {
                            ListViewItem.ListViewSubItem subitem = new ListViewItem.ListViewSubItem();
                            subitem.Text = s;
                            item.SubItems.Add(subitem);
                        }

                        this.listView1.Items.Add(item);
                    }
                }

                this.btnExtract.Enabled = true;
                _DisplayedZip = zipFile;
                this.listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                this.listView1.EndUpdate();

            }
            catch (Exception exc1)
            {
                this.listView1.Clear();
                this.listView1.EndUpdate();
                MessageBox.Show(String.Format("There was a problem opening that file! [file={0}, problem={1}]",
                    zipFile, exc1.Message), "Whoops!", MessageBoxButtons.OK);
            }

        }


        private void btnExtractDirBrowse_Click(object sender, EventArgs e)
        {
            // pop a dialog to ask where to extract
            // Configure the "select folder" dialog box
            //_folderName = tbDirName.Text;
            //_folderName = (System.IO.Directory.Exists(_folderName)) ? _folderName : "";
            var dlg1 = new Ionic.Utils.FolderBrowserDialogEx
            {
                Description = "Select a folder to extract to:",
                ShowNewFolderButton = true,
                ShowEditBox = true,
                //NewStyle = false,
                SelectedPath = tbExtractDir.Text,
                ShowFullPathInEditBox = true,
            };
            dlg1.RootFolder = System.Environment.SpecialFolder.MyComputer;

            var result = dlg1.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.tbExtractDir.Text = dlg1.SelectedPath;
                // actually extract the files
            }


        }



        private void btnExtract_Click(object sender, EventArgs e)
        {
            if (!System.IO.File.Exists(_DisplayedZip)) return;
            KickoffExtract();
        }


        private void KickoffExtract()
        {
            if (String.IsNullOrEmpty(this.tbExtractDir.Text)) return;

            _hrt = new HiResTimer();
            _hrt.Start();

            _working = true;
            _operationCanceled = false;
            _nFilesCompleted = 0;
            _totalBytesAfterCompress = 0;
            _totalBytesBeforeCompress = 0;
            PauseUI(null);
            lblStatus.Text = "Extracting...";

            var options = new ExtractWorkerOptions
            {
                ExtractLocation = this.tbExtractDir.Text,
                Selection = this.tbSelectionToExtract.Text,
                OpenExplorer = this.chkOpenExplorer.Checked,
                ExtractExisting = (ExtractExistingFileAction) comboExistingFileAction.SelectedIndex,
            };

            _workerThread = new Thread(this.DoExtract);
            _workerThread.Name = "Zip Extractor thread";
            _workerThread.Start(options);
            this.Cursor = Cursors.WaitCursor;
        }


        private void PauseUI(string msg)
        {
            // this set for Zipping
            if (msg != null)
                this.btnZipUp.Text = msg;
            this.btnZipUp.Enabled = false;
            this.btnZipupDirBrowse.Enabled = false;
            this.btnCreateZipBrowse.Enabled = false;
            this.btnClearItemsToZip.Enabled = false;
            this.btnCancel.Enabled = true;

            // this for Extract
            this.btnExtract.Enabled = false;
            this.btnExtract.Text = "working...";
            this.btnExtractDirBrowse.Enabled = false;
            this.btnCancel.Enabled = true;
            this.btnReadZipBrowse.Enabled = false;
        }


        private void zip_ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            if (e.EventType == ZipProgressEventType.Extracting_EntryBytesWritten)
            {
                StepEntryProgress(e);
            }

            else if (e.EventType == ZipProgressEventType.Extracting_AfterExtractEntry)
            {
                StepArchiveProgress(e);
            }
            if (_setCancel)
                e.Cancel = true;
        }


        private void OnExtractDone()
        {
            if (this.lblStatus.InvokeRequired)
            {
                this.lblStatus.Invoke(new MethodInvoker(this.OnExtractDone));
            }
            else
            {
                _hrt.Stop();
                System.TimeSpan ts = new System.TimeSpan(0, 0, (int)_hrt.Seconds);
                lblStatus.Text = String.Format("Done, Extracted {0} files, time: {1}",
                           _nFilesCompleted,
                           ts.ToString());
                ResetUiState();

                // remember the successful selection strings
                if (!_selectionCompletions.Contains(this.tbSelectionToZip.Text))
                {
                    if (_selectionCompletions.Count >= _MaxMruListSize)
                        _selectionCompletions.RemoveAt(0);
                    _selectionCompletions.Add(this.tbSelectionToZip.Text);
                }
            }
        }


        bool _setCancel = false;
        private void DoExtract(object p)
        {
            ExtractWorkerOptions options = p as ExtractWorkerOptions;

            bool extractCancelled = false;
            _setCancel = false;
            string currentPassword = "";

            try
            {
                using (var zip = ZipFile.Read(_DisplayedZip))
                {
                    System.Collections.Generic.ICollection<ZipEntry> collection = null;
                    if (String.IsNullOrEmpty(options.Selection))
                        collection = zip.Entries;
                    else
                        collection = zip.SelectEntries(options.Selection);

                    _totalEntriesToProcess = collection.Count;
                    zip.ExtractProgress += zip_ExtractProgress;
                    SetProgressBars();
                    foreach (global::Ionic.Zip.ZipEntry entry in collection)
                    {
                        if (_setCancel) { extractCancelled = true; break; }
                        if (entry.Encryption == global::Ionic.Zip.EncryptionAlgorithm.None)
                        {
                            try
                            {
                                entry.Extract(options.ExtractLocation, options.ExtractExisting);
                            }
                            catch (Exception ex1)
                            {
                                string msg = String.Format("Faisled to extract entry {0} -- {1}",
                                               entry.FileName,
                                               ex1.Message.ToString());
                                DialogResult result =
                                    MessageBox.Show(msg,
                                            String.Format("Error Extracting {0}", entry.FileName),
                                            MessageBoxButtons.OKCancel,
                                            MessageBoxIcon.Exclamation,
                                            MessageBoxDefaultButton.Button1);

                                if (result == DialogResult.Cancel)
                                {
                                    _setCancel = true;
                                    extractCancelled = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            bool done = false;
                            while (!done)
                            {
                                if (currentPassword == "")
                                {
                                    string t = PromptForPassword(entry.FileName);
                                    if (t == "")
                                    {
                                        done = true; // escape ExtractWithPassword loop
                                        continue;
                                    }
                                    currentPassword = t;
                                }

                                if (currentPassword == null) // cancel all
                                {
                                    _setCancel = true;
                                    currentPassword = "";
                                    break;
                                }

                                try
                                {
                                    entry.ExtractWithPassword(options.ExtractLocation, options.ExtractExisting, currentPassword);
                                    done = true;
                                }
                                catch (Exception ex2)
                                {
                                    // Retry here in the case of bad password.
                                    if (ex2 as Ionic.Zip.BadPasswordException != null)
                                    {
                                        currentPassword = "";
                                        continue; // loop around, ask for password again
                                    }
                                    else
                                    {
                                        string msg =
                                            String.Format("Failed to extract the password-encrypted entry {0} -- {1}",
                                                  entry.FileName, ex2.Message.ToString());
                                        DialogResult result =
                                            MessageBox.Show(msg,
                                                    String.Format("Error Extracting {0}",
                                                          entry.FileName),
                                                    MessageBoxButtons.OKCancel,
                                                    MessageBoxIcon.Exclamation,
                                                    MessageBoxDefaultButton.Button1);

                                        done = true; // done with this entry
                                        if (result == DialogResult.Cancel)
                                        {
                                            _setCancel = true;
                                            extractCancelled = true;
                                            break;
                                        }
                                    }
                                }
                            } // while
                        } // else (encryption)
                    } // foreach
                } // using


            }
            catch (Exception ex1)
            {
                MessageBox.Show(String.Format("There's been a problem extracting that zip file.  {0}", ex1.Message),
                "Error Extracting", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                Application.Exit();
            }

            OnExtractDone();

            if (extractCancelled) return;

            if (options.OpenExplorer)
            {
                string w = System.IO.Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.System));
                if (w == null) w = "c:\\windows";
                try
                {
                    System.Diagnostics.Process.Start(System.IO.Path.Combine(w, "explorer.exe"), options.ExtractLocation);
                }
                catch { }
            }
        }


        private string PromptForPassword(string entryName)
        {
            PasswordDialog dlg1 = new PasswordDialog();
            dlg1.EntryName = entryName;

            // ask for password in a loop until user enters a proper one,
            // or clicks skip or cancel.
            bool done = false;
            do
            {
                dlg1.ShowDialog();
                done = (dlg1.Result != PasswordDialog.PasswordDialogResult.OK ||
                    dlg1.Password != "");
            } while (!done);

            if (dlg1.Result == PasswordDialog.PasswordDialogResult.OK)
                return dlg1.Password;

            else if (dlg1.Result == PasswordDialog.PasswordDialogResult.Skip)
                return "";

            // cancel
            return null;
        }


        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Recycle the progress bars the cancel button, and the status textbox.
            // An alternative way to accomplish a similar thing is to visually place
            // the progress bars OFF the tabs.
            if (this.tabControl1.SelectedIndex == 0)
            {
                if (this.tabPage2.Controls.Contains(this.progressBar1))
                    this.tabPage2.Controls.Remove(this.progressBar1);
                if (this.tabPage2.Controls.Contains(this.progressBar2))
                    this.tabPage2.Controls.Remove(this.progressBar2);
                if (this.tabPage2.Controls.Contains(this.btnCancel))
                    this.tabPage2.Controls.Remove(this.btnCancel);
                if (this.tabPage2.Controls.Contains(this.lblStatus))
                    this.tabPage2.Controls.Remove(this.lblStatus);

                if (!this.tabPage1.Controls.Contains(this.lblStatus))
                    this.tabPage1.Controls.Add(this.lblStatus);
                if (!this.tabPage1.Controls.Contains(this.progressBar1))
                    this.tabPage1.Controls.Add(this.progressBar1);
                if (!this.tabPage1.Controls.Contains(this.progressBar2))
                    this.tabPage1.Controls.Add(this.progressBar2);
                if (!this.tabPage1.Controls.Contains(this.btnCancel))
                    this.tabPage1.Controls.Add(this.btnCancel);

                // swap the comboBox for Encoding to the selected panel
                    if (groupBox2.Controls.Contains(comboEncoding))
                    {
                        groupBox2.Controls.Remove(comboEncoding);
                        tabPage1.Controls.Add(comboEncoding);
                        int xpos = this.tbZipToOpen.Location.X ;
                        this.comboEncoding.Location = new System.Drawing.Point(xpos, this.tbZipToOpen.Location.Y + this.comboEncoding.Height + 4);
                    }
            this.toolTip1.SetToolTip(this.comboEncoding, "use this encoding to read the file");

            }
            else if (this.tabControl1.SelectedIndex == 1)
            {
                if (this.tabPage1.Controls.Contains(this.progressBar1))
                    this.tabPage1.Controls.Remove(this.progressBar1);
                if (this.tabPage1.Controls.Contains(this.progressBar2))
                    this.tabPage1.Controls.Remove(this.progressBar2);
                if (this.tabPage1.Controls.Contains(this.btnCancel))
                    this.tabPage1.Controls.Remove(this.btnCancel);
                if (this.tabPage1.Controls.Contains(this.lblStatus))
                    this.tabPage1.Controls.Remove(this.lblStatus);

                if (!this.tabPage2.Controls.Contains(this.lblStatus))
                    this.tabPage2.Controls.Add(this.lblStatus);
                if (!this.tabPage2.Controls.Contains(this.progressBar1))
                    this.tabPage2.Controls.Add(this.progressBar1);
                if (!this.tabPage2.Controls.Contains(this.progressBar2))
                    this.tabPage2.Controls.Add(this.progressBar2);
                if (!this.tabPage2.Controls.Contains(this.btnCancel))
                    this.tabPage2.Controls.Add(this.btnCancel);

                // swap the comboBox for Encoding to the selected panel
                    if (tabPage1.Controls.Contains(comboEncoding))
                    {
                        tabPage1.Controls.Remove(comboEncoding);
                        groupBox2.Controls.Add(comboEncoding);
                        this.comboEncoding.Location = new System.Drawing.Point(104, 85);
                        this.comboEncoding.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
                    }
            this.toolTip1.SetToolTip(this.comboEncoding, "use this encoding when saving the file");

            }
        }

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            // prevent switching TABs if working
            if (_working) e.Cancel = true;
        }


        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Create an instance of the ColHeader class.
            SortableColumnHeader clickedCol = (SortableColumnHeader)this.listView1.Columns[e.Column];

            // Set the ascending property to sort in the opposite order.
            clickedCol.SortAscending = !clickedCol.SortAscending;

            // Get the number of items in the list.
            int numItems = this.listView1.Items.Count;

            // Turn off display while data is repoplulated.
            this.listView1.BeginUpdate();

            // Populate an ArrayList with a SortWrapper of each list item.
            List<ItemWrapper> list = new List<ItemWrapper>();
            for (int i = 0; i < numItems; i++)
            {
                list.Add(new ItemWrapper(this.listView1.Items[i], e.Column));
            }

            if (e.Column == 0 || e.Column == 3 || e.Column == 5)
                list.Sort(new ItemWrapper.NumericComparer(clickedCol.SortAscending));
            else
                list.Sort(new ItemWrapper.StringComparer(clickedCol.SortAscending));

            // Clear the list, and repopulate with the sorted items.
            this.listView1.Items.Clear();
            for (int i = 0; i < numItems; i++)
                this.listView1.Items.Add(list[i].Item);

            // Turn display back on.
            this.listView1.EndUpdate();
        }


        private void tbPassword_TextChanged(object sender, EventArgs e)
        {
            if (this.tbPassword.Text == "")
            {
                if (_mostRecentEncryption == null && this.comboEncryption.SelectedItem.ToString() != "None")
                {
                    _mostRecentEncryption = this.comboEncryption.SelectedItem.ToString();
                    SelectNamedEncryption("None");
                }
            }
            else
            {
                if (_mostRecentEncryption != null && this.comboEncryption.SelectedItem.ToString() == "None")
                {
                    SelectNamedEncryption(_mostRecentEncryption);
                }
                _mostRecentEncryption = null;
            }
        }


        private void LoadAboutInfo()
        {
            var a = System.Reflection.Assembly.GetEntryAssembly();
            string[] resourceNames = a.GetManifestResourceNames();
            if (resourceNames != null && resourceNames.Length > 0)
            {
                String name =  "Ionic.Zip.Forms.About.rtf";
                var s = a.GetManifestResourceStream(name);
                var bytes = ReadAllBytes(s);
                this.richTextBox1.Rtf = System.Text.Encoding.ASCII.GetString(bytes);
                s.Close();
            }
        }



        /// <summary>
        ///   Reads the contents of the stream into a byte array.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     Like File.ReadAllBytes(), but for a stream.
        ///   </para>
        /// </remarks>
        /// <param name="stream">The stream to read.</param>
        /// <returns>A byte array containing the contents of the stream.</returns>
        /// <exception cref="NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        private byte[] ReadAllBytes(System.IO.Stream source)
        {
            long originalPosition = source.Position;
            source.Position = 0;

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = source.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = source.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                source.Position = originalPosition;
            }
        }



        private void ZipForm_Load(object sender, EventArgs e)
        {
            if (_initialFileToLoad != null)
            {
                // select the page that opens zip files.
                this.tabControl1.SelectedIndex = 0;
                //this.tabPage1.Select();
                this.tbZipToOpen.Text = _initialFileToLoad;
                btnOpen_Click(null, null);
            }
            LoadAboutInfo();
        }

        private void tbDirectoryToZip_Leave(object sender, EventArgs e)
        {
            this.tbDirectoryInArchive.Text = System.IO.Path.GetFileName(this.tbDirectoryToZip.Text);
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }


        private void listView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("FileDrop") && (e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                // Get the data.
                var filePaths = (String[])e.Data.GetData("FileDrop");

                // allow drop of one file on listView1, drop multiple files on listView2.
                if (filePaths.Length == 1 || sender == this.listView2)
                    //A file is being dragged and it can be copied so provide feedback to the user.
                    e.Effect = DragDropEffects.Copy;
            }
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            // The data can only be dropped if it is a file list and it can be copied.
            if (e.Data.GetDataPresent("FileDrop") && (e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                // Get the data.
                var filePaths = (String[])e.Data.GetData("FileDrop");

                // The data is an array of file paths.
                // If it is a single file and ends in .zip, then we know how to open it
                // and display the contents.
                // If it is more than one file, then we don't know what to do with it.
                if (filePaths.Length == 1)
                {
                    DisplayZipFile(filePaths[0]);
                    this.tbZipToOpen.Text = filePaths[0];
                    this.tbExtractDir.Text = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filePaths[0]), System.IO.Path.GetFileNameWithoutExtension(filePaths[0]) + "_files");
                }
            }
        }

        private void listView2_DragDrop(object sender, DragEventArgs e)
        {
            // The data can only be dropped if it is a file list and it can be copied.
            if (e.Data.GetDataPresent("FileDrop") && (e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                // Get the data.
                var filePaths = (String[])e.Data.GetData("FileDrop");
                this.listView2.BeginUpdate();
                foreach (var f in filePaths)
                {
                    var item = new ListViewItem();

                    // first subitem is the local filename  on disk
                    var subitem = new ListViewItem.ListViewSubItem();
                    subitem.Text = f;
                    item.SubItems.Add(subitem);

                    // next subitem is the directory name to use in the archive
                    subitem = new ListViewItem.ListViewSubItem();
                    //subitem.Text = String.IsNullOrEmpty(_lastDirectory) ? this.tbDirectoryInArchive.Text : _lastDirectory;
                    subitem.Text = String.IsNullOrEmpty(_lastDirectory)
                        ? System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(f))
                        : _lastDirectory;
                    item.SubItems.Add(subitem);

                    // additional subitem (to be added): new filename in archive, if any
                    subitem = new ListViewItem.ListViewSubItem();
                    item.SubItems.Add(subitem);

                    this.listView2.Items.Add(item);
                }

                this.listView2.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                this.listView2.EndUpdate();
            }
        }

        private void listView2_SubItemClicked(object sender, ListViewEx.SubItemEventArgs e)
        {
            //this.AcceptButton = null;

            // Prevent editing the 0th column - it's the checkbox.  We want the checkbox
            // label to remain "".
            if (e.SubItem == 0) return;

            this.listView2.StartEditing(this.textBox1, e.Item, e.SubItem);
            //this.textBox1.Focus(); // to get the RETURN key?  no. this did not work.
        }


        private void listView2_SubItemEndEditing(object sender, ListViewEx.SubItemEndEditingEventArgs e)
        {
            if (!e.Cancel)
            {
                e.DisplayText = textBox1.Text;
                //this.AcceptButton = this.btnZipUp;
                //this.listView2.Select();
                // this.listView2.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
            //this.listView2.EndEditing(true);
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                this.listView2.EndEditing(true);
                e.Handled = true;
                _lastDirectory = this.textBox1.Text;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int nAdded = 0;
            PauseUI(null);
            try
            {
                // do file selection, add each item into the list box
                var fs = new global::Ionic.FileSelector(this.tbSelectionToZip.Text,this.chkTraverseJunctions.Checked);
                var files = fs.SelectFiles(this.tbDirectoryToZip.Text, this.chkRecurse.Checked);
                this.listView2.BeginUpdate();
                foreach (String f in files)
                {
                    var item = new ListViewItem();
                    // first subitem is the filename
                    var subitem = new ListViewItem.ListViewSubItem();
                    subitem.Text = f;
                    item.SubItems.Add(subitem);
                    // second subitem is the directory name in the archive.
                    subitem = new ListViewItem.ListViewSubItem();
                    var dirInArchive = this.tbDirectoryInArchive.Text;
                    var subDir = System.IO.Path.GetDirectoryName(f.Replace(this.tbDirectoryToZip.Text, ""));
                    subDir = subDir.Substring(1);
                    subitem.Text = System.IO.Path.Combine(dirInArchive, subDir);
                    item.SubItems.Add(subitem);

                    // third subitem is the filename in the archive, if ay
                     subitem = new ListViewItem.ListViewSubItem();
                    item.SubItems.Add(subitem);

                    this.listView2.Items.Add(item);
                    nAdded++;
                }

                this.listView2.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                this.listView2.EndUpdate();
                this.lblStatus.Text = String.Format("Added {0} entries;  {1} total entries to save.", nAdded, this.listView2.Items.Count);


                // remember the successful selection strings
                if (!_selectionCompletions.Contains(this.tbSelectionToZip.Text))
                {
                    if (_selectionCompletions.Count >= _MaxMruListSize)
                        _selectionCompletions.RemoveAt(0);
                    _selectionCompletions.Add(this.tbSelectionToZip.Text);
                }

            }
            catch
            { }
            ResetUiState();
            //_disableMasterChecking = false;
        }


        private void btnClearItemsToZip_Click(object sender, EventArgs e)
        {
            int rCount=0;
            foreach (ListViewItem item in this.listView2.Items)
            {
                if (item.Checked)
                {
                    this.listView2.Items.Remove(item);
                    rCount++;
                }
            }

            // After removing all the checked items, all items  are now unchecked.
            // We can set the mast checkbox to unchecked.  But, we have to protect
            // against infinite recursion.
            _disableMasterChecking = true;
            this.checkBox1.Checked = false;
            _disableMasterChecking = false;
            this.lblStatus.Text = (rCount == 1)
                ? String.Format("Cleared 1 entry.  {1} remaining entries to save.", rCount, this.listView2.Items.Count)
                : String.Format("Cleared {0} entries.  {1} remaining entries to save.", rCount, this.listView2.Items.Count);
        }



        bool _disableMasterChecking = false;
        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {
            // prevent infinite recursion.
            if (_disableMasterChecking) return;

            // if we have a mixed state, then it happened programmatically
            if (this.checkBox1.CheckState == CheckState.Indeterminate) return;

            _disableListViewCheckedEvent = true;
            _disableMasterChecking = true;
            // change state of ALL items
            foreach (ListViewItem item in this.listView2.Items)
            {
                item.Checked = this.checkBox1.Checked;
            }
            _disableMasterChecking = false;
            _disableListViewCheckedEvent = false;
        }


        private bool _disableListViewCheckedEvent;
        private void listView2_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            // prevent infinite recursion
            if (_disableListViewCheckedEvent) return;

            System.Drawing.Point p = this.listView2.PointToClient(new System.Drawing.Point(Cursor.Position.X, Cursor.Position.Y));
            ListViewItem lvi;
            int ix = this.listView2.GetSubItemAt(p.X, p.Y, out lvi);

            // lvi is null when the checkchange comes from a non-mouse event, like when
            // a new item is being added to the list.
            if (lvi == null) return;

            // ix is 0 when the checkbox is the thing that was tickled with the mouse.
            if (ix == 0)
                MaybeSetMasterCheckbox();
            else
            {
                // Revert the checkChange if it was due to a mouse click on a subitem other than the 0th one.
                // The Checked property has already been changed (in ListView.WndProc?); we need to undo it.
                // In order to avoid an endless recursion, set the disable flag, first.
                _disableListViewCheckedEvent = true;
                lvi.Checked = !lvi.Checked;
                _disableListViewCheckedEvent = false;
            }
        }


        private void MaybeSetMasterCheckbox()
        {
            if (_disableMasterChecking) return;

            bool uniform = true;
            // check the state of all the items
            for (int i = 1; i < this.listView2.Items.Count; i++)
            {
                if (this.listView2.Items[i].Checked != this.listView2.Items[i - 1].Checked)
                {
                    uniform = false;
                    break;
                }
            }

            _disableMasterChecking = true;
            if (uniform)
                this.checkBox1.CheckState = (this.listView2.Items[0].Checked) ? CheckState.Checked : CheckState.Unchecked;
            else
                this.checkBox1.CheckState = CheckState.Indeterminate;
            _disableMasterChecking = false;
        }

        private void listView2_BeforeLabelEdit(object sender, LabelEditEventArgs e)
        {
            // never let the use edit the label associated to the main listview item,
            // the label on the item checkbox.
            e.CancelEdit = true;
        }


        private int _progress2MaxFactor;
        private int _totalEntriesToProcess;
        private bool _working;
        private bool _operationCanceled;
        private int _nFilesCompleted;
        private long _totalBytesBeforeCompress;
        private long _totalBytesAfterCompress;
        private Thread _workerThread;
        private static string TB_COMMENT_NOTE = "-zip file comment here-";
        private static string TB_EXTRACT_DIR_NOTE = "-default extract directory-";
        private static string TB_EXE_ON_UNPACK_NOTE = "-command line to execute here-";
        private String _mostRecentEncryption;
        private string _initialFileToLoad;
        private string _lastDirectory;
    }



    // The ColHeader class is a ColumnHeader object with an
    // added property for determining an ascending or descending sort.
    // True specifies an ascending order, false specifies a descending order.
    public class SortableColumnHeader : ColumnHeader
    {
        public bool SortAscending;
        public SortableColumnHeader(string text)
        {
            this.Text = text;
            this.SortAscending = true;
        }
    }


    // An instance of the SortWrapper class is created for
    // each item and added to the ArrayList for sorting.
    public class ItemWrapper
    {
        internal ListViewItem Item;
        internal int Column;

        // A SortWrapper requires the item and the index of the clicked column.
        public ItemWrapper(ListViewItem item, int column)
        {
            Item = item;
            Column = column;
        }

        // Text property for getting the text of an item.
        public string Text
        {
            get { return Item.SubItems[Column].Text; }
        }

        // Implementation of the IComparer
        public class StringComparer : IComparer<ItemWrapper>
        {
            bool ascending;

            // Constructor requires the sort order;
            // true if ascending, otherwise descending.
            public StringComparer(bool asc)
            {
                this.ascending = asc;
            }

            // Implemnentation of the IComparer:Compare
            // method for comparing two objects.
            public int Compare(ItemWrapper xItem, ItemWrapper yItem)
            {
                string xText = xItem.Item.SubItems[xItem.Column].Text;
                string yText = yItem.Item.SubItems[yItem.Column].Text;
                return xText.CompareTo(yText) * (this.ascending ? 1 : -1);
            }
        }

        public class NumericComparer : IComparer<ItemWrapper>
        {
            bool ascending;

            // Constructor requires the sort order;
            // true if ascending, otherwise descending.
            public NumericComparer(bool asc)
            {
                this.ascending = asc;
            }

            // Implementation of the IComparer:Compare
            // method for comparing two objects.
            public int Compare(ItemWrapper xItem, ItemWrapper yItem)
            {
                int x = 0, y = 0;
                try
                {
                    x = Int32.Parse(xItem.Item.SubItems[xItem.Column].Text);
                    y = Int32.Parse(yItem.Item.SubItems[yItem.Column].Text);
                }
                catch
                {
                    try
                    {
                        // lop off one char for %
                        String trimmed = null;
                        trimmed = xItem.Item.SubItems[xItem.Column].Text;
                        trimmed = trimmed.Substring(0, trimmed.Length - 1);
                        x = Int32.Parse(trimmed);
                        trimmed = xItem.Item.SubItems[yItem.Column].Text;
                        trimmed = trimmed.Substring(0, trimmed.Length - 1);
                        y = Int32.Parse(trimmed);
                    }
                    catch { }
                }
                return (x - y) * (this.ascending ? 1 : -1);
            }
        }
    }
    public class ExtractWorkerOptions
    {
        public string ExtractLocation;
        public Ionic.Zip.ExtractExistingFileAction ExtractExisting;
        public bool OpenExplorer;
        public String Selection;
    }

    public class SaveWorkerOptions
    {
        public string ZipName;
        public string Selection;
        public bool TraverseJunctions;
        public bool RemoveFilesAfterExe;
        public string Encoding;
        public Ionic.Zip.ZipOption EncodingUsage;
        public string Comment;
        public string Password;
        public string ExeOnUnpack;
        public string ExtractDirectory;
        public int ExtractExistingFile;
        public int ZipFlavor;
        public int MaxSegmentSize;
        public Ionic.Zlib.CompressionLevel CompressionLevel;
        public Ionic.Zip.CompressionMethod CompressionMethod;
        public Ionic.Zip.EncryptionAlgorithm Encryption;
        public Zip64Option Zip64;
        public bool WindowsTimes;
        public bool UnixTimes;
        public ItemToAdd[] Entries;
    }

    public class ItemToAdd
    {
        public string LocalFileName;
        public string DirectoryInArchive;
        public string FileNameInArchive;
    }


    internal class HiResTimer
    {
        // usage:
        //
        //  hrt= new HiResTimer();
        //  hrt.Start();
        //     ... do work ...
        //  hrt.Stop();
        //  System.Console.WriteLine("elapsed time: {0:N4}", hrt.Seconds);
        //

        [System.Runtime.InteropServices.DllImport("KERNEL32")]
        private static extern bool QueryPerformanceCounter(ref long lpPerformanceCount);

        [System.Runtime.InteropServices.DllImport("KERNEL32")]
        private static extern bool QueryPerformanceFrequency(ref long lpFrequency);

        private long m_TickCountAtStart = 0;
        private long m_TickCountAtStop = 0;
        private long m_ElapsedTicks = 0;

        public HiResTimer()
        {
            m_Frequency = 0;
            QueryPerformanceFrequency(ref m_Frequency);
        }

        public void Start()
        {
            m_TickCountAtStart = 0;
            QueryPerformanceCounter(ref m_TickCountAtStart);
        }

        public void Stop()
        {
            m_TickCountAtStop = 0;
            QueryPerformanceCounter(ref m_TickCountAtStop);
            m_ElapsedTicks = m_TickCountAtStop - m_TickCountAtStart;
        }

        public void Reset()
        {
            m_TickCountAtStart = 0;
            m_TickCountAtStop = 0;
            m_ElapsedTicks = 0;
        }

        public long Elapsed
        {
            get { return m_ElapsedTicks; }
        }

        public float Seconds
        {
            get { return ((float)m_ElapsedTicks / (float)m_Frequency); }
        }

        private long m_Frequency = 0;
        public long Frequency
        {
            get { return m_Frequency; }
        }

    }

}
