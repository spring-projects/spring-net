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
using Spring.Aop.Framework;
using Spring.Aop.Support;

namespace Spring.Transaction.Interceptor
{
	/// <summary>
	/// Advisor driven by a <see cref="Spring.Transaction.Interceptor.ITransactionAttributeSource"/>, used to include
	/// a <see cref="Spring.Transaction.Interceptor.TransactionInterceptor"/> for methods that
	/// are transactional.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Because the AOP framework caches advice calculations, this is normally
	/// faster than just letting the <see cref="Spring.Transaction.Interceptor.TransactionInterceptor"/>
	/// run and find out itself that it has no work to do.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Griffin Caprio (.NET)</author>
	[Serializable]
	public class TransactionAttributeSourceAdvisor : StaticMethodMatcherPointcutAdvisor
	{
	    #region Fields

	    private ITransactionAttributeSource _transactionAttributeSource;

	    #endregion

	    #region Constructor(s)

	    /// <summary>
	    /// Initializes a new instance of the <see cref="TransactionAttributeSourceAdvisor"/> class.
	    /// </summary>
	    /// <remarks>
	    /// 	<p>
	    /// This is an abstract class, and as such has no publicly
	    /// visible constructors.
	    /// </p>
	    /// </remarks>
	    public TransactionAttributeSourceAdvisor()
	    {
	    }

	    /// <summary>
	    /// Creates a new instance of the
	    /// <see cref="Spring.Transaction.Interceptor.TransactionAttributeSourceAdvisor"/> class.
	    /// </summary>
	    /// <param name="transactionInterceptor">
	    /// The pre-configured transaction interceptor.
	    /// </param>
	    public TransactionAttributeSourceAdvisor( TransactionInterceptor transactionInterceptor )
	        : base( transactionInterceptor )
	    {
	        SetTxAttributeSource(transactionInterceptor);
	    }

	    #endregion

	    #region Properties

	    /// <summary>
	    /// Sets the transaction interceptor.
	    /// </summary>
	    /// <value>The transaction interceptor.</value>
	    public TransactionInterceptor TransactionInterceptor
	    {
	        set
	        {
	            //TODO refactor
	            Advice = value;
	            SetTxAttributeSource(value);
	        }
        }

        #endregion

	    #region Methods

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
            return (_transactionAttributeSource.ReturnTransactionAttribute(method, targetClass) != null);
        }

        /// <summary>
        /// Sets the tx attribute source.
        /// </summary>
        /// <param name="transactionInterceptor">The transaction interceptor.</param>
	    protected void SetTxAttributeSource(TransactionInterceptor transactionInterceptor)
	    {
	        if (transactionInterceptor.TransactionAttributeSource == null)
	        {
	            throw new AopConfigException("Cannot construct a TransactionAttributeSourceAdvisor using a " +
	                                         "TransactionInterceptor that has no TransactionAttributeSource configured.");
	        }
	        _transactionAttributeSource = transactionInterceptor.TransactionAttributeSource;
	    }

	    #endregion
	}
}
