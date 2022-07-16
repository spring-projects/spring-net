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

using System.Xml;
using Spring.Aop.Framework.AutoProxy;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;

#endregion

namespace Spring.Aop.Config
{
    /// <summary>
    /// Utility class for handling registration of auto-proxy creators used internally by the
    /// <code>aop</code> and <code>tx</code> namespace tags.
    /// </summary>
    /// <author>Rob Harrop</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    /// <author>Erich Eichinger (.NET)</author>
    public class AopNamespaceUtils
    {
        /// <summary>
        ///  The object name of the internally managed auto-proxy creator.
        /// </summary>
        public static readonly string AUTO_PROXY_CREATOR_OBJECT_NAME = "Spring.Aop.Config.InternalAutoProxyCreator";

        /// <summary>
        /// The type of the APC that handles advisors with object role <see cref="ObjectRole.ROLE_INFRASTRUCTURE"/>.
        /// </summary>
        private static readonly Type InfrastructureAutoProxyCreatorType = typeof(InfrastructureAdvisorAutoProxyCreator);

        /// <summary>
        /// Registers the internal auto proxy creator if necessary.
        /// </summary>
        /// <param name="parserContext">The parser context.</param>
        /// <param name="sourceElement">The source element.</param>
        public static void RegisterAutoProxyCreatorIfNecessary(ParserContext parserContext, XmlElement sourceElement)
        {
            AssertUtils.ArgumentNotNull(parserContext, "parserContext");
            IObjectDefinitionRegistry registry = parserContext.Registry;
            RegisterAutoProxyCreatorIfNecessary(registry);
        }

        /// <summary>
        /// Registers the internal auto proxy creator if necessary.
        /// </summary>
        public static void RegisterAutoProxyCreatorIfNecessary(IObjectDefinitionRegistry registry)
        {
            AssertUtils.ArgumentNotNull(registry, "registry");

            if (!registry.ContainsObjectDefinition(AUTO_PROXY_CREATOR_OBJECT_NAME))
            {
                RootObjectDefinition objectDefinition = new RootObjectDefinition(InfrastructureAutoProxyCreatorType);
                objectDefinition.Role = ObjectRole.ROLE_INFRASTRUCTURE;
                objectDefinition.PropertyValues.Add("order", int.MaxValue);
                registry.RegisterObjectDefinition(AUTO_PROXY_CREATOR_OBJECT_NAME, objectDefinition);
            }
        }

        /// <summary>
        /// Forces the auto proxy creator to use decorator proxy.
        /// </summary>
        /// <param name="registry">The registry.</param>
        public static void ForceAutoProxyCreatorToUseDecoratorProxy(IObjectDefinitionRegistry registry)
        {
            if (registry.ContainsObjectDefinition(AUTO_PROXY_CREATOR_OBJECT_NAME))
            {
                IObjectDefinition definition = registry.GetObjectDefinition(AUTO_PROXY_CREATOR_OBJECT_NAME);
                definition.PropertyValues.Add("ProxyTargetType", true);
            }
        }
    }
}
