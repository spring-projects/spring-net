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
using AopAlliance.Aop;
using Spring.Aop.Framework;
using Spring.Aop.Support;

namespace Spring.Transaction.Interceptor
{
	/// <summary>
	/// Advisor driven by a <see cref="Spring.Transaction.Interceptor.ITransactionAttributeSource"/>,
    /// used to exclude a general advice <see cref="IAdvice"/> from methods that
	/// are non-transactional.
	/// </summary>
	/// <remarks>
	/// <para>
	/// To put it another way, use this advisor when you would like to associate other AOP
	/// advice based on the pointcut specified by declarative transaction demarcation,
	/// attibute based or otherwise.
	/// </para>
	/// <p>
	/// Because the AOP framework caches advice calculations, this is normally
    /// faster than just letting the <see cref="AopAlliance.Intercept.IInterceptor"/>
	/// run and find out itself that it has no work to do.
	/// </p>
	/// </remarks>
	/// <author>Mark Pollack (.NET)</author>
	[Serializable]
	public class DefaultTransactionAttributeSourceAdvisor : StaticMethodMatcherPointcutAdvisor
	{
		private ITransactionAttributeSource _transactionAttributeSource;

		/// <summary>
		/// Creates a new instance of the
        /// <see cref="Spring.Transaction.Interceptor.DefaultTransactionAttributeSourceAdvisor"/> class.
		/// </summary>
        public DefaultTransactionAttributeSourceAdvisor(ITransactionAttributeSource transactionAttributeSource, IAdvice advice)
			: base( advice )
		{
            if (transactionAttributeSource == null)
			{
                throw new AopConfigException("Cannot construct a DefaultTransactionAttributeSourceAdvisor if " +
					"TransactionAttributeSource is null.");
			}
            _transactionAttributeSource = transactionAttributeSource;
		}

		/// <summary>
		/// Tests the input method to see if it's covered by the advisor.
		/// </summary>
		/// <param name="method">The method to match.</param>
		/// <param name="targetClass">The <see cref="System.Type"/> to match against.</param>
		/// <returns>
		/// <b>True</b> if the supplied <paramref name="method"/> is covered by the advisor.
		/// </returns>
		public override bool Matches(MethodInfo method, Type targetClass)
		{
		    return ( _transactionAttributeSource.ReturnTransactionAttribute( (MethodInfo)method, targetClass ) != null);
		}
	}
}
