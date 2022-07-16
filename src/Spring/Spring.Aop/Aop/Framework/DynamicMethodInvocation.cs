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

using System.Collections;
using System.Reflection;

using Spring.Util;
using Spring.Reflection.Dynamic;
using AopAlliance.Intercept;

#endregion

namespace Spring.Aop.Framework
{
    /// <summary>
    /// Invokes a target method using dynamic reflection.
    /// </summary>
    /// <seealso cref="Spring.Reflection.Dynamic.DynamicMethod"/>
    /// <author>Aleksandar Seovic</author>
    /// <author>Bruno Baia</author>
    [Serializable]
    public class DynamicMethodInvocation : AbstractMethodInvocation
    {
        /// <summary>
        /// The method invocation that is to be invoked on the proxy.
        /// </summary>
        private MethodInfo proxyMethod;

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Aop.Framework.DynamicMethodInvocation"/> class.
        /// </summary>
        /// <param name="proxy">The AOP proxy.</param>
        /// <param name="target">The target object.</param>
        /// <param name="method">The target method proxied.</param>
        /// <param name="proxyMethod">The method to invoke on proxy.</param>
        /// <param name="arguments">The target method's arguments.</param>
        /// <param name="targetType">
        /// The <see cref="System.Type"/> of the target object.</param>
        /// <param name="interceptors">
        /// The list of interceptors that are to be applied. May be
        /// <cref lang="null"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If any of the <paramref name="target"/> or <paramref name="method"/>
        /// parameters is <see langword="null"/>.
        /// </exception>
        public DynamicMethodInvocation(
            object proxy, object target, MethodInfo method, MethodInfo proxyMethod,
            object[] arguments, Type targetType, IList interceptors)
            : base(proxy, target, method, arguments, targetType, interceptors)
        {
            this.proxyMethod = proxyMethod;
        }

        /// <summary>
        /// The method invocation that is to be invoked on the proxy.
        /// </summary>
        protected MethodInfo ProxyMethod
        {
            get { return proxyMethod; }
            set { proxyMethod = value; }
        }

        /// <summary>
        /// Invokes the joinpoint using dynamic reflection.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Subclasses can override this to use custom invocation.
        /// </p>
        /// </remarks>
        /// <returns>
        /// The return value of the invocation of the joinpoint.
        /// </returns>
        /// <exception cref="System.Exception">
        /// If invoking the joinpoint resulted in an exception.
        /// </exception>
        /// <see cref="Spring.Aop.Framework.AbstractMethodInvocation.InvokeJoinpoint"/>
        protected override object InvokeJoinpoint()
		{
		    MethodInfo targetMethodInfo = this.proxyMethod ?? Method;

            IDynamicMethod targetMethod = new SafeMethod(targetMethodInfo);

	        try
            {
                AssertUtils.Understands(Target, "target", targetMethodInfo);
                return targetMethod.Invoke(Target, Arguments);
            }
            // Only happens if fallback to standard reflection.
            catch (TargetInvocationException ex)
            {
                throw ReflectionUtils.UnwrapTargetInvocationException(ex);
            }
        }

        /// <summary>
        /// Creates a new <see cref="Spring.Aop.Framework.DynamicMethodInvocation"/> instance
        /// from the specified <see cref="AopAlliance.Intercept.IMethodInvocation"/> and
        /// increments the interceptor index.
        /// </summary>
        /// <param name="invocation">
        /// The current <see cref="AopAlliance.Intercept.IMethodInvocation"/> instance.
        /// </param>
        /// <returns>
        /// The new <see cref="AopAlliance.Intercept.IMethodInvocation"/> instance to use.
        /// </returns>
        protected override IMethodInvocation PrepareMethodInvocationForProceed(IMethodInvocation invocation)
        {
            var rmi = new DynamicMethodInvocation(Proxy, Target, Method, ProxyMethod, Arguments, TargetType, Interceptors);
            rmi.CurrentInterceptorIndex = CurrentInterceptorIndex + 1;

            return rmi;
        }
    }
}
