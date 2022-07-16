#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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
using System.Reflection;
using System.Text;

using Spring.Core.TypeResolution;
using Spring.Util;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// <see cref="Spring.Objects.Factory.IFactoryObject"/> implementation that
    /// retrieves a static or non-static <b>public</b> field value.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Typically used for retrieving <b>public</b> constants.
    /// </p>
    /// </remarks>
    /// <example>
    /// <p>
    /// The following example retrieves the <see cref="System.DBNull.Value"/> field value...
    /// </p>
    /// <code escaped="true">
    /// <object id="dbnull" type="Spring.Objects.Factory.Config.FieldRetrievingFactoryObject, Spring.Core">
    ///     <property name="TargetType" value="System.DBNull"/>
    ///     <property name="TargetField" value="Value"/>
    /// </object>
    /// </code>
    /// <p>
    /// The previous example could also have been written using the convenience
    /// <see cref="Spring.Objects.Factory.Config.FieldRetrievingFactoryObject.StaticField"/>
    /// property, like so...
    /// </p>
    /// <code escaped="true">
    /// <object id="dbnull" type="Spring.Objects.Factory.Config.FieldRetrievingFactoryObject, Spring.Core">
    ///     <property name="StaticField" value="System.DBNull.Value"/>
    /// </object>
    /// </code>
    /// <p>
    /// This class also implements the <see cref="Spring.Objects.Factory.IObjectNameAware"/>
    /// interface
    /// (<see cref="Spring.Objects.Factory.Config.FieldRetrievingFactoryObject.ObjectName"/>).
    /// If the id (or name) of one's
    /// <see cref="Spring.Objects.Factory.Config.FieldRetrievingFactoryObject"/>
    /// object definition is set to the <see cref="System.Type.AssemblyQualifiedName"/>
    /// of the <see langword="static"/> field to be retrieved, then the id (or
    /// name) of one's object definition will be used for the name of the
    /// <see langword="static"/> field lookup. See below for an example of this
    /// concise style of definition.
    /// </p>
    /// <code escaped="true">
    /// <!-- returns the value of the DBNull.Value field -->
    /// <object id="System.DBNull.Value"
    ///         type="Spring.Objects.Factory.Config.FieldRetrievingFactoryObject, Spring.Core"/>
    ///
    /// <!-- returns the value of the Type.Delimiter field -->
    /// <object id="System.Type.Delimiter"
    ///         type="Spring.Objects.Factory.Config.FieldRetrievingFactoryObject, Spring.Core"/>
    /// </code>
    /// <p>
    /// The usage for retrieving instance fields is similar. No example is shown
    /// because public instance fields are <i>generally</i> bad practice; but if
    /// you have some legacy code that exposes public instance fields, or if you
    /// just really like coding public instance fields, then you can use this
    /// <see cref="Spring.Objects.Factory.IFactoryObject"/> implementation to
    /// retrieve such field values.
    /// </p>
    /// </example>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
    [Serializable]
    public class FieldRetrievingFactoryObject : IFactoryObject, IInitializingObject, IObjectNameAware
    {
        private string targetField;
        private object targetObject;
        private Type targetType;
        private FieldInfo field;
        private string objectName;
        private string staticField;

        /// <summary>
        /// The <see cref="System.Type.AssemblyQualifiedName"/> of the
        /// <see langword="static"/> field to be retrieved.
        /// </summary>
        public string StaticField
        {
            set { this.staticField = value; }
        }

        /// <summary>
        /// Set the name of the object in the object factory that created this object.
        /// </summary>
        /// <value>
        /// The name of the object in the factory.
        /// </value>
        /// <remarks>
        /// <p>
        /// In the context of the
        /// <see cref="Spring.Objects.Factory.Config.FieldRetrievingFactoryObject"/>
        /// class, the
        /// <see cref="Spring.Objects.Factory.Config.FieldRetrievingFactoryObject.ObjectName"/>
        /// value will be interepreted as the value of the
        /// <see cref="Spring.Objects.Factory.Config.FieldRetrievingFactoryObject.StaticField"/>
        /// property if no value has been explicitly assigned to the
        /// <see cref="Spring.Objects.Factory.Config.FieldRetrievingFactoryObject.StaticField"/>
        /// property. This allows for concise object definitions with just an id or name;
        /// see the class documentation for
        /// <see cref="Spring.Objects.Factory.Config.FieldRetrievingFactoryObject"/>
        /// for an example of this style of usage.
        /// </p>
        /// </remarks>
        public string ObjectName
        {
            set { this.objectName = value; }
        }

        /// <summary>
        /// The name of the field the value of which is to be retrieved.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If the
        /// <see cref="Spring.Objects.Factory.Config.FieldRetrievingFactoryObject.TargetObject"/>
        /// has been set (and is not <cref lang="null"/>), then the value of this property
        /// refers to an instance field name; it otherwise refers to a <see langword="static"/>
        /// field name.
        /// </p>
        /// </remarks>
        public string TargetField
        {
            get { return targetField; }
            set { targetField = value; }
        }

        /// <summary>
        /// The object instance on which the field is defined.
        /// </summary>
        public object TargetObject
        {
            get { return targetObject; }
            set { targetObject = value; }
        }

        /// <summary>
        /// The <see cref="System.Type"/> on which the field is defined.
        /// </summary>
        public Type TargetType
        {
            get { return targetType; }
            set { targetType = value; }
        }

        /// <summary>
        /// The <see cref="System.Type"/> of object that this
        /// <see cref="Spring.Objects.Factory.IFactoryObject"/> creates, or
        /// <cref lang="null"/> if not known in advance.
        /// </summary>
        public Type ObjectType
        {
            get { return (this.field == null) ? null : this.field.FieldType; }
        }

        /// <summary>
        /// Is the object managed by this factory a singleton or a prototype?
        /// </summary>
        public bool IsSingleton
        {
            get { return true; }
        }

        /// <summary>
        /// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// after it has set all object properties supplied
        /// (and satisfied <see cref="Spring.Objects.Factory.IObjectFactoryAware"/>
        /// and ApplicationContextAware).
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method allows the object instance to perform initialization only
        /// possible when all object properties have been set and to throw an
        /// exception in the event of misconfiguration.
        /// </p>
        /// </remarks>
        /// <exception cref="System.Exception">
        /// In the event of misconfiguration (such as failure to set an essential
        /// property) or if initialization fails.
        /// </exception>
        public void AfterPropertiesSet()
        {
            if (TargetType != null && TargetObject != null)
            {
                throw new ArgumentException(
                    "Only one of the TargetType or TargetObject properties can be set, not both.");
            }
            if (TargetType == null && TargetObject == null)
            {
                if (TargetField != null)
                {
                    throw new ArgumentException(
                        "Specify the TargetType or TargetObject property in combination with the TargetField property.");
                }

                // if no other property specified, use the object name for the static field expression...
                if (StringUtils.IsNullOrEmpty(this.staticField))
                {
                    this.staticField = this.objectName;
                }
                ParseAndSetFromStaticFieldValue();
            }
            if (TargetField == null)
            {
                throw new ArgumentException("The TargetField property is required.");
            }
            BindingFlags fieldFlags = BindingFlags.Public | BindingFlags.IgnoreCase;
            if (TargetObject == null)
            {
                // a static field...
                fieldFlags |= BindingFlags.Static;
            }
            else
            {
                // an instance field...
                fieldFlags |= BindingFlags.Instance;
                TargetType = TargetObject.GetType();
            }
            this.field = targetType.GetField(TargetField, fieldFlags);
            if (this.field == null)
            {
                throw new ObjectDefinitionStoreException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "No such field '{0}' on [{1}].", TargetField,
                        TargetObject == null ? TargetType : TargetObject));
            }
        }

        /// <summary>
        /// Return an instance (possibly shared or independent) of the object
        /// managed by this factory.
        /// </summary>
        /// <returns>
        /// An instance (possibly shared or independent) of the object managed by
        /// this factory.
        /// </returns>
        /// <see cref="Spring.Objects.Factory.IFactoryObject.GetObject"/>
        public object GetObject()
        {
            if (TargetObject != null)
            {
                return this.field.GetValue(TargetObject);
            }
            else
            {
                return this.field.GetValue(null);
            }
        }

        private void ParseAndSetFromStaticFieldValue()
        {
            TypeAssemblyHolder info = new TypeAssemblyHolder(this.staticField);
            // the info.TypeName should now contains the Type name, followed by a
            // period, followed by the name of the field
            int lastDotIndex = info.TypeName.LastIndexOf('.');
            if (lastDotIndex == -1
                || lastDotIndex == info.TypeName.Length)
            {
                throw new ArgumentException(
                    "The value passed to the StaticField property must be a fully " +
                    "qualified Type plus field name: " +
                    "e.g. 'Example.MyExampleClass.MyField, MyAssembly'");
            }
            string typeName = info.TypeName.Substring(0, lastDotIndex);
            string fieldName = info.TypeName.Substring(lastDotIndex + 1);
            StringBuilder buffer = new StringBuilder(typeName);
            if (info.IsAssemblyQualified)
            {
                buffer.Append(TypeAssemblyHolder.TypeAssemblySeparator);
                buffer.Append(info.AssemblyName);
            }
            TargetType = TypeResolutionUtils.ResolveType(buffer.ToString());
            TargetField = fieldName;
        }
    }
}
