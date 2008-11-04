#region License

/*
 * Copyright 2002-2006 the original author or authors.
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

#region Imports

using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

using Spring.Context;
using Spring.Context.Support;

#endregion

namespace Spring.IocQuickStart.AppContext
{
    /// <summary>
    /// Sample application showing use of IApplicationContext
    /// </summary>
    internal class MainApp
    {
        private static ResourcesDisplayForm form;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            try
            {
                IApplicationContext ctx = ContextRegistry.GetContext();                   
                form = new ResourcesDisplayForm();

                DemoMessageLocalization(ctx);
                DemoImageResource(ctx);
                Application.Run(form);
            }
            catch (Exception e)
            {
                write(e.Message);
                write(e.StackTrace);
                if (e.InnerException != null)
                {
                    write(e.InnerException.Message);
                }
            }
        }

        /// <summary>
        /// Retrieve the image from the application context.
        /// </summary>
        /// <param name="ctx">An instance of the Spring application context</param>
        public static void DemoImageResource(IApplicationContext ctx)
        {
            Image image = ctx.GetResourceObject(Keys.BUBBLECHAMBER) as Image;
            write("------------");
            write("Loaded image");
            write("Width = " + image.Size.Width + ", Height = " + image.Size.Height);

            form.Image = image;
        }

        /// <summary>
        /// Retrieve various localized messages from the application context.
        /// </summary>
        /// <param name="ctx"></param>
        public static void DemoMessageLocalization(IApplicationContext ctx)
        {
            CultureInfo spanishCultureInfo = new CultureInfo("es");

            write("----------------------------------");
            write("Resolve message key 'HelloMessage'");
            write("----------------------------------");
            string msg = ctx.GetMessage(Keys.HELLO_MESSAGE,
                                        CultureInfo.CurrentCulture,
                                        "Mr.", "Anderson");

            write("Current culture resolved message = " + msg);

            string esMsg = ctx.GetMessage(Keys.HELLO_MESSAGE,
                                          spanishCultureInfo,
                                          "Mr.", "Anderson");
            write("Spanish culture resolved message = " + esMsg);


            write("--------------------------------------------------------------------------");
            write("Now using message argument that itself implements IMessageSourceResolvable");
            write("--------------------------------------------------------------------------");

            string[] codes = {Keys.FEMALE_GREETING};
            DefaultMessageSourceResolvable dmr = new DefaultMessageSourceResolvable(codes, null);

            msg = ctx.GetMessage(Keys.HELLO_MESSAGE,
                                 CultureInfo.CurrentCulture,
                                 dmr, "Anderson");
            write("Current culture resolved message = " + msg);

            esMsg = ctx.GetMessage(Keys.HELLO_MESSAGE,
                                   spanishCultureInfo,
                                   dmr, "Anderson");
            write("Spanish culture resolved message = " + esMsg);

            Person p = new Person();
            write("------------------------");
            write("Appling Person resources");
            write("------------------------");
            ctx.ApplyResources(p, "person", CultureInfo.CurrentUICulture);
            write(p.ToString());
        }

        public static void write(string text)
        {
            Console.WriteLine(text);
            form.AppendText(text + "\n");
        }
    }
}