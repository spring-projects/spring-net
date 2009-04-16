<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="DataBinding_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Spring.NET Web Framework Quick Start Guide - Bi-directional DataBinding</title>
</head>
<body>
    <h2><a href="../Default.aspx">Welcome to Spring.NET Web Framework Quick Start Guide</a></h2>
    <div>
    <h2>Bi-directional DataBinding</h2>
    <p>
    Samples in this section demonstrate Spring.NET's capabilities of bi-directional 
    binding data between your model and form.
    </p>
    <h3><a href="HelloWorld/Default.aspx">Hello World</a></h3>
    <p>
    This very simple web form demonstrates basic bi-directional data binding.
    </p>
    <h3><a href="EventHandling/Default.aspx">Event Handling</a></h3>
    <p>
    This example builds on the previous one by adding postback event handler
    that will convert the name to uppercase.
    </p>
    <h3><a href="Collections/Default.aspx">HttpRequestListBindingContainer</a></h3>
    <p>
    This example demonstrates unbinding data from request into a list using a HttpRequestListBindingContainer.
    </p>
    <h3><a href="EmployeeInfo/Default.aspx">Employee Info</a></h3>
    <p>
    Shows basics of bi-directional databinding with simple webcontrols (TextBox etc.)
    </p>
    <h3><a href="RobustEmployeeInfo/Default.aspx">Robust Employee Info</a></h3>
    <p>
    Demonstrates basic model validation capabilities
    </p>
    <h3><a href="NestedEmployeeInfo/Default.aspx">Nested Employee Info</a></h3>
    <p>
    Demonstrates basic model validation capabilities on controls
    </p>
    <h3><a href="Lists/Default.aspx">Lists</a></h3>
    <p>
    Demonstrates bi-directional binding of multi-selection listboxes
    </p>
    <h3><a href="EasyEmployeeInfo/Default.aspx">Easy Employee Info</a></h3>
    <p>
    Finally shows, how easy life can be when using a DataBindingPanel
    </p>
    </div>
</body>
</html>
