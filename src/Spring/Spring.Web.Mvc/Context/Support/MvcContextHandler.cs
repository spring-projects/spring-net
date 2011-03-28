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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spring.Context.Support;

namespace Spring.Context.Support
{
    /// <summary>
    /// Context Handler for ASP.NET MVC Applications
    /// </summary>
    public class MvcContextHandler : ContextHandler
    {

        /// <summary>
        /// The <see cref="System.Type"/> of <see cref="Spring.Context.IApplicationContext"/>
        /// created if no <c>type</c> attribute is specified on a <c>context</c> element.
        /// </summary>
        /// <value></value>        
        protected override Type DefaultApplicationContextType
        {
            get { return typeof(MvcApplicationContext); }
        }


        /// <summary>
        /// Get the context's case-sensitivity to use if none is specified
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// 	<p>
        /// Derived handlers may override this property to change their default case-sensitivity.
        /// </p>
        /// 	<p>
        /// Defaults to 'true'.
        /// </p>
        /// </remarks>
        protected override bool DefaultCaseSensitivity
        {
            get { return false; }
        }
    }
}
