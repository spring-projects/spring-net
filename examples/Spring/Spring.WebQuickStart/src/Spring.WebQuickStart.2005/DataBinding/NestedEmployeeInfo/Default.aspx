<%@ Page Language="C#" EnableViewState="false" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="DataBinding_RobustEmployeeInfo_Default" %>
<%-- @ Reference Control="EmployeeInfoEditor.ascx" --%>
<%@ Register TagPrefix="user" TagName="EmployeeInfoEditor" Src="EmployeeInfoEditor.ascx" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Data Binding - Nested Employee Info example</title>
</head>
<body>
    <h2><a href="../../Default.aspx">Welcome to Spring.NET Web Framework Quick Start Guide</a></h2>
    <h2><a href="../Default.aspx">Bi-directional DataBinding</a></h2>
    <form id="form1" runat="server">
    <div>
    <h2>Nested Employee Info example</h2>
    <p>
    This is the same as Robust Employee Info example except that all controls are placed on nested a user control
    </p>
    <!-- this summary control captures validation errors from the "Editor" control below -->
    <spring:ValidationSummary ID="summary" Provider="summary" ValidationContainerName="Editor" runat="server" />
    <user:EmployeeInfoEditor ID="Editor" runat="server" />
    </div>
    </form>
</body>
</html>
