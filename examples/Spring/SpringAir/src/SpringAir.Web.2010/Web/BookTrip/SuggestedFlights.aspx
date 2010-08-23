<%@ Page Language="c#" Inherits="SuggestedFlights" CodeFile="SuggestedFlights.aspx.cs" %>
<%@ Import Namespace="SpringAir.Domain" %>

<asp:Content ID="body" ContentPlaceHolderID="body" runat="server">
    <h4 align="center">
        <asp:Label ID="caption" runat="server" /></h4>    
    <br />
    <table cellspacing="0" cellpadding="0" width="90%" align="center" border="0">
        <tr>
            <td>&nbsp;<spring:ValidationSummary ID="validationSummary" Provider="summary" runat="server" /><br/></td>
        </tr>    
        <tr>
            <td>
                <input id="outboundFlight" type="hidden" name="outboundFlight" runat="server" />
                <asp:Repeater ID="outboundFlightList" runat="server">
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
                                <input name="outboundFlightSelection" type="radio" onclick="<%= outboundFlight.ClientID %>.value = '<%# Container.ItemIndex %>';" /></td>
                            <td>
                                <%# ((Flight) Container.DataItem).FlightNumber %>
                            </td>
                            <td>
                                <%# ((Flight) Container.DataItem).DepartureTime.ToString() %>
                            </td>
                            <td>
                                <%# ((Flight) Container.DataItem).DepartureAirport.Description %>
                            </td>
                            <td>
                                <%# ((Flight) Container.DataItem).DestinationAirport.Description %>
                            </td>
                            <td>
                                <%# ((Flight) Container.DataItem).Aircraft.Model %>
                            </td>
                            <td>
                                <%# ((Flight) Container.DataItem).SeatPlan %>
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                            </tbody></table>
                    </FooterTemplate>
                </asp:Repeater>
            </td>
        </tr>
        <tr>
            <td>
                &nbsp;<br/>
            </td>
        </tr>
        <tr>
            <td>
                <input id="returnFlight" type="hidden" name="returnFlight" runat="server"/>
                <asp:Repeater ID="returnFlightList" runat="server">
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
                                <input id="returnFlightSelection" name="returnFlightSelection" type="radio" onclick="<%= returnFlight.ClientID %>.value = '<%# Container.ItemIndex %>';" /></td>
                            <td>
                                <%# ((Flight) Container.DataItem).FlightNumber %>
                            </td>
                            <td>
                                <%# ((Flight) Container.DataItem).DepartureTime.ToString() %>
                            </td>
                            <td>
                                <%# ((Flight) Container.DataItem).DepartureAirport.Description %>
                            </td>
                            <td>
                                <%# ((Flight) Container.DataItem).DestinationAirport.Description %>
                            </td>
                            <td>
                                <%# ((Flight) Container.DataItem).Aircraft.Model %>
                            </td>
                            <td>
                                <%# ((Flight) Container.DataItem).SeatPlan %>
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                        </tbody></table>
                    </FooterTemplate>
                </asp:Repeater>
            </td>
        </tr>
        <tr>
            <td class="buttonBar">
                <br/>
                <asp:Button ID="bookFlights" OnClick="BookFlights" runat="server"/></td>
        </tr>
    </table>
</asp:Content>
