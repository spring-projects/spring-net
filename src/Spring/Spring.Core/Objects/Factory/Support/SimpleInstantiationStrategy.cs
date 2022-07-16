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

using Common.Logging;

using Spring.Core.TypeResolution;
using Spring.Util;

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Simple object instantiation strategy for use in
    /// <see cref="Spring.Objects.Factory.IObjectFactory"/> implementations.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Does not support method injection, although it provides hooks for subclasses
    /// to override to add method injection support, for example by overriding methods.
    /// </p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Rick Evans (.NET)</author>
    /// <seealso cref="Spring.Objects.Factory.Support.MethodInjectingInstantiationStrategy"/>
    [Serializable]
    public class SimpleInstantiationStrategy : IInstantiationStrategy
    {
        /// <summary>
        /// The shared <see cref="Common.Logging.ILog"/> instance for this class (and derived classes).
        /// </summary>
        protected static readonly ILog log =
            LogManager.GetLogger(typeof(SimpleInstantiationStrategy));

        /// <summary>
        /// Instantiate an instance of the object described by the supplied
        /// <paramref name="definition"/> from the supplied <paramref name="factory"/>.
        /// </summary>
        /// <param name="definition">
        /// The definition of the object that is to be instantiated.
        /// </param>
        /// <param name="name">
        /// The name associated with the object definition. The name can be the null
        /// or zero length string if we're autowiring an object that doesn't belong
        /// to the supplied <paramref name="factory"/>.
        /// </param>
        /// <param name="factory">
        /// The owning <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// </param>
        /// <returns>
        /// An instance of the object described by the supplied
        /// <paramref name="definition"/> from the supplied <paramref name="factory"/>.
        /// </returns>
        public virtual object Instantiate(
            RootObjectDefinition definition, string name, IObjectFactory factory)
        {
            AssertUtils.ArgumentNotNull(definition, "definition");
            AssertUtils.ArgumentNotNull(factory, "factory");

            if (log.IsTraceEnabled) log.Trace(string.Format("instantiating object '{0}'", name));

            if (definition.HasMethodOverrides)
            {
                return InstantiateWithMethodInjection(definition, name, factory);
            }
            else
            {
                Type objectType = definition.HasObjectType
                    ? definition.ObjectType
                    : TypeResolutionUtils.ResolveType(definition.ObjectTypeName);
                ConstructorInfo constructor = GetZeroArgConstructorInfo(objectType);
                return ObjectUtils.InstantiateType(constructor, ObjectUtils.EmptyObjects);
            }
        }

        /// <summary>
        /// Gets the zero arg ConstructorInfo object, if the type offers such functionality.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Zero argument ConstructorInfo</returns>
        /// <exception cref="FatalReflectionException">
        /// If the type does not have a zero-arg constructor.
        /// </exception>
        private ConstructorInfo GetZeroArgConstructorInfo(Type type)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.DeclaredOnly;

            ConstructorInfo constructor = type.GetConstructor(flags, null, Type.EmptyTypes, null);
            if (constructor == null)
            {
                throw new FatalReflectionException(string.Format(
                    CultureInfo.InvariantCulture, "Cannot instantiate a class that does not have a no-argument constructor [{0}].", type));
            }
            return constructor;
        }

        /// <summary>
        /// Instantiate an instance of the object described by the supplied
        /// <paramref name="definition"/> from the supplied <paramref name="factory"/>.
        /// </summary>
        /// <param name="definition">
        /// The definition of the object that is to be instantiated.
        /// </param>
        /// <param name="name">
        /// The name associated with the object definition. The name can be the null
        /// or zero length string if we're autowiring an object that doesn't belong
        /// to the supplied <paramref name="factory"/>.
        /// </param>
        /// <param name="factory">
        /// The owning <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// </param>
        /// <param name="constructor">
        /// The <see cref="System.Reflection.ConstructorInfo"/> to be used to instantiate
        /// the object.
        /// </param>
        /// <param name="arguments">
        /// Any arguments to the supplied <paramref name="constructor"/>. May be null.
        /// </param>
        /// <returns>
        /// An instance of the object described by the supplied
        /// <paramref name="definition"/> from the supplied <paramref name="factory"/>.
        /// </returns>
        public virtual object Instantiate(
            RootObjectDefinition definition, string name, IObjectFactory factory,
            ConstructorInfo constructor, object[] arguments)
        {
            if (definition.HasMethodOverrides)
            {
                return InstantiateWithMethodInjection(definition, name, factory, constructor, arguments);
            }
            else
            {
                return ObjectUtils.InstantiateType(constructor, arguments);
            }
        }

        /// <summary>
        /// Instantiate an instance of the object described by the supplied
        /// <paramref name="definition"/> from the supplied <paramref name="factory"/>.
        /// </summary>
        /// <param name="definition">
        /// The definition of the object that is to be instantiated.
        /// </param>
        /// <param name="name">
        /// The name associated with the object definition. The name can be the null
        /// or zero length string if we're autowiring an object that doesn't belong
        /// to the supplied <paramref name="factory"/>.
        /// </param>
        /// <param name="factory">
        /// The owning <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// </param>
        /// <param name="factoryMethod">
        /// The <see cref="System.Reflection.MethodInfo"/> to be used to get the object.
        /// </param>
        /// <param name="arguments">
        /// Any arguments to the supplied <paramref name="factoryMethod"/>. May be null.
        /// </param>
        /// <returns>
        /// An instance of the object described by the supplied
        /// <paramref name="definition"/> from the supplied <paramref name="factory"/>.
        /// </returns>
        public virtual object Instantiate(
            RootObjectDefinition definition, string name, IObjectFactory factory,
            MethodInfo factoryMethod, object[] arguments)
        {
            object instance = null;
            object target = null;
            if (StringUtils.HasText(definition.FactoryObjectName))
            {
                target = factory[definition.FactoryObjectName];
            }
            try
            {
                // the target will be null if using a static factory method
                instance = factoryMethod.Invoke(target, arguments);
            }
            catch (TargetInvocationException ex)
            {
                string msg = string.Format(
                    CultureInfo.InvariantCulture,
                    "Factory method '{0}' threw an Exception.", factoryMethod);

                #region Instrumentation

                if (log.IsWarnEnabled)
                {
                    log.Warn(msg, ex.InnerException);
                }

                #endregion

                throw new ObjectDefinitionStoreException(msg, ex.InnerException);
            }
            catch (Exception ex)
            {
                throw new ObjectDefinitionStoreException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Factory method '{0}' threw an Exception.", factoryMethod), ex);
            }
            return instance;
        }

        /// <summary>
        /// Instantiate an instance of the object described by the supplied
        /// <paramref name="definition"/> from the supplied <paramref name="factory"/>,
        /// injecting methods as appropriate.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The default implementation of this method is to throw a
        /// <see cref="System.InvalidOperationException"/>.
        /// </p>
        /// <p>
        /// Derived classes can override this method if they can instantiate an object
        /// with the Method Injection specified in the supplied
        /// <paramref name="definition"/>. Instantiation should use a no-arg constructor.
        /// </p>
        /// </remarks>
        /// <param name="definition">
        /// The definition of the object that is to be instantiated.
        /// </param>
        /// <param name="objectName">
        /// The name associated with the object definition. The name can be a
        /// <see lang="null"/> or zero length string if we're autowiring an object that
        /// doesn't belong to the supplied <paramref name="factory"/>.
        /// </param>
        /// <param name="factory">
        /// The owning <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// </param>
        /// <returns>
        /// An instance of the object described by the supplied
        /// <paramref name="definition"/> from the supplied <paramref name="factory"/>.
        /// </returns>
        protected virtual object InstantiateWithMethodInjection(
            RootObjectDefinition definition, string objectName, IObjectFactory factory)
        {
            throw new InvalidOperationException("Method Injection not supported in SimpleInstantiationStrategy");
        }

        /// <summary>
        /// Instantiate an instance of the object described by the supplied
        /// <paramref name="definition"/> from the supplied <paramref name="factory"/>,
        /// injecting methods as appropriate.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The default implementation of this method is to throw a
        /// <see cref="System.InvalidOperationException"/>.
        /// </p>
        /// <p>
        /// Derived classes can override this method if they can instantiate an object
        /// with the Method Injection specified in the supplied
        /// <paramref name="definition"/>. Instantiation should use the supplied
        /// <paramref name="constructor"/> and attendant <paramref name="arguments"/>.
        /// </p>
        /// </remarks>
        /// <param name="definition">
        /// The definition of the object that is to be instantiated.
        /// </param>
        /// <param name="objectName">
        /// The name associated with the object definition. The name can be the null
        /// or zero length string if we're autowiring an object that doesn't belong
        /// to the supplied <paramref name="factory"/>.
        /// </param>
        /// <param name="factory">
        /// The owning <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// </param>
        /// <param name="constructor">
        /// The <see cref="System.Reflection.ConstructorInfo"/> to be used to instantiate
        /// the object.
        /// </param>
        /// <param name="arguments">
        /// Any arguments to the supplied <paramref name="constructor"/>. May be null.
        /// </param>
        /// <returns>
        /// An instance of the object described by the supplied
        /// <paramref name="definition"/> from the supplied <paramref name="factory"/>.
        /// </returns>
        protected virtual object InstantiateWithMethodInjection(
            RootObjectDefinition definition, string objectName, IObjectFactory factory,
            ConstructorInfo constructor, object[] arguments)
        {
            throw new InvalidOperationException("Method Injection not supported in SimpleInstantiationStrategy");
        }
    }
}
