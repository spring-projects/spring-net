<%@ WebService Language="C#" Debug="true" Class="foo" %>
using System.Web.Services;


public class foo
{
    [WebMethod()]
    public string echo(string input)
    {
        return input;
    }
}
