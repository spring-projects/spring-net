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

using System.Reflection;
using System.Reflection.Emit;
using Spring.Proxy;

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// This MethodBuilder emits a Callback-call before calling the base method
    /// </summary>
    /// <author>Erich Eichinger</author>
    /// <version>$Id: SupportsWebDependencyInjectionMethodBuilder.cs,v 1.1 2007/08/01 23:11:01 markpollack Exp $</version>
    internal class SupportsWebDependencyInjectionMethodBuilder : BaseProxyMethodBuilder
    {
        private FieldInfo _appContextField;
        private MethodInfo _callbackMethod;

        /// <summary>
        /// Initializes a new instance of a SupportsWebDependencyInjectionMethodBuilder
        /// </summary>
        public SupportsWebDependencyInjectionMethodBuilder(TypeBuilder typeBuilder, IProxyTypeGenerator proxyGenerator,
                                                           FieldInfo appContextField, MethodInfo callbackMethod)
            : base(typeBuilder, proxyGenerator, true)
        {
            _appContextField = appContextField;
            _callbackMethod = callbackMethod;
        }

        /// <summary>
        /// Inserts a call to a callback-method before actually calling the base-method.
        /// </summary>
        /// <param name="il">The IL generator to use</param>
        /// <param name="method">The method to proxy</param>
        /// <param name="interfaceMethod">The interface definition of this method, if applicable</param>
        protected override void GenerateMethod(ILGenerator il, MethodInfo method, MethodInfo interfaceMethod)
        {
            this.GenerateCallbackMethodCall(il, method, interfaceMethod);
            base.GenerateMethod(il, method, interfaceMethod);
        }

        /// <summary>
        /// Emits the callback invocation.
        /// </summary>
        private void GenerateCallbackMethodCall(ILGenerator il, MethodInfo method, MethodInfo interfaceMethod)
        {
            // setup parameters for call

            // IApplicationContext is always first parameter!
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, _appContextField);

            // lookup parameters needed for callback - they are matched by type in order!
            ParameterInfo[] callbackParams = _callbackMethod.GetParameters();
            ParameterInfo[] paramArray = method.GetParameters();

            for (int j = 1; j < callbackParams.Length; j++)
            {
                for (int i = 0; i < paramArray.Length; i++)
                {
                    if (paramArray[i].ParameterType == callbackParams[j].ParameterType)
                    {
                        il.Emit(OpCodes.Ldarg_S, i + 1);
                        break;
                    }
                }
            }

            // invoke static(!) callback
            il.EmitCall(OpCodes.Call, _callbackMethod, null);
            return;
        }
    }
}