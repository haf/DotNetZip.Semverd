Imports System.IO
Imports Ionic.Zip
Imports System.Threading
Imports System.ComponentModel

Public Class Form1
    
    Private _backgroundWorker1 As System.ComponentModel.BackgroundWorker
    Private _saveCanceled As Boolean
    Private _totalBytesAfterCompress As Long
    Private _totalBytesBeforeCompress As Long
    Private _nFilesCompleted As Integer
    Private _progress2MaxFactor As Integer
    Private _entriesToZip As Integer
    Private _appCuKey As Microsoft.Win32.RegistryKey
    Private AppRegyPath As String = "Software\Ionic\VBzipUp"
    Private rvn_ZipFile As String = "zipfile"
    Private rvn_DirToZip As String = "dirToZip"

    ' Delegates for invocation of UI from other threads
    Private Delegate Sub SaveEntryProgress(ByVal e As SaveProgressEventArgs)
    Private Delegate Sub ButtonClick(ByVal sender As Object, ByVal e As EventArgs)
    
    Private Sub btnDirBrowse_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDirBrowse.Click
        Dim folderName As String = Me.tbDirToZip.Text
        Dim dlg1 As New FolderBrowserDialog

        dlg1.SelectedPath = IIf(Directory.Exists(folderName), folderName, "c:\")
        dlg1.ShowNewFolderButton = False
        If (dlg1.ShowDialog = DialogResult.OK) Then
            'Me._folderName = dlg1.get_SelectedPath
            Me.tbDirToZip.Text = folderName
        End If
    End Sub


    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnZipUp.Click
        Me.KickoffZipup()
    End Sub


    Private Sub KickoffZipup()
        Dim folderName As String = Me.tbDirToZip.Text
        If (((Not folderName Is Nothing) AndAlso (folderName <> "")) AndAlso ((Not Me.tbZipToCreate.Text Is Nothing) AndAlso (Me.tbZipToCreate.Text <> ""))) Then
            If File.Exists(Me.tbZipToCreate.Text) Then
                If (MessageBox.Show(String.Format("The file you have specified ({0}) already exists.  Do you want to overwrite this file?", _
                                                  Me.tbZipToCreate.Text), "Confirmation is Required", _
                                                  MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes) Then
                    Return
                End If
                File.Delete(Me.tbZipToCreate.Text)
            End If
            Me._saveCanceled = False
            Me._nFilesCompleted = 0
            Me._totalBytesAfterCompress = 0
            Me._totalBytesBeforeCompress = 0
            Me.btnZipUp.Enabled = False
            Me.btnZipUp.Text = "Zipping..."
            Me.btnCancel.Enabled = True
            Me.lblStatus.Text = "Zipping..."
            Dim options As New WorkerOptions
            options.ZipName = Me.tbZipToCreate.Text
            options.Folder = folderName
            
            _backgroundWorker1 = New System.ComponentModel.BackgroundWorker()
            _backgroundWorker1.WorkerSupportsCancellation = False
            _backgroundWorker1.WorkerReportsProgress = False
            AddHandler Me._backgroundWorker1.DoWork, New DoWorkEventHandler(AddressOf Me.DoSave)
            _backgroundWorker1.RunWorkerAsync(options)
            
        End If
    End Sub

    
    Private Sub DoSave(ByVal sender As Object, ByVal e As DoWorkEventArgs)
        Dim options As WorkerOptions = e.Argument
        Try
            Using zip1 As ZipFile = New ZipFile
                zip1.AddDirectory(options.Folder)
                Me._entriesToZip = zip1.EntryFileNames.Count
                Me.SetProgressBars()
                AddHandler zip1.SaveProgress, New EventHandler(Of SaveProgressEventArgs)(AddressOf Me.zip1_SaveProgress)
                zip1.Save(options.ZipName)
            End Using
        Catch exc1 As Exception
            MessageBox.Show(String.Format("Exception while zipping: {0}", exc1.Message))
            Me.btnCancel_Click(Nothing, Nothing)
        End Try
    End Sub

    
    Private Sub zip1_SaveProgress(ByVal sender As Object, ByVal e As SaveProgressEventArgs)
        If Me._saveCanceled Then
            e.Cancel = True
            Return
        End If
        
        Select Case e.EventType
            Case ZipProgressEventType.Saving_AfterWriteEntry
                Me.StepArchiveProgress(e)
                Exit Select
            Case ZipProgressEventType.Saving_Completed
                Me.SaveCompleted()
                Exit Select
            Case ZipProgressEventType.Saving_EntryBytesRead
                Me.StepEntryProgress(e)
                Exit Select
        End Select
    End Sub



    Private Sub StepArchiveProgress(ByVal e As SaveProgressEventArgs)
        If Me.progressBar1.InvokeRequired Then
            Me.progressBar1.Invoke(New SaveEntryProgress(AddressOf Me.StepArchiveProgress), New Object() {e})
        ElseIf Not Me._saveCanceled Then
            Me._nFilesCompleted += 1
            Me.progressBar1.PerformStep()
            Me._totalBytesAfterCompress = (Me._totalBytesAfterCompress + e.CurrentEntry.CompressedSize)
            Me._totalBytesBeforeCompress = (Me._totalBytesBeforeCompress + e.CurrentEntry.UncompressedSize)
            ' progressBar2 is the one dealing with the item being added to the archive
            ' if we got this event, then the add of that item (or file) is complete, so we 
            ' update the progressBar2 appropriately.
            Me.progressBar2.Value = Me.progressBar2.Maximum = 1
            MyBase.Update()
        End If
    End Sub


    Private Sub SaveCompleted()
        If Me.lblStatus.InvokeRequired Then
            Me.lblStatus.Invoke(New MethodInvoker(AddressOf SaveCompleted))
            'Me.lblStatus.Invoke(New MethodInvoker(Me, DirectCast(Me.SaveCompleted, IntPtr)))
        Else
            Me.lblStatus.Text = String.Format("Done, Compressed {0} files, {1:N0}% of original", Me._nFilesCompleted, ((100 * Me._totalBytesAfterCompress) / CDbl(Me._totalBytesBeforeCompress)))
            Me.ResetState()
        End If
    End Sub


    Private Sub StepEntryProgress(ByVal e As SaveProgressEventArgs)
        If Me.progressBar2.InvokeRequired Then
            Me.progressBar2.Invoke(New SaveEntryProgress(AddressOf Me.StepEntryProgress), New Object() {e})
        ElseIf Not Me._saveCanceled Then
            If (Me.progressBar2.Maximum = 1) Then
                Dim entryMax As Long = e.TotalBytesToTransfer
                Dim absoluteMax As Long = &H7FFFFFFF
                Me._progress2MaxFactor = 0
                Do While (entryMax > absoluteMax)
                    entryMax = (entryMax / 2)
                    Me._progress2MaxFactor += 1
                Loop
                If (CInt(entryMax) < 0) Then
                    entryMax = (entryMax * -1)
                End If
                Me.progressBar2.Maximum = CInt(entryMax)
                Me.lblStatus.Text = String.Format("{0} of {1} files...({2})", (Me._nFilesCompleted + 1), Me._entriesToZip, e.CurrentEntry.FileName)
            End If
            Dim xferred As Integer = CInt((e.BytesTransferred >> Me._progress2MaxFactor))
            Me.progressBar2.Value = IIf((xferred >= Me.progressBar2.Maximum), Me.progressBar2.Maximum, xferred)
            MyBase.Update()
        End If
    End Sub


    Private Sub ResetState()
        Me.btnCancel.Enabled = False
        Me.btnZipUp.Enabled = True
        Me.btnZipUp.Text = "Zip it!"
        Me.progressBar1.Value = 0
        Me.progressBar2.Value = 0
        Me.Cursor = Cursors.Default
    End Sub


    
    Private Sub SetProgressBars()
        If Me.ProgressBar1.InvokeRequired Then
            'Me.ProgressBar1.Invoke(New MethodInvoker(Me, DirectCast(Me.SetProgressBars, IntPtr)))
            Me.ProgressBar1.Invoke(New MethodInvoker(AddressOf SetProgressBars))
        Else
            Me.ProgressBar1.Value = 0
            Me.ProgressBar1.Maximum = Me._entriesToZip
            Me.ProgressBar1.Minimum = 0
            Me.ProgressBar1.Step = 1
            Me.ProgressBar2.Value = 0
            Me.ProgressBar2.Minimum = 0
            Me.ProgressBar2.Maximum = 1
            Me.ProgressBar2.Step = 2
        End If
    End Sub


    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        If Me.lblStatus.InvokeRequired Then
            Me.lblStatus.Invoke(New ButtonClick(AddressOf Me.btnCancel_Click), New Object() {sender, e})
        Else
            Me._saveCanceled = True
            Me.lblStatus.Text = "Canceled..."
            Me.ResetState()
        End If
    End Sub


    Private Sub SaveFormToRegistry()
        If AppCuKey IsNot Nothing Then
            If Not String.IsNullOrEmpty(tbZipToCreate.Text) Then
                AppCuKey.SetValue(rvn_ZipFile, Me.tbZipToCreate.Text)
            End If
            If Not String.IsNullOrEmpty(tbDirToZip.Text) Then
                AppCuKey.SetValue(rvn_DirToZip, tbDirToZip.Text)
            End If
        End If
    End Sub

    Private Sub LoadFormFromRegistry()
        If AppCuKey IsNot Nothing Then
            Dim s As String
            s = AppCuKey.GetValue(rvn_ZipFile)
            If Not String.IsNullOrEmpty(s) Then
                Me.tbZipToCreate.Text = s
            End If
            s = AppCuKey.GetValue(rvn_DirToZip)
            If Not String.IsNullOrEmpty(s) Then
                tbDirToZip.Text = s
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

Public Class WorkerOptions
    ' Fields
    'Public Comment As String
    'Public CompressionLevel As CompressionLevel
    'Public Encoding As String
    'Public Encryption As EncryptionAlgorithm
    Public Folder As String
    'Public Password As String
    'Public Zip64 As Zip64Option
    'Public ZipFlavor As Integer
    Public ZipName As String
End Class


