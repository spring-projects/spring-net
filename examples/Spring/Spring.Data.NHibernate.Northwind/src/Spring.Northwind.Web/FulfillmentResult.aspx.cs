using System;
using System.IO;

using Common.Logging;

using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository;

using Spring.Northwind.Service;
using Spring.Web.UI;

using ILog=log4net.ILog;
using LogManager=log4net.LogManager;

public partial class FullfillmentResult : Page
{
    private IFulfillmentService fulfillmentService;
    private ICustomerEditController customerEditController;

    public IFulfillmentService FulfillmentService
    {
        set { fulfillmentService = value; }
    }

    public ICustomerEditController CustomerEditController
    {
        set { customerEditController = value; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        // gather log4net output with small hack to get results...
        ILoggerRepository repository = LogManager.GetRepository();
        IAppender[] appenders = repository.GetAppenders();
        MemoryAppender appender = null;
        foreach (IAppender a in appenders)
        {
            if (a is MemoryAppender)
            {
                // we found our appender to look results from
                appender = a as MemoryAppender;
                break;
            }
        }

        if (appender != null)
        {
            appender.Clear();
            fulfillmentService.ProcessCustomer(customerEditController.CurrentCustomer.Id);
            LoggingEvent[] events = appender.GetEvents();
            StringWriter stringWriter = new StringWriter();
            PatternLayout layout = new PatternLayout("%date{HH:mm:ss} %-5level %logger{1}: %message<br />");
            foreach (LoggingEvent loggingEvent in events)
            {
                layout.Format(stringWriter, loggingEvent);
            }

            results.Text = stringWriter.ToString();
        }

    }
    protected void customerOrders_Click(object sender, EventArgs e)
    {
        SetResult("Back");
    }
}
