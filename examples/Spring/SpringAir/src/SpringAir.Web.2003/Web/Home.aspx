<%@ Page language="c#" Inherits="SpringAir.Web.Home" CodeBehind="Home.aspx.cs" %>
<%@ Register TagPrefix="spring" Namespace="Spring.Web.UI.Controls" Assembly="Spring.Web" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
	<body>
		<spring:Content id="body" contentPlaceholderId="body" runat="server">
			<p>
	            SpringAir models a flight reservation system.
                <br />
	            Using SpringAir, you can browse flights, <a href="BookTrip/TripForm.aspx">book a trip</a> and use <a href="../BookingAgentWebService.asmx">Booking Agent Web Service</a>.
            </p>
			<p>
				SpringAir demonstrates the following features of Spring.NET's web support...
			</p>
			<ul>
				<li>Spring.NET IoC container configuration</li>
				<li>Dependency Injection as applied to ASP.NET pages</li>
				<li>Master Page support in ASP.NET 1.1</li>
				<li>Bi-directional databinding</li>
				<li>Web Service support</li>
				<li>Declarative validation of domain objects</li>
				<li>Internationalization</li>
				<li>Result mapping to better encapsulate page navigation flows</li>
			</ul>
		</spring:Content>
	</body>
</html>
