<%@ Page Language="C#" Debug="true" EnableViewState="false" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="DataBinding_EventHandling_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Data Binding - Event Handling example</title>
</head>
<body>
    <h2><a href="../../Default.aspx">Welcome to Spring.NET Web Framework Quick Start Guide</a></h2>
    <h2><a href="../Default.aspx">Bi-directional DataBinding</a></h2>
    <form id="form1" runat="server">
    <div>
    <h2>Event Handling example</h2>
    <p>
    This example builds on the previous one by adding postback event handler
    that will convert the name to uppercase.
    </p>
    <p>
    This particular example shows the main reason data binding framework was developed
    in a first place. The idea is that developer should almost never have to access
    controls directly within postback event handlers. Instead, controls should
    automatically populate data model on postback and they should be refreshed
    from the data model just before the page is rendered again.
    </p>
    <p>
    As you can see, developer can modify data model directly within postback event handler 
    and all the changes will be reflected on the controls when the page is rendered. In 
    this particular example we use properties within the Page class to hold data model,
    but there are other alternatives as well, that we'll show you in the later examples.
    </p>
    <p>
    Enter Your Name: <asp:TextBox ID="txtName" runat="server" AutoPostBack="false" Width="150" EnableViewState="False" /> 
                     <asp:Button ID="btnCapitalize" text="Capitalize" runat="server" OnClick="btnCapitalize_Click" />
    </p>
    <div id="divGreeting" runat="server">
    <b>Hello, <asp:Label ID="lblName" runat="server" /></b>
    </div>
    </div>
    </form>
</body>
</html>
