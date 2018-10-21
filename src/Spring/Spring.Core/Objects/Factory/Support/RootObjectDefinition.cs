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

#region Imports

using System;
using System.Runtime.Serialization;

using Spring.Objects.Factory.Config;

#endregion

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// A plain-vanilla object definition.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This is the most common type of object definition;
    /// <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/> instances
    /// do not derive from a parent
    /// <see cref="Spring.Objects.Factory.Config.IObjectDefinition"/>, and usually
    /// (but not always - see below) have an
    /// <see cref="Spring.Objects.Factory.Support.AbstractObjectDefinition.ObjectType"/>
    /// and (optionally) some
    /// <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues"/> and
    /// <see cref="Spring.Objects.IPropertyValues"/>.
    /// </p>
    /// <p>
    /// Note that <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/>
    /// instances do not have to specify an
    /// <see cref="Spring.Objects.Factory.Support.AbstractObjectDefinition.ObjectType"/> :
    /// This can be useful for deriving
    /// <see cref="Spring.Objects.Factory.Support.ChildObjectDefinition"/> instances
    /// from such definitions, each with it's own
    /// <see cref="Spring.Objects.Factory.Support.AbstractObjectDefinition.ObjectType"/>,
    /// inheriting common property values and other settings from the parent.
    /// </p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
    /// <seealso cref="Spring.Objects.Factory.Support.ChildObjectDefinition"/>
    [Serializable]
    public class RootObjectDefinition : AbstractObjectDefinition
    {
        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/> class.
        /// </summary>
        public RootObjectDefinition()
        {}

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/>
        /// class.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> of the object to instantiate.
        /// </param>
        public RootObjectDefinition(Type type)
        {
            ObjectType = type;
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/>
        /// class.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> of the object to instantiate.
        /// </param>
        /// <param name="singleton">
        /// <see langword="true"/> if this object definition defines a singleton object.
        /// </param>
        public RootObjectDefinition(Type type, bool singleton)
        {
            ObjectType = type;
            IsSingleton = singleton;
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/> class
        /// for a singleton, providing property values and constructor arguments.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> of the object to instantiate.
        /// </param>
        /// <param name="arguments">
        /// The <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues"/>
        /// to be applied to a new instance of the object.
        /// </param>
        /// <param name="properties">
        /// The <see cref="Spring.Objects.MutablePropertyValues"/> to be applied to
        /// a new instance of the object.
        /// </param>
        public RootObjectDefinition(
            Type type, ConstructorArgumentValues arguments, MutablePropertyValues properties)
            : base(arguments, properties)
        {
            ObjectType = type;
        }


        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/> class
        /// for a singleton using the supplied
        /// <see cref="Spring.Objects.Factory.Config.AutoWiringMode"/>.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> of the object to instantiate.
        /// </param>
        /// <param name="autowireMode">
        /// The autowiring mode.
        /// </param>
        public RootObjectDefinition(Type type, AutoWiringMode autowireMode)
        {
            ObjectType = type;
            AutowireMode = autowireMode;
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/> class
        /// for a singleton using the supplied
        /// <see cref="Spring.Objects.Factory.Config.AutoWiringMode"/>.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> of the object to instantiate.
        /// </param>
        /// <param name="autowireMode">
        /// The autowiring mode.
        /// </param>
        /// <param name="dependencyCheck">
        /// Whether to perform a dependency check for objects (not
        /// applicable to autowiring a constructor, thus ignored there)
        /// </param>
        public RootObjectDefinition(
            Type type, AutoWiringMode autowireMode, bool dependencyCheck)
        {
            ObjectType = type;
            AutowireMode = autowireMode;
            if (dependencyCheck
                && ResolvedAutowireMode != AutoWiringMode.Constructor)
            {
                DependencyCheck = DependencyCheckingMode.Objects;
            }
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/> class
        /// with the given singleton status, providing property values.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> of the object to instantiate.
        /// </param>
        /// <param name="properties">
        /// The <see cref="Spring.Objects.MutablePropertyValues"/> to be applied to
        /// a new instance of the object.
        /// </param>
        public RootObjectDefinition(
            Type type, MutablePropertyValues properties) : base(null, properties)
        {
            ObjectType = type;
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/> class
        /// with the given singleton status, providing property values.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> of the object to instantiate.
        /// </param>
        /// <param name="properties">
        /// The <see cref="Spring.Objects.MutablePropertyValues"/> to be applied to
        /// a new instance of the object.
        /// </param>
        /// <param name="singleton">
        /// <see langword="true"/> if this object definition defines a singleton object.
        /// </param>
        public RootObjectDefinition(
            Type type, MutablePropertyValues properties, bool singleton) : base(null, properties)
        {
            ObjectType = type;
            IsSingleton = singleton;
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/> class
        /// for a singleton, providing property values and constructor arguments.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Takes an object class name to avoid eager loading of the object class.
        /// </p>
        /// </remarks>
        /// <param name="typeName">
        /// The assembly qualified <see cref="System.Type.FullName"/> of the object to instantiate.
        /// </param>
        /// <param name="properties">
        /// The <see cref="Spring.Objects.MutablePropertyValues"/> to be applied to
        /// a new instance of the object.
        /// </param>
        /// <param name="arguments">
        /// The <see cref="Spring.Objects.Factory.Config.ConstructorArgumentValues"/>
        /// to be applied to a new instance of the object.
        /// </param>
        public RootObjectDefinition(
            string typeName, ConstructorArgumentValues arguments, MutablePropertyValues properties)
            : base(arguments, properties)
        {
            ObjectTypeName = typeName;
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Support.RootObjectDefinition"/> class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Deep copy constructor.
        /// </p>
        /// </remarks>
        /// <param name="other">
        /// The definition that is to be copied.
        /// </param>
        public RootObjectDefinition(IObjectDefinition other) : base(other)
        {
        }

        protected RootObjectDefinition(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Is always <c>null</c> for a <see cref="RootObjectDefinition"/>.
        /// </summary>
        /// <remarks>
        /// It is safe to request this property's value. Setting any other value than <c>null</c> will 
        /// raise an <see cref="ArgumentException"/>.
        /// </remarks>
        /// <exception cref="ArgumentException">Raised on any attempt to set a non-null value on this property.</exception>
        public override string ParentName
        {
            get => null;
            set
            {
                if (value != null)
                {
                    throw new ArgumentException("Root Object cannot be changed into a child oject with parent reference");
                }
            }
        }

        /// <summary>
        /// Validate this object definition.
        /// </summary>
        /// <exception cref="Spring.Objects.Factory.Support.ObjectDefinitionValidationException">
        /// In the case of a validation failure.
        /// </exception>
        public override void Validate()
        {
            base.Validate();
            if (HasObjectType)
            {
                if (typeof(IFactoryObject).IsAssignableFrom(ObjectType)
                    && !IsSingleton)
                {
                    throw new ObjectDefinitionValidationException(
                        "IFactoryObject must be defined as a singleton - " +
                            "IFactoryObjects themselves are not allowed to be prototypes.");
                }
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
            return $"{GetType().Name} : {base.ToString()}";
        }
    }
}