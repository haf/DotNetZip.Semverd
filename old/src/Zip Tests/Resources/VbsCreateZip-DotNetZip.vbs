' VbsCreateZip-DotNetZip.vbs
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
' Time-stamp: <2009-May-30 06:23:21>
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

Dim filename
Dim dirToZip
Dim password
Dim extractLocation



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

    WScript.echo("")
    WScript.echo("Instantiating a ZipFile Object...")
    
    Dim zip1 
    Set zip1 = CreateObject("Ionic.Zip.ZipFile")

    If Not (password = "") Then
        WScript.echo("using AES256 encryption...")
        zip1.Encryption = 3

        WScript.echo("setting the password...")
        zip1.Password = password
    End IF

    WScript.echo("adding a directory...")
    zip1.AddDirectory(dirToZip)

    WScript.echo("setting the save name to (" & pathToZipFile & ")...")
    zip1.Name = pathToZipFile

    WScript.echo("Saving...")
    zip1.Save()

    WScript.echo("Disposing...")
    zip1.Dispose()
    
End Sub


Sub Main()
    
    dim args

    set args = WScript.Arguments 

    If (args.Length < 2) Then
        
        WScript.Echo "VbsCreatezip.vbs - create a zip file using the DotNetZip COM object."
        WScript.Echo "  usage: VbsCreatezip.vbs  <zipToCreate>  <dirToZip>"
        Exit Sub
    End If


    password = ""
    
    If (args.Count > 2) Then
        For i = 2 To args.Count-1
            If (args(i) = "-p") Then
                i= i+1
                password= args(i+1)
            End If
        Next
    End If

    CreateZip args(0), args(1)

End Sub

Call Main
