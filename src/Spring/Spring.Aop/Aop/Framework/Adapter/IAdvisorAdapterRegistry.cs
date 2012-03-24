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

using AopAlliance.Intercept;

#endregion

namespace Spring.Aop.Framework.Adapter
{
    /// <summary>
    /// A registry of
    /// <see cref="Spring.Aop.Framework.Adapter.IAdvisorAdapter"/> instances.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Implementations <b>must</b> also automatically register adapters for
    /// <see cref="AopAlliance.Intercept.IInterceptor"/> types.
    /// </p>
    /// <note>
    /// This is an SPI interface, that should not need to be implemented by any
    /// Spring.NET user.
    /// </note>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Aleksandar Seovic (.NET)</author>
    public interface IAdvisorAdapterRegistry
    {
        /// <summary>
        /// Returns an <see cref="Spring.Aop.IAdvisor"/> wrapping the supplied
        /// <paramref name="advice"/>.
        /// </summary>
        /// <param name="advice">
        /// The object that should be an advice, such as
        /// <see cref="Spring.Aop.IBeforeAdvice"/> or
        /// <see cref="Spring.Aop.IThrowsAdvice"/>.
        /// </param>
        /// <returns> 
        /// An <see cref="Spring.Aop.IAdvisor"/> wrapping the supplied
        /// <paramref name="advice"/>. Never returns <cref lang="null"/>. If
        /// the <paramref name="advice"/> parameter is an
        /// <see cref="Spring.Aop.IAdvisor"/>, it will simply be returned.
        /// </returns>
        /// <exception cref="UnknownAdviceTypeException">
        /// If no registered
        /// <see cref="Spring.Aop.Framework.Adapter.IAdvisorAdapter"/> can wrap
        /// the supplied <paramref name="advice"/>.
        /// </exception>
        IAdvisor Wrap(object advice);

        /// <summary>
        /// Returns an <see cref="AopAlliance.Intercept.IInterceptor"/> to
        /// allow the use of the supplied <paramref name="advisor"/> in an
        /// interception-based framework. 
        /// </summary>
        /// <remarks>
        /// <p>
        /// Don't worry about the pointcut associated with the
        /// <see cref="Spring.Aop.IAdvisor"/>; if it's an
        /// <see cref="Spring.Aop.IPointcutAdvisor"/>, just return an
        /// interceptor.
        /// </p>
        /// </remarks>
        /// <param name="advisor">
        /// The advisor to find an interceptor for.
        /// </param>
        /// <returns>
        /// An interceptor to expose this advisor's behaviour.
        /// </returns>
        /// <exception cref="UnknownAdviceTypeException">
        /// If the advisor type is not understood by any registered
        /// <see cref="Spring.Aop.Framework.Adapter.IAdvisorAdapter"/>.
        /// </exception>
        IInterceptor GetInterceptor(IAdvisor advisor);

        /// <summary>
        /// Register the given <see cref="Spring.Aop.Framework.Adapter.IAdvisorAdapter"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Note that it is not necessary to register adapters for
        /// <see cref="AopAlliance.Intercept.IInterceptor"/> instances: these
        /// must be automatically recognized by an
        /// <see cref="Spring.Aop.Framework.Adapter.IAdvisorAdapterRegistry"/>
        /// implementation.
        /// </p>
        /// </remarks>
        /// <param name="adapter">
        /// An <see cref="Spring.Aop.Framework.Adapter.IAdvisorAdapter"/> that
        /// understands the particular advisor and advice types.
        /// </param>
        void RegisterAdvisorAdapter(IAdvisorAdapter adapter);
    }
}