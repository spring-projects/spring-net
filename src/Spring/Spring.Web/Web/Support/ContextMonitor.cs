#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

using System.Web;

using Spring.Context;
using Spring.Context.Support;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;

namespace Spring.Web.Support
{
    /// <summary>
    /// <see cref="IHttpHandler"/> implementation that allows users to monitor state
    /// of the Spring.NET web application context.
    /// </summary>
    /// <remarks>
    /// <p>
    ///
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class ContextMonitor : IHttpHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContextMonitor"/> class.
        /// </summary>
        public ContextMonitor()
        {}

        /// <summary>
        /// Processes HTTP request.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext"/> object that provides references to the intrinsic server objects (for example, <see langword="Request"/>, <see langword="Response"/>, <see langword="Session"/>, and <see langword="Server"/>)<see langword=""/> used to service HTTP requests.</param>
        public void ProcessRequest(HttpContext context)
        {
            HttpResponse res = context.Response;
            res.BufferOutput = true;

            RenderHeader(res.Output, context.Request.ApplicationPath);

            if (!(WebApplicationContext.Current is IConfigurableApplicationContext appContext))
            {
                throw new InvalidOperationException(
                    "Implementations of IApplicationContext must also implement IConfigurableApplicationContext");
            }

            var names = appContext.GetObjectDefinitionNames();
            for (var i = 0; i < names.Count; i++)
            {
                string name = names[i];
                RenderObjectDefinition(res.Output, name, appContext.ObjectFactory.GetObjectDefinition(name));
            }

            RenderFooter(res.Output);
        }

        /// <summary>
        /// Gets a value indicating whether another request can use
        /// this <see cref="T:System.Web.IHttpHandler"/> instance.
        /// </summary>
        /// <value><c>True</c> if this handler is reusable, <c>False</c> otherwise.</value>
        public bool IsReusable
        {
            get { return true; }
        }

        private void RenderHeader(TextWriter tw, string contextName)
        {
            tw.WriteLine("<html>");
            tw.WriteLine("  <head>");
            tw.WriteLine("    <title>Spring.NET Context Monitor - " + contextName + "</title>");
            tw.WriteLine("  </head>");
            tw.WriteLine("  <body>");
            tw.WriteLine("    <table border='1' cellspacing='0' cellpadding='2'>");
            tw.WriteLine("      <tr>");
            RenderHeaderCell(tw, "Name");
            RenderHeaderCell(tw, "Type");
            RenderHeaderCell(tw, "Is Abstract");
            RenderHeaderCell(tw, "Is Singleton");
            RenderHeaderCell(tw, "Is Lazy Init");
            RenderHeaderCell(tw, "Scope");
            RenderHeaderCell(tw, "Page Name");
            tw.WriteLine("  </tr>");

        }

        private void RenderHeaderCell(TextWriter tw, string text)
        {
            tw.WriteLine("        <th>" + text + "</th>");
        }

        private void RenderObjectDefinition(TextWriter tw, string name, IObjectDefinition def)
        {
            tw.WriteLine("      <tr>");
            RenderCell(tw, name);
            RenderCell(tw, def.ObjectTypeName);
            RenderCell(tw, def.IsAbstract);
            RenderCell(tw, def.IsSingleton);
            RenderCell(tw, def.IsLazyInit);
            RenderCell(tw, (def is IWebObjectDefinition ? ((IWebObjectDefinition) def).Scope.ToString() : "&nbsp;"));
            RenderCell(tw, (def is IWebObjectDefinition && ((IWebObjectDefinition) def).IsPage ? ((IWebObjectDefinition) def).PageName : "&nbsp;"));
            tw.WriteLine("  </tr>");
        }

        private void RenderCell(TextWriter tw, object value)
        {
            tw.WriteLine("        <td>" + (value == null ? "&nbsp;" : value.ToString()) + "</td>");
        }

        private void RenderFooter(TextWriter tw)
        {
            tw.WriteLine("    </table>");
            tw.WriteLine("  </body>");
            tw.WriteLine("</html>");
        }
    }
}
