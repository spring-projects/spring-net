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

using AopAlliance.Aop;
using AopAlliance.Intercept;

#endregion

namespace Spring.Aop.Framework.Adapter
{
    /// <summary>
    /// <see cref="Spring.Aop.Framework.Adapter.IAdvisorAdapter"/> implementation
    /// to enable <see cref="Spring.Aop.IMethodBeforeAdvice"/> to be used in the
    /// Spring.NET AOP framework.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Aleksandar Seovic (.NET)</author>
    [Serializable]
    internal class BeforeAdviceAdapter : IAdvisorAdapter
    {
        /// <summary>
        /// Returns <see langword="true"/> if the supplied
        /// <paramref name="advice"/> is an instance of the
        /// <see cref="Spring.Aop.IMethodBeforeAdvice"/> interface.
        /// </summary>
        /// <param name="advice">The advice to check.</param>
        /// <returns>
        /// <see langword="true"/> if the supplied <paramref name="advice"/> is
        /// an instance of the <see cref="Spring.Aop.IMethodBeforeAdvice"/> interface;
        /// <see langword="false"/> if not or if the supplied
        /// <paramref name="advice"/> is <cref lang="null"/>.
        /// </returns>
        public virtual bool SupportsAdvice(IAdvice advice)
        {
            return advice is IMethodBeforeAdvice;
        }

        /// <summary>
        /// Wraps the supplied <paramref name="advisor"/>'s
        /// <see cref="Spring.Aop.IAdvisor.Advice"/> within a
        /// <see cref="Spring.Aop.Framework.Adapter.MethodBeforeAdviceInterceptor"/>
        /// instance.
        /// </summary>
        /// <param name="advisor">
        /// The advisor exposing the <see cref="AopAlliance.Aop.IAdvice"/> that
        /// is to be wrapped.
        /// </param>
        /// <returns>
        /// The supplied <paramref name="advisor"/>'s
        /// <see cref="Spring.Aop.IAdvisor.Advice"/> wrapped within a
        /// <see cref="Spring.Aop.Framework.Adapter.MethodBeforeAdviceInterceptor"/>
        /// instance.
        /// </returns>
        public virtual IInterceptor GetInterceptor(IAdvisor advisor)
        {
            IMethodBeforeAdvice advice = (IMethodBeforeAdvice) advisor.Advice;
            return new MethodBeforeAdviceInterceptor(advice);
        }
    }
}
