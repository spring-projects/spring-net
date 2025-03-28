#region License

/*
 * Copyright 2002-2009 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Spring.Context;
using Spring.Context.Support;
using Spring.NmsQuickStart.Client.UI;

namespace Spring.NmsQuickStart.Client
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
                log.LogError("Spring.NmsQuickStart.Client is broken.", e);
            }
        }

        private static void ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            log.LogError(e.Exception, "Uncaught application exception.");
            Application.Exit();
        }
    }
}
