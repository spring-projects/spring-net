<%@ Page Language="C#" MasterPageFile="~/Shared/MasterPage.master" CodeFile="CustomerEditor.aspx.cs"
    Inherits="CustomerEditor" %>

<asp:Content ID="content" ContentPlaceHolderID="content" runat="server">
    <spring:DataBindingPanel runat="server" ID="panel">
        <fieldset title="Customer details">
            <table>
                <tr>
                    <td>
                        ID:</td>
                    <td>
                        <asp:Label runat="server" BindingTarget="CurrentCustomer.Id" BindingDirection="TargetToSource"
                            ReadOnly="true" /></td>
                </tr>
                <tr>
                    <td>
                        Contact:</td>
                    <td>
                        <asp:Label runat="server" BindingTarget="CurrentCustomer.ContactName" /></td>
                </tr>
            </table>
        </fieldset>
    </spring:DataBindingPanel>
    <asp:Button runat="server" ID="btnSave" Text="Edit" />
</asp:Content>
