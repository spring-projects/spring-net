#region License

/*
 * Copyright 2002-2008 the original author or authors.
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

using System.Collections;
using System.Web.UI;
using Spring.Util;

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// An implementation of <see cref="IHierarchicalWebNavigator"/> specific for <see cref="Control"/>s. 
    /// The navigator hierarchy equals the control hierarchy when using a <see cref="WebFormsResultWebNavigator"/>.
    /// </summary>
    public class WebFormsResultWebNavigator : ResultWebNavigator
    {
        /// <summary>
        /// Finds the next <see cref="IWebNavigator"/> up  the control hierarchy, 
        /// starting at the specified <paramref name="control"/>.
        /// </summary>
        /// <remarks>
        /// This method checks both, for controls implementing <see cref="IWebNavigator"/> or <see cref="IWebNavigable"/>. In addition 
        /// when MasterPages are used, it interprets the control hierarchy as control-&gt;page-&gt;masterpage.
        /// </remarks>
        /// <param name="control">the control to start the search with.</param>
        /// <param name="includeSelf">include checking the control itself or start search with its parent.</param>
        /// <returns>If found, the next <see cref="IWebNavigator"/> up the hierarchy. <c>null</c> otherwise</returns>
        public static IWebNavigator FindWebNavigator( Control control, bool includeSelf )
        {
            while (control != null)
            {
                if (!includeSelf)
                {
                    // Get next parent in hierarchy
                    control = WebUtils.GetLogicalParent( control );
                }
                includeSelf = false;

                if (control is IWebNavigable || control is IWebNavigator)
                {
                    return (control is IWebNavigable) ? ((IWebNavigable)control).WebNavigator : (IWebNavigator)control;
                }
            }
            return null;
        }

        private Control _owner;

        /// <summary>
        /// Creates a new instance of a <see cref="IHierarchicalWebNavigator"/> for the specified control.
        /// </summary>
        /// <param name="owner">the control to be associated with this navigator.</param>
        /// <param name="results">a dictionary containing results</param>
        /// <param name="ignoreCase">determines, whether to interpret result names case-sensitive or not.</param>
        public WebFormsResultWebNavigator( Control owner, IDictionary results, bool ignoreCase )
            : base( null, results, ignoreCase )
        { 
            AssertUtils.ArgumentNotNull(owner, "owner");
            _owner = owner;
        }

        public override IWebNavigator ParentNavigator
        {
            get
            {
                if (base.ParentNavigator == null)
                {
                    base.ParentNavigator = FindWebNavigator(_owner, false);
                }
                return base.ParentNavigator;
            }
            set
            {
                throw new System.NotSupportedException("cannot set parent navigator on a WebFormsResultWebNavigator");
            }
        }
    }
}