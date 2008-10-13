<%@ Page Language="C#" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title><%=Title%></title>
</head>
<body>
    <h2><a href="../../Default.aspx">Welcome to Spring.NET Web Framework Quick Start Guide</a></h2>
    <h2><a href="../Default.aspx">Dependency Injection</a></h2>
    <div>
    <form id="form1" runat="server">
    <p>
    The links below demonstrate dependency injection on custom handlers using <b>MappingHandlerFactory</b>. 
    The example code shows, how to map all url requests to spring and let MappingHandlerFactory resolve urls to 
    container-managed IHttpHandlerFactory or IHttpHandler objects.
    </p>
    <pre>
        // web.config

        &lt;httpHandlers&gt;
          &lt;!-- map all requests to spring (just for demo - don't do this at home!) --&gt;
          &lt;add verb=&quot;*&quot; path=&quot;*.*&quot; type=&quot;Spring.Web.Support.MappingHandlerFactory, Spring.Web&quot; /&gt;
        &lt;/httpHandlers&gt;

        // spring-objects.config

        &lt;object type=&quot;Spring.Web.Support.MappingHandlerFactoryConfigurer, Spring.Web&quot;&gt;
          &lt;property name=&quot;HandlerMap&quot;&gt;
	          &lt;dictionary&gt;
	              &lt;entry key=&quot;\.ashx$&quot; value=&quot;standardHandlerFactory&quot; /&gt;
	              &lt;!-- map any request ending with *.whatever to standardHandlerFactory --&gt;
	              &lt;entry key=&quot;\.whatever$&quot; value=&quot;specialHandlerFactory&quot; /&gt;
	          &lt;/dictionary&gt;
          &lt;/property&gt;
        &lt;/object&gt;

        &lt;object name=&quot;standardHandlerFactory&quot; type=&quot;Spring.Web.Support.DefaultHandlerFactory, Spring.Web&quot; /&gt;

        &lt;object name=&quot;specialHandlerFactory&quot; type=&quot;MySpecialHandlerFactoryImpl&quot; /&gt;
    </pre>
    <ul>
		<li><a href="DemoHandler.ashx">DemoHandler.ashx</a></li>
		<li><a href="AnotherCustomHandler.whatever">AnotherCustomHandler.whatever</a></li>
	</ul>
    </form>
    </div>
</body>
</html>
