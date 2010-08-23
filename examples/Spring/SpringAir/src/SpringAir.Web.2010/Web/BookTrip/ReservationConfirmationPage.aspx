<%@ Page language="c#" Inherits="ReservationConfirmationPage" CodeFile="ReservationConfirmationPage.aspx.cs" %>
<%@ Import Namespace="SpringAir.Domain" %>

<asp:Content id="body" contentPlaceholderId="body" runat="server">
  <h4 align="center"><asp:Label id="caption" Runat="server" /></h4>
  <p align="center">
    <asp:Label id="confirmationLabel" Runat="server" />
  </p>
  <br />
  <br />
  <asp:Repeater id="itinerary" Runat="server">
    <HeaderTemplate>
      <table border="0" width="90%" cellpadding="0" cellspacing="0" align="center" class="suggestedTable">
        <thead>
          <tr class="suggestedTableCaption">
            <th colspan="5">
              <%= GetMessage("itinerary") %>
            </th>
          </tr>
          <tr class="suggestedTableColnames">
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
          </tr>
        </thead>
        <tbody>
    </HeaderTemplate>
    <ItemTemplate>
      <tr class="suggestedTableRow">
        <td><%# ((Flight) Container.DataItem).FlightNumber %></td>
        <td><%# ((Flight) Container.DataItem).DepartureTime.ToString() %></td>
        <td><%# ((Flight) Container.DataItem).DepartureAirport.Description %></td>
        <td><%# ((Flight) Container.DataItem).DestinationAirport.Description %></td>
        <td><%# ((Flight) Container.DataItem).Aircraft.Model %></td>
      </tr>
    </ItemTemplate>
    <FooterTemplate>
      </tbody></table>
    </FooterTemplate>
  </asp:Repeater>
</asp:Content>
