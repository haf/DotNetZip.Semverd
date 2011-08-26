Imports Ionic.Zip
Imports System.Linq
Imports System.IO

Public Class Form1

    Private _appCuKey As Microsoft.Win32.RegistryKey
    Private AppRegyPath As String = "Software\Ionic\VBzipTreeView"
    Private rvn_ZipFile As String = "zipfile"

    Private Sub Button1_Click(ByVal sender As System.Object, _
                              ByVal e As System.EventArgs) Handles Button1.Click
        PopulateTreeView()
    End Sub

    Private zip As Ionic.Zip.ZipFile

    ''' <summary>
    ''' Populates TreeView1 with the entries in the zipfile, named by TextBox1
    ''' </summary>
    Private Sub PopulateTreeView()
        Try
            zip = ZipFile.Read(Me.tbZipToOpen.Text)

            Me.TreeView1.Nodes.Clear()
            For Each e As ZipEntry In zip
                AddTreeNode(e.FileName)
            Next

        Catch ex As Exception
            '' eg, file does not exist, or access denied, etc
            MessageBox.Show("Exception: " + ex.ToString(), _
                            "Exception during zip processing", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Exclamation)

        Finally
            If Not (zip Is Nothing) Then
                zip.Dispose()
            End If

        End Try
    End Sub

    ''' <summary>
    ''' Add a node to the Treeview, for the given ZipEntry name
    ''' </summary>
    ''' <param name="name">name of the ZipEntry</param>
    ''' <returns>the TreeNode added</returns>
    ''' <remarks>
    ''' <para>
    ''' Entries in a zip file exist in a "flat container space": there is a single container, 
    ''' the zip file itself, that contains all entries. Even though each entry has a filename 
    ''' attached to it, and that filename may include a hierarchial directory path, the entry is 
    ''' always contained in the zipfile itself.  Even though it is possible to include directory 
    ''' entries in a zip file, those directory entries are not, themselves, containers - they do 
    ''' not contain other zip entries.  
    ''' </para>
    ''' <para>
    ''' This method overlays the zipentry, which exists only in a flat namespace, into a hierarchial 
    ''' tree, creating that tree from the pathname on the entry.  If the entry name is /a/b/c.txt,  
    ''' then this method adds 3 nodes to the TreeView, one for each segment in the path. 
    ''' </para>
    ''' <para>
    ''' The method is smart enough to find existing nodes matching subsegements of the path
    ''' for an entry. 
    ''' </para>
    ''' </remarks>
    Private Function AddTreeNode(ByVal name As String) As TreeNode
        If (name.EndsWith("/")) Then
            name = name.Substring(0, name.Length - 1)
        End If
        Dim node As TreeNode = FindNodeForTag(name, Me.TreeView1.Nodes)
        If Not (node Is Nothing) Then
            Return node
        End If
        Dim pnodeCollection As TreeNodeCollection
        Dim parent As String = Path.GetDirectoryName(name)
        If (parent = "") Then
            pnodeCollection = Me.TreeView1.Nodes
        Else
            pnodeCollection = AddTreeNode(parent.Replace("\", "/")).Nodes
        End If
        node = New TreeNode
        node.Text = Path.GetFileName(name)
        node.Tag = name ' full path
        pnodeCollection.Add(node)
        Return node
    End Function

    ''' <summary>
    ''' Returns the TreeNode for a given name 
    ''' </summary>
    ''' <param name="name">name of the ZipEntry</param>
    ''' <param name="nodes">The TreeNodeCollection to search</param>
    ''' <returns>the matching TreeNode, or nothing if none exists</returns>
    ''' <remarks>
    ''' This method is used by AddTreeNode() to find existing nodes.  
    ''' </remarks>
    Private Function FindNodeForTag(ByVal name As String, ByRef nodes As TreeNodeCollection) As TreeNode
        For Each node As TreeNode In nodes
            If (name = node.Tag) Then
                Return node
            ElseIf (name.StartsWith(node.Tag + "/")) Then
                Return FindNodeForTag(name, node.Nodes)
            End If
        Next
        Return Nothing
    End Function

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Dim openFileDialog1 As OpenFileDialog = New OpenFileDialog

        openFileDialog1.InitialDirectory = Me.tbZipToOpen.Text
        openFileDialog1.Filter = "zip files|*.zip|EXE files|*.exe|All Files|*.*"
        openFileDialog1.FilterIndex = 1
        openFileDialog1.RestoreDirectory = True

        If (openFileDialog1.ShowDialog() = DialogResult.OK) Then
            Me.tbZipToOpen.Text = openFileDialog1.FileName
            If (System.IO.File.Exists(Me.tbZipToOpen.Text)) Then
                Button1_Click(sender, e)
            End If
        End If

    End Sub

    Private Sub TreeView1_DoubleClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TreeView1.DoubleClick
        If (e Is Nothing) Then
            '' 
        End If
    End Sub


    Private Sub SaveFormToRegistry()
        If AppCuKey IsNot Nothing Then
            If Not String.IsNullOrEmpty(Me.tbZipToOpen.Text) Then
                AppCuKey.SetValue(rvn_ZipFile, Me.tbZipToOpen.Text)
            End If
        End If
    End Sub

    Private Sub LoadFormFromRegistry()
        If AppCuKey IsNot Nothing Then
            Dim s As String
            s = AppCuKey.GetValue(rvn_ZipFile)
            If Not String.IsNullOrEmpty(s) Then
                Me.tbZipToOpen.Text = s
            End If
        End If
    End Sub


    Public ReadOnly Property AppCuKey() As Microsoft.Win32.RegistryKey
        Get
            If (_appCuKey Is Nothing) Then
                Me._appCuKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(AppRegyPath, True)
                If (Me._appCuKey Is Nothing) Then
                    Me._appCuKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(AppRegyPath)
                End If
            End If
            Return _appCuKey
        End Get
    End Property

    Private Sub Form1_FormClosing(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        SaveFormToRegistry()
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        LoadFormFromRegistry()
    End Sub
    
End Class
