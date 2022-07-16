#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

using System.Web.Mvc;
using Spring.Context;

namespace Spring.Web.Mvc
{
    /// <summary>
    /// ActionInvoker implementation that enables the <see cref="IApplicationContext"/>to satisfy dependencies on ActionFilter attributes.
    /// </summary>
    public class SpringActionInvoker : ControllerActionInvoker
    {
        private readonly IApplicationContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpringActionInvoker"/> class.
        /// </summary>
        /// <param name="context">The IApplicationContext.</param>
        public SpringActionInvoker(IApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves information about the action filters.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="actionDescriptor">The action descriptor.</param>
        /// <returns>Information about the action filters.</returns>
        protected override FilterInfo GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            //let the base class do the actual work as usual
            var filterInfo = base.GetFilters(controllerContext, actionDescriptor);

            //configure each collection of filters using the IApplicationContext
            foreach (IActionFilter filter in filterInfo.ActionFilters.Where(f => f != null))
            {
                _context.ConfigureObject(filter, filter.GetType().FullName);
            }

            foreach (IAuthorizationFilter filter in filterInfo.AuthorizationFilters.Where(f => f != null))
            {
                _context.ConfigureObject(filter, filter.GetType().FullName);
            }

            foreach (IExceptionFilter filter in filterInfo.ExceptionFilters.Where(f => f != null))
            {
                _context.ConfigureObject(filter, filter.GetType().FullName);
            }

            foreach (IResultFilter filter in filterInfo.ResultFilters.Where(f => f != null))
            {
                _context.ConfigureObject(filter, filter.GetType().FullName);
            }

            return filterInfo;
        }

    }

}
