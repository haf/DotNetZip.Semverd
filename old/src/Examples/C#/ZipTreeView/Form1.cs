using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Ionic.Zip;

namespace ZipTreeView
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.textBox1.BackColor = System.Drawing.Color.White;
            string txt = this.textBox1.Text;
            try
            {
                this.treeView1.Nodes.Clear();
                using (var zip = ZipFile.Read(txt))
                {
                    foreach (var entry in zip)
                    {
                        AddTreeNode(entry.FileName);
                    }
                }
            }
            catch
            {
                this.textBox1.BackColor = System.Drawing.Color.MistyRose;
                MessageBox.Show("Exception reading that zip file.");
            }
        }

        private TreeNode AddTreeNode(string name)
        {
            if (name.EndsWith("/"))
                name = name.Substring(0, name.Length - 1);

            TreeNode node = FindNodeForTag(name, this.treeView1.Nodes);
            if (node != null)
                return node;
            String parent = Path.GetDirectoryName(name);
            TreeNodeCollection pnodeCollection = (parent == "")
                ? this.treeView1.Nodes
                : AddTreeNode(parent.Replace("\\", "/")).Nodes;

            node = new TreeNode()
            {
                Text = Path.GetFileName(name),
                Tag = name // ' full path
            };
            pnodeCollection.Add(node);
            return node;
        }

        // Returns the TreeNode for a given name 
        private TreeNode FindNodeForTag(string name, TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                if (name == (string) node.Tag)
                    return node;
                else if (name.StartsWith(node.Tag + "/"))
                    return FindNodeForTag(name, node.Nodes);
            }
            return null;
        }

    }
}
