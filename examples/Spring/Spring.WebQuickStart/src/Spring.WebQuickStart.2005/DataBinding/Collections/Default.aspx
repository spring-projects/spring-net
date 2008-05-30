<%@ Page Language="C#" EnableViewState="false" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="DataBinding_Collections_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Data Binding - HttpRequestListBindingContainer example</title>
    <script>
        function addRow() {
            var src = document.getElementById('itemTemplate');
            var dst = document.getElementById('products');
            dst.innerHTML += '<div>' + src.innerHTML + '</div>';
        }
    </script>
</head>
<body>
    <h2><a href="../../Default.aspx">Welcome to Spring.NET Web Framework Quick Start Guide</a></h2>
    <h2><a href="../Default.aspx">Bi-directional DataBinding</a></h2>
    <form id="form1" runat="server">
    <div>
    <h2>HttpRequestListBindingContainer example</h2>
    <p>
    This form demonstrates unbinding data from HttpRequest.Form collection into a list model using HttpRequestListBindingContainer.
    </p>
    <p>
    HttpRequestListBindingContainer extracts posted values from the request and populates the specified IList by creating objects 
    of the type specified and populating each of these objects according to the requestBindings collection.
    </p>
    <pre>
        HttpRequestListBindingContainer requestBindings =
            new HttpRequestListBindingContainer("sku,name,quantity,price", "Products", typeof(ProductInfo));
        requestBindings.AddBinding("sku", "Sku");
        requestBindings.AddBinding("name", "Name");
        requestBindings.AddBinding("quantity", "Quantity", quantityFormatter);
        requestBindings.AddBinding("price", "Price", priceFormatter);
    </pre>
    Edit List: <br />
    <div id="products">
        <table>
            <tr><td>Firstname</td><td>Lastname</td><td>Quantity</td><td>Price/Item</td></tr>
    <% 
    foreach (ProductInfo p in Products) 
    {
    %>
            <tr>
                <td><input name="sku" type="text" value="<%= p.Sku %>" style="width: 50px;" /></td>
                <td><input name="name" type="text" value="<%= p.Name %>" style="width: 150px;" /></td>
                <td><input name="quantity" type="text" value="<%= QuantityFormatter.Format(p.Quantity) %>" style="width: 30px;" /></td>
                <td><input name="price" type="text" value="<%= PriceFormatter.Format(p.Price) %>" style="width: 50px;" /></td>
            </tr>
    <%
    } 
    %>
        </table>
    </div>
    <asp:Button ID="btnAdd" Text="Add" runat="server" OnClick="btnAdd_Click" />
    <asp:Button ID="btnSave" Text="Save" runat="server" OnClick="btnSave_Click" />
    <button onclick="addRow();">Add row using javascript</button>
    </div>
    </form>
      <div id="itemTemplate" style="display: none">
        <input name="sku" type="text" value="" style="width: 50px;" />
        <input name="name" type="text" value="" style="width: 150px;" />
        <input name="quantity" type="text" value="0" style="width: 30px;" />
        <input name="price" type="text" value="<%= PriceFormatter.Format(0) %>" style="width: 50px;" />
        <br />
      </div>  
</body>
</html>
