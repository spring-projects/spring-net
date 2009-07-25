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
            <a href="customerlist.aspx">&laquo; Back to customer list</a> | <a href="CustomerEditor.aspx">
                Edit Customer &raquo;</a>
        </div>
    </div>
</asp:Content>
