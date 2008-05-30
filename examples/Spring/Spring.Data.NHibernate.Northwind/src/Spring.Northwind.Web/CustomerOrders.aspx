<%@ Page Language="C#" AutoEventWireup="true" CodeFile="CustomerOrders.aspx.cs" Inherits="CustomerOrders" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
      <asp:DataGrid id="customerOrders" runat="server"
           AllowPaging="false"
           AllowSorting="false"
           BorderColor="black"
           BorderWidth="1"
           CellPadding="3"
           AutoGenerateColumns="false"
           ShowFooter="true"
      >
         <Columns>

            <asp:BoundColumn HeaderText="OrderID" DataField="ID" />
            <asp:BoundColumn HeaderText="OrderDate" DataField="OrderDate" />
            <asp:BoundColumn HeaderText="ShippedDate" DataField="ShippedDate"/>

         </Columns>
      </asp:DataGrid>
    </div>
    </form>
</body>
</html>
