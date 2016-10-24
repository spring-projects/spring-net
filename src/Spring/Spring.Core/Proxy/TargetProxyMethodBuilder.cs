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

using System.Reflection;
using System.Reflection.Emit;

#endregion

namespace Spring.Proxy
{
    /// <summary>
    /// Implementation of IProxyMethodBuilder that delegates method calls to target object.
    /// </summary>
    /// <author>Bruno Baia</author>
    public class TargetProxyMethodBuilder : AbstractProxyMethodBuilder
    {
        #region Constructor(s) / Destructor

        /// <summary>
        /// Creates a new instance of the method builder.
        /// </summary>
        /// <param name="typeBuilder">The type builder to use.</param>
        /// <param name="proxyGenerator">
        /// The <see cref="IProxyTypeGenerator"/> implementation to use.
        /// </param>
        /// <param name="explicitImplementation">
        /// <see langword="true"/> if the interface is to be
        /// implemented explicitly; otherwise <see langword="false"/>.
        /// </param>
        public TargetProxyMethodBuilder(
            TypeBuilder typeBuilder, IProxyTypeGenerator proxyGenerator, bool explicitImplementation)
            : base(typeBuilder, proxyGenerator, explicitImplementation)
        {
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Generates the proxy method.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="method">The method to proxy.</param>
        /// <param name="interfaceMethod">
        /// The interface definition of the method, if applicable.
        /// </param>
        protected override void GenerateMethod(
            ILGenerator il, MethodInfo method, MethodInfo interfaceMethod)
        {
            LocalBuilder returnValue = null;
            if (method.ReturnType != typeof(void))
            {
                returnValue = il.DeclareLocal(method.ReturnType);
            }

            CallDirectTargetMethod(il, interfaceMethod);

            // unboxing is not necessary because we called the method directly
            if (returnValue != null)
            {
                il.Emit(OpCodes.Stloc, returnValue);

                if (!method.ReturnType.IsValueType)
                {
                    ProcessReturnValue(il, returnValue);
                }
                il.Emit(OpCodes.Ldloc, returnValue);
            }
        }

        #endregion
    }
}
