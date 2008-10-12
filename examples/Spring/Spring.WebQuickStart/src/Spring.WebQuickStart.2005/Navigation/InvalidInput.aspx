<%@ Page Language="C#" EnableViewState="true" AutoEventWireup="true" CodeFile="InvalidInput.aspx.cs" Inherits="Navigation_InvalidInput" %>
<%@ Register TagPrefix="spring" Namespace="Spring.Web.UI.Controls" Assembly="Spring.Web" %>

<asp:Content runat="server" contentPlaceHolderId="content">
    <p>
    The value you entered is invalid
    </p>
<spring:DataBindingPanel runat="server">
    <table cellpadding="3" cellspacing="3" border="0">
        <tr>
            <td>Age you entered:</td>
            <td>
                <asp:Label ID="lblInput" runat="server" EnableViewState="false" BindingSource="Text" BindingTarget="Context.Items['input']" BindingDirection="TargetToSource" />
            </td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td><asp:Button ID="btnBack" runat="server" Text="Back" OnClick="btnBack_Click" /></td>
        </tr>
    </table>
</spring:DataBindingPanel>    
</asp:Content>
