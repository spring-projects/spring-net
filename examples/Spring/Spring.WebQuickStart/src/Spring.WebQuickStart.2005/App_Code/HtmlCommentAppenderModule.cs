using System;
using System.Web;

/// <summary>
/// Summary description for CustomModule
/// </summary>
public class HtmlCommentAppenderModule : IHttpModule
{
    private string appendText;

    public HtmlCommentAppenderModule()
    {
    }

    public string AppendText
    {
        get { return this.appendText; }
        set { this.appendText = value; }
    }

    public void Init(HttpApplication application)
    {
        application.PostRequestHandlerExecute += new System.EventHandler(application_PostRequestHandlerExecute);
    }

    void application_PostRequestHandlerExecute(object sender, System.EventArgs e)
    {
        ((HttpApplication)sender).Context.Response.Write( string.Format( "{0}<!-- {1} -->", Environment.NewLine, appendText ) );
    }

    public void Dispose()
    {
    }
}
