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

using System;
using System.Text;
using Spring.Objects.Factory.Config;
using Spring.Util;

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Object definition for definitions that inherit settings from their
    /// parent (object definition).
    /// </summary>
    /// <remarks>
    /// <p>
    /// Will use the <see cref="Spring.Objects.Factory.Support.AbstractObjectDefinition.ObjectType"/>
    /// of the parent object definition if none is specified, but can also
    /// override it. In the latter case, the child's
    /// <see cref="Spring.Objects.Factory.Support.AbstractObjectDefinition.ObjectType"/>
    /// must be compatible with the parent, i.e. accept the parent's property values
    /// and constructor argument values (if any).
    /// </p>
    /// <p>
    /// A <see cref="Spring.Objects.Factory.Support.ChildObjectDefinition"/> will
    /// inherit all of the <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues"/>,
    /// <see cref="Spring.Objects.IPropertyValues"/>, and
    /// <see cref="Spring.Objects.Factory.Config.EventValues"/> from it's parent
    /// object definition, with the option to add new values. If the
    /// <see cref="Spring.Objects.Factory.Support.AbstractObjectDefinition.InitMethodName"/>,
    /// <see cref="Spring.Objects.Factory.Support.AbstractObjectDefinition.DestroyMethodName"/>,
    /// and / or <see langword="static"/>
    /// <see cref="Spring.Objects.Factory.Support.AbstractObjectDefinition.FactoryMethodName"/>
    /// properties are specified, they will override the corresponding parent settings.
    /// </p>
    /// <p>
    /// The remaining settings will <i>always</i> be taken from the child definition:
    /// <see cref="Spring.Objects.Factory.Support.AbstractObjectDefinition.DependsOn"/>,
    /// <see cref="Spring.Objects.Factory.Support.AbstractObjectDefinition.AutowireMode"/>,
    /// <see cref="Spring.Objects.Factory.Support.AbstractObjectDefinition.DependencyCheck"/>,
    /// <see cref="Spring.Objects.Factory.Support.AbstractObjectDefinition.IsSingleton"/>,
    /// and
    /// <see cref="Spring.Objects.Factory.Support.AbstractObjectDefinition.IsLazyInit"/>
    /// </p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
    /// <seealso cref="Spring.Objects.Factory.Support.RootObjectDefinition"/>
    [Serializable]
    public class ChildObjectDefinition : AbstractObjectDefinition
    {
        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.ChildObjectDefinition"/>
        /// class.
        /// </summary>
        /// <param name="parentName">
        /// The name of the parent object.
        /// </param>
        public ChildObjectDefinition(string parentName)
        {
            this.parentName = parentName;
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.ChildObjectDefinition"/>
        /// class.
        /// </summary>
        /// <param name="parentName">
        /// The name of the parent object.
        /// </param>
        /// <param name="properties">
        /// The additional property values (if any) of the child.
        /// </param>
        public ChildObjectDefinition(string parentName, MutablePropertyValues properties)
            : base(null, properties)
        {
            this.parentName = parentName;
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.ChildObjectDefinition"/>
        /// class.
        /// </summary>
        /// <param name="parentName">
        /// The name of the parent object.
        /// </param>
        /// <param name="arguments">
        /// The <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues"/>
        /// to be applied to a new instance of the object.
        /// </param>
        /// <param name="properties">
        /// The additional property values (if any) of the child.
        /// </param>
        public ChildObjectDefinition(
            string parentName, ConstructorArgumentValues arguments, MutablePropertyValues properties)
            : base(arguments, properties)
        {
            this.parentName = parentName;
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.ChildObjectDefinition"/>
        /// class.
        /// </summary>
        /// <param name="parentName">
        /// The name of the parent object.
        /// </param>
        /// <param name="type">
        /// The class of the object to instantiate.
        /// </param>
        /// <param name="arguments">
        /// The <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues"/>
        /// to be applied to a new instance of the object.
        /// </param>
        /// <param name="properties">
        /// The additional property values (if any) of the child.
        /// </param>
        public ChildObjectDefinition(
            string parentName, Type type, ConstructorArgumentValues arguments, MutablePropertyValues properties)
            : base(arguments, properties)
        {
            this.parentName = parentName;
            ObjectType = type;
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.ChildObjectDefinition"/>
        /// class.
        /// </summary>
        /// <param name="parentName">
        /// The name of the parent object.
        /// </param>
        /// <param name="typeName">
        /// The <see cref="System.Type.AssemblyQualifiedName"/> of the object to
        /// instantiate.
        /// </param>
        /// <param name="arguments">
        /// The <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues"/>
        /// to be applied to a new instance of the object.
        /// </param>
        /// <param name="properties">
        /// The additional property values (if any) of the child.
        /// </param>
        public ChildObjectDefinition(
            string parentName, string typeName, ConstructorArgumentValues arguments, MutablePropertyValues properties)
            : base(arguments, properties)
        {
            this.parentName = parentName;
            ObjectTypeName = typeName;
        }

        /// <summary>
        /// The name of the parent object definition.
        /// </summary>
        /// <remarks>
        /// This value is <b>required</b>.
        /// </remarks>
        /// <value>
        /// The name of the parent object definition.
        /// </value>
        public override string ParentName
        {
            get { return parentName; }
            set { parentName = value; }
        }

        /// <summary>
        /// Validate this object definition.
        /// </summary>
        /// <remarks>
        /// <p>
        /// A common cause of validation failures is a missing value for the
        /// <see cref="Spring.Objects.Factory.Support.ChildObjectDefinition.ParentName"/>
        /// property; <see cref="Spring.Objects.Factory.Support.ChildObjectDefinition"/> by
        /// their very nature <b>require</b> that the
        /// <see cref="Spring.Objects.Factory.Support.ChildObjectDefinition.ParentName"/>
        /// be set.
        /// </p>
        /// </remarks>
        /// <exception cref="Spring.Objects.Factory.Support.ObjectDefinitionValidationException">
        /// In the case of a validation failure.
        /// </exception>
        public override void Validate()
        {
            base.Validate();
            if (StringUtils.IsNullOrEmpty(parentName))
            {
                throw new ObjectDefinitionValidationException(
                    "The 'ParentName' property must be set in ChildObjectDefinition.");
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
            StringBuilder buffer = new StringBuilder();
            buffer.Append(GetType().Name).Append(" with parent '");
            buffer.Append(ParentName).Append("' : ").Append(base.ToString());
            return buffer.ToString();
        }

        private string parentName;
    }
}