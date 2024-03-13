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
        Me.Label1 = New System.Windows.Forms.Label
        Me.Label2 = New System.Windows.Forms.Label
        Me.tbDirToZip = New System.Windows.Forms.TextBox
        Me.tbZipToCreate = New System.Windows.Forms.TextBox
        Me.btnDirBrowse = New System.Windows.Forms.Button
        Me.btnZipUp = New System.Windows.Forms.Button
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar
        Me.ProgressBar2 = New System.Windows.Forms.ProgressBar
        Me.lblStatus = New System.Windows.Forms.Label
        Me.btnCancel = New System.Windows.Forms.Button
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(11, 13)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(78, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "directory to zip:"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(11, 39)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(84, 13)
        Me.Label2.TabIndex = 1
        Me.Label2.Text = "zip file to create:"
        '
        'tbDirToZip
        '
        Me.tbDirToZip.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.tbDirToZip.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest
        Me.tbDirToZip.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories
        Me.tbDirToZip.Location = New System.Drawing.Point(107, 9)
        Me.tbDirToZip.Name = "tbDirToZip"
        Me.tbDirToZip.Size = New System.Drawing.Size(327, 20)
        Me.tbDirToZip.TabIndex = 2
        '
        'tbZipToCreate
        '
        Me.tbZipToCreate.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.tbZipToCreate.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest
        Me.tbZipToCreate.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem
        Me.tbZipToCreate.Location = New System.Drawing.Point(106, 35)
        Me.tbZipToCreate.Name = "tbZipToCreate"
        Me.tbZipToCreate.Size = New System.Drawing.Size(327, 20)
        Me.tbZipToCreate.TabIndex = 6
        '
        'btnDirBrowse
        '
        Me.btnDirBrowse.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnDirBrowse.Location = New System.Drawing.Point(440, 10)
        Me.btnDirBrowse.Name = "btnDirBrowse"
        Me.btnDirBrowse.Size = New System.Drawing.Size(32, 19)
        Me.btnDirBrowse.TabIndex = 4
        Me.btnDirBrowse.Text = "..."
        Me.btnDirBrowse.UseVisualStyleBackColor = True
        '
        'btnZipUp
        '
        Me.btnZipUp.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnZipUp.Location = New System.Drawing.Point(377, 72)
        Me.btnZipUp.Name = "btnZipUp"
        Me.btnZipUp.Size = New System.Drawing.Size(94, 23)
        Me.btnZipUp.TabIndex = 0
        Me.btnZipUp.Text = "Zip It!"
        Me.btnZipUp.UseVisualStyleBackColor = True
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ProgressBar1.Location = New System.Drawing.Point(12, 105)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(459, 13)
        Me.ProgressBar1.TabIndex = 6
        '
        'ProgressBar2
        '
        Me.ProgressBar2.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ProgressBar2.Location = New System.Drawing.Point(13, 124)
        Me.ProgressBar2.Name = "ProgressBar2"
        Me.ProgressBar2.Size = New System.Drawing.Size(459, 13)
        Me.ProgressBar2.TabIndex = 7
        '
        'lblStatus
        '
        Me.lblStatus.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblStatus.AutoSize = True
        Me.lblStatus.Location = New System.Drawing.Point(11, 148)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(16, 13)
        Me.lblStatus.TabIndex = 8
        Me.lblStatus.Text = "..."
        '
        'btnCancel
        '
        Me.btnCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnCancel.Enabled = False
        Me.btnCancel.Location = New System.Drawing.Point(277, 72)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(94, 23)
        Me.btnCancel.TabIndex = 8
        Me.btnCancel.Text = "Cancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(484, 172)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.lblStatus)
        Me.Controls.Add(Me.ProgressBar2)
        Me.Controls.Add(Me.ProgressBar1)
        Me.Controls.Add(Me.btnZipUp)
        Me.Controls.Add(Me.btnDirBrowse)
        Me.Controls.Add(Me.tbZipToCreate)
        Me.Controls.Add(Me.tbDirToZip)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.MinimumSize = New System.Drawing.Size(500, 208)
        Me.Name = "Form1"
        Me.Text = "DotNetZip WinForms VB Zip Creator"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents tbDirToZip As System.Windows.Forms.TextBox
    Friend WithEvents tbZipToCreate As System.Windows.Forms.TextBox
    Friend WithEvents btnDirBrowse As System.Windows.Forms.Button
    Friend WithEvents btnZipUp As System.Windows.Forms.Button
    Friend WithEvents ProgressBar1 As System.Windows.Forms.ProgressBar
    Friend WithEvents ProgressBar2 As System.Windows.Forms.ProgressBar
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents btnCancel As System.Windows.Forms.Button

End Class
