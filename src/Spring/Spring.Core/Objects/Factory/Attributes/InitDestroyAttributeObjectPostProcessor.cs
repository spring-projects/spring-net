#region License

/*
 * Copyright © 2010-2011 the original author or authors.
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
using System.Reflection;
using Common.Logging;
using Spring.Objects;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;
using Spring.Core;

namespace Spring.Objects.Factory.Attributes
{
    /// <summary>
    /// <see cref="InitDestroyAttributeObjectPostProcessor"/> implementation
    /// that invokes attributed init and destroy methods. Allows for an attributation
    /// alternative to Spring's <see cref="IInitializingObject"/> and 
    /// <see cref="IDisposable"/> callback interfaces.
    /// 
    /// Invoke and destroy annotations may be applied to methods of any visibility:
    /// public, protected, or private. Multiple such methods
    /// may be annotated, but it is recommended to only annotate one single
    /// init method and destroy method, respectively.
    /// </summary>
    public class InitDestroyAttributeObjectPostProcessor : IDestructionAwareObjectPostProcessor, IObjectFactoryAware, IOrdered
    {
        private static readonly ILog Logger = LogManager.GetLogger<InitDestroyAttributeObjectPostProcessor>();

        private IConfigurableListableObjectFactory _objectFactory;
        private readonly IDictionary<string, LifecycleLifecycleMetadata> _lifecycleMetadataCache;

        private int _order = int.MaxValue;
        private Type _initAttributeType;
        private Type _destroyAttributeType;


        /// <summary>
        /// Return the order value of this object, where a higher value means greater in
        ///             terms of sorting.
        /// </summary>
        /// <remarks>
        /// <p>Normally starting with 0 or 1, with <see cref="F:System.Int32.MaxValue"/> indicating
        ///             greatest. Same order values will result in arbitrary positions for the affected
        ///             objects.
        ///             </p><p>Higher value can be interpreted as lower priority, consequently the first object
        ///             has highest priority.
        ///             </p>
        /// </remarks>
        /// <returns>
        /// The order value.
        /// </returns>
        public int Order { get { return _order; } private set { _order = value; } }

        /// <summary>
        /// Specify the init attribute to check for, indicating initialization
        /// methods to call after configuration of an object.
        /// </summary>
        public Type InitAttributeType { get { return _initAttributeType; } set { _initAttributeType = value;  } }

        /// <summary>
        /// Specify the destroy attribute to check for, indicating disposal
        /// methods to call before object is destroyed
        /// </summary>
        public Type DestroyAttributeType { get { return _destroyAttributeType; } set { _destroyAttributeType = value; } }


        /// <summary>
        /// 
        /// </summary>
        public IObjectFactory ObjectFactory
        {
            set { _objectFactory = value as IConfigurableListableObjectFactory; }
        }

        /// <summary>
        /// Creates InitDestroy Post Processor with default attribute types of
        /// <see cref="PostConstructAttribute"/>
        /// </summary>
        public InitDestroyAttributeObjectPostProcessor()
        {
            _initAttributeType = typeof (PostConstructAttribute);
            _destroyAttributeType = typeof (PreDestroyAttribute);

            _lifecycleMetadataCache = new Dictionary<string, LifecycleLifecycleMetadata>();
        }

        /// <summary>
        /// Applies PostConstruct init method initialisation if instance is attributed
        /// </summary>
        /// <param name="instance">The new object instance.
        ///             </param><param name="name">The name of the object.
        ///             </param>
        /// <returns>
        /// The object instance to use, either the original or a wrapped one.
        /// </returns>
        /// <exception cref="T:Spring.Objects.ObjectsException">In case of errors.
        ///             </exception>
        public object PostProcessBeforeInitialization(object instance, string name)
        {
            try
            {
                var metadata = FindLifecycleMetadata(instance.GetType(), name);
                metadata.InvokeInitMethods(instance, name);
            }
            catch (Exception e)
            {
                throw new ObjectCreationException(name, "Couldn't invoke init method", e);
            }

            return instance;
        }

        /// <summary>
        /// No special post processing after initialization
        /// </summary>
        public object PostProcessAfterInitialization(object instance, string objectName)
        {
            return instance;
        }


        /// <summary>
        /// Executed PreDestroy methods in given order for provided instance
        /// </summary>
        /// <param name="instance">The new object instance.</param><param name="name">The name of the object.</param><exception cref="T:Spring.Objects.ObjectsException">In case of errors.
        ///             </exception>
        public void PostProcessBeforeDestruction(object instance, string name)
        {
            try
            {
                var metadata = FindLifecycleMetadata(instance.GetType(), name);
                metadata.InvokeDestroyMethods(instance, name);
            }
            catch (Exception e)
            {
                throw new ObjectsException("Couldn't invoke destroy method", e);
            }
        }

        private LifecycleLifecycleMetadata FindLifecycleMetadata(Type instanceType, string name)
        {
            if (_lifecycleMetadataCache.ContainsKey(name))
                return _lifecycleMetadataCache[name];

            lock (_lifecycleMetadataCache)
            {
                var metadata = BuildLifecycleMetadata(instanceType, name);
                _lifecycleMetadataCache.Add(name, metadata);
                return metadata;
            }
        }

        private LifecycleLifecycleMetadata BuildLifecycleMetadata(Type instanceType, string name)
        {
            var initMethods = new List<LifecycleElement>();
            var destroyMethods = new List<LifecycleElement>();

            do
            {
                var curInitMethods = new List<LifecycleElement>();
                var curDestroyMethods = new List<LifecycleElement>();
                var methods = instanceType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                foreach (var methodInfo in methods)
                {
                    var initAttribute =
                        Attribute.GetCustomAttribute(methodInfo, _initAttributeType) as PostConstructAttribute;
                    if (initAttribute != null && methodInfo.DeclaringType == instanceType)
                    {
                        Logger.Debug(m => m("Found init method on class [{0}]: {1}", instanceType.Name, methodInfo.Name));
                        curInitMethods.Add(new LifecycleElement(methodInfo, initAttribute.Order));
                    }

                    var destroyAttribute =
                        Attribute.GetCustomAttribute(methodInfo, _destroyAttributeType) as PreDestroyAttribute;
                    if (destroyAttribute != null && methodInfo.DeclaringType == instanceType)
                    {
                        Logger.Debug(m => m("Found destroy method on class [{0}]: {1}", instanceType.Name, methodInfo.Name));
                        curDestroyMethods.Add(new LifecycleElement(methodInfo, destroyAttribute.Order));
                    }
                }
                
                initMethods.InsertRange(0, curInitMethods.OrderBy(e => e.Order));
                destroyMethods.InsertRange(0, curDestroyMethods.OrderBy(e => e.Order));
                instanceType = instanceType.BaseType;
            } 
            while (instanceType != null && instanceType != typeof(Object));

            var objectDef = _objectFactory.GetObjectDefinition(name);
            var metadata = new LifecycleLifecycleMetadata(initMethods, destroyMethods);
            metadata.CheckConfigMembers(objectDef);

            return metadata;
        }


        private class LifecycleLifecycleMetadata
        {
            private readonly IList<LifecycleElement> _initMethods;
            private readonly IList<LifecycleElement> _destroyMethods;


            public LifecycleLifecycleMetadata(IList<LifecycleElement> initMethods, IList<LifecycleElement> destroyMethods)
            {
                _initMethods = initMethods;
                _destroyMethods = destroyMethods;
            }

            public void CheckConfigMembers(IObjectDefinition objectDef)
            {
                lock (_initMethods)
                {
                    for (int i = 0; i < _initMethods.Count; i++)
                        {
                            if (!_initMethods[i].CheckConfig(objectDef))
                                _initMethods.Remove(_initMethods[i]);
                        }
                }
                lock (_destroyMethods)
                {
                    for (int i = 0; i < _destroyMethods.Count; i++)
                    {
                        if (!_destroyMethods[i].CheckConfig(objectDef))
                            _destroyMethods.Remove(_destroyMethods[i]);
                    }
                }
            }

            public void InvokeInitMethods(object instance, string objectName)
            {
                foreach (var lifecycleElement in _initMethods)
                {
                    lifecycleElement.Invoke(instance, objectName);
                }
            }

            public void InvokeDestroyMethods(object instance, string objectName)
            {
                foreach (var lifecycleElement in _destroyMethods)
                {
                    lifecycleElement.Invoke(instance, objectName);
                }
            }

        }


        private class LifecycleElement
        {
            private readonly MethodInfo _method;
            private readonly int _order;

            public int Order { get { return _order; } }


            public LifecycleElement(MethodInfo method, int order)
            {
                _method = method;
                _order = order;
            }

            public bool CheckConfig(IObjectDefinition objectDef)
            {
                if (_method.Name == objectDef.InitMethodName || _method.Name == objectDef.DestroyMethodName)
                    return false;

                return true;
            }

            public void Invoke(object instance, string objectName)
            {
                Logger.Debug(m => m("Invoking init method on object '" + objectName + "': " + _method.Name));
                _method.Invoke(instance, new object[] {});
            }
        }

    }
}
