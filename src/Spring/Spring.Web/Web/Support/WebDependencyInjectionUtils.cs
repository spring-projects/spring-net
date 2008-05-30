#region License

/*
 * Copyright 2002-2007 the original author or authors.
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

using System.Web.UI;
using Spring.Context;
using Spring.Context.Support;
using Spring.Util;

namespace Spring.Web.Support
{
    /// <summary>
    /// This is 
    /// </summary>
    /// <remarks>
    ///
    /// </remarks>
    /// <author>Mark Pollack</author>
    /// <version>$Id: WebDependencyInjectionUtils.cs,v 1.3 2008/05/13 14:22:47 oakinger Exp $</version>
    public class WebDependencyInjectionUtils
    {
        /// <summary>
        /// Injects dependencies into all controls in the hierarchy 
        /// based on template definitions in the Spring config file.
        /// </summary>
        /// <param name="applicationContext">ApplicationContext to be used</param>
        /// <param name="control">Control to inject dependencies into.</param>
        public static void InjectDependenciesRecursive(IApplicationContext applicationContext, Control control)
        {
            if (applicationContext != null)
            {
                InjectDependenciesRecursiveInternal(applicationContext, control);
            }
        }

        private static void InjectDependenciesRecursiveInternal(IApplicationContext appContext, Control control)
        {
            if (control is LiteralControl) return; // nothing to do

            ISupportsWebDependencyInjection diControl = control as ISupportsWebDependencyInjection;
            if (diControl != null && diControl.DefaultApplicationContext != null)
            {
                return; // nothing to do anymore - control cares for its children
            }

            // "intercept" Control to make it DI-aware
            ControlInterceptor.EnsureControlIntercepted(appContext, control);

            // if the control is a UserControl, use ApplicationContext from it's physical location
            IApplicationContext appContextToUse = appContext;
            UserControl userControl = control as UserControl;
            if (userControl != null)
            {
                appContextToUse = GetControlApplicationContext(appContext, userControl);
            }

            // set ApplicationContext instance
            if (control is IApplicationContextAware)
            {
                ((IApplicationContextAware)control).ApplicationContext = appContextToUse;
            }

            // inject dependencies using control's context
            appContextToUse.ConfigureObject(control, control.GetType().FullName);

            // and now go for control's children
            if (control.HasControls())
            {
                ControlCollection childControls = control.Controls;
                int childCount = childControls.Count;
                for (int i = 0; i < childCount; i++)
                {
                    Control c = childControls[i];
                    if (!(c is LiteralControl))
                    {
                        InjectDependenciesRecursiveInternal(appContext, c);
                    }
                }
            }
        }

        /// <summary>
        /// Aquires an ApplicationContext according to a Control's TemplateSourceDirectory
        /// </summary>
        /// <returns>if availabe, the control's IApplicationContext instance - defaultContext otherwise</returns>
        private static IApplicationContext GetControlApplicationContext(IApplicationContext defaultContext, Control control)
        {
            // use control's directory for resolving context
            string srcDir = control.TemplateSourceDirectory;
            if (StringUtils.HasLength(srcDir))
            {
                if (!srcDir.EndsWith("/")) srcDir += "/";
                return WebApplicationContext.GetContext(srcDir);
            }
            else
            {
                return defaultContext;
            }
        }
    }
}