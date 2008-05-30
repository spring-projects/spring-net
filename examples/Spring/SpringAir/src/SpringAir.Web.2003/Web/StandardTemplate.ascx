<%@ Control language="c#" AutoEventWireup="false" inherits="SpringAir.Web.StandardTemplate" codebehind="StandardTemplate.ascx.cs" %>
<%@ Register TagPrefix="spring" Namespace="Spring.Web.UI.Controls" Assembly="Spring.Web" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
  <spring:Head runat="server">
    <title>
      <spring:ContentPlaceHolder id="title" runat="server">
            <%= GetMessage("default.title") %>  
      </spring:ContentPlaceHolder>
    </title>
    <link href="<%= Page.CssRoot %>/default.css" type="text/css" rel="stylesheet">
    <spring:ContentPlaceHolder id="head" runat="server"></spring:ContentPlaceHolder>
  </spring:Head>
  <body>
    <form id="form" method="post" runat="server">
      <div id="container">
        <div id="logo">
          <spring:LocalizedImage id="logoImage" imageName="spring-air-logo.jpg" borderWidth="0" runat="server" />
        </div>
        <div id="content">
          <div class="panel11" style="BACKGROUND: url(<%= Page.ImagesRoot %>/panel-top-left.gif)"></div>
          <div class="panel12" style="BACKGROUND: url(<%= Page.ImagesRoot %>/panel-top-middle.gif)"></div>
          <div class="panel13" style="BACKGROUND: url(<%= Page.ImagesRoot %>/panel-top-right.gif)"></div>
          <div class="panel21" style="BACKGROUND: url(<%= Page.ImagesRoot %>/panel-left-middle.gif)"></div>
          <div class="panel23" style="BACKGROUND: url(<%= Page.ImagesRoot %>/panel-right-middle.gif)"></div>
          <div class="panel31" style="BACKGROUND: url(<%= Page.ImagesRoot %>/panel-bottom-left.gif)"></div>
          <div class="panel32" style="BACKGROUND: url(<%= Page.ImagesRoot %>/panel-bottom-middle.gif)"></div>
          <div class="panel33" style="BACKGROUND: url(<%= Page.ImagesRoot %>/panel-bottom-right.gif)"></div>
          <div class="panelContent">
            <spring:ContentPlaceHolder id="body" runat="server" />
            <br>
            <br>
          </div>
        </div>
        <div id="copyright">
          <img class="divider" src="<%= Page.ImagesRoot %>/line.jpg" alt="" width="500" height="1">
          <br>
          © Copyright 2005-2006, <a href="http://www.springframework.net/" title="Spring.NET Framework">
            Spring.NET</a>, under the terms of the Apache 2.0 software license
          license.
          <br>
          <asp:LinkButton ID="english" Runat="server" CommandArgument="en-US" CausesValidation="False">English</asp:LinkButton>&nbsp;
          <asp:LinkButton ID="serbianLatin" Runat="server" CommandArgument="sr-SP-Latn" CausesValidation="False">Srpski</asp:LinkButton>&nbsp;
          <asp:LinkButton ID="serbianCyrillic" Runat="server" CommandArgument="sr-SP-Cyrl" CausesValidation="False">Српски</asp:LinkButton>
        </div>
      </div>
    </form>
  </body>
</html>
