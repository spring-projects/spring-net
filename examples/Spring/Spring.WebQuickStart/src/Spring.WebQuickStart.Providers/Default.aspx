<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <h1><asp:Label ID="LblTest" runat="server" Text="Welcome to Provider Test"></asp:Label></h1>
     <a id="SitemapTestLink" href="SitemapTest.aspx"> test sitemap provider</a>
    <br />
    <a id="MembershipTestLink" href="TestProviders/MembershipTest.aspx"> test membership provider </a>    
    <br />
    <a id="ProfileTestLink" href="TestProviders/ProfileTest.aspx"> test profile provider </a>
    <br />
    <a id="RoleProviderLink" href="TestProviders/RolesTest.aspx"> test role provider<br /></a>
    <br />
        </div>
    </form>
</body>
</html>
