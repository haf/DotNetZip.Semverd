// Form.State.cs
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
    public partial class ZipForm
    {
        /// This app uses the windows registry to store config data for itself.
        ///     - creates a registry key for this DotNetZip Winforms app, if one does not exist
        ///     - stores and retrieves the most recent settings.
        ///     - this is done on a per user basis. (HKEY_CURRENT_USER)
        private void FillFormFromRegistry()
        {
            if (!stateLoaded)
            {
                if (AppCuKey != null)
                {
                    var s = (string)AppCuKey.GetValue(_rvn_DirectoryToZip);
                    if (s != null)
                    {
                        this.tbDirectoryToZip.Text = s;
                        this.tbDirectoryInArchive.Text = System.IO.Path.GetFileName(this.tbDirectoryToZip.Text);
                    }

                    s = (string)AppCuKey.GetValue(_rvn_SelectionToZip);
                    if (s != null) this.tbSelectionToZip.Text = s;

                    s = (string)AppCuKey.GetValue(_rvn_SelectionToExtract);
                    if (s != null) this.tbSelectionToExtract.Text = s;

                    s = (string)AppCuKey.GetValue(_rvn_ZipTarget);
                    if (s != null) this.tbZipToCreate.Text = s;

                    s = (string)AppCuKey.GetValue(_rvn_ZipToOpen);
                    if (s != null) this.tbZipToOpen.Text = s;

                    s = (string)AppCuKey.GetValue(_rvn_ExtractLoc);
                    if (s != null) this.tbExtractDir.Text = s;
                    else
                        this.tbExtractDir.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                    s = (string)AppCuKey.GetValue(_rvn_Encoding);
                    if (s != null)
                        SelectNamedEncoding(s);

                    s = (string)AppCuKey.GetValue(_rvn_EncodingUsage);
                    if (s != null)
                        SelectNamedEncodingUsage(s);

                    s = (string)AppCuKey.GetValue(_rvn_CompLevel);
                    if (s != null)
                    {
                        SelectNamedCompressionLevel(s);
                    }
                    else SelectNamedCompressionLevel("Default");

                    s = (string)AppCuKey.GetValue(_rvn_CompMethod);
                    if (s != null)
                    {
                        SelectNamedCompressionMethod(s);
                    }
                    else SelectNamedCompressionMethod("Deflate");

                    s = (string)AppCuKey.GetValue(_rvn_Encryption);
                    if (s != null)
                    {
                        SelectNamedEncryption(s);
                        this.tbPassword.Text = "";
                    }

                    int x = (Int32)AppCuKey.GetValue(_rvn_ZipFlavor, 0);
                    if (x >= 0 && x <= 2)
                        this.comboFlavor.SelectedIndex = x;

                    x = (Int32)AppCuKey.GetValue(_rvn_Zip64Option, 0);
                    if (x >= 0 && x <= 2)
                        this.comboZip64.SelectedIndex = x;

                    x = (Int32)AppCuKey.GetValue(_rvn_ExtractExistingFileAction, 0);
                    if (x >= 0 && x <= comboExistingFileAction.Items.Count)
                        this.comboExistingFileAction.SelectedIndex = x;

                    x = (Int32)AppCuKey.GetValue(_rvn_FormTab, 1);
                    if (x == 0 || x == 1)
                        this.tabControl1.SelectedIndex = x;

                    x = (Int32)AppCuKey.GetValue(_rvn_HidePassword, 1);
                    this.chkHidePassword.Checked = (x != 0);

                    x = (Int32)AppCuKey.GetValue(_rvn_OpenExplorer, 1);
                    this.chkOpenExplorer.Checked = (x != 0);

                    x = (Int32)AppCuKey.GetValue(_rvn_TraverseJunctions, 1);
                    this.chkTraverseJunctions.Checked = (x != 0);

                    x = (Int32)AppCuKey.GetValue(_rvn_RecurseDirs, 1);
                    this.chkRecurse.Checked = (x != 0);

                    x = (Int32)AppCuKey.GetValue(_rvn_RemoveFiles, 1);
                    this.chkRemoveFiles.Checked = (x != 0);

                    numRuns = (Int32)AppCuKey.GetValue(_rvn_Runs, 0);

                    // get the MRU list of selection expressions
                    _selectionCompletions = new System.Windows.Forms.AutoCompleteStringCollection();
                    string history = (string)AppCuKey.GetValue(_rvn_SelectionCompletions, "");
                    if (!String.IsNullOrEmpty(history))
                    {
                        string[] items = history.Split('¡');
                        if (items != null && items.Length > 0)
                        {
                            foreach (string item in items)
                                _selectionCompletions.Add(item.XmlUnescapeIexcl());
                        }
                    }



                    // set the geometry of the form
                    s = (string)AppCuKey.GetValue(_rvn_Geometry);
                    if (!String.IsNullOrEmpty(s))
                    {
                        int[] p = Array.ConvertAll<string, int>(s.Split(','),
                                                                new Converter<string, int>((t) => { return Int32.Parse(t); }));
                        if (p != null && p.Length == 5)
                        {
                            this.Bounds = ConstrainToScreen(new System.Drawing.Rectangle(p[0], p[1], p[2], p[3]));

                            // Starting a window minimized is confusing...
                            //this.WindowState = (FormWindowState)p[4];
                        }
                    }

                    AppCuKey.Close();
                    AppCuKey = null;

                    tbPassword_TextChanged(null, null);

                    stateLoaded = true;
                }
            }
        }



        private void SaveFormToRegistry()
        {
            if (this.tbExtractDir.InvokeRequired) return; // skip it

            if (AppCuKey != null)
            {
                AppCuKey.SetValue(_rvn_DirectoryToZip, this.tbDirectoryToZip.Text);
                AppCuKey.SetValue(_rvn_SelectionToZip, this.tbSelectionToZip.Text);
                AppCuKey.SetValue(_rvn_SelectionToExtract, this.tbSelectionToExtract.Text);
                AppCuKey.SetValue(_rvn_ZipTarget, this.tbZipToCreate.Text);
                AppCuKey.SetValue(_rvn_ZipToOpen, this.tbZipToOpen.Text);
                AppCuKey.SetValue(_rvn_Encoding, this.comboEncoding.SelectedItem.ToString());
                AppCuKey.SetValue(_rvn_EncodingUsage, this.comboEncodingUsage.SelectedItem.ToString());
                AppCuKey.SetValue(_rvn_CompLevel, this.comboCompLevel.SelectedItem.ToString());
                AppCuKey.SetValue(_rvn_CompMethod, this.comboCompMethod.SelectedItem.ToString());
                if (this.tbPassword.Text == "")
                {
                    if (!String.IsNullOrEmpty(_mostRecentEncryption))
                        AppCuKey.SetValue(_rvn_Encryption, _mostRecentEncryption);
                }
                else
                    AppCuKey.SetValue(_rvn_Encryption, this.comboEncryption.SelectedItem.ToString());

                AppCuKey.SetValue(_rvn_ExtractLoc, this.tbExtractDir.Text);

                int x = this.comboFlavor.SelectedIndex;
                AppCuKey.SetValue(_rvn_ZipFlavor, x);

                x = this.comboZip64.SelectedIndex;
                AppCuKey.SetValue(_rvn_Zip64Option, x);

                x = this.comboExistingFileAction.SelectedIndex;
                AppCuKey.SetValue(_rvn_ExtractExistingFileAction, x);

                AppCuKey.SetValue(_rvn_FormTab, this.tabControl1.SelectedIndex);

                AppCuKey.SetValue(_rvn_LastRun, System.DateTime.Now.ToString("yyyy MMM dd HH:mm:ss"));
                x = (Int32)AppCuKey.GetValue(_rvn_Runs, 0);
                x++;
                AppCuKey.SetValue(_rvn_Runs, x);

                AppCuKey.SetValue(_rvn_HidePassword, this.chkHidePassword.Checked ? 1 : 0);
                AppCuKey.SetValue(_rvn_OpenExplorer, this.chkOpenExplorer.Checked ? 1 : 0);
                AppCuKey.SetValue(_rvn_TraverseJunctions, this.chkTraverseJunctions.Checked ? 1 : 0);
                AppCuKey.SetValue(_rvn_RecurseDirs, this.chkRecurse.Checked ? 1 : 0);
                AppCuKey.SetValue(_rvn_RemoveFiles, this.chkRemoveFiles.Checked ? 1 : 0);

                // the selection completion list
                var converted = _selectionCompletions.ToList().ConvertAll(z => z.XmlEscapeIexcl());
                string history = String.Join("¡", converted.ToArray());
                AppCuKey.SetValue(_rvn_SelectionCompletions, history);


                // store the size of the form
                int w = 0, h = 0, left = 0, top = 0;
                if (this.Bounds.Width < this.MinimumSize.Width || this.Bounds.Height < this.MinimumSize.Height)
                {
                    // RestoreBounds is the size of the window prior to last minimize action.
                    // But the form may have been resized since then!
                    w = this.RestoreBounds.Width;
                    h = this.RestoreBounds.Height;
                    left = this.RestoreBounds.Location.X;
                    top = this.RestoreBounds.Location.Y;
                }
                else
                {
                    w = this.Bounds.Width;
                    h = this.Bounds.Height;
                    left = this.Location.X;
                    top = this.Location.Y;
                }
                AppCuKey.SetValue(_rvn_Geometry,
                  String.Format("{0},{1},{2},{3},{4}",
                        left, top, w, h, (int)this.WindowState));

                AppCuKey.Close();
            }
        }


        private System.Drawing.Rectangle ConstrainToScreen(System.Drawing.Rectangle bounds)
        {
            Screen screen = Screen.FromRectangle(bounds);
            System.Drawing.Rectangle workingArea = screen.WorkingArea;
            int width = Math.Min(bounds.Width, workingArea.Width);
            int height = Math.Min(bounds.Height, workingArea.Height);
            // mmm....minimax
            int left = Math.Min(workingArea.Right - width, Math.Max(bounds.Left, workingArea.Left));
            int top = Math.Min(workingArea.Bottom - height, Math.Max(bounds.Top, workingArea.Top));
            return new System.Drawing.Rectangle(left, top, width, height);
        }



        public Microsoft.Win32.RegistryKey AppCuKey
        {
            get
            {
                if (_appCuKey == null)
                {
                    _appCuKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(_AppRegyPath, true);
                    if (_appCuKey == null)
                        _appCuKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(_AppRegyPath);
                }
                return _appCuKey;
            }
            set { _appCuKey = null; }
        }

        private int numRuns;

        private Microsoft.Win32.RegistryKey _appCuKey;
        private static string _AppRegyPath = "Software\\Dino Chiesa\\DotNetZip Winforms Tool";
        private static string _rvn_FormTab = "FormTab";
        private static string _rvn_Geometry = "Geometry";
        private static string _rvn_TraverseJunctions = "TraverseJunctions";
        private static string _rvn_RecurseDirs = "RecurseDirs";
        private static string _rvn_RemoveFiles = "RemoveFiles";
        private static string _rvn_HidePassword = "HidePassword";
        private static string _rvn_ExtractExistingFileAction = "ExtractExistingFileAction";
        private static string _rvn_OpenExplorer = "OpenExplorer";
        private static string _rvn_ExtractLoc = "ExtractLoc";
        private static string _rvn_DirectoryToZip = "DirectoryToZip";
        private static string _rvn_SelectionToZip = "SelectionToZip";
        private static string _rvn_SelectionToExtract = "SelectionToExtract";
        private static string _rvn_SelectionCompletions= "SelectionCompletions";
        private static string _rvn_ZipTarget = "ZipTarget";
        private static string _rvn_ZipToOpen = "ZipToOpen";
        private static string _rvn_Encoding = "Encoding";
        private static string _rvn_EncodingUsage = "EncodingUsage";
        private static string _rvn_CompLevel = "CompressionLevel";
        private static string _rvn_CompMethod = "CompressionMethod";
        private static string _rvn_Encryption = "Encryption";
        private static string _rvn_ZipFlavor = "ZipFlavor";
        private static string _rvn_Zip64Option = "Zip64Option";
        private static string _rvn_LastRun = "LastRun";
        private static string _rvn_Runs = "Runs";

        private readonly int _MaxMruListSize = 14;
        private System.Windows.Forms.AutoCompleteStringCollection _selectionCompletions;
        private bool stateLoaded;
    }
}