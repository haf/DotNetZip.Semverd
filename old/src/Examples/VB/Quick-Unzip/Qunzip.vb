'' qunzip.vb
'' ------------------------------------------------------------------
''
'' A simple app that unzips a file, showing a progress bar. 
'' It is both a console app, and a winforms app.
'' 
'' It correctly does the multi-threading to allow smooth UI update.
''
'' compile it with:
''      c:\.net3.5\vbc.exe /t:exe /debug:full /optimize- /R:System.dll /R:System.Data.dll  /R:Ionic.Zip.dll
''                                /out:Qunzip.exe Qunzip.vb
'' 
'' built on host: DINOCH-2
'' Created Thu Aug 06 15:34:17 2009
''
'' last saved: 
'' Time-stamp: <2009-October-26 23:03:36>
'' ------------------------------------------------------------------
''
'' Copyright (c) 2009 by Dino Chiesa
'' All rights reserved!
''
'' Licensed under the Microsoft Public License.
'' see http://www.opensource.org/licenses/ms-pl.html
''
'' ------------------------------------------------------------------

Imports System
Imports System.Reflection
Imports System.Windows.Forms
Imports Ionic.Zip


Namespace Ionic.Zip.Examples.VB

    Friend Class QuickUnzip
        <System.Runtime.InteropServices.DllImport("kernel32.dll")> _
        Private Shared Function AllocConsole() As Boolean
        End Function

        <System.Runtime.InteropServices.DllImport("kernel32.dll")> _
        Private Shared Function AttachConsole(ByVal pid As Integer) As Boolean
        End Function

        <STAThread> _
        Public Shared Sub Main(ByVal args As String()) 
            If (args.Length <> 2) Then
                '' open a new window so we can write to it.
                QuickUnzip.AllocConsole
                QuickUnzip.Usage(args)
                Return
            Else
                Application.EnableVisualStyles
                Application.SetCompatibleTextRenderingDefault(False)
                Dim f As New QuickUnzipForm(args(0), args(1))
                Application.Run(f)
            End If
            Return 
        End Sub

        Private Shared Function Usage(ByVal args As String()) as Integer
            Console.WriteLine("QuickUnzip.  Usage:  QuickUnzip <zipfile> <extractDirectory>")
            Console.WriteLine(System. Environment.NewLine & "<ENTER> to continue...")
            Console.ReadLine
            Return 1
        End Function

    End Class



    Public Class QuickUnzipForm
        Inherits Form

        Public Sub New()
            Me.components = Nothing
            Me.InitializeComponent
        End Sub

        Public Sub New(ByVal zipfile As String, ByVal directory As String)
            Me.New()
            Me.zipfileName = zipfile
            Me.extractDirectory = directory
        End Sub

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            If (disposing AndAlso (Not Me.components Is Nothing)) Then
                Me.components.Dispose
            End If
            MyBase.Dispose(disposing)
        End Sub

        Private Sub FixTitle()
            Me.Text = String.Format("Quick Unzip {0}", Me.zipfileName)
        End Sub

        Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container
            Me.label1 = New Label
            Me.progressBar1 = New ProgressBar
            MyBase.SuspendLayout
            Me.label1.AutoSize = True
            Me.label1.Location = New System.Drawing.Point(12, 12)
            Me.label1.Name = "label1"
            Me.label1.Size = New System.Drawing.Size(50, 13)
            Me.label1.TabIndex = 2
            Me.label1.Text = "Progress"
            Me.progressBar1.Anchor = (AnchorStyles.Right Or (AnchorStyles.Left Or AnchorStyles.Top))
            Me.progressBar1.Location = New System.Drawing.Point(12, 36)
            Me.progressBar1.Name = "progressBar1"
            Me.progressBar1.Size = New System.Drawing.Size(436, 18)
            Me.progressBar1.Step = 1
            Me.progressBar1.TabIndex = 7
            MyBase.AutoScaleDimensions = New System.Drawing.SizeF(6!, 13!)
            MyBase.AutoScaleMode = AutoScaleMode.Font
            MyBase.ClientSize = New System.Drawing.Size(460, 80)
            MyBase.Controls.Add(Me.label1)
            MyBase.Controls.Add(Me.progressBar1)
            MyBase.Name = "QuickUnzipForm"
            Me.Text = "QuickUnzip"
            AddHandler MyBase.Load, New EventHandler(AddressOf Me.QuickUnzipForm_Load)
            AddHandler MyBase.Shown, New EventHandler(AddressOf Me.QuickUnzipForm_Shown)
            MyBase.ResumeLayout(False)
            MyBase.PerformLayout
        End Sub

        Public Sub OnTimerEvent(ByVal source As Object, ByVal e As EventArgs)
            MyBase.Close
        End Sub

        Private Sub QuickUnzipForm_Load(ByVal sender As Object, ByVal e As EventArgs)
            Me.FixTitle
        End Sub

        Private Sub QuickUnzipForm_Shown(ByVal sender As Object, ByVal e As EventArgs)
            '' For info on running long-running tasks in response to button clicks,
            '' in  VB.NET WinForms, see
            '' http://msdn.microsoft.com/en-us/library/ms951089.aspx
            Dim args(2) As String
            args(0) = Me.zipfileName
            args(1) = Me.extractDirectory
            Dim worker As System.Threading.Thread
            worker = New System.Threading.Thread(New System.Threading.ParameterizedThreadStart(AddressOf UnzipFile))
            worker.Start(args)
        End Sub
        
        Private Sub UnzipFile(ByVal args As String())
            Try 
                Using zip As ZipFile = ZipFile.Read(args(0))
                    Me.progressBar1.Maximum = zip.Entries.Count
                    Dim entry As ZipEntry
                    For Each entry In zip
                        UpdateUi(entry.FileName)
                        entry.Extract(args(1), ExtractExistingFileAction.OverwriteSilently)
                        '' Sleep a little because it's really fast
                        System.Threading.Thread.Sleep(20)
                    Next
                    UpdateUi(String.Format("Finished unzipping {0} entries", zip.Entries.Count))
                End Using
            Catch ex1 As Exception
                Me.label1.Text = ("Exception: " & ex1.ToString)
            End Try
            '' close the form 1000 ms after the unzip completes.
            Dim timer1 As New System.Timers.Timer(1000)
            timer1.Enabled = True
            timer1.AutoReset = False
            AddHandler timer1.Elapsed, New System.Timers.ElapsedEventHandler(AddressOf Me.OnTimerEvent)
        End Sub

        Private Sub UpdateUi(ByVal filename As String)
            If Me.InvokeRequired  Then
                '' invoke on the proper thread 
                Me.Invoke(New Action(Of String)(AddressOf UpdateUi),  New Object() { filename })
            Else
                Me.label1.Text = filename
                Me.progressBar1.PerformStep
                MyBase.Update
            End If
        End Sub
        
        ' Fields
        Private extractDirectory As String
        Private zipfileName As String
        Private components As System.ComponentModel.IContainer
        Private label1 As Label
        Private progressBar1 As ProgressBar
    End Class

End Namespace
