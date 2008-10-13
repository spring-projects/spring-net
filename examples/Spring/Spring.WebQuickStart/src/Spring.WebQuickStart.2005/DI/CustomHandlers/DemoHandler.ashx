<%@ WebHandler Language="C#" Class="DemoHandler" %>

using System;
using System.Web;

public class DemoHandler : IHttpHandler
{
    private string _outputText;

    public string OutputText
    {
        get { return _outputText; }
        set { _outputText = value; }
    }

    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "text/plain";
        context.Response.Write("injected text:" + _outputText);
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}