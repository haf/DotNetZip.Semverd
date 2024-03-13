<%@ LANGUAGE = VBScript %>
<%  Option Explicit %>
<%

' -------------------------------------------------------
' ASP DotNetZip Example
' -------------------------------------------------------
' This example ASP page uses DotNetZip (Ionic.Zip.dll) via COM
' interop.  The page opens a zip file, then allows the user
' to download any individual file within the zip file. 
' 
'' To get this to work, you must be sure to register DotNetZip for COM
'' interop (regasm).  Also you need to be sure that IIS/ASP has the correct
'' permissions to instantiate the ZipFile object.  In my experience I Was
'' able to do this by copying Ionic.Zip.dll to the
'' c:\windows\system32\inetsrv directory, then calling "regasm /codebbase
'' Ionic.Zip.dll" from within that directory.

'' This example assumes that the ASP page is deployed into a directory,
'' that contains a subdirectory called "fodder".  Fodder must be readable,
'' and should contain one or more zip files.  This page allows the user to
'' select a zip file, then select a file within the zip file, and download
'' that file.
''
''



If Request.Form("Submit") = "Download" Then 
    dim pathForZipFile, fileToDownload
    pathForZipFile= Request.Form("zipFile")
    if pathForZipFile <> "" Then
        fileToDownload = Request.Form("fileToDownload")
        Response.Clear
        Response.AddHeader "Content-Disposition", "attachment; filename=" & fileToDownload  
        Response.ContentType = "application/octet-stream"  

        pathForZipFile = Server.MapPath("fodder\" & pathForZipFile)

        dim zip, ms
        set zip = Server.CreateObject("Ionic.Zip.ZipFile")
        zip.Initialize(pathForZipFile)

        set ms = Server.CreateObject("System.IO.MemoryStream")

        dim selectedEntry, entry
        For Each entry in zip
            If entry.FileName = fileToDownload  Then 
                set selectedEntry = entry 
            End If 
        Next

        selectedEntry.Extract_3(ms)
        zip.Dispose

        dim fred
        fred = ms.ToArray

        Response.BinaryWrite(fred)
        ms.Dispose
    End If
    
Else

%>

<html>
    <HEAD>
        <TITLE>Simple DotNetZip Example</TITLE>
        <style>
        BODY { font-family: Verdana, Arial, Helvetica, sans-serif;font-size: 10pt;}
        TD {  font-family: Verdana, Arial, Helvetica, sans-serif;  font-size: 8pt;}
        TH {  font-family: Verdana, Arial, Helvetica, sans-serif;  font-size: 10pt;}
        H2 { font-size: 16pt; font-weight: bold; font-family: Verdana, Arial, Helvetica, sans-serif; color:Navy;}
        H1 { font-size: 20pt; font-weight: bold; font-family: Verdana, Arial, Helvetica, sans-serif; color:Blue;}
        </style>


    <script language="Javascript">

      function Download(file, zipFile)
      {
          document.form1.fileToDownload.value = file;
          document.form1.zipFile.value = zipFile;
          document.form1.submit();
      }

    </script>



    <script RUNAT=Server language="VBScript">

    '-------------------------------------
    ' This reads the given zip file. 
    '-------------------------------------
    Sub DisplayContentsOfZip
        dim pathForZipFile
        pathForZipFile= Request.Form("selectedZip")
        if pathForZipFile <> "" Then
            pathForZipFile = Server.MapPath("fodder\" & pathForZipFile)

            dim zip
            set zip = Server.CreateObject("Ionic.Zip.ZipFile")
            zip.Initialize(pathForZipFile)

            response.write "<table border=1><tr><th>Name</th><th>last modified</th></tr>"
                
            dim entry, fN
            For Each entry in zip
                If Right(entry.FileName,1) <> "/" Then 
                    Response.Write "<tr><TD><input type='submit' name='Submit' value='Download' onClick=" & chr(34) & "Download('" & _
                       entry.FileName & "', '" & Request.Form("selectedZip")  & "');" & chr(34) & " ></TD><td>" & _
                       entry.FileName & "</td><td>" & entry.LastModified & "</td></tr>"
                End If
            Next
            response.write "</table>"
            zip.Dispose()
        End If
    End Sub


    '-------------------------------------
    ' This function builds and returns the 
    ' option list for the form. eg:
    '    <OPTION value="file1.zip">file1.zip</OPTION>
    '    <OPTION value="file2.zip">file2.zip</OPTION>
    '    <OPTION value="file3.zip">file3.zip</OPTION>
    '-------------------------------------
    Function FileList()
        Dim fso, folder, ext, item, result
        result = ""
        Set fso= Server.CreateObject("Scripting.FileSystemObject")
        Set folder = FSO.GetFolder(Server.MapPath("fodder"))
        For Each item In folder.Files
            ext = Right(item.Name,4)
            If (ext = ".zip") Then
                result = result & "<OPTION value='" & item.Name & "'>" & item.Name & "</OPTION>"
            End If
        Next 
        Set fso = Nothing
        Set folder = Nothing
        FileList = result
    End Function

    </script>

    </HEAD>

<body>
<h1>ASP DotNetZip</h1>

<p> This page shows how to use <a
href="http://DotNetZip.codeplex.com">DotNetZip</a> from an ASP (Classic)
page.  This page reads zip files and allows the browser to download
items from the zip files.  </p>

<form METHOD="POST" id='form1' name='form1'>

    <TABLE style="border:1; cellspacing:1; cellpadding:1;">
        <TR> <TD>Select a Zip file:</TD>
          <TD><SELECT id='select1' name='selectedZip'>
              <%= fileList %>
              </SELECT>
          </TD>
        </TR>
        <TR> <TD/><TD><input type='submit' name="Submit" Value="Read Zip"/></TD> </TR>
    </TABLE>

    <input type="hidden" name="fileToDownload" value="">
    <input type="hidden" name="zipFile" value="">

<%
 DisplayContentsOfZip
%>

</form>

</body>
</html>

<%
End If
%>