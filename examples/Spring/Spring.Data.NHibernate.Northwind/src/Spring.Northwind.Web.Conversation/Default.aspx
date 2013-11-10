<%@ Page Language="C#" MasterPageFile="~/Shared/MasterPage.master" AutoEventWireup="true" Inherits="_Default" Codebehind="Default.aspx.cs" %>

<asp:Content ID="content" ContentPlaceHolderID="content" runat="server">

<h1>Welcome to Spring.NET Northwind sample application!</h1>

<p>
This sample demostrates Spring.NET NHibernate integration and concepts. All data access is done using NHibernate and
all transactions are automatically handled by Spring.NET.
</p>

<asp:LinkButton ID="customerList" runat="server" onclick="customerList_Click">Proceed to customer listing &raquo;</asp:LinkButton>
</asp:Content>