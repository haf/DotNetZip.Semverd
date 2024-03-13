' VbsCreateZip-ShellApp.vbs
' ------------------------------------------------------------------
'
' Copyright (c) 2009 Dino Chiesa and Microsoft Corporation.
' All rights reserved.
'
' This code module is part of DotNetZip, a zipfile class library.
'
' ------------------------------------------------------------------
'
' This code is licensed under the Microsoft Public License.
' See the file License.txt for the license details.
' More info on: http://dotnetzip.codeplex.com
'
' ------------------------------------------------------------------
'
' last saved (in emacs):
' Time-stamp: <2011-June-18 21:41:42>
'
' ------------------------------------------------------------------
'
' This is a script file that creates a zip file from a specified directory.
' It uses the Shell.Application object to do the zipping.
' This script is used for compatibility testing of the DotNetZip output.
'
' created Fri, 29 May 2009
'
' ------------------------------------------------------------------


Sub NewZip(pathToZipFile)

    WScript.Echo "Newing up a zip file (" & pathToZipFile & ") "

    Dim fso
    Set fso = CreateObject("Scripting.FileSystemObject")
    Dim file
    Set file = fso.CreateTextFile(pathToZipFile)

    file.Write Chr(80) & Chr(75) & Chr(5) & Chr(6) & String(18, 0)

    file.Close
    Set fso = Nothing
    Set file = Nothing

    WScript.Sleep 500

End Sub



Sub CreateZip(pathToZipFile, dirToZip)

    WScript.Echo "Creating zip  (" & pathToZipFile & ") from (" & dirToZip & ")"

    Dim fso
    Set fso= Wscript.CreateObject("Scripting.FileSystemObject")

    If fso.FileExists(pathToZipFile) Then
        WScript.Echo "That zip file already exists - deleting it."
        fso.DeleteFile pathToZipFile
    End If

    If Not fso.FolderExists(dirToZip) Then
        WScript.Echo "The directory to zip does not exist."
        Exit Sub
    End If

    NewZip pathToZipFile

    pathToZipFile = fso.GetAbsolutePathName(pathToZipFile)
    dirToZip = fso.GetAbsolutePathName(dirToZip)

    dim sa
    set sa = CreateObject("Shell.Application")

    Dim zip
    Set zip = sa.NameSpace(pathToZipFile)

    WScript.Echo "opening dir  (" & dirToZip & ")"

    Dim d
    Set d = sa.NameSpace(dirToZip)

    For Each s In d.items
        WScript.Echo  s
    Next


    ' http://msdn.microsoft.com/en-us/library/bb787866(VS.85).aspx
    ' ===============================================================
    ' 4 = do not display a progress box
    ' 16 = Respond with "Yes to All" for any dialog box that is displayed.
    ' 128 = Perform the operation on files only if a wildcard file name (*.*) is specified.
    ' 256 = Display a progress dialog box but do not show the file names.
    ' 2048 = Version 4.71. Do not copy the security attributes of the file.
    ' 4096 = Only operate in the local directory. Don't operate recursively into subdirectories.

    WScript.Echo "copying files..."

    zip.CopyHere d.items, 4


'     WScript.Echo "d.items.Count = " & d.items.Count
'     WScript.Echo "zip.items.Count = " & zip.items.Count

    sLoop = 0
    Do Until d.Items.Count <= zip.Items.Count
        Wscript.Sleep(1000)
'         sLoop = sLoop + 1
'         If (sLoop = 10) Then
'             WScript.Echo "/ items so far = " & zip.items.Count
'             WScript.Echo "(looking for " & d.items.Count & " items)"
'             sLoop = 0
'         End IF
    Loop

End Sub



Sub Main()

    dim args

    set args = WScript.Arguments

    If (args.Length = 2) Then

        CreateZip args(0), args(1)

    Else
        WScript.Echo "VbsCreatezip.vbs - create a zip file using the Shell.Application object."
        WScript.Echo "  usage: VbsCreatezip.vbs  <zipToCreate>  <dirToZip>"
        WScript.Echo "  "
    End If

End Sub

Call Main
