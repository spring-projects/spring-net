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

using System.Reflection.Emit;

using Spring.Proxy;

#endregion

namespace Spring.Aop.Framework.DynamicProxy
{
    /// <summary>
    /// <see cref="Spring.Proxy.IProxyMethodBuilder"/> implementation that delegates 
    /// method calls to an <see cref="Spring.Aop.Framework.IAdvised"/> instance.
    /// </summary>
    /// <author>Bruno Baia</author>
    public class IAdvisedProxyMethodBuilder : TargetProxyMethodBuilder
    {
        #region Fields

        /// <summary>
        /// The <see cref="IAopProxyTypeGenerator"/> implementation to use.
        /// </summary>
        private IAopProxyTypeGenerator _aopProxyGenerator;

        #endregion

        #region Constructor(s) / Destructor

        /// <summary>
        /// Creates a new instance of the method builder.
        /// </summary>
        /// <param name="typeBuilder">The type builder to use.</param>
        /// <param name="aopProxyGenerator">
        /// The <see cref="IAopProxyTypeGenerator"/> implementation to use.
        /// </param>
        public IAdvisedProxyMethodBuilder(
            TypeBuilder typeBuilder, IAopProxyTypeGenerator aopProxyGenerator)
            : base(typeBuilder, aopProxyGenerator, true)
        {
            this._aopProxyGenerator = aopProxyGenerator;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Generates the IL instructions that pushes 
        /// the target instance on which calls should be delegated to.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        protected override void PushTarget(ILGenerator il)
        {
            _aopProxyGenerator.PushAdvisedProxy(il);
        }

        #endregion
    }
}
