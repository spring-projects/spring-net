<%@ Page Language="c#" Inherits="TripForm" CodeFile="TripForm.aspx.cs" %>

<asp:Content ID="head" ContentPlaceHolderID="head" runat="server">

    <script language="javascript" type="text/javascript">
      <!--
      function showReturnCalendar(isVisible)
      {
          document.getElementById('<%= returningOnDate.ClientID %>').style.visibility = isVisible? '': 'hidden';
          document.getElementById('returningOnCalendar').style.visibility = isVisible? '': 'hidden';
      }
      -->
    </script>

</asp:Content>

<asp:Content ID="body" ContentPlaceHolderID="body" runat="server">
    <div style="text-align: center">
        <h4><asp:Label ID="caption" runat="server"></asp:Label></h4>
        <spring:ValidationSummary ID="validationSummary" runat="server" />
        <table>
            <tr class="formLabel">
                <td>&nbsp;</td>
                <td colspan="3">
                    <spring:RadioButtonGroup ID="tripMode" runat="server">
                        <asp:RadioButton ID="OneWay" onclick="showReturnCalendar(false);" runat="server" />
                        <asp:RadioButton ID="RoundTrip" onclick="showReturnCalendar(true);" runat="server" />
                    </spring:RadioButtonGroup>
                </td>
            </tr>
            <tr>
                <td class="formLabel" align="right">
                    <asp:Label ID="leavingFrom" runat="server" /></td>
                <td nowrap="nowrap">
                    <asp:DropDownList ID="leavingFromAirportCode" runat="server" />
                    <spring:ValidationError id="departureAirportErrors" runat="server" />
                </td>
                <td class="formLabel" align="right">
                    <asp:Label ID="goingTo" runat="server" /></td>
                <td nowrap="nowrap">
                    <asp:DropDownList ID="goingToAirportCode" runat="server" />
                    <spring:ValidationError id="destinationAirportErrors" runat="server" />
                </td>
            </tr>
            <tr>
                <td class="formLabel" align="right">
                    <asp:Label ID="leavingOn" runat="server" /></td>
                <td nowrap="nowrap">
                    <spring:Calendar ID="leavingFromDate" runat="server" Width="75px" AllowEditing="true" Skin="system" />
                    <spring:ValidationError id="departureDateErrors" runat="server" />
                </td>
                <td class="formLabel" align="right">
                    <asp:Label ID="returningOn" runat="server" /></td>
                <td nowrap="nowrap">
                    <div id="returningOnCalendar">
                        <spring:Calendar ID="returningOnDate" runat="server" Width="75px" AllowEditing="true" Skin="system" />
                        <spring:ValidationError id="returnDateErrors" runat="server" />
                    </div>
                </td>
            </tr>
            <tr>
                <td class="buttonBar" colspan="4">
                    <br/>
                    <asp:Button ID="findFlights" OnClick="SearchForFlights" runat="server"/></td>
            </tr>
        </table>
    </div>

    <script language="javascript" type="text/javascript">
          if (document.getElementById('<%= tripMode.ClientID %>').value == 'OneWay')
              showReturnCalendar(false);
          else
              showReturnCalendar(true);
    </script>

</asp:Content>
