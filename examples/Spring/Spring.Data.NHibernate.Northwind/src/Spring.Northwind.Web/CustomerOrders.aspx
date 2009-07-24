<%@ Page Language="C#" MasterPageFile="~/Shared/MasterPage.master" AutoEventWireup="true"
    CodeFile="CustomerOrders.aspx.cs" Inherits="CustomerOrders" %>

<asp:Content ID="content" ContentPlaceHolderID="content" runat="server">
    <asp:DataGrid ID="customerOrders" runat="server" AllowPaging="false" AllowSorting="false"
        BorderColor="black" BorderWidth="1" CellPadding="3" AutoGenerateColumns="false"
        ShowFooter="true">
        <Columns>
            <asp:BoundColumn HeaderText="OrderID" DataField="ID" />
            <asp:BoundColumn HeaderText="OrderDate" DataFormatString="{0:d}" DataField="OrderDate" />
            <asp:BoundColumn HeaderText="ShippedDate" DataFormatString="{0:d}" DataField="ShippedDate" />
        </Columns>
    </asp:DataGrid>
</asp:Content>
