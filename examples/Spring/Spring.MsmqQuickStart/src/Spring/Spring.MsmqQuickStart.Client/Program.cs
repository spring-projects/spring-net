using System;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Spring.Context;
using Spring.Context.Support;
using Spring.MsmqQuickStart.Client.UI;


namespace Spring.MsmqQuickStart.Client
{
    static class Program
    {

        private static readonly ILogger log = LogManager.GetLogger(typeof(Program));

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                log.LogInformation("Running....");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                using (IApplicationContext ctx = ContextRegistry.GetContext())
                {
                    StockForm stockForm = new StockForm();
                    Application.ThreadException += ThreadException;
                    Application.Run(stockForm);
                }
            }
            catch (Exception e)
            {
                log.LogError(e, "Spring.MsmqQuickStart.Client is broken.");
            }
        }

        private static void ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            log.LogError(e.Exception, "Uncaught application exception.");
            Application.Exit();
        }
    }
}
