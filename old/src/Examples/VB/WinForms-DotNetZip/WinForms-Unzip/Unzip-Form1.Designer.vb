<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.tbZipToOpen = New System.Windows.Forms.TextBox
        Me.Label1 = New System.Windows.Forms.Label
        Me.btnZipBrowse = New System.Windows.Forms.Button
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar
        Me.btnUnzip = New System.Windows.Forms.Button
        Me.lblStatus = New System.Windows.Forms.Label
        Me.tbExtractDir = New System.Windows.Forms.TextBox
        Me.Label2 = New System.Windows.Forms.Label
        Me.btnExtractDirBrowse = New System.Windows.Forms.Button
        Me.btnCancel = New System.Windows.Forms.Button
        Me.SuspendLayout()
        '
        'tbZipToOpen
        '
        Me.tbZipToOpen.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.tbZipToOpen.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest
        Me.tbZipToOpen.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem
        Me.tbZipToOpen.Location = New System.Drawing.Point(15, 23)
        Me.tbZipToOpen.Name = "tbZipToOpen"
        Me.tbZipToOpen.Size = New System.Drawing.Size(389, 20)
        Me.tbZipToOpen.TabIndex = 5
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 6)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(62, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Unzip a file:"
        '
        'btnZipBrowse
        '
        Me.btnZipBrowse.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnZipBrowse.Location = New System.Drawing.Point(410, 22)
        Me.btnZipBrowse.Name = "btnZipBrowse"
        Me.btnZipBrowse.Size = New System.Drawing.Size(30, 23)
        Me.btnZipBrowse.TabIndex = 7
        Me.btnZipBrowse.Text = "..."
        Me.btnZipBrowse.UseVisualStyleBackColor = True
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ProgressBar1.Location = New System.Drawing.Point(12, 133)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(428, 15)
        Me.ProgressBar1.TabIndex = 3
        '
        'btnUnzip
        '
        Me.btnUnzip.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnUnzip.Location = New System.Drawing.Point(365, 104)
        Me.btnUnzip.Name = "btnUnzip"
        Me.btnUnzip.Size = New System.Drawing.Size(75, 23)
        Me.btnUnzip.TabIndex = 0
        Me.btnUnzip.Text = "Unzip"
        Me.btnUnzip.UseVisualStyleBackColor = True
        '
        'lblStatus
        '
        Me.lblStatus.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblStatus.AutoSize = True
        Me.lblStatus.Location = New System.Drawing.Point(12, 179)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(16, 13)
        Me.lblStatus.TabIndex = 5
        Me.lblStatus.Text = "..."
        '
        'tbExtractDir
        '
        Me.tbExtractDir.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.tbExtractDir.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest
        Me.tbExtractDir.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories
        Me.tbExtractDir.Location = New System.Drawing.Point(15, 68)
        Me.tbExtractDir.Name = "tbExtractDir"
        Me.tbExtractDir.Size = New System.Drawing.Size(389, 20)
        Me.tbExtractDir.TabIndex = 10
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(12, 52)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(66, 13)
        Me.Label2.TabIndex = 7
        Me.Label2.Text = "To directory:"
        '
        'btnExtractDirBrowse
        '
        Me.btnExtractDirBrowse.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnExtractDirBrowse.Location = New System.Drawing.Point(410, 67)
        Me.btnExtractDirBrowse.Name = "btnExtractDirBrowse"
        Me.btnExtractDirBrowse.Size = New System.Drawing.Size(30, 23)
        Me.btnExtractDirBrowse.TabIndex = 12
        Me.btnExtractDirBrowse.Text = "..."
        Me.btnExtractDirBrowse.UseVisualStyleBackColor = True
        '
        'btnCancel
        '
        Me.btnCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnCancel.Enabled = False
        Me.btnCancel.Location = New System.Drawing.Point(365, 154)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(75, 23)
        Me.btnCancel.TabIndex = 20
        Me.btnCancel.TabStop = False
        Me.btnCancel.Text = "Cancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(452, 198)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnExtractDirBrowse)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.tbExtractDir)
        Me.Controls.Add(Me.lblStatus)
        Me.Controls.Add(Me.btnUnzip)
        Me.Controls.Add(Me.ProgressBar1)
        Me.Controls.Add(Me.btnZipBrowse)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.tbZipToOpen)
        Me.Name = "Form1"
        Me.Text = "DotNetZip Simple Unzip"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents tbZipToOpen As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents btnZipBrowse As System.Windows.Forms.Button
    Friend WithEvents ProgressBar1 As System.Windows.Forms.ProgressBar
    Friend WithEvents btnUnzip As System.Windows.Forms.Button
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents tbExtractDir As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents btnExtractDirBrowse As System.Windows.Forms.Button
    Friend WithEvents btnCancel As System.Windows.Forms.Button

End Class
