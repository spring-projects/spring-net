<%@ Import Namespace="SpringAir.Domain" %>
<%@ Page language="c#" Inherits="SpringAir.Web.SuggestedFlights" CodeBehind="SuggestedFlights.aspx.cs" %>
<%@ Register TagPrefix="spring" Namespace="Spring.Web.UI.Controls" Assembly="Spring.Web" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
  <body>
    <spring:Content id="body" contentPlaceholderId="body" runat="server">
      <H4 align="center">
        <asp:Label id="caption" runat="server"></asp:Label></H4>
        <spring:ValidationSummary id="validationSummary" provider="summary" runat="server"></spring:ValidationSummary><BR>
        <TABLE cellSpacing="0" cellPadding="0" width="90%" align="center" border="0">
        <TR>
          <TD><INPUT id="outboundFlight" type="hidden" name="outboundFlight" runat="server">
            <asp:Repeater id="outboundFlightList" Runat="server">
              <HeaderTemplate>
                <table border="0" width="100%" cellpadding="0" cellspacing="0" class="suggestedTable">
                  <thead>
                    <tr class="suggestedTableCaption">
                      <th colspan="7">
                        <%= GetMessage("outboundFlights") %>
                      </th>
                    </tr>
                    <tr class="suggestedTableColnames">
                      <th>
                        &nbsp;
                      </th>
                      <th>
                        <%= GetMessage("flightNumber") %>
                      </th>
                      <th>
                        <%= GetMessage("departureDate") %>
                      </th>
                      <th>
                        <%= GetMessage("departureAirport") %>
                      </th>
                      <th>
                        <%= GetMessage("destinationAirport") %>
                      </th>
                      <th>
                        <%= GetMessage("aircraft") %>
                      </th>
                      <th>
                        <%= GetMessage("seatPlan") %>
                      </th>
                    </tr>
                  </thead>
                  <tbody>
              </HeaderTemplate>
              <ItemTemplate>
                <tr class="suggestedTableRow">
                  <td>
                    <input name="outboundFlightSelection" type="radio"
					        onclick="outboundFlight.value = '<%# Container.ItemIndex %>';"/></td>
                  <td><%# ((Flight) Container.DataItem).FlightNumber %></td>
                  <td><%# ((Flight) Container.DataItem).DepartureTime.ToString() %></td>
                  <td><%# ((Flight) Container.DataItem).DepartureAirport.Description %></td>
                  <td><%# ((Flight) Container.DataItem).DestinationAirport.Description %></td>
                  <td><%# ((Flight) Container.DataItem).Aircraft.Model %></td>
                  <td><%# ((Flight) Container.DataItem).SeatPlan %></td>
                </tr>
              </ItemTemplate>
              <FooterTemplate>
      </TABLE>
              </FooterTemplate>
            </asp:Repeater></TD></TR>
  <TR>
        <TD>&nbsp;<BR>
        </TD>
      </TR>
  <TR>
        <TD><INPUT id="returnFlight" type="hidden" name="returnFlight" runat="server">
          <asp:Repeater id="returnFlightList" Runat="server">
            <HeaderTemplate>
              <table border="0" width="100%" cellpadding="0" cellspacing="0" class="suggestedTable">
                <thead>
                  <tr class="suggestedTableCaption">
                    <th colspan="7">
                      <%= GetMessage("returnFlights") %>
                    </th>
                  </tr>
                  <tr class="suggestedTableColnames">
                    <th>
                      &nbsp;</th>
                    <th>
                      <%= GetMessage("flightNumber") %>
                    </th>
                    <th>
                      <%= GetMessage("departureDate") %>
                    </th>
                    <th>
                      <%= GetMessage("departureAirport") %>
                    </th>
                    <th>
                      <%= GetMessage("destinationAirport") %>
                    </th>
                    <th>
                      <%= GetMessage("aircraft") %>
                    </th>
                    <th>
                      <%= GetMessage("seatPlan") %>
                    </th>
                  </tr>
                </thead>
                <tbody>
            </HeaderTemplate>
            <ItemTemplate>
              <tr class="suggestedTableRow">
                <td>
                  <input id="returnFlightSelection" name="returnFlightSelection" type="radio" 
						          onclick="returnFlight.value = '<%# Container.ItemIndex %>';"/></td>
                <td><%# ((Flight) Container.DataItem).FlightNumber %></td>
                <td><%# ((Flight) Container.DataItem).DepartureTime.ToString() %></td>
                <td><%# ((Flight) Container.DataItem).DepartureAirport.Description %></td>
                <td><%# ((Flight) Container.DataItem).DestinationAirport.Description %></td>
                <td><%# ((Flight) Container.DataItem).Aircraft.Model %></td>
                <td><%# ((Flight) Container.DataItem).SeatPlan %></td>
              </tr>
            </ItemTemplate>
            <FooterTemplate>
              </tbody></table>
            </FooterTemplate>
          </asp:Repeater></TD>
      </TR>
  <TR>
        <TD class="buttonBar"><BR>
          <asp:Button id="bookFlights" runat="server"></asp:Button></TD>
      </TR></TBODY></TABLE>
    </spring:Content>
  </body>
</HTML>
