<%@ Page Language="C#" EnableViewState="true" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="Navigation_Default" %>
<%@ Register TagPrefix="spring" Namespace="Spring.Web.UI.Controls" Assembly="Spring.Web" %>

<asp:Content runat="server" ContentPlaceHolderId="content">
    <p>
    Just like in the previous example, you should put a breakpoint
    on the Debug.Write(Employee) statement in the event handler for the Save button, 
    so you can see how Employee object was populated by the framework.
    </p>
<spring:DataBindingPanel runat="server">
    <table cellpadding="3" cellspacing="3" border="0">
<spring:Panel VisibleIf="Context.Request['fromInvalidInput'] != null" runat="server">
        <tr>
            <td></td>
            <td>
                You need to enter a value greater than 21 to pass.
            </td>
        </tr>
</spring:Panel>    
        <tr>
            <td>Your age:</td>
            <td>
                <asp:TextBox ID="txtAge" runat="server" BindingTarget="Age" />
            </td>
        </tr>
        <tr>
            <td><asp:Button ID="btnReset" runat="server" Text="Reset" OnClick="btnReset_Click" /></td>
            <td><asp:Button ID="btnContinue" runat="server" Text="Continue" OnClick="btnContinue_Click" /></td>
        </tr>
    </table>
</spring:DataBindingPanel>    
</asp:Content>