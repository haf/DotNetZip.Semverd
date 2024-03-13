' PackResources.vbs
' ------------------------------------------------------------------
'
' Copyright (c) 2010 Dino Chiesa
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
' Time-stamp: <2011-July-23 20:02:17>
'
' ------------------------------------------------------------------
'
' This is a script file that packs the resources files into a zip,
' for inclusion into the zip dll.
'
' This script assumes it will be run by Visual Studio, as a prebuild
' script, starting with the current directory of
' {DotNetZip}\Zip Partial DLL\bin\{Debug,Release}
'
' Wed, 10 Feb 2010  12:24
'
' ------------------------------------------------------------------


Sub NewZip(pathToZipFile)

    ' WScript.Echo "Creating a new zip file (" & pathToZipFile & ") "

    Dim fso
    Set fso = CreateObject("Scripting.FileSystemObject")
    Dim file
    Set file = fso.CreateTextFile(pathToZipFile)

    '' this is the content for an empty zip file
    file.Write Chr(80) & Chr(75) & Chr(5) & Chr(6) & String(18, 0)

    file.Close
    Set fso = Nothing
    Set file = Nothing

    WScript.Sleep 500

End Sub



Function DatesAreSubstantiallyDifferent(d1, d2)

    Dim result
    Dim s

    ''' WScript.Echo "d1= " & d1
    ''' WScript.Echo "d2= " & d2

    '' http://www.w3schools.com/vbscript/func_datediff.asp
    s = DateDiff("s",d1,d2)
    ''' WScript.Echo "s= " & s

    '' 2 seconds or less in either direction is ok.
    If (s < 3 AND s > -3) Then
        result = False
    Else
        result = True
    End If

    DatesAreSubstantiallyDifferent = result

End Function





Sub CreateZip(pathToZipFile, dirToZip)

    Dim fso
    Set fso= Wscript.CreateObject("Scripting.FileSystemObject")

    Dim fullPathToZipFile
    fullPathToZipFile = fso.GetAbsolutePathName(pathToZipFile)

    Dim fullDirToZip
    fullDirToZip = fso.GetAbsolutePathName(dirToZip)

    If Not fso.FolderExists(fullDirToZip) Then
        ' WScript.Echo "The directory to zip does not exist."
        Exit Sub
    End If

    ' WScript.Echo "Checking zip " & fullPathToZipFile
    ' WScript.Echo "  against directory " &  fullDirToZip

    dim sa
    set sa = CreateObject("Shell.Application")


    ' http://msdn.microsoft.com/en-us/library/bb787866(VS.85).aspx
    ' ===============================================================
    '    4 = do not display a progress box
    '   16 = Respond with "Yes to All" for any dialog box that is displayed.
    '  128 = Perform the operation on files only if a wildcard file name (*.*) is specified.
    '  256 = Display a progress dialog box but do not show the file names.
    ' 2048 = Version 4.71. Do not copy the security attributes of the file.
    ' 4096 = Only operate in the local directory. Don't operate recursively into subdirectories.

    Dim fcount
    fcount = 0

    Dim needRepack
    needRepack = -1
    Dim zip
    Dim folder, file, builtpath, pass, d1, d2, folderItem
    Set folder = fso.GetFolder(fullDirToZip)

    '' do this in 2 passes.  First pass checks if any file in the zip has been updated.
    '' 2nd pass is performed only if necessary, and actually copies all the files into the new zip.

    pass = 0
    Do Until pass > 1

        For Each file in folder.Files
            '' check or zip any file that is not .zip, not .resx and not ending in ~ (emacs backup file)
            If (Right(file.name,4) <> ".zip" AND Right(file.name,5) <> ".resx" AND Right(file.name,1) <> "~") Then
                builtpath = fso.BuildPath(fullDirToZip, file.Name)
                If (pass = 0) Then
                    If (needRepack = -1) Then
                        '' first file only
                        If Not fso.FileExists(fullPathToZipFile) Then
                            ' WScript.Echo "The zip file does not exist."
                            '' no zip means, always need to repack
                            needRepack = 1
                        Else
                            Set zip = sa.NameSpace(fullPathToZipFile)
                            needRepack = 0
                        End If
                    End If

                    '' only check if we need to repack this file, if
                    '' necessary; in other words, if none of the prior
                    '' files need to be repacked.
                    If (needRepack = 0) Then
                        '' check if the file has been updated
                        d1 = file.DateLastModified
                        Set folderItem = zip.ParseName(file.Name)
                        If (Not folderItem Is Nothing) Then
                            d2 = folderItem.ModifyDate
                            Set folderItem = Nothing
                        Else
                            '' dummy
                            d2 = "01/01/2001 6:05:00 PM"
                        End If

                        If DatesAreSubstantiallyDifferent(d1,d2) Then
                            needRepack = 1
                        End If
                    End If

                Else
                    '' pass = 1
                    If (fcount = 0) Then
                        Wscript.Sleep(400)
                    End If
                    ' WScript.Echo builtpath
                    zip.CopyHere builtpath, 0
                    fcount = fcount + 1
                    '' Delay between each item. With no, the zip fails with
                    '' "file not found" or "No Read Permission" or some other
                    '' spurious error.
                    Wscript.Sleep(450)
                End If

            End If
        Next

        If (pass = 0) Then
            If (needRepack <> 0) Then
                '' reaching pass 1 means we delete and re-create the zip file
                ' WScript.Echo "The resources zip needs to be re-packed. "
                Set zip = Nothing
                If fso.FileExists(fullPathToZipFile) Then
                    ' WScript.Echo "That zip file already exists - deleting it."
                    fso.DeleteFile fullPathToZipFile
                    '' give it time to be really deleted
                    Wscript.Sleep(2400)
                End If
                NewZip fullPathToZipFile
                Set zip = sa.NameSpace(fullPathToZipFile)
            Else
                ' WScript.Echo "The resources zip does not need to be updated."
                '' insure we skip the 2nd pass.
                pass = pass + 1
            End If

        Else
            '' the zip process is asynchronous. wait for completion,
            '' but don't wait forever.
            Dim sLoop
            sLoop = 0
            ' WScript.Echo "Verifying the count..."
            Do Until fcount <= zip.Items.Count
                Wscript.Sleep(400)
                sLoop = sLoop + 1
                If ((sLoop Mod 6) = 0) Then
                    ' WScript.Echo "  have " & zip.items.Count & " items so far, need " & fcount
                ElseIf sLoop > 80 Then
                    ' WScript.Echo "Giving up..."
                    Set zip = Nothing
                    Wscript.Sleep(1200)
                    If fso.FileExists(fullPathToZipFile) Then
                        fso.DeleteFile fullPathToZipFile
                    End If
                    Err.Raise 1460, "PackResources.vbs/CreateZip", "Timeout waiting for ZIP completion"
                End If
            Loop
        End If
        pass = pass + 1

    Loop

    Set fso = Nothing
    Set sa = Nothing
    Set zip = Nothing
    Set folder = Nothing

End Sub



CreateZip "..\..\Resources\zippedResources.zip", "..\..\Resources"
