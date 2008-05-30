<%@ Control Language="C#" EnableViewState="false" AutoEventWireup="false" CodeFile="CustomControl.ascx.cs" Inherits="DI_HelloWorld_CustomControl" %>

    <p>
    The message you can see below has been injected by &lt;object type="CustomControl.ascx"&gt; object definition:
    </p>
    <p>
    '<%=AnotherMessage%>'
    </p>
    <p>
    The TextBox below has been filled in by typename using the &lt;object name="System.Web.UI.WebControls.TextBox"&gt; object definition:
    </p>    
    <p><asp:TextBox runat="server" id="myTextBox"/></p>

    <p>
    Use a spring:Panel to selectively enable/disable automatic dependency injection:
    </p>    
    <spring:Panel runat="server" ID="diContainer" suppressDependencyInjection="true" renderContainerTag="false">
        <asp:TextBox runat="server" id="TextBox2"/>
    </spring:Panel>
