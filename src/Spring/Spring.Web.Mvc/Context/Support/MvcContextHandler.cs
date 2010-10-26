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
        /// <seealso cref="GetContextType"/>
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
