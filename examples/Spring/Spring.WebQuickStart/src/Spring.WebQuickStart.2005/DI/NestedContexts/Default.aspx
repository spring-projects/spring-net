<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="DI_NestedContexts_Default" %>
<%@ Register TagPrefix="ctl" TagName="ControlA" src="ContextA/CustomControl.ascx" %>
<%@ Register TagPrefix="ctl" TagName="ControlB" src="ContextB/CustomControl.ascx" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Dependency Injection - Nested Contexts example</title>
</head>
<body>
    <h2><a href="../../Default.aspx">Welcome to Spring.NET Web Framework Quick Start Guide</a></h2>
    <h2><a href="../Default.aspx">Dependency Injection</a></h2>
    <div>
    <h2>Nested Contexts example</h2>    
    <form id="form1" runat="server">
    <p>
    This sample illustrates nesting of contexts.
    </p>
    <p>
    Control "A" below demonstrates configuring a control within the page's context:
    </p>
    <p>
    <ctl:ControlA runat="server" />
    </p>
    <p>
    Control "B" below demonstrates configuring a control within the page's context, 
    but overriding the definition in the control's context.
    </p>
    <p>
    <ctl:ControlB runat="server" />
    </p>
    </form>
    </div>
</body>
</html>
