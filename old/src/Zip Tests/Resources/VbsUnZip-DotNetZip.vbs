' VbsUnzip-DotNetZip.vbs
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
' Time-stamp: <2009-May-30 07:08:28>
'
' ------------------------------------------------------------------
'
' This is a script file that unzips a specified zip file to a specified directory. 
' It uses the DotNetZip library, via COM, to do the unzipping.
' This script is used for compatibility testing of the DotNetZip output.
'
' created Fri, 29 May 2009  17:07
'
' ------------------------------------------------------------------



Sub UnpackZip(pathToZipFile, extractLocation)

    WScript.Echo "Unpacking zip  (" & pathToZipFile & ") to (" & extractLocation & ")"

    Dim zip
    WScript.echo("Instantiating a ZipFile object...")
    Set zip = CreateObject("Ionic.Zip.ZipFile")
    
    WScript.echo("Initialize (Read)...")
    zip.Initialize(pathToZipFile)

    WScript.echo("extracting...")
    zip.ExtractAll(extractLocation)

    WScript.echo("Disposing...")
    zip.Dispose()

    WScript.echo("Done.")
    
End Sub


Sub Main()
    
    dim args

    set args = WScript.Arguments 

    If (args.Length = 2) Then
        
        UnpackZip args(0), args(1)
        
    Else
        WScript.Echo "VbsUnzip.vbs - unzip a zip file using the DotNetZip library."
        WScript.Echo "  usage: VbsUnzip.vbs  <pathToZip>  <extractLocation>"
    End If
    
End Sub

Call Main
