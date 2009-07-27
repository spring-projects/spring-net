<%@ Page Language="C#" MasterPageFile="~/Shared/MasterPage.master" CodeFile="CustomerEditor.aspx.cs"
    Inherits="CustomerEditor" %>

<asp:Content ID="content" ContentPlaceHolderID="content" runat="server">
    <h1>
        Update customer information</h1>
    <div align="center">
        <spring:DataBindingPanel runat="server">
            <fieldset>
                <legend>Customer details</legend>
                <table>
                    <tr>
                        <td class="formLabelCell">
                            ID:</td>
                        <td class="formValueCell">
                            <asp:TextBox runat="server" BindingTarget="CurrentCustomer.Id" BindingDirection="TargetToSource"
                                ReadOnly="true" /></td>
                    </tr>
                    <tr>
                        <td class="formLabelCell">
                            Contact:</td>
                        <td class="formValueCell">
                            <asp:TextBox runat="server" BindingTarget="CurrentCustomer.ContactName" /></td>
                    </tr>
                    <tr>
                        <td colspan="2" style="text-align: right; padding-top: 10px">
                            <asp:Button runat="server" ID="btnSave" Text="Save" /></td>
                    </tr>
                </table>
            </fieldset>
        </spring:DataBindingPanel>
        <div class="actionPanel">
            <asp:LinkButton ID="customerList" runat="server" onclick="customerList_Click">&laquo; Back to customer list</asp:LinkButton> |
            <asp:LinkButton ID="cancel" runat="server" onclick="cancel_Click">Cancel edit &raquo;</asp:LinkButton>
        </div>
    </div>
</asp:Content>
