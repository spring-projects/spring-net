<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="DI_NestedContexts_ContextB_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Object definition inheritance example</title>
</head>
<body>
    <form id="form1" runat="server">
    <p>
    <i><%=Message%></i>
    </p>
    <p>
    Below you can see the message from the definition 'GlobalMessage' that is inherited within this child context:
    </p>
    <p>
    <i><%=GlobalMessage%></i>
    </p>
    </form>
</body>
</html>
