#region License

/*
 * Copyright © 2002-2006 the original author or authors.
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

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

using Spring.Util;

#endregion

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
        #region Fields

        /// <summary>
        /// The local variable to store
        /// the <see cref="Spring.Aop.Framework.ITargetSourceWrapper"/> instance.
        /// </summary>
        protected LocalBuilder targetSource;

        #endregion

        #region Constructor(s) / Destructor

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
            IAopProxyTypeGenerator aopProxyGenerator, bool explicitImplementation, IDictionary targetMethods)
            : base(typeBuilder, aopProxyGenerator, explicitImplementation, targetMethods)
        {
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Creates local variable declarations.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="method">The method to proxy.</param>
        protected override void DeclareLocals(ILGenerator il, MethodInfo method)
        {
            base.DeclareLocals(il, method);
            targetSource = il.DeclareLocal(typeof(ITargetSourceWrapper));

#if DEBUG
            targetSource.SetLocalSymInfo("targetSource");
#endif
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
            Label jmpEndFinally = il.DefineLabel();

            // save target source so we can call Dispose later
            PushAdvisedProxy(il);
            il.Emit(OpCodes.Ldfld, References.TargetSourceWrapperField);
            il.Emit(OpCodes.Stloc, targetSource);

            // open try/finally block
            il.BeginExceptionBlock();

            base.GenerateMethodLogic(il, method, interfaceMethod);

            // open finally block
            il.BeginFinallyBlock();

            // call Dispose on target source
            il.Emit(OpCodes.Ldloc, targetSource);
            il.Emit(OpCodes.Brfalse, jmpEndFinally);
            il.Emit(OpCodes.Ldloc, targetSource);
            il.EmitCall(OpCodes.Callvirt, References.DisposeMethod, null);
            
            il.MarkLabel(jmpEndFinally);

            // close try/finally block
            il.EndExceptionBlock();
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

        #endregion
    }
}
