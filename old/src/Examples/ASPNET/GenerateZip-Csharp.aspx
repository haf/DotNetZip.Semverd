<%@ Page
    Language="C#"
    Debug="true"
%>


<%@ Import Namespace="System.Text" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="Ionic.Zip" %>
<%@ Import Namespace="System.Collections.Generic" %>

<script language="C#" runat="server">

// ZipExample.aspx
//
// This .aspx page demonstrates how to use the DotNetZip library from within ASP.NET.
//
// To run it,
//  1. drop the Ionic.Zip.dll into the \bin directory of yoru asp.net app
//  2. create a subdirectory called "fodder" in your web app directory.
//  3. copy into that directory a variety of random files.
//  4. insure your web.config is properly set up (See below)
//
//
// notes:
//  This requies the .NET Framework 3.5 - because it uses the ListView control that is
//  new for ASP.NET in the .NET Framework v3.5.
//
//  To use this control, you must add the new web controls.  Also, you must use the v3.5 compiler.
//  Here's an example web.config that works with this aspx file:
//
//    <configuration>
//      <system.web>
//        <trust level="Medium" />
//        <compilation defaultLanguage="c#" />
//        <pages>
//          <controls>
//            <add tagPrefix="asp" namespace="System.Web.UI.WebControls" assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" />
//          </controls>
//        </pages>
//      </system.web>
//      <system.codedom>
//        <compilers>
//          <compiler language="c#;cs;csharp"
//                extension=".cs"
//                warningLevel="4"
//                type="Microsoft.CSharp.CSharpCodeProvider, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089">
//            <providerOption name="CompilerVersion" value="v3.5" />
//            <providerOption name="WarnAsError" value="false" />
//          </compiler>
//
//          <compiler language="vb;vbs;visualbasic;vbscript"
//                    extension=".vb"
//                    warningLevel="4"
//                    type="Microsoft.VisualBasic.VBCodeProvider, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089">
//            <providerOption name="CompilerVersion" value="v3.5" />
//            <providerOption name="OptionInfer" value="false" />
//            <providerOption name="WarnAsError" value="false" />
//          </compiler>
//        </compilers>
//      </system.codedom>
//    </configuration>
//
//




public String width = "100%";

public void Page_Load (Object sender, EventArgs e)
{
    try
    {
        if ( !Page.IsPostBack ) {
            // populate the dropdownlist
            // must have a directory called "fodder" in the web app directory
            String sMappedPath= Server.MapPath("fodder");

            var fqFilenames= new List<String>(System.IO.Directory.GetFiles(sMappedPath));
            var filenames= fqFilenames.ConvertAll((s) => { return s.Replace(sMappedPath+"\\", ""); });

            ErrorMessage.InnerHtml = "";

            FileListView.DataSource = filenames;
            FileListView.DataBind();
        }

    }
    catch (Exception)
    {
        // Ignored
    }
}


public void btnGo_Click (Object sender, EventArgs e)
{
    ErrorMessage.InnerHtml ="";   // debugging only
    var filesToInclude= new System.Collections.Generic.List<String>();
    String sMappedPath= Server.MapPath("fodder");
    var source= FileListView.DataKeys as DataKeyArray ;

    foreach (var item in  FileListView.Items)
    {
        CheckBox chkbox= item.FindControl("include") as CheckBox ;
        Label lbl= item.FindControl("label") as Label ;

        if (chkbox!=null  && lbl != null)
        {
            if (chkbox.Checked)
            {
                ErrorMessage.InnerHtml += String.Format("adding file: {0}<br/>\n", lbl.Text);
                filesToInclude.Add(System.IO.Path.Combine(sMappedPath,lbl.Text));
            }
        }
    }

    if (filesToInclude.Count==0)
    {
        ErrorMessage.InnerHtml += "You did not select any files?<br/>\n";
    }
    else
    {
        Response.Clear();
        Response.BufferOutput= false;

        System.Web.HttpContext c= System.Web.HttpContext.Current;
        String ReadmeText= String.Format("README.TXT\n\nHello!\n\n" +
                                         "This is a zip file that was dynamically generated at {0}\n" +
                                         "by an ASP.NET Page running on the machine named '{1}'.\n" +
                                         "The server type is: {2}\n"+
                                         "The password used: '{3}'\n" +
                                         "Encryption: {4}\n",
                                         System.DateTime.Now.ToString("G"),
                                         System.Environment.MachineName,
                                         c.Request.ServerVariables["SERVER_SOFTWARE"],
                                         tbPassword.Text,
                                         (chkUseAes.Checked)?EncryptionAlgorithm.WinZipAes256.ToString() : "None"
                                         );
        string archiveName= String.Format("archive-{0}.zip", DateTime.Now.ToString("yyyy-MMM-dd-HHmmss"));
        Response.ContentType = "application/zip";
        Response.AddHeader("content-disposition", "inline; filename=\"" + archiveName + "\"");

        using (ZipFile zip = new ZipFile())
        {
            // the Readme.txt file will not be password-protected.
            zip.AddEntry("Readme.txt", ReadmeText, Encoding.Default);
            if (!String.IsNullOrEmpty(tbPassword.Text))
            {
                zip.Password = tbPassword.Text;
                if (chkUseAes.Checked)
                    zip.Encryption = EncryptionAlgorithm.WinZipAes256;
            }

            // filesToInclude is a string[] or List<String>
            zip.AddFiles(filesToInclude, "files");

            zip.Save(Response.OutputStream);
        }
        Response.Close();
    }
}

</script>



<html>
  <head>
    <link rel="stylesheet" href="style/basic.css">
  </head>

  <body>

    <form id="Form" runat="server">

      <h3> <span id="Title" runat="server" />Zip Files from ASP.NET </h3>

      <p>This page uses the .NET Zip library (see <a
      href="http:///DotNetZip.codeplex.com">http://DotNetZip.codeplex.com</a>)
      to dynamically create a zip archive, and then download it to the
      browser through Response.OutputStream.  This page is implemented in C#.</p>

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

