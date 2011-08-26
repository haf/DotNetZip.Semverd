using System;
using System.IO;
using System.Windows.Forms;
using Ionic.Zip;

namespace SmartDeviceProject3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            CustomInit();
        }

        #region Helpers
        private void CustomInit()
        {
            //add folder images to imagelist
            this.imageList1.Images.Add(new System.Drawing.Bitmap(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Ionic.Examples.Smartphone.Zip.Images.Folder.gif")));
            this.imageList1.Images.Add(new System.Drawing.Bitmap(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Ionic.Examples.Smartphone.Zip.Images.Device.gif")));
            this.imageList1.Images.Add(new System.Drawing.Bitmap(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Ionic.Examples.Smartphone.Zip.Images.Card.gif")));

            //set default image index to 0
            tvFolders.ImageIndex = 0;
            tvFolders.SelectedImageIndex = 0;

            //use a backslash between path levels
            tvFolders.PathSeparator = "\\";

            //setup default tree
            Reset();
        }

        public void Reset()
        {
            //restores dialog to initial settings and repopulates folders
            this._selectedpath = "";
            tvFolders.Nodes.Clear();

            //add the root (device) node to the tree
            AddRoot();

            //add root level subfolders
            AddChildren(tvFolders.Nodes[0]);

            //expand the top level folders
            tvFolders.Nodes[0].Expand();
        }


        private void AddRoot()
        {
            //stop updates during filling
            tvFolders.BeginUpdate();

            //add an empty node (use device image)
            TreeNode root = tvFolders.Nodes.Add("");
            root.ImageIndex = 1;
            root.SelectedImageIndex = 1;

            tvFolders.EndUpdate();
        }



        /// <summary>
        /// Adds subfolders to a specified node in the tree
        /// </summary>
        /// <param name="tn">Node to add sub-folders to</param>
        private void AddChildren(TreeNode tn)
        {
            //stop updates during filling
            tvFolders.BeginUpdate();

            //path to query for subfolders
            string path = (tn.FullPath == "")
                ? "\\"
                : tn.FullPath;

            //clear any existing subnodes 
            tn.Nodes.Clear();

            //get all folders beneath the selected node
            foreach (string directory in System.IO.Directory.GetDirectories(path))
            {
                TreeNode node = new TreeNode();
                //format human friendly name
                node.Text = directory.Substring(directory.LastIndexOf("\\") + 1, directory.Length - directory.LastIndexOf("\\") - 1);

                //change icon if folder is a storage card
                DirectoryInfo di = new DirectoryInfo(directory);
                if ((di.Attributes & FileAttributes.Temporary) == FileAttributes.Temporary)
                {
                    node.ImageIndex = 2;
                    node.SelectedImageIndex = 2;
                }

                //add to root of tree
                tn.Nodes.Add(node);
            }

            //get all files beneath the selected node
            foreach (string zipfilename in System.IO.Directory.GetFiles(path))
            {
                TreeNode node = new TreeNode();
                //format human friendly name
                node.Text = System.IO.Path.GetFileName(zipfilename);
                //node.Text = zipfile.Substring(directory.LastIndexOf("\\") + 1, directory.Length - directory.LastIndexOf("\\") - 1);

                //add to root of tree
                tn.Nodes.Add(node);
            }

            //restore events etc
            tvFolders.EndUpdate();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the path selected by the user.
        /// </summary>
        public string SelectedPath
        {
            get
            {
                return _selectedpath;
            }
            set
            {
                //check that value passed is a valid file system path
                if (Directory.Exists(value))
                {
                    _selectedpath = value;

                    //split the path into each folder layer
                    string[] layers = value.Split('\\');

                    //mark the current node
                    TreeNode tn = tvFolders.Nodes[0];

                    //loop through the folder levels
                    foreach (string folderlevel in layers)
                    {
                        //ignore blank (top-level node)
                        if (folderlevel != "")
                        {
                            //add sub-folders
                            AddChildren(tn);

                            //find the required subfolder
                            foreach (TreeNode thisnode in tn.Nodes)
                            {
                                //check node text
                                if (thisnode.Text == folderlevel)
                                {
                                    //if it does set it and continue processing the next level
                                    tn = thisnode;
                                    break;
                                }
                            }
                        }
                    }

                    //select the final node
                    tvFolders.SelectedNode = tn;

                }
            }
        }
        #endregion

        #region Private Fields
        private string _selectedpath;
        #endregion

        #region Events
        private void tvFolders_AfterSelect(object sender, TreeViewEventArgs e)
        {
            //set the currently selected folder/file
            _selectedpath = (tvFolders.SelectedNode.FullPath == "")
                ? "\\"
                : tvFolders.SelectedNode.FullPath;

            if (System.IO.Directory.Exists(_selectedpath))
            {
                menuItemUnzip.Enabled = false;
                //if there are no child nodes we will check for any and add them
                if (tvFolders.SelectedNode.Nodes.Count == 0)
                {
                    AddChildren(tvFolders.SelectedNode);
                }
            }
            else if (Ionic.Zip.ZipFile.IsZipFile(_selectedpath))
            {
                menuItemUnzip.Enabled = true;
            }
        }


        private void menuItemUnzip_Click(object sender, EventArgs e)
        {
            if (System.IO.File.Exists(_selectedpath))
            {
                UnzipSelectedFile();
            }
        }


        private void UnzipSelectedFile()
        {
            string parent = System.IO.Path.GetDirectoryName(_selectedpath);
            string dir = System.IO.Path.Combine(parent,
                System.IO.Path.GetFileNameWithoutExtension(_selectedpath));
            try
            {
                using (var zip1 = Ionic.Zip.ZipFile.Read(_selectedpath))
                {
                    foreach (var entry in zip1)
                    {
                        entry.Extract(dir, ExtractExistingFileAction.OverwriteSilently);
                    }
                }

                // re-populate the treeview with the extracted files:
                AddChildren(tvFolders.SelectedNode.Parent);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        private void menuItemDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (System.IO.File.Exists(_selectedpath))
                {
                    System.IO.File.Delete(_selectedpath);
                }
                else if (System.IO.Directory.Exists(_selectedpath))
                {
                    System.IO.Directory.Delete(_selectedpath, true);
                }

                // refresh the treeview 
                AddChildren(tvFolders.SelectedNode.Parent);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void menuItemQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void menuItemAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("DotNetZip CF Unzipper.  " + 
                "This example shows how to use the DotNetZip library in a .NET Compact Framework application. " +
            "Built in VS2008 in December 2008.",
                "About Unzipper",
                MessageBoxButtons.OK,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
        }

        #endregion
    }
}