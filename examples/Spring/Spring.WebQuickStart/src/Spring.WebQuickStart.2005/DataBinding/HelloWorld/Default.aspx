<%@ Page Language="C#" EnableViewState="false" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="DataBinding_HelloWorld_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Data Binding - Hello World example</title>
</head>
<body>
    <h2><a href="../../Default.aspx">Welcome to Spring.NET Web Framework Quick Start Guide</a></h2>
    <h2><a href="../Default.aspx">Bi-directional DataBinding</a></h2>
    <form id="form1" runat="server">
    <div>
    <h2>Hello World example</h2>
    <p>
    This very simple web form demonstrates basic bi-directional data binding.
    </p>
    <p>
    Once you enter your name into the text box and submit the form (by hitting Enter), 
    Spring.NET will evaluate data binding rules configured in the code-behind file and 
    set the Name property of the page to the value of txtName.Text property.
    </p>
    <p>
    After that, page will be rendered again and Name property will be bound both
    to the text box, so you can change the value, and to the label below the text
    box, in order to show you how you can use one-way bindings to set non-writable
    properties.
    </p>
    <p>
    Enter Your Name: <asp:TextBox ID="txtName" runat="server" Width="150" EnableViewState="False"/>
    </p>
    <div id="divGreeting" runat="server">
    <b>Hello, <asp:Label ID="lblName" runat="server" /></b>
    </div>
    </div>
    </form>
</body>
</html>
