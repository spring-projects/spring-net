<%@ Page Language="C#" MasterPageFile="~/Shared/MasterPage.master" Inherits="CustomerList" Codebehind="CustomerList.aspx.cs" %>
<%@ Reference page="CustomerEditor.aspx" %>

<asp:Content ID="content" ContentPlaceHolderID="content" runat="server">

<h1>Customers in database</h1>

<p>
    Below you can see all customers in the database paged to grid.
    By clicking customer name you enter the details and edit information.
    By clicking orders you can see all orders that customer has. Orders can
    be in shipped state (they have shipped date) or they can be waiting for 
    shipment. You can ship orders that aren't shipped yet on the orders screen.
</p>
<p>
    The customer entity is created with Spring and injected with an ICustomerClassificationCalculator
    which includes basic calculation algorithm that we can use to calculate customer's value to us.
</p>
<br />
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
            <asp:ButtonColumn HeaderText="Name" DataTextField="ContactName" CommandName="ViewCustomer" />
            <asp:BoundColumn HeaderText="Company" DataField="CompanyName"/>
            <asp:BoundColumn HeaderText="Classification" DataField="Classification"/>
            <asp:ButtonColumn CommandName="ViewOrders" Text="Orders" />
             <asp:TemplateColumn>
                 <ItemTemplate>
                     <asp:Button ID="BtnShowOrders" runat="server" Text="+" OnClick="BtnShowOrders_Click" />
                     <div style="position:relative">
                         <div id="DivCustomerOrderList" runat="server" visible="false">
                             <asp:DataGrid ID="DtgCustomerOrderList" runat="server" Visible="False" 
                                 AutoGenerateColumns="false" Width="400px">
                                 <Columns>
                                     <asp:BoundColumn HeaderText="Id" DataField="Id" />
                                     <asp:BoundColumn HeaderText="ShipName" DataField="ShipName" />
                                     <asp:BoundColumn HeaderText="ShipAddress" DataField="ShipAddress" />
                                 </Columns>
                             </asp:DataGrid>
                         </div>
                     </div>
                 </ItemTemplate>
             </asp:TemplateColumn>
         </Columns>
      </asp:DataGrid>
</asp:Content>