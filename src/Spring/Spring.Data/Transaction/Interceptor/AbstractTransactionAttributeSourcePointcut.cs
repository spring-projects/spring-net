#region License

/*
 * Copyright 2002-2010 the original author or authors.
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
using Spring.Aop.Support;

namespace Spring.Transaction.Interceptor
{
    public abstract class AbstractTransactionAttributeSourcePointcut : StaticMethodMatcherPointcut
    {
        #region Overrides of StaticMethodMatcher

        /// <summary>
        /// Does the supplied <paramref name="method"/> satisfy this matcher?
        /// </summary>
        /// <remarks>
        /// <p>
        /// Must be implemented by a derived class in order to specify matching
        /// rules.
        /// </p>
        /// </remarks>
        /// <param name="method">The candidate method.</param>
        /// <param name="targetType">
        /// The target <see cref="System.Type"/> (may be <see langword="null"/>,
        /// in which case the candidate <see cref="System.Type"/> must be taken
        /// to be the <paramref name="method"/>'s declaring class).
        /// </param>
        /// <returns>
        /// <see langword="true"/> if this this method matches statically.
        /// </returns>
        public override bool Matches(MethodInfo method, Type targetType)
        {
            ITransactionAttributeSource tas = TransactionAttributeSource;
            return (tas == null || TransactionAttributeSource.ReturnTransactionAttribute(method, targetType) != null);
        }

        #endregion

        protected abstract ITransactionAttributeSource TransactionAttributeSource { get; }
    }
}
