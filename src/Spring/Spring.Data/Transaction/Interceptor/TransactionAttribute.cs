#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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
using System.Data;
using Spring.Transaction.Support;


#endregion

namespace Spring.Transaction.Interceptor
{
	/// <summary>
	/// .NET Attribute for describing transactional behavior of methods in a class. 
	/// </summary>
	/// <remarks>This attribute type is generally directly comparable
	/// to Spring's <see cref="RuleBasedTransactionAttribute"/> class and
	/// in fact <see cref="AttributesTransactionAttributeSource"/> will
    /// directly convert the data to a <see cref="RuleBasedTransactionAttribute"/>
    /// so that Spring's transaction support code does not have to know about
    /// attributes. If no rules are relevant to the exception it will be treaded
    /// like DefaultTransactionAttribute, (rolling back on all exceptions).
	/// <para>
    /// The default property values are TransactionPropagation.Required, IsolationLevel.ReadCommitted,
    /// DefaultTransactionDefinition.TIMEOUT_DEFAULT (can be changed, by default is the default
    /// value of the underlying transaction subsystem)
    /// ReadOnly = false, and Type.EmtptyTypes specified for Rollbackfor and NoRollbackFor exception
    /// types.
    /// </para>
    /// </remarks>
	/// <author>Mark Pollack (.NET)</author>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, 
         Inherited = true)]
    [Serializable]
	public class TransactionAttribute : Attribute
	{
		#region Fields
        private TransactionPropagation _transactionPropagation = TransactionPropagation.Required;
        private IsolationLevel _isolationLevel = IsolationLevel.ReadCommitted;
        private int _timeout = DefaultTransactionDefinition.TIMEOUT_DEFAULT;
        private bool _readOnly = false;
        private Type[] _rollbackTypes = Type.EmptyTypes;
        private Type[] _noRollbackTypes = Type.EmptyTypes;
#if NET_2_0
	    private System.Transactions.EnterpriseServicesInteropOption _esInteropOption =
            System.Transactions.EnterpriseServicesInteropOption.Automatic;
#endif	

		#endregion

		#region Constructor (s)
		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionAttribute"/> class.
        /// </summary>
		public TransactionAttribute()
		{

		}

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionAttribute"/> class.
        /// </summary>
        /// <param name="transactionPropagation">The transaction propagation.</param>
        public TransactionAttribute(TransactionPropagation transactionPropagation) : this()
        {
            _transactionPropagation = transactionPropagation;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionAttribute"/> class.
        /// </summary>
        /// <param name="transactionPropagation">The transaction propagation.</param>
        /// <param name="isolationLevel">The isolation level.</param>
        public TransactionAttribute(TransactionPropagation transactionPropagation,
                                    IsolationLevel isolationLevel) : this(transactionPropagation)
        {
            _isolationLevel = isolationLevel;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionAttribute"/> class.
        /// </summary>
        /// <param name="isolationLevel">The isolation level.</param>
        public TransactionAttribute(IsolationLevel isolationLevel)
        {
            _isolationLevel = isolationLevel;
        }

		#endregion

		#region Properties

        /// <summary>
        /// Gets the transaction propagation.
        /// </summary>
        /// <remarks>Defaults to TransactionPropagation.Required</remarks>
        /// <value>The transaction propagation.</value>
	    public TransactionPropagation TransactionPropagation
	    {
	        get { return _transactionPropagation; }
	    }

        /// <summary>
        /// Gets the isolation level.
        /// </summary>
        /// <remarks>Defaults to IsolationLevel.Unspecified</remarks>
        /// <value>The isolation level.</value>
	    public IsolationLevel IsolationLevel
	    {
	        get { return _isolationLevel; }
	    }

        /// <summary>
        /// Gets or sets the timeout.
        /// </summary>
        /// <remarks>Defaults to the default timeout of the underlying transaction system.</remarks>
        /// <value>The timeout.</value>
	    public int Timeout
	    {
	        get { return _timeout; }
	        set { _timeout = value; }
	    }

        /// <summary>
        /// Gets or sets a value indicating whether the transaction is readonly.
        /// </summary>
        /// <remarks>Defaults to false</remarks>
        /// <value><c>true</c> if read-only; otherwise, <c>false</c>.</value>
	    public bool ReadOnly
	    {
	        get { return _readOnly; }
	        set { _readOnly = value; }
	    }

        /// <summary>
        /// Gets or sets the zero or more exception types which
        /// indicating which exception types must cause a transaction
        /// rollback.
        /// </summary>
        /// <remarks>This is the preferred way to construct a rollback rule,
        /// matching the exception class and subclasses.</remarks>
        /// <value>The rollback types.</value>
	    public Type[] RollbackFor
	    {
	        get { return _rollbackTypes; }
	        set { _rollbackTypes = value; }
	    }

        /// <summary>
        /// Gets or sets zero or more exceptions types which
        /// indicationg which exception type must <c>not</c>
        /// cause a transaction rollback.
        /// </summary>
        /// <remarks>This is the preferred way to construct a rollback rule,
        /// matching the exception type.</remarks>
        /// <value>The no rollback for.</value>
	    public Type[] NoRollbackFor
	    {
	        get { return _noRollbackTypes; }
	        set { _noRollbackTypes = value; }
	    }

       
#if NET_2_0
        /// <summary>
        /// Gets or sets the enterprise services interop option.
        /// </summary>
        /// <value>The enterprise services interop option.</value>
	    public System.Transactions.EnterpriseServicesInteropOption EnterpriseServicesInteropOption
	    {
	        get { return _esInteropOption;}
            set { _esInteropOption = value; }
	    }
#endif

	    #endregion

		#region Methods

		#endregion

	}
}
