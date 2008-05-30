<%@ Register TagPrefix="spring" Namespace="Spring.Web.UI.Controls" Assembly="Spring.Web" %>
<%@ Page language="c#" Inherits="SpringAir.Web.TripForm" CodeBehind="TripForm.aspx.cs" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
  <spring:Content id="head" contentPlaceholderId="head" runat="server">
    <SCRIPT language="javascript">
      function showReturnCalendar(isVisible)
      {
          document.getElementById('returningOn').style.visibility = isVisible? '': 'hidden';
          document.getElementById('returningOnCalendar').style.visibility = isVisible? '': 'hidden';
      }
    </SCRIPT>
  </spring:Content>
  <body>
    <spring:Content id="body" contentPlaceholderId="body" runat="server">
      <DIV style="TEXT-ALIGN: center">
        <H4>
          <asp:Label id="caption" runat="server"></asp:Label></H4>
          <spring:ValidationSummary id="validationSummary" runat="server"></spring:ValidationSummary>
        <TABLE>
          <TR>
          <TR class="formLabel">
            <TD>&nbsp;</TD>
            <TD colSpan="3">
              <spring:RadioButtonGroup id="tripMode" runat="server">
                <asp:RadioButton id="OneWay" onclick="showReturnCalendar(false);" runat="server"></asp:RadioButton>
                <asp:RadioButton id="RoundTrip" onclick="showReturnCalendar(true);" runat="server"></asp:RadioButton>
              </spring:RadioButtonGroup></TD>
          </TR>
          <TR>
            <TD class="formLabel" align="right">
              <asp:Label id="leavingFrom" Runat="server"></asp:Label></TD>
            <TD nowrap="true">
              <asp:DropDownList id="leavingFromAirportCode" runat="server"></asp:DropDownList>
              <spring:ValidationError id="departureAirportErrors" runat="server"></spring:ValidationError></TD>
            <TD class="formLabel" align="right">
              <asp:Label id="goingTo" Runat="server"></asp:Label></TD>
            <TD nowrap="true">
              <asp:DropDownList id="goingToAirportCode" runat="server"></asp:DropDownList>
              <spring:ValidationError id="destinationAirportErrors" runat="server"></spring:ValidationError></TD>
          </TR>
          <TR>
            <TD class="formLabel" align="right">
              <asp:Label id="leavingOn" Runat="server"></asp:Label></TD>
            <TD nowrap="true">
              <spring:Calendar id="leavingFromDate" runat="server" width="75px" allowEditing="true" skin="system"></spring:Calendar>
              <spring:ValidationError id="departureDateErrors" runat="server"></spring:ValidationError></TD>
            <TD class="formLabel" align="right">
              <asp:Label id="returningOn" Runat="server"></asp:Label></TD>
            <TD nowrap="true">
              <DIV id="returningOnCalendar">
                <spring:Calendar id="returningOnDate" runat="server" width="75px" allowEditing="true" skin="system"></spring:Calendar>
                <spring:ValidationError id="returnDateErrors" runat="server"></spring:ValidationError></DIV>
            </TD>
          </TR>
          <TR>
            <TD class="buttonBar" colSpan="4"><BR>
              <asp:Button id="findFlights" runat="server"></asp:Button></TD>
          </TR>
        </TABLE>
      </DIV>
      <SCRIPT language="javascript">
          if (document.getElementById('tripMode').value == 'OneWay')
              showReturnCalendar(false);
          else
              showReturnCalendar(true);
      </SCRIPT>
    </spring:Content>
  </body>
</HTML>
