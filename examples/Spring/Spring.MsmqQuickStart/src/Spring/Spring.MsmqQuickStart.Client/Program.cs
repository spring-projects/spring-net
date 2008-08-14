using System;
using System.Threading;
using System.Windows.Forms;
using Common.Logging;
using Spring.Context;
using Spring.Context.Support;
using Spring.MsmqQuickStart.Client.UI;


namespace Spring.MsmqQuickStart.Client
{
    static class Program
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                log.Info("Running....");
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
                log.Error("Spring.MsmqQuickStart.Client is broken.", e);
            }
        }

        private static void ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            log.Error("Uncaught application exception.", e.Exception);
            Application.Exit();
        }
    }
}