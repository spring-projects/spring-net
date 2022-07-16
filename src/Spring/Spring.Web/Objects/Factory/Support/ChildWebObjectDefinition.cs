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

using System.Globalization;

using Spring.Objects.Factory.Config;
using Spring.Util;

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Web object definitions extend <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/>
    /// by adding scope property.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This is the most common type of object definition in ASP.Net web applications
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class ChildWebObjectDefinition : ChildObjectDefinition, IWebObjectDefinition
    {
//        private ObjectScope _scope = ObjectScope.Default;
        private string _pageName;

        #region Constructors

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.ChildWebObjectDefinition"/> class
        /// for a singleton, providing property values and constructor arguments.
        /// </summary>
        /// <param name="parentName">Name of the parent object definition.</param>
        /// <param name="type">The class of the object to instantiate.</param>
        /// <param name="arguments">
        /// The <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues"/>
        /// to be applied to a new instance of the object.
        /// </param>
        /// <param name="properties">
        /// The <see cref="Spring.Objects.MutablePropertyValues"/> to be applied to
        /// a new instance of the object.
        /// </param>
        public ChildWebObjectDefinition(string parentName, Type type, ConstructorArgumentValues arguments, MutablePropertyValues properties)
            : base(parentName, type, arguments, properties)
        {}

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.ChildWebObjectDefinition"/> class
        /// for a singleton, providing property values and constructor arguments.
        /// </summary>
        /// <param name="parentName">Name of the parent object definition.</param>
        /// <param name="typeName">The class name of the object to instantiate.</param>
        /// <param name="arguments">
        /// The <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues"/>
        /// to be applied to a new instance of the object.
        /// </param>
        /// <param name="properties">
        /// The <see cref="Spring.Objects.MutablePropertyValues"/> to be applied to
        /// a new instance of the object.
        /// </param>
        public ChildWebObjectDefinition(string parentName, string typeName, ConstructorArgumentValues arguments, MutablePropertyValues properties)
            : base(parentName, typeName, arguments, properties)
        {}

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.ChildWebObjectDefinition"/> class
        /// for an .aspx page, providing property values.
        /// </summary>
        /// <param name="parentName">Name of the parent object definition.</param>
        /// <param name="pageName">Name of the .aspx page to instantiate.</param>
        /// <param name="properties">
        /// The <see cref="Spring.Objects.MutablePropertyValues"/> to be applied to
        /// a new instance of the object.
        /// </param>
        public ChildWebObjectDefinition(string parentName, string pageName, MutablePropertyValues properties)
            : base(parentName, WebObjectUtils.GetPageType(pageName), null, properties)
        {
			_pageName = WebUtils.CombineVirtualPaths(VirtualEnvironment.CurrentExecutionFilePath, pageName);
		}

        #endregion

        /// <summary>
        /// Object scope.
        /// </summary>
        ObjectScope IWebObjectDefinition.Scope
        {
            get { return (ObjectScope)Enum.Parse(typeof(ObjectScope), this.Scope, true); }
            set { this.Scope = value.ToString(); }
        }

        /// <summary>
        /// Returns true if web object is .aspx page.
        /// </summary>
        public bool IsPage
        {
            get { return _pageName != null; }
        }

        /// <summary>
        /// Gets the rooted url of the .aspx page, if object definition represents page.
        /// </summary>
        public string PageName
        {
            get { return _pageName; }
        }

        /// <summary>
        /// Forces ASP pages to be treated as prototypes all the time inorder to comply with ASP.Net requirements.
        /// </summary>
        public override bool IsSingleton
        {
            get
            {
                if (IsPage)
                {
                    return false;
                }
                else if (0 == string.Compare("application", this.Scope, true)
                    || 0 == string.Compare("session", this.Scope, true)
                    || 0 == string.Compare("request", this.Scope, true))
                {
                    return true;
                }
                else
                {
                    return base.IsSingleton;
                }
            }
            set { base.IsSingleton = value; }
        }

        /// <summary>
        /// Overrides this object's values using values from <c>other</c> argument.
        /// </summary>
        /// <param name="other">The object to copy values from.</param>
        public override void OverrideFrom(IObjectDefinition other)
        {
            base.OverrideFrom(other);
            if (other is IWebObjectDefinition)
            {
                //                this._scope = ((IWebObjectDefinition) other).Scope;
                this._pageName = ((IWebObjectDefinition)other).PageName;
            }
        }

        /// <summary>
        /// A <see cref="System.String"/> that represents the current
        /// <see cref="System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents the current
        /// <see cref="System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "Child web object with class [{0}] defined in {1}",
                (IsPage ? PageName : ObjectTypeName),
                ResourceDescription);
        }

    }
}
