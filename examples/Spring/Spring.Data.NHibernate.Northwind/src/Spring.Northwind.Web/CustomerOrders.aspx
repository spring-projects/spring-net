<%@ Page Language="C#" MasterPageFile="~/Shared/MasterPage.master" AutoEventWireup="true"
    CodeFile="CustomerOrders.aspx.cs" Inherits="CustomerOrders" %>

<asp:Content ID="content" ContentPlaceHolderID="content" runat="server">

    <h1>Orders for customer <strong><asp:Literal ID="customerName" runat="server" /></strong></h1>

    <p>
        On this screen you can see all orders that customer has. By clicking "Process Orders" you
        can send service request to ship all unshipped orders for that customer. Behind the scenes
        Spring.NET will begin service level transaction that propagates to DAO layer which then
        automatically participates int the existing transaction.
    </p>

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
    
    <p>
        <asp:Button ID="processOrders" runat="server" Text="Process Orders" 
            onclick="processOrders_Click" />
    </p>
    <div class="actionPanel">
        <asp:LinkButton ID="customerList" runat="server" onclick="customerList_Click">&laquo; Back to customer list</asp:LinkButton>   
    </div>
    </div>
</asp:Content>
