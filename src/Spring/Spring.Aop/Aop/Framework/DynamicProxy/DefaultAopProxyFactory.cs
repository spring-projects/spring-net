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

using System.Reflection;
using Spring.Proxy;
using Spring.Util;

namespace Spring.Aop.Framework.DynamicProxy
{
    /// <summary>
    /// Default implementation of the
    /// <see cref="Spring.Aop.Framework.IAopProxyFactory"/> interface,
    /// either creating a decorator-based dynamic proxy or 
    /// a composition-based dynamic proxy.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Creates a decorator-base proxy if one the following is true :
    /// - the "ProxyTargetType" property is set
    /// - no interfaces have been specified
    /// </p>
    /// <p>
    /// In general, specify "ProxyTargetType" to enforce a decorator-based proxy,
    /// or specify one or more interfaces to use a composition-based proxy.
    /// </p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Aleksandar Seovic (.NET)</author>
    /// <author>Bruno Baia (.NET)</author>
    /// <author>Erich Eichinger (.NET)</author>
    /// <seealso cref="Spring.Aop.Framework.IAopProxyFactory"/>
    [Serializable]
    public class DefaultAopProxyFactory : AbstractAopProxyFactory
    {
        /// <summary>
        /// Force transient assemblies to be resolvable by <see cref="Assembly.Load(string)"/>.
        /// </summary>
        static DefaultAopProxyFactory()
        {
            SystemUtils.RegisterLoadedAssemblyResolver();
        }

        /// <summary>
        /// Creates an actual proxy instance based on the supplied <paramref name="advisedSupport"/>
        /// </summary>
        /// <param name="advisedSupport"></param>
        /// <returns></returns>
        protected override IAopProxy DoCreateAopProxyInstance(AdvisedSupport advisedSupport)
        {
            if (advisedSupport.ProxyType == null)
            {
                IProxyTypeBuilder typeBuilder;
                if ((advisedSupport.ProxyTargetType) ||
                    (advisedSupport.Interfaces.Count == 0))
                {
                    typeBuilder = new DecoratorAopProxyTypeBuilder(advisedSupport);
                }
                else
                {
                    typeBuilder = new CompositionAopProxyTypeBuilder(advisedSupport);
                }
                advisedSupport.ProxyType = BuildProxyType(typeBuilder);
                advisedSupport.ProxyConstructor = advisedSupport.ProxyType.GetConstructor(new Type[] { typeof(IAdvised) });
            }
            return (IAopProxy)advisedSupport.ProxyConstructor.Invoke(new object[] { advisedSupport });
        }

        /// <summary>
        /// Generates the proxy type.
        /// </summary>
        /// <param name="typeBuilder">
        /// The <see cref="Spring.Proxy.IProxyTypeBuilder"/> to use
        /// </param>
        /// <returns>The generated proxy class.</returns>
        protected virtual Type BuildProxyType(IProxyTypeBuilder typeBuilder)
        {
            return typeBuilder.BuildProxyType();
        }
    }
}
