#region License

/*
 * Copyright © 2002-2008 the original author or authors.
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
    /// that delegates method calls to the base method.
    /// </summary>
    /// <author>Bruno Baia</author>
    public class BaseAopProxyMethodBuilder : AbstractAopProxyMethodBuilder
    {
        #region Constructor(s) / Destructor

        /// <summary>
        /// Creates a new instance of the method builder.
        /// </summary>
        /// <param name="typeBuilder">The type builder to use.</param>
        /// <param name="aopProxyGenerator">
        /// The <see cref="IAopProxyTypeGenerator"/> implementation to use.
        /// </param>
        /// <param name="targetMethods">
        /// The dictionary to cache the list of target 
        /// <see cref="System.Reflection.MethodInfo"/>s.
        /// </param>
        /// <param name="onProxyTargetMethods">
        /// The dictionary to cache the list of target  
        /// <see cref="System.Reflection.MethodInfo"/>s defined on the proxy.
        /// </param>
        public BaseAopProxyMethodBuilder(
            TypeBuilder typeBuilder, IAopProxyTypeGenerator aopProxyGenerator, 
            IDictionary targetMethods, IDictionary onProxyTargetMethods)
            : base(typeBuilder, aopProxyGenerator, false, targetMethods, onProxyTargetMethods)
        {
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Create static field that will cache target method when defined on the proxy.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="method">The target method.</param>
        protected override void GenerateOnProxyTargetMethodCacheField(
            ILGenerator il, MethodInfo method)
        {
            if (method.IsVirtual && !method.IsFinal)
            {
                // generate proxy method
                MethodBuilder baseMethod = typeBuilder.DefineMethod("proxy_" + method.Name,
                    MethodAttributes.Public | MethodAttributes.HideBySig,
                    CallingConventions.Standard,
                    method.ReturnType, ReflectionUtils.GetParameterTypes(method));

#if NET_2_0
                DefineGenericParameters(baseMethod, method);
#endif
                DefineParameters(baseMethod, method);

                ILGenerator localIL = baseMethod.GetILGenerator();

                localIL.Emit(OpCodes.Ldarg_0);
                // setup parameters for call
                for (int i = 0; i < method.GetParameters().Length; i++)
                {
                    localIL.Emit(OpCodes.Ldarg_S, i + 1);
                }
                localIL.EmitCall(OpCodes.Call, method, null);
                localIL.Emit(OpCodes.Ret);

                // create static field that will cache proxy method
                string methodId = GenerateMethodCacheFieldId(method);
                onProxyTargetMethods.Add(methodId, method);

                onProxyTargetMethodCacheField = typeBuilder.DefineField(
                    methodId, typeof(MethodInfo), FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.InitOnly);

#if NET_2_0
                MakeGenericMethod(il, method, onProxyTargetMethodCacheField, genericOnProxyTargetMethod);
#endif
            }
        }

        /// <summary>
        /// Calls target method directly.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="method">The method to proxy.</param>
        /// <param name="interfaceMethod">
        /// The interface definition of the method, if applicable.
        /// </param>
        protected override void CallDirectProxiedMethod(
            ILGenerator il, MethodInfo method, MethodInfo interfaceMethod)
        {
            CallDirectBaseMethod(il, method);
        }

        #endregion
    }
}
