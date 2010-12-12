<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN" "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>ASP.NET AJAX 1.0 integration with Spring.NET</title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager runat="server" ID="scriptManager">
            <Scripts>
                <asp:ScriptReference Path="ContactWebServiceMethods.js" />
            </Scripts>
            <Services>
                <asp:ServiceReference Path="ContactWebService.asmx" />
            </Services>
        </asp:ScriptManager>
        <div>
            <p>
                Web Service available at this url : <a href="ContactWebService.asmx">ContactWebService.asmx</a>
                <br />
                <br />
                Client scripts generated at this url : <a href="ContactWebService.asmx/js">ContactWebService.asmx/js</a>
                & <a href="ContactWebService.asmx/jsdebug">ContactWebService.asmx/jsdebug</a>
            </p>
            <hr />
            <p>
                This sample demonstrates the use of client script with a web service created by
                Spring.NET's WebServiceExporter.
            </p>
            <p>
                Email :
                <input id="emailInput" name="emailInput" type="text" />
                <button onclick="GetEmails(emailInput.value, 10); return false;">
                    Search</button>
                (Hint : type the letter 'b' or 'm')
            </p>
            <p>
                Results :
                <br />
                <span id="ResultSpan" />
            </p>
        </div>
    </form>
</body>
</html>
