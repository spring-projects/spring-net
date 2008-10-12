<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Ok.aspx.cs" Inherits="Navigation_Ok" %>

<asp:Content runat="server" contentPlaceHolderId="content">
    <p>
    Passcode you entered was ok
    </p>
<spring:DataBindingPanel runat="server">
    <table cellpadding="3" cellspacing="3" border="0">
        <tr>
            <td>The passcode is correct:</td>
            <td>
                <asp:Label ID="lblInput" runat="server" BindingSource="Text" BindingTarget="Request['age']" BindingDirection="TargetToSource" />
            </td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td><asp:Button ID="btnRestart" runat="server" Text="Back" OnClick="btnRestart_Click" /></td>
        </tr>
    </table>
</spring:DataBindingPanel>    
</asp:Content>