<%@ Page Language="C#" EnableViewState="true" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="Navigation_Default" %>
<%@ Register TagPrefix="spring" Namespace="Spring.Web.UI.Controls" Assembly="Spring.Web" %>

<asp:Content runat="server" ContentPlaceHolderId="content">
    <p>
    This example demonstrates the navigation support. Instead of directly redirecting to concrete links,
    Spring.Web allows for "symbolic" names for navigation targets and navigate to such a target by
    calling SetResult( symbolicName ) on the page object.
    </p>
<spring:DataBindingPanel ID="ctlDataBindingPanel" runat="server">
    <table cellpadding="3" cellspacing="3" border="0">
<spring:Panel VisibleIf="Context.Request['fromInvalidInput'] != null" runat="server">
        <tr>
            <td></td>
            <td>
                You need to enter a value greater than 21 to pass!
            </td>
        </tr>
</spring:Panel>    
        <tr>
            <td></td>
            <td>
            By entering a value greater 21 in the box below, you will be redirected to the "Ok" page. Otherwise
            "InvalidInput" will show up:
            </td>
        </tr>
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
