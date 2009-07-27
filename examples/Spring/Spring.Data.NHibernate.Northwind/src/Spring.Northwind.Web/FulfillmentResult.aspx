<%@ Page Language="C#" MasterPageFile="~/Shared/MasterPage.master" AutoEventWireup="true"
    CodeFile="FulfillmentResult.aspx.cs" Inherits="FullfillmentResult" %>

<%@ Register Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI" TagPrefix="asp" %>
<asp:Content ID="content" ContentPlaceHolderID="content" runat="server">
    <h1>Order processing results</h1>
    
    <p>
        Below you can see the results from order fullfilment.
    </p>
    
    <div align="center">
        <div class="actionResultWindow">
            <asp:Label ID="results" runat="server" Text=""></asp:Label>
        </div>
        <div class="actionPanel">
            <asp:LinkButton ID="customerOrders" runat="server" OnClick="customerOrders_Click">&laquo; Back to customer order list</asp:LinkButton>
        </div>
    </div>
</asp:Content>
