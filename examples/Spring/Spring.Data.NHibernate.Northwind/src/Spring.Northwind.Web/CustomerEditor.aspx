<%@ Page Language="C#" MasterPageFile="~/Shared/MasterPage.master" CodeFile="CustomerEditor.aspx.cs"
    Inherits="CustomerEditor" %>

<asp:Content ID="content" ContentPlaceHolderID="content" runat="server">
    <spring:DataBindingPanel runat="server">
        <table>
            <tr>
                <td>
                    ID:</td>
                <td>
                    <asp:TextBox runat="server" BindingTarget="CurrentCustomer.Id" BindingDirection="TargetToSource"
                        ReadOnly="true" /></td>
            </tr>
            <tr>
                <td>
                    Contact:</td>
                <td>
                    <asp:TextBox runat="server" BindingTarget="CurrentCustomer.ContactName" /></td>
            </tr>
        </table>
    </spring:DataBindingPanel>
    <asp:Button runat="server" ID="btnSave" Text="Save" />
</asp:Content>
