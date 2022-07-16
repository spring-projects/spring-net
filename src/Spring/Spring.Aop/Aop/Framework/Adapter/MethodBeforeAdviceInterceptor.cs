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

using AopAlliance.Intercept;
using Spring.Util;

#endregion

namespace Spring.Aop.Framework.Adapter
{
    /// <summary>
    /// <see cref="AopAlliance.Intercept.IInterceptor"/> implementation that
    /// wraps <see cref="Spring.Aop.IMethodBeforeAdvice"/> instances.
    /// </summary>
    /// <remarks>
    /// <p>
    /// In the future Spring.NET may also offer a more efficient alternative
    /// solution in cases where there is no interception advice and therefore
    /// no need to create an <see cref="AopAlliance.Intercept.IMethodInvocation"/>
    /// object.
    /// </p>
    /// <p>
    /// Used internally by the Spring.NET AOP framework: application developers
    /// should not need to use this class directly.
    /// </p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Aleksandar Seovic (.NET)</author>
    [Serializable]
    internal sealed class MethodBeforeAdviceInterceptor : IMethodInterceptor
    {
        private IMethodBeforeAdvice advice;

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Aop.Framework.Adapter.MethodBeforeAdviceInterceptor"/>
        /// class.
        /// </summary>
        /// <param name="advice">
        /// The <see cref="Spring.Aop.IMethodBeforeAdvice"/> that is to be wrapped.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="advice"/> is <see langword="null"/>.
        /// </exception>
        public MethodBeforeAdviceInterceptor(IMethodBeforeAdvice advice)
        {
            AssertUtils.ArgumentNotNull(advice, "advice");
            this.advice = advice;
        }

        /// <summary>
        /// Executes interceptor before the target method successfully returns.
        /// </summary>
        /// <param name="invocation">
        /// The method invocation that is being intercepted.
        /// </param>
        /// <returns>
        /// The result of the call to the
        /// <see cref="AopAlliance.Intercept.IJoinpoint.Proceed"/> method of
        /// the supplied <paramref name="invocation"/>.
        /// </returns>
        /// <exception cref="System.Exception">
        /// If any of the interceptors in the chain or the target object itself
        /// throws an exception.
        /// </exception>
        public object Invoke(IMethodInvocation invocation)
        {
            advice.Before(invocation.Method, invocation.Arguments, invocation.This);
            return invocation.Proceed();
        }
    }
}
