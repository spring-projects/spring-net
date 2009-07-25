<%@ Page Language="C#" MasterPageFile="~/Shared/MasterPage.master" CodeFile="CustomerList.aspx.cs" Inherits="CustomerList" %>
<%@ Reference page="CustomerEditor.aspx" %>

<asp:Content ID="content" ContentPlaceHolderID="content" runat="server">

<h1>Customers in database</h1>

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
            <asp:TemplateColumn HeaderText="Name">
                <ItemTemplate><a href="CustomerView.aspx?id="><%# Eval("ContactName")%></a></ItemTemplate>
            </asp:TemplateColumn> 
            <asp:BoundColumn HeaderText="Company" DataField="CompanyName"/>
            <asp:ButtonColumn CommandName="ViewOrders" Text="Orders" />

         </Columns>
      </asp:DataGrid>
</asp:Content>