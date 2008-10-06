<%@ Page Language="C#" EnableViewState="false" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="DI_HelloWorld_Default" %>
<%@ Register TagPrefix="ctl" TagName="CustomControl" src="CustomControl.ascx" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title><%=Title%></title>
</head>
<body>
    <h2><a href="../../Default.aspx">Welcome to Spring.NET Web Framework Quick Start Guide</a></h2>
    <h2><a href="../Default.aspx">Dependency Injection</a></h2>
    <div>
    <h2><a href="splashpage.aspx">Hello World example</a></h2>    
    <form id="form1" runat="server">
    <p>
    This very simple web form demonstrates dependency injection on pages and controls.
    </p>
    <p>
    The message you can see below has been injected by &lt;object type="Default.aspx"&gt; object definition:
    </p>
    <p>
    '<%=Message%>'
    </p>
    <p>
    <ctl:CustomControl runat="server" ID="myCustomControl" />
    </p>
    </form>
    </div>
</body>
</html>
