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

using System.Reflection;
using System.Reflection.Emit;

namespace Spring.Aop.Framework.DynamicProxy
{
    /// <summary>
    /// <see cref="Spring.Proxy.IProxyMethodBuilder"/> implementation
    /// that delegates method calls to introduction object.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    /// <author>Bruno Baia</author>
    public class IntroductionProxyMethodBuilder : AbstractAopProxyMethodBuilder
    {
        /// <summary>
        /// The index of the introduction to delegate call to.
        /// </summary>
        protected int index;

        /// <summary>
        /// Creates a new instance of the method builder.
        /// </summary>
        /// <param name="typeBuilder">The type builder to use.</param>
        /// <param name="aopProxyGenerator">
        /// The <see cref="IAopProxyTypeGenerator"/> implementation to use.
        /// </param>
        /// <param name="targetMethods">
        ///
        /// </param>
        /// <param name="index">index of the introduction to delegate call to</param>
        public IntroductionProxyMethodBuilder(
            TypeBuilder typeBuilder, IAopProxyTypeGenerator aopProxyGenerator,
            IDictionary<string, MethodInfo> targetMethods, int index)
            : base(typeBuilder, aopProxyGenerator, true, targetMethods)
        {
            this.index = index;
        }

        /// <summary>
        /// Generates the IL instructions that pushes
        /// the introduction type on stack.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        protected override void PushTargetType(ILGenerator il)
        {
            PushTarget(il);
            il.EmitCall(OpCodes.Call, References.GetTypeMethod, null);
        }

        /// <summary>
        /// Generates the IL instructions that pushes
        /// the introduction instance on stack.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        protected override void PushTarget(ILGenerator il)
        {
            PushAdvisedProxy(il);
            il.Emit(OpCodes.Ldfld, References.IntroductionsField);
            il.Emit(OpCodes.Ldc_I4, index);
            il.Emit(OpCodes.Ldelem_Ref);
        }

        /// <summary>
        /// Calls proxied method directly.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="method">The method to proxy.</param>
        /// <param name="interfaceMethod">
        /// The interface definition of the method, if applicable.
        /// </param>
        protected override void CallDirectProxiedMethod(
            ILGenerator il, MethodInfo method, MethodInfo interfaceMethod)
        {
            CallDirectTargetMethod(il, interfaceMethod);
        }
    }
}
