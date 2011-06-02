<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Spring.CachingQuickStart.Web._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head>
    <title>Caching QuickStart</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <h2>
            Using Cache Aspect:
        </h2>
        <p>
            <asp:TextBox ID="GetByIdTextBox" runat="server"></asp:TextBox>
            <br />
            <asp:Button ID="GetByIdButton" runat="server" Text="GetById" OnClick="GetByIdButton_Click" />
            <br />
            <asp:Label ID="GetByIdLabel" runat="server" Text=""></asp:Label>
        </p>
        <hr />
        <p>
            <asp:Button ID="FindAllButton" runat="server" Text="FindAll" OnClick="FindAllButton_Click" />
            <br />
            <asp:Repeater ID="FindAllRepeater" runat="server">
                <HeaderTemplate>
                    <table cellpadding="1" cellspacing="1" border="1">
                        <tr>
                            <th>
                                ID
                            </th>
                            <th>
                                TITLE
                            </th>
                        </tr>
                </HeaderTemplate>
                <ItemTemplate>
                        <tr>
                            <td>
                                <%# Eval("ID") %>
                            </td>
                            <td>
                                <%# Eval("Title") %>
                            </td>
                        </tr>
                </ItemTemplate>
                <FooterTemplate>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
        </p>
        <hr />
        <p>
            ID:&nbsp;<asp:TextBox ID="SaveIdTextBox" runat="server"></asp:TextBox>
            <br />
            Title:&nbsp;<asp:TextBox ID="SaveTitleTextBox" runat="server"></asp:TextBox>
            <br />
            <asp:Button ID="SaveButton" runat="server" Text="Save" 
                onclick="SaveButton_Click" />
        </p>
        <hr />
        <p>
            <asp:TextBox ID="DeleteIdTextBox" runat="server"></asp:TextBox>
            <br />
            <asp:Button ID="DeleteButton" runat="server" Text="Delete" 
                onclick="DeleteButton_Click" />
        </p>
        <hr />
        <p>
            &nbsp;</p>
        <h2>
            Using Caching API (ICache interface) programmatically to show cache content:
        </h2>
        <p>
            <asp:Repeater ID="CacheRepeater" runat="server">
                <HeaderTemplate>
                    <table cellpadding="1" cellspacing="1" border="1">
                        <tr>
                            <th>
                                KEY
                            </th>
                            <th>
                                VALUE
                            </th>
                        </tr>
                </HeaderTemplate>
                <ItemTemplate>
                        <tr>
                            <td>
                                <%# Eval("Key") %>
                            </td>
                            <td>
                                <%# Eval("Value") %>
                            </td>
                        </tr>
                </ItemTemplate>
                <FooterTemplate>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
            <br />
            <asp:Button ID="ClearButton" runat="server" Text="Clear" 
                    onclick="ClearButton_Click" />
            <asp:Button ID="RefreshButton" runat="server" Text="Refresh" 
                    onclick="RefreshButton_Click" />
        </p>
    </div>
    </form>
</body>
</html>
