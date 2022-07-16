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

#region Imports

using Common.Logging;
using Spring.Aop.Target;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Support;

#endregion

namespace Spring.Aop.Framework.AutoProxy.Target
{
    /// <summary>
    /// Summary description for AbstractPrototypeBasedTargetSourceCreator.
    /// </summary>
    public abstract class AbstractPrototypeTargetSourceCreator : ITargetSourceCreator
    {
        /// <summary>
        /// The logger
        /// </summary>
        protected readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region ITargetSourceCreator Members

        /// <summary>
        /// Create a special TargetSource for the given object, if any.
        /// </summary>
        /// <param name="objectType">the type of the object to create a TargetSource for</param>
        /// <param name="name">the name of the object</param>
        /// <param name="factory">the containing factory</param>
        /// <returns>
        /// a special TargetSource or null if this TargetSourceCreator isn't
        /// interested in the particular object
        /// </returns>
        public ITargetSource GetTargetSource(Type objectType, string name, IObjectFactory factory)
        {
            AbstractPrototypeTargetSource prototypeTargetSource = CreatePrototypeTargetSource(objectType, name, factory);
            if (prototypeTargetSource == null)
            {
                return null;
            }
            else
            {
                if (!(factory is IObjectDefinitionRegistry))
                {
                    if (logger.IsWarnEnabled)
                        logger.Warn("Cannot do autopooling with a IObjectFactory that doesn't implement IObjectDefinitionRegistry");
                    return null;
                }
                IObjectDefinitionRegistry definitionRegistry = (IObjectDefinitionRegistry) factory;
                RootObjectDefinition definition = (RootObjectDefinition) definitionRegistry.GetObjectDefinition(name);

                if (logger.IsInfoEnabled)
                    logger.Info("Configuring AbstractPrototypeBasedTargetSource...");

                // Infinite cycle will result if we don't use a different factory,
                // because a GetObject() call with this objectName will go through the autoproxy
                // infrastructure again.
                // We to override just this object definition, as it may reference other objects
                // and we're happy to take the parent's definition for those.
                DefaultListableObjectFactory objectFactory = new DefaultListableObjectFactory(factory);

                // Override the prototype object
                objectFactory.RegisterObjectDefinition(name, definition);

                // Complete configuring the PrototypeTargetSource
                prototypeTargetSource.TargetObjectName = name;
                prototypeTargetSource.ObjectFactory = objectFactory;

                return prototypeTargetSource;
            }
        }

        #endregion

        /// <summary>
        /// Creates the prototype target source.
        /// </summary>
        /// <param name="objectType">The type of the object to create a target source for.</param>
        /// <param name="name">The name.</param>
        /// <param name="factory">The factory.</param>
        /// <returns></returns>
        protected abstract AbstractPrototypeTargetSource CreatePrototypeTargetSource(Type objectType, string name,
                                                                                     IObjectFactory factory);

    }
}
