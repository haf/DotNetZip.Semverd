<%@ Page
    Language="C#"
    EnableViewState="False"
    Debug="True"
%>


<%@ Import Namespace="System.Text" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="Ionic.Zip" %>
<%@ Import Namespace="System.Collections.Generic" %>

<script language="C#" runat="server">



private void GiveListOfFiles(string fodderPath)
{
    // display the list of files
    var allFiles = new List<String>
        (Array.ConvertAll
         (Directory.GetFiles(fodderPath),
          (p) => Path.GetFileName(p) ));

    FileListView.DataSource = allFiles;
    FileListView.DataBind();
}



public void Page_Load (Object sender, EventArgs e)
{
    String fodderPath= Server.MapPath(".");
    string filename = Request.QueryString["file"];
    if (filename != null)
    {
        string qualifiedFname = Path.Combine(fodderPath, filename);
        if (File.Exists(qualifiedFname))
        {
            Response.Clear();
            Response.BufferOutput= false;

            System.Web.HttpContext c= System.Web.HttpContext.Current;
            String ReadmeText= String.Format("README.TXT\n\nHello!\n\n" +
                                             "This is a zip file that was dynamically generated at {0}\n" +
                                             "by an ASP.NET Page running on the machine named '{1}'.\n",
                                             System.DateTime.Now.ToString("G"),
                                             System.Environment.MachineName );
            string archiveName= String.Format("archive-{0}.zip", DateTime.Now.ToString("yyyy-MMM-dd-HHmmss"));
            Response.ContentType = "application/zip";
            Response.AddHeader("content-disposition", "inline; filename=\"" + archiveName + "\"");

            using (ZipFile zip = new ZipFile())
            {
                zip.AddEntry("Readme.txt", ReadmeText, Encoding.Default);
                zip.AddFile(qualifiedFname, "");
                zip.Save(Response.OutputStream);
            }
            // Response.Close();
            Response.End();
        }
    }

    GiveListOfFiles(fodderPath);

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
      to dynamically create a zip archive, and then send it to the
      browser through Response.OutputStream.  </p>

      <span style="color:red" id="ErrorMessage" runat="server"/>
      <br/>

      <p>To generate a zip file,
      specify a fodder file in the "file" parameter of the query
      string. Choose from one of these: </p>
      <br/>
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

