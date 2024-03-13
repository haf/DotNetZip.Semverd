<%@ Page
    Language="VB"
    Debug="true"
%>

<%@ Import Namespace="System.Text" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="Ionic.Zip" %>
<%@ Import Namespace="System.Collections.Generic" %>

<script language="VB" runat="server">

' ZipExample.aspx
'
' This .aspx page demonstrates how to use the DotNetZip library from within ASP.NET.
'
' To run it,
'  1. drop the Ionic.Zip.dll into the \bin directory of your asp.net app
'  2. create a subdirectory called "fodder" in your web app directory.
'  3. copy into that directory a variety of random files.
'  4. insure your web.config is properly set up (See below)
'
'
' notes:
'  This requies the .NET Framework 3.5 - because it uses the ListView control that is
'  new for ASP.NET in the .NET Framework v3.5.
'
'  To use this control, you must add the new web controls.  Also, you must use the v3.5 compiler.
'  Here's an example web.config that works with this aspx file:
'
'    <configuration>
'      <system.web>
'        <trust level="Medium" />
'        <compilation defaultLanguage="c#" />
'        <pages>
'          <controls>
'            <add tagPrefix="asp" namespace="System.Web.UI.WebControls"
'                 assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" />
'          </controls>
'        </pages>
'      </system.web>
'      <system.codedom>
'        <compilers>
'          <compiler language="c#;cs;csharp"
'                extension=".cs"
'                warningLevel="4"
'                type="Microsoft.CSharp.CSharpCodeProvider, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089">
'            <providerOption name="CompilerVersion" value="v3.5" />
'            <providerOption name="WarnAsError" value="false" />
'          </compiler>
'
'          <compiler language="vb;vbs;visualbasic;vbscript"
'                    extension=".vb"
'                    warningLevel="4"
'                    type="Microsoft.VisualBasic.VBCodeProvider, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089">
'            <providerOption name="CompilerVersion" value="v3.5" />
'            <providerOption name="OptionInfer" value="false" />
'            <providerOption name="WarnAsError" value="false" />
'          </compiler>
'
'      </system.codedom>
'    </configuration>
'
'


Dim width as String = "100%"

Public Sub Page_Load (ByVal sender As Object, ByVal e As System.EventArgs)
    Try
        If Not ( Page.IsPostBack ) Then
            ' populate the dropdownlist
            ' must have a directory called "fodder" in the web app
            Dim sMappedPath as  String= Server.MapPath("fodder")

            Dim fqFilenames As New List(Of String)(System.IO.Directory.GetFiles(sMappedPath))

            Dim filenames as List(Of String) = _
                fqFilenames.ConvertAll (Function(s) s.Replace(sMappedPath & "\", ""))

            ErrorMessage.InnerHtml = ""

            FileListView.DataSource = filenames
            FileListView.DataBind()
        End If

    Catch
        ' Ignored
    End Try

End Sub



Public Sub btnGo_Click (ByVal sender As System.Object, ByVal e As System.EventArgs)

    ErrorMessage.InnerHtml =""   ' debugging only
    Dim filesToInclude as New System.Collections.Generic.List(Of String)()
    Dim sMappedPath as String= Server.MapPath("fodder")
    Dim source As DataKeyArray= FileListView.DataKeys

    For Each item As ListViewDataItem in FileListView.Items

        Dim chkbox As CheckBox= CType(item.FindControl("include"), CheckBox)
        Dim lbl As Label = CType(item.FindControl("label"), Label)

        If Not (chkbox Is Nothing  OR  lbl Is Nothing) Then
            If (chkbox.Checked) Then
                ErrorMessage.InnerHtml = ErrorMessage.InnerHtml & _
                        String.Format("adding file: {0}<br/>\n", lbl.Text)
                filesToInclude.Add(System.IO.Path.Combine(sMappedPath,lbl.Text))
            End If
        End If
    Next

    If (filesToInclude.Count=0) Then
        ErrorMessage.InnerHtml = ErrorMessage.InnerHtml & "You did not select any files?<br/>\n"
    Else
        Response.Clear
        Response.BufferOutput= false

        Dim enc as Ionic.Zip.EncryptionAlgorithm = Ionic.Zip.EncryptionAlgorithm.None
        If (chkUseAes.Checked) Then
            enc = EncryptionAlgorithm.WinZipAes256
        End If

        Dim c As System.Web.HttpContext = System.Web.HttpContext.Current
        Dim ReadmeText As String= String.Format("README.TXT\n\nHello!\n\n" & _
                                         "This is a zip file that was dynamically generated at {0}\n" & _
                                         "by an ASP.NET Page running on the machine named '{1}'.\n" & _
                                         "The server type is: {2}\n" & _
                                         "The password used: '{3}'\n" & _
                                         "Encryption: {4}\n", _
                                         System.DateTime.Now.ToString("G"), _
                                         System.Environment.MachineName, _
                                         c.Request.ServerVariables("SERVER_SOFTWARE"), _
                                         tbPassword.Text, _
                                         enc.ToString )
        Dim archiveName as String= String.Format("archive-{0}.zip", DateTime.Now.ToString("yyyy-MMM-dd-HHmmss"))
        Response.ContentType = "application/zip"
        Response.AddHeader("Content-Disposition", "inline; filename=" & chr(34) & archiveName & chr(34))

        ' In some cases, saving a zip directly to Response.OutputStream can
        ' present problems for the unzipper, especially on Macintosh.
        ' To workaround that, you can save to a file, then copy the file,
        ' via a FileStream, to the Response.OutputStream.
        Dim tempfile As String = "c:\temp\" & archiveName
        Using zip as new ZipFile()
            ' the Readme.txt file will not be password-protected.
            zip.AddEntry("Readme.txt", ReadmeText, Encoding.Default)
            If Not String.IsNullOrEmpty(tbPassword.Text) Then
                zip.Password = tbPassword.Text
                zip.Encryption = enc
            End If

            ' filesToInclude is a string[] or List<String>
            zip.AddFiles(filesToInclude, "files")

            ' save the zip to a filesystem file
            zip.Save(tempfile)
        End Using

        ' open and read the file, and copy it to Response.OutputStream
        Using fs as System.IO.FileStream = System.IO.File.OpenRead("c:\temp\" & archiveName)
            dim b(1024) as Byte
            dim n as New Int32
            n=-1
            While (n <> 0)
                n = fs.Read(b,0,b.Length)
                If (n <> 0)
                    Response.OutputStream.Write(b,0,n)
                End If
            End While
        End Using
        Response.Close
        System.IO.File.Delete(tempfile)

    End If

End Sub


</script>



<html>
  <head>
    <link rel="stylesheet" href="style/basic.css">
  </head>

  <body>

    <form id="Form" runat="server">

      <h3> <span id="Title" runat="server" />Zip Files from ASP.NET </h3>

      <p>This page uses the .NET Zip library (see <a
      href="http://DotNetZip.codeplex.com">http://DotNetZip.codeplex.com/</a>)
      to dynamically create a zip archive, and then download it to the
      browser through Response.OutputStream, via a FileStream.  This page is implemented
      in VB.NET.</p>

      <p>In some cases, saving a zip directly to Response.OutputStream can
        present problems for the unzipper, especially on Macintosh.
        To workaround that, you can save to a file, then copy the contents of
        the file, via a FileStream, to the Response.OutputStream.
      </p>

      <span class="SampleTitle"><b>Check the boxes to select the files, set a password if you like,
      then click the button to zip them up.</b></span>
      <br/>
      <br/>
      Password: <asp:TextBox id="tbPassword" Password='true' Text="" AutoPostBack runat="server"/>
      <span style="color:Red">(Optional)</span>
      <br/>
      <br/>
      Use AES?: <asp:CheckBox id="chkUseAes" AutoPostBack runat="server"/>
      <br/>
      <br/>
      <asp:Button id="btnGo" Text="Zip checked files" AutoPostBack OnClick="btnGo_Click" runat="server"/>

      <br/>
      <br/>
      <span style="color:red" id="ErrorMessage" runat="server"/>
      <br/>

      <asp:ListView ID="FileListView" runat="server">

        <LayoutTemplate>
          <table>
            <tr ID="itemPlaceholder" runat="server" />
          </table>
        </LayoutTemplate>

        <ItemTemplate>
          <tr>
            <td><asp:Checkbox ID="include" runat="server"/></td>
            <td><asp:Label id="label" runat="server" Text="<%# Container.DataItem %>" /></td>
          </tr>
        </ItemTemplate>

        <EmptyDataTemplate>
          <div>Nothing to see here...</div>
        </EmptyDataTemplate>

      </asp:ListView>


    </form>

  </body>

</html>

