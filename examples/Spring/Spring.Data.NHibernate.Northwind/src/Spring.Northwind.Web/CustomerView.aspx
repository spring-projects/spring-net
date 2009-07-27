<%@ Page Language="C#" MasterPageFile="~/Shared/MasterPage.master" CodeFile="CustomerView.aspx.cs"
    Inherits="CustomerView" %>

<asp:Content ID="content" ContentPlaceHolderID="content" runat="server">
    <div align="center">
        <spring:DataBindingPanel runat="server" ID="panel">
            <fieldset>
                <legend>Customer details</legend>
                <table>
                    <tr>
                        <td class="formLabelCell">
                            ID:</td>
                        <td class="formValueCell">
                            <%= CurrentCustomer.Id %>
                        </td>
                    </tr>
                    <tr>
                        <td class="formLabelCell">
                            Contact:</td>
                        <td class="formValueCell">
                            <%= CurrentCustomer.ContactName %>
                        </td>
                    </tr>
                </table>
            </fieldset>
        </spring:DataBindingPanel>
        <div class="actionPanel">
            <asp:LinkButton ID="customerList" runat="server" onclick="customerList_Click">&laquo; Back to customer list</asp:LinkButton> | 
            <asp:LinkButton ID="editCustomer" runat="server" onclick="editCustomer_Click">Edit Customer &raquo;</asp:LinkButton>
        </div>
    </div>
</asp:Content>
