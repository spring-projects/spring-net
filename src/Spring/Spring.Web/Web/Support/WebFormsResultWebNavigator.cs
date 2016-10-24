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
    /// <remarks>
    /// <para>
    /// This implementation supports 2 different navigator hierarchies:
    /// <ul>
    /// <li>The default hierarchy defined by <see cref="IHierarchicalWebNavigator.ParentNavigator"/></li>
    /// <li>The hierarchy defined by a web form's <see cref="Control.Parent"/> hierarchy.</li>
    /// </ul>
    /// </para>
    /// <para>
    /// This implementation always checks the standard hierarchy first and - if a destination cannot be resolved, falls back 
    /// to the control hierarchy for resolving a specified navigation destination.
    /// </para>
    /// </remarks>
    public class WebFormsResultWebNavigator : DefaultResultWebNavigator
    {
        /// <summary>
        /// Holds the result match from <see cref="FindNavigableParent"/>.
        /// </summary>
        protected class NavigableControlInfo
        {
            /// <summary>
            /// The matching control 
            /// </summary>
            public readonly Control Control;
            /// <summary>
            /// The <see cref="IWebNavigator"/> instance associated with the control. May be null.
            /// </summary>
            public readonly IWebNavigator WebNavigator;

            /// <summary>
            /// Initializes the new match instance.
            /// </summary>
            /// <param name="control">the matching control. Must not be null!</param>
            public NavigableControlInfo( Control control )
            {
                AssertUtils.ArgumentNotNull(control, "control");

                Control = control;

                if (Control is IWebNavigable)
                {
                    WebNavigator = ((IWebNavigable)Control).WebNavigator;
                }
                else if (Control is IWebNavigator)
                {
                    WebNavigator = (IWebNavigator)control;
                }
            }
        }

        /// <summary>
        /// Finds the next <see cref="IWebNavigator"/> up  the control hierarchy, 
        /// starting at the specified <paramref name="control"/>.
        /// </summary>
        /// <remarks>
        /// This method checks both, for controls implementing <see cref="IWebNavigator"/> or <see cref="IWebNavigable"/>. In addition 
        /// when MasterPages are used, it interprets the control hierarchy as control-&gt;page-&gt;masterpage. (<see cref="WebUtils.GetLogicalParent"/>).
        /// </remarks>
        /// <param name="control">the control to start the search with.</param>
        /// <param name="includeSelf">include checking the control itself or start search with its parent.</param>
        /// <param name="restrictToValidNavigatorsOnly">requires <see cref="IWebNavigable"/>s to hold a valid <see cref="IWebNavigable.WebNavigator"/> instance.</param>
        /// <returns>If found, the next <see cref="IWebNavigator"/> or <see cref="IWebNavigable"/>.
        /// <c>null</c> otherwise</returns>
        protected static NavigableControlInfo FindNavigableParent( Control control, bool includeSelf, bool restrictToValidNavigatorsOnly )
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
                    NavigableControlInfo nci = new NavigableControlInfo(control);
                    if (!restrictToValidNavigatorsOnly)
                    {
                        return nci;
                    }
                    if (nci.WebNavigator != null)
                    {
                        return nci;
                    }
                }
            }
            return null;
        }

        private readonly Control _owner;

        /// <summary>
        /// The <see cref="Control"/> that this <see cref="WebFormsResultWebNavigator"/> is associated with.
        /// </summary>
        public Control Owner
        {
            get { return _owner; }
        }

        /// <summary>
        /// Creates a new instance of a <see cref="IHierarchicalWebNavigator"/> for the specified control.
        /// </summary>
        /// <param name="owner">the control to be associated with this navigator.</param>
        /// <param name="parent">the direct parent of this navigator</param>
        /// <param name="initialResults">a dictionary containing results</param>
        /// <param name="ignoreCase">specifies how to handle case for destination names.</param>
        public WebFormsResultWebNavigator( Control owner, IWebNavigator parent, IDictionary initialResults, bool ignoreCase )
            : base( parent, initialResults, ignoreCase )
        {
            AssertUtils.ArgumentNotNull( owner, "owner" );
            _owner = owner;
        }

        /// <summary>
        /// Determines, whether this navigator or one of its parents can 
        /// navigate to the destination specified in <paramref name="destination"/>.
        /// </summary>
        /// <param name="destination">the name of the navigation destination</param>
        /// <returns>true, if this navigator can navigate to the destination.</returns>
        public override bool CanNavigateTo( string destination )
        {
            return CheckCanNavigate( destination, true );
        }

        /// <summary>
        /// Check, whether this navigator can navigate to the specified <paramref name="destination"/>.
        /// </summary>
        /// <param name="destination">the destination name to check.</param>
        /// <param name="includeControlHierarchy">
        /// whether the check shall include the <see cref="Owner"/> control hierarchy or
        /// the standard <see cref="IHierarchicalWebNavigator.ParentNavigator"/> hierarchy only.
        /// </param>
        protected bool CheckCanNavigate( string destination, bool includeControlHierarchy )
        {
            // check the default path
            if (base.CanNavigateTo( destination ))
            {
                return true;
            }

            // include checking the control hierarchy
            if (includeControlHierarchy)
            {
                NavigableControlInfo nci = FindNavigableParent( this._owner, false, true );
                if (nci != null)
                {
                    // when delegating upwards, the control containing the matching result
                    // will appear as sender - this makes dealing with expressions more "natural".
                    return nci.WebNavigator.CanNavigateTo( destination );
                }
            }

            return false;
        }

        /// <summary>
        /// Returns a redirect url string that points to the 
        /// <see cref="Spring.Web.Support.Result.TargetPage"/> defined by this
        /// result evaluated using this Page for expression 
        /// </summary>
        /// <param name="destination">Name of the result.</param>
        /// <param name="sender">the instance that issued this request</param>
        /// <param name="context">The context to use for evaluating the SpEL expression in the Result</param>
        /// <returns>A redirect url string.</returns>
        public override string GetResultUri( string destination, object sender, object context )
        {
            if (this.CheckCanNavigate( destination, false ))
            {
                return base.GetResultUri( destination, sender, context );
            }

            NavigableControlInfo nci = FindNavigableParent( this._owner, false, true );
            if (nci != null)
            {
                // when delegating upwards, the control containing the matching result
                // will appear as sender - this makes dealing with expressions more "natural".
                return nci.WebNavigator.GetResultUri( destination, nci.Control, context );
            }

            return HandleUnknownDestination( destination, sender, context );
        }

        /// <summary>
        /// Redirects user to a URL mapped to specified result name.
        /// </summary>
        /// <param name="destination">Name of the result.</param>
        /// <param name="sender">the instance that issued this request</param>
        /// <param name="context">The context to use for evaluating the SpEL expression in the Result.</param>
        public override void NavigateTo( string destination, object sender, object context )
        {
            if (this.CheckCanNavigate( destination, false ))
            {
                base.NavigateTo( destination, sender, context );
                return;
            }

            NavigableControlInfo nci = FindNavigableParent( this._owner, false, true );
            if (nci != null)
            {
                // when delegating upwards, the control containing the matching result
                // will appear as sender - this makes dealing with expressions more "natural".
                nci.WebNavigator.NavigateTo( destination, nci.Control, context );
                return;
            }

            HandleUnknownDestination( destination, sender, context );
        }

        /// <summary>
        /// Return the next available <see cref="IWebNavigator"/> within 
        /// this <see cref="Owner"/> control's parent hierarchy.
        /// </summary>
        public IWebNavigator ParentControlNavigator
        {
            get
            {
                // nci.WebNavigator is guaranteed to be non-null!
                NavigableControlInfo nci = FindNavigableParent( this._owner, false, true );
                if (nci == null) return null;
                return nci.WebNavigator;
            }
        }
    }
}