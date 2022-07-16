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
    /// that delegates method calls to target object.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    /// <author>Bruno Baia</author>
    public class TargetAopProxyMethodBuilder : AbstractAopProxyMethodBuilder
    {
        /// <summary>
        /// The local variable to store the target instance.
        /// </summary>
        protected LocalBuilder target;

        /// <summary>
        /// Creates a new instance of the method builder.
        /// </summary>
        /// <param name="typeBuilder">The type builder to use.</param>
        /// <param name="aopProxyGenerator">
        /// The <see cref="IAopProxyTypeGenerator"/> implementation to use.
        /// </param>
        /// <param name="explicitImplementation">
        /// <see langword="true"/> if the interface is to be
        /// implemented explicitly; otherwise <see langword="false"/>.
        /// </param>
        /// <param name="targetMethods">
        /// The dictionary to cache the list of target
        /// <see cref="System.Reflection.MethodInfo"/>s.
        /// </param>
        public TargetAopProxyMethodBuilder(TypeBuilder typeBuilder,
            IAopProxyTypeGenerator aopProxyGenerator, bool explicitImplementation, IDictionary<string, MethodInfo> targetMethods)
            : base(typeBuilder, aopProxyGenerator, explicitImplementation, targetMethods)
        {
        }

        /// <summary>
        /// Creates local variable declarations.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="method">The method to proxy.</param>
        protected override void DeclareLocals(ILGenerator il, MethodInfo method)
        {
            base.DeclareLocals(il, method);
            target = il.DeclareLocal(typeof(object));

#if DEBUG && !NETSTANDARD
            target.SetLocalSymInfo("target");
#endif
        }

        /// <summary>
        /// Generates the IL instructions that pushes
        /// the target instance on which calls should be delegated to.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        protected override void PushTarget(ILGenerator il)
        {
            il.Emit(OpCodes.Ldloc, target);
        }

        /// <summary>
        /// Generates method logic.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="method">The method to proxy.</param>
        /// <param name="interfaceMethod">
        /// The interface definition of the method, if applicable.
        /// </param>
        protected override void GenerateMethodLogic(
            ILGenerator il, MethodInfo method, MethodInfo interfaceMethod)
        {
            PushAdvisedProxy(il);
            il.Emit(OpCodes.Ldfld, References.TargetSourceField);
            il.EmitCall(OpCodes.Callvirt, References.GetTargetMethod, null);
            il.Emit(OpCodes.Stloc, target);

            base.GenerateMethodLogic(il, method, interfaceMethod);

            PushAdvisedProxy(il);
            il.Emit(OpCodes.Ldfld, References.TargetSourceField);
            PushTarget(il);
            il.EmitCall(OpCodes.Callvirt, References.GetReleaseTargetMethod, null);
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
            if (interfaceMethod != null)
                CallDirectTargetMethod(il, interfaceMethod);
            else
                CallDirectTargetMethod(il, method);
        }
    }
}
