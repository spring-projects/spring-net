using System.Web;

/// <summary>
/// Summary description for NoOpHandler
/// </summary>
public class MyCustomHttpHandler : IHttpHandler
{
    public string MessageText = "<unconfigured>";

	public void ProcessRequest(HttpContext context)
	{
		context.Response.ContentType = "text/plain";
		context.Response.Write(@"Response from MyCustomHttpHandler:" + MessageText);
	}

	public bool IsReusable
	{
		get { return true; }
	}
}
