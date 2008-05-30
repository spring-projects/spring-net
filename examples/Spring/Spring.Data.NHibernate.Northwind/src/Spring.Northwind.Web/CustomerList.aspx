<%@ Page Language="C#" CodeFile="CustomerList.aspx.cs" Inherits="CustomerList" %>
<%@ Reference page="CustomerEditor.aspx" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
      <asp:DataGrid id="customerList" runat="server"
           AllowPaging="true"
           AllowSorting="false"
           BorderColor="black"
           BorderWidth="1"
           CellPadding="3"
           AutoGenerateColumns="false"
           ShowFooter="true"
      >
         <Columns>

            <asp:BoundColumn HeaderText="Id" DataField="ID" />
            <asp:BoundColumn HeaderText="Name" DataField="ContactName" />
            <asp:BoundColumn HeaderText="Company" DataField="CompanyName"/>
            <asp:ButtonColumn CommandName="EditCustomer" Text="Edit" />
            <asp:ButtonColumn CommandName="ViewOrders" Text="Orders" />

         </Columns>
      </asp:DataGrid>
    </div>
    </form>
</body>
</html>
