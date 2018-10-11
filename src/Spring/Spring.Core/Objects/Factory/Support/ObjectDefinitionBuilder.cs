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

using System;
using System.Collections.Generic;
using Spring.Objects.Factory.Config;

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Programmatic means of constructing a <see cref="IObjectDefinition"/> using the builder pattern.  Intended primarily
    /// for use when implementing custom namespace parsers.
    /// </summary>
    /// <remarks>Set methods are used instead of properties, so that chaining of methods can be used to create
    /// 'one-liner'definitions that set multiple properties at one.</remarks>
    /// <author>Rod Johnson</author>
    /// <author>Rob Harrop</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public class ObjectDefinitionBuilder
    {
        private AbstractObjectDefinition objectDefinition;

        private IObjectDefinitionFactory objectDefinitionFactory;

        private int constructorArgIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDefinitionBuilder"/> class, private
        /// to force use of factory methods.
        /// </summary>
        private ObjectDefinitionBuilder()
        {
        }

        /// <summary>
        /// Creates a new <see cref="ObjectDefinitionBuilder"/> used to construct a <see cref="Spring.Objects.Factory.Support.GenericObjectDefinition"/>.
        /// </summary>
        public static ObjectDefinitionBuilder GenericObjectDefinition()
        {
           ObjectDefinitionBuilder builder = new ObjectDefinitionBuilder();
            builder.objectDefinition = new GenericObjectDefinition();
            return builder;
        }
       
        /// <summary>
        /// Creates a new <see cref="ObjectDefinitionBuilder"/> used to construct a <see cref="Spring.Objects.Factory.Support.GenericObjectDefinition"/>.
        /// </summary>
        /// <param name="objectType">the <see cref="Type"/> of the object that the definition is being created for</param>
        public static ObjectDefinitionBuilder GenericObjectDefinition(Type objectType)
        {
           ObjectDefinitionBuilder builder = new ObjectDefinitionBuilder();
            builder.objectDefinition = new GenericObjectDefinition();
            builder.objectDefinition.ObjectType = objectType;
            return builder;
        }
       
        /// <summary>
        /// Creates a new <see cref="ObjectDefinitionBuilder"/> used to construct a <see cref="Spring.Objects.Factory.Support.GenericObjectDefinition"/>.
        /// </summary>
        /// <param name="objectTypeName">the name of the <see cref="Type"/> of the object that the definition is being created for</param>
        public static ObjectDefinitionBuilder GenericObjectDefinition(string objectTypeName)
        {
           ObjectDefinitionBuilder builder = new ObjectDefinitionBuilder();
            builder.objectDefinition = new GenericObjectDefinition();
            builder.objectDefinition.ObjectTypeName = objectTypeName;
            return builder;
        }
       
        /// <summary>
        /// Create a new <code>ObjectDefinitionBuilder</code> used to construct a root object definition.
        /// </summary>
        /// <param name="objectDefinitionFactory">The object definition factory.</param>
        /// <param name="objectTypeName">The type name of the object.</param>
        /// <returns>A new <code>ObjectDefinitionBuilder</code> instance.</returns>
        public static ObjectDefinitionBuilder RootObjectDefinition(IObjectDefinitionFactory objectDefinitionFactory,
                                                                   string objectTypeName)
        {
            return RootObjectDefinition(objectDefinitionFactory, objectTypeName, null);
        }

        /// <summary>
        /// Create a new <code>ObjectDefinitionBuilder</code> used to construct a root object definition.
        /// </summary>
        /// <param name="objectDefinitionFactory">The object definition factory.</param>
        /// <param name="objectTypeName">Name of the object type.</param>
        /// <param name="factoryMethodName">Name of the factory method.</param>
        /// <returns>A new <code>ObjectDefinitionBuilder</code> instance.</returns>
        public static ObjectDefinitionBuilder RootObjectDefinition(IObjectDefinitionFactory objectDefinitionFactory,
                                                                   string objectTypeName,
                                                                   string factoryMethodName)
        {
            ObjectDefinitionBuilder builder = new ObjectDefinitionBuilder();

            builder.objectDefinitionFactory = objectDefinitionFactory;

            // Pass in null for parent name and also AppDomain to force object definition to be register by name and not type.
            builder.objectDefinition =
                           objectDefinitionFactory.CreateObjectDefinition(objectTypeName, null, null);

            builder.objectDefinition.FactoryMethodName = factoryMethodName;

            return builder;

        }        

        /// <summary>
        /// Create a new <code>ObjectDefinitionBuilder</code> used to construct a root object definition.
        /// </summary>
        /// <param name="objectDefinitionFactory">The object definition factory.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>A new <code>ObjectDefinitionBuilder</code> instance.</returns>
        public static ObjectDefinitionBuilder RootObjectDefinition(IObjectDefinitionFactory objectDefinitionFactory,
                                                                   Type objectType) 
        {
            return RootObjectDefinition(objectDefinitionFactory, objectType, null);
        }

        /// <summary>
        /// Create a new <code>ObjectDefinitionBuilder</code> used to construct a root object definition.
        /// </summary>
        /// <param name="objectDefinitionFactory">The object definition factory.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="factoryMethodName">Name of the factory method.</param>
        /// <returns>A new <code>ObjectDefinitionBuilder</code> instance.</returns>
        public static ObjectDefinitionBuilder RootObjectDefinition(IObjectDefinitionFactory objectDefinitionFactory,
                                                                   Type objectType, string factoryMethodName)
        {
            ObjectDefinitionBuilder builder = new ObjectDefinitionBuilder();
            
            builder.objectDefinitionFactory = objectDefinitionFactory;

            builder.objectDefinition =
                objectDefinitionFactory.CreateObjectDefinition(objectType.FullName, null, AppDomain.CurrentDomain);

            builder.objectDefinition.ObjectType = objectType;
            builder.objectDefinition.FactoryMethodName = factoryMethodName;
            return builder;
        }

        /// <summary>
        /// Create a new <code>ObjectDefinitionBuilder</code> used to construct a child object definition..
        /// </summary>
        /// <param name="objectDefinitionFactory">The object definition factory.</param>
        /// <param name="parentObjectName">Name of the parent object.</param>
        /// <returns></returns>
        public static ObjectDefinitionBuilder ChildObjectDefinition(IObjectDefinitionFactory objectDefinitionFactory,
                                                                    string parentObjectName)
        {
            ObjectDefinitionBuilder builder = new ObjectDefinitionBuilder();

            builder.objectDefinitionFactory = objectDefinitionFactory;

            builder.objectDefinition =
              objectDefinitionFactory.CreateObjectDefinition(null, parentObjectName, AppDomain.CurrentDomain);

            return builder;
        }


        /// <summary>
        /// Gets the current object definition in its raw (unvalidated) form.
        /// </summary>
        /// <value>The raw object definition.</value>
        public AbstractObjectDefinition RawObjectDefinition
        {
            get { return objectDefinition; }

        }

        /// <summary>
        /// Validate and gets the object definition.
        /// </summary>
        /// <value>The object definition.</value>
        public AbstractObjectDefinition ObjectDefinition
        {
            get
            {
                objectDefinition.Validate();
                return objectDefinition;
            }
        }


        //TODO add expression support.

        /// <summary>
        /// Adds the property value under the given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns>The current <code>ObjectDefinitionBuilder</code>.</returns>
        public ObjectDefinitionBuilder AddPropertyValue(string name, object value)
        {
           objectDefinition.PropertyValues.Add(new PropertyValue(name, value));
           return this;
        }

        /// <summary>
        /// Adds a reference to the specified object name under the property specified.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="objectName">Name of the object.</param>
        /// <returns>The current <code>ObjectDefinitionBuilder</code>.</returns>
        public ObjectDefinitionBuilder AddPropertyReference(string name, string objectName)
        {
            objectDefinition.PropertyValues.Add(new PropertyValue(name, new RuntimeObjectReference(objectName)));
            return this;
        }


        /// <summary>
        /// Adds an index constructor arg value.  The current index is tracked internally and all addtions are
        /// at the present point
        /// </summary>
        /// <param name="value">The constructor arg value.</param>
        /// <returns>The current <code>ObjectDefinitionBuilder</code>.</returns>
        public ObjectDefinitionBuilder AddConstructorArg(object value)
        {
            objectDefinition.ConstructorArgumentValues.AddIndexedArgumentValue(constructorArgIndex++,value);
            return this;
        }

        /// <summary>
        /// Adds a reference to the named object as a constructor argument.
        /// </summary>
        /// <param name="objectName">Name of the object.</param>
        /// <returns></returns>
        public ObjectDefinitionBuilder AddConstructorArgReference(string objectName)
        {
            return AddConstructorArg(new RuntimeObjectReference(objectName));
        }


        /// <summary>
        /// Sets the name of the factory method to use for this definition.
        /// </summary>
        /// <param name="factoryMethod">The factory method.</param>
        /// <returns>The current <code>ObjectDefinitionBuilder</code>.</returns>
        public ObjectDefinitionBuilder SetFactoryMethod(string factoryMethod)
        {
            objectDefinition.FactoryMethodName = factoryMethod;
            return this;
        }

        /// <summary>
        /// Sets the name of the factory object to use for this definition.
        /// </summary>
        /// <param name="factoryObject">The factory object.</param>
        /// <param name="factoryMethod">The factory method.</param>
        /// <returns>The current <code>ObjectDefinitionBuilder</code>.</returns>
        public ObjectDefinitionBuilder SetFactoryObject(string factoryObject, string factoryMethod)
        {
            objectDefinition.FactoryObjectName = factoryObject;
            objectDefinition.FactoryMethodName = factoryMethod;
            return this;
        }

        /// <summary>
        /// Sets whether or not this definition describes a singleton object.
        /// </summary>
        /// <param name="singleton">if set to <c>true</c> [singleton].</param>
        /// <returns>The current <code>ObjectDefinitionBuilder</code>.</returns>
        public ObjectDefinitionBuilder SetSingleton(bool singleton)
        {
            objectDefinition.IsSingleton = singleton;
            return this;
        }

        /// <summary>
        /// Sets whether objects or not this definition is abstract.
        /// </summary>
        /// <param name="flag">if set to <c>true</c> [flag].</param>
        /// <returns>The current <code>ObjectDefinitionBuilder</code>.</returns>
        public ObjectDefinitionBuilder SetAbstract(bool flag)
        {
            objectDefinition.IsAbstract = flag;
            return this;
        }

        /// <summary>
        /// Sets whether objects for this definition should be lazily initialized or not.
        /// </summary>
        /// <param name="lazy">if set to <c>true</c> [lazy].</param>
        /// <returns>The current <code>ObjectDefinitionBuilder</code>.</returns>
        public ObjectDefinitionBuilder SetLazyInit(bool lazy)
        {
            objectDefinition.IsLazyInit = lazy;
            return this;
        }

        /// <summary>
        /// Sets the autowire mode for this definition.
        /// </summary>
        /// <param name="autowireMode">The autowire mode.</param>
        /// <returns>The current <code>ObjectDefinitionBuilder</code>.</returns>
        public ObjectDefinitionBuilder SetAutowireMode(AutoWiringMode autowireMode)
        {
            objectDefinition.AutowireMode = autowireMode;
            return this;
        }

        /// <summary>
        /// Sets the autowire candidate value for this definition.
        /// </summary>
        /// <param name="autowireCandidate">The autowire candidate value</param>
        /// <returns></returns>
        public ObjectDefinitionBuilder SetAutowireCandidate(bool autowireCandidate)
        {
            objectDefinition.IsAutowireCandidate = autowireCandidate;
            return this;
        }

        /// <summary>
        /// Sets the primary value for this definition.
        /// </summary>
        /// <param name="primary">If object is primary</param>
        /// <returns></returns>
        public ObjectDefinitionBuilder SetPrimary(bool primary)
        {
            objectDefinition.IsPrimary = primary;
            return this;
        }

        /// <summary>
        /// Sets the dependency check mode for this definition.
        /// </summary>
        /// <param name="dependencyCheck">The dependency check.</param>
        /// <returns>The current <code>ObjectDefinitionBuilder</code>.</returns>
        public ObjectDefinitionBuilder SetDependencyCheck(DependencyCheckingMode dependencyCheck)
        {
            objectDefinition.DependencyCheck = dependencyCheck;
            return this;
        }


        /// <summary>
        /// Sets the name of the destroy method for this definition.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <returns>The current <code>ObjectDefinitionBuilder</code>.</returns>
        public ObjectDefinitionBuilder SetDestroyMethodName(string methodName)
        {
            objectDefinition.DestroyMethodName = methodName;
            return this;
        }

        /// <summary>
        /// Sets the name of the init method for this definition.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <returns>The current <code>ObjectDefinitionBuilder</code>.</returns>
        public ObjectDefinitionBuilder SetInitMethodName(string methodName)
        {
            objectDefinition.InitMethodName = methodName;
            return this;
        }

        /// <summary>
        /// Sets the resource description for this definition.
        /// </summary>
        /// <param name="resourceDescription">The resource description.</param>
        /// <returns>The current <code>ObjectDefinitionBuilder</code>.</returns>
        public ObjectDefinitionBuilder SetResourceDescription(string resourceDescription)
        {
            objectDefinition.ResourceDescription = resourceDescription;
            return this;
        }

        /// <summary>
        /// Adds the specified object name to the list of objects that this definition depends on.
        /// </summary>
        /// <param name="objectName">Name of the object.</param>
        /// <returns>The current <code>ObjectDefinitionBuilder</code>.</returns>
        public ObjectDefinitionBuilder AddDependsOn(string objectName)
        {
            if (objectDefinition.DependsOn == null)
            {
                objectDefinition.DependsOn = new[] {objectName};
            }
            else
            {
                var list = new List<string>(objectDefinition.DependsOn.Count + 1);
                list.AddRange(objectDefinition.DependsOn);
                list.Add(objectName);
                objectDefinition.DependsOn = list;
            }
            return this;
        }
    }
}