<%@ Page Language="C#" MasterPageFile="~/Shared/MasterPage.master" AutoEventWireup="true"
    CodeFile="CustomerOrders.aspx.cs" Inherits="CustomerOrders" %>

<asp:Content ID="content" ContentPlaceHolderID="content" runat="server">

    <h1>Orders for customer <strong><asp:Literal ID="customerName" runat="server" /></strong></h1>

    <div align="center">
    <asp:DataGrid ID="customerOrders" runat="server" AllowPaging="false" AllowSorting="false"
        BorderColor="black" BorderWidth="1" CellPadding="3" AutoGenerateColumns="false"
        ShowFooter="true">
        <HeaderStyle CssClass="" />
        <ItemStyle CssClass="" />
        <AlternatingItemStyle CssClass="" />
        <Columns>
            <asp:BoundColumn HeaderText="OrderID" DataField="ID" />
            <asp:BoundColumn HeaderText="OrderDate" DataFormatString="{0:d}" DataField="OrderDate" />
            <asp:BoundColumn HeaderText="ShippedDate" DataFormatString="{0:d}" DataField="ShippedDate" />
        </Columns>
    </asp:DataGrid>
    
    <div class="actionPanel">
        <a href="CustomerList.aspx">&laquo; Back to customer listing</a>    
    </div>
    </div>
</asp:Content>
