#region License

/*
 * Copyright 2002-2004 the original author or authors.
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

using System;
using System.Data;
using System.Text;

namespace Spring.Transaction.Support
{
	/// <summary>
	/// Default implementation of the <see cref="Spring.Transaction.ITransactionDefinition"/>
	/// interface, offering object-style configuration and sensible default values.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Base class for both <see cref="System.SystemException"/> and
	/// <see cref="Spring.Transaction.Interceptor.DefaultTransactionAttribute"/>.
	/// </p>
	/// </remarks>
	/// <author>Juergen Hoeller</author>
	/// <author>Griffin Caprio (.NET)</author>
	/// <author>Mark Pollack (.NET)</author>
	[Serializable]
	public class DefaultTransactionDefinition : ITransactionDefinition
	{
		/// <summary>
		/// Prefix for Propagation settings.
		/// </summary>
		public static readonly string PROPAGATION_CONSTANT_PREFIX = "PROPAGATION";

		/// <summary>
		/// Prefix for IsolationLevel settings.
		/// </summary>
		public static readonly string ISOLATION_CONSTANT_PREFIX = "ISOLATION";

		/// <summary>
		/// Prefix for transaction timeout values in description strings.
		/// </summary>
		public static readonly string TIMEOUT_PREFIX = "timeout_";

		/// <summary>
		/// Marker for read-only transactions in description strings.
		/// </summary>
		public static readonly string READ_ONLY_MARKER = "readOnly";

		/// <summary>
		/// The default transaction timeout.
		/// </summary>
		public const int TIMEOUT_DEFAULT = -1;

	    //TODO Refactoring to sync with Spring 2.0 for nt/enums for various default values.
	    
		private TransactionPropagation _transactionPropagation = TransactionPropagation.Required;
		private IsolationLevel _transactionIsolationLevel = IsolationLevel.ReadCommitted;
		private int _timeout = DefaultTransactionDefinition.TIMEOUT_DEFAULT;
	    private bool _readOnly = false;
        private string _name = null;
#if NET_2_0
	    private System.Transactions.EnterpriseServicesInteropOption _esInteropOption;

#endif
		
		/// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Transaction.Support.DefaultTransactionDefinition"/> class.
		/// </summary>
		public DefaultTransactionDefinition() {}

		/// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Transaction.Support.DefaultTransactionDefinition"/> class
        /// with the supplied <see cref="Spring.Transaction.TransactionPropagation"/>
        /// behaviour.
		/// </summary>
		/// <param name="transactionPropagation">
		/// The desired <see cref="Spring.Transaction.TransactionPropagation"/> behavior.
		/// </param>
		public DefaultTransactionDefinition( TransactionPropagation transactionPropagation )
		{
			_transactionPropagation = transactionPropagation;
		}

		#region ITransactionDefinition Members

        // TODO change method name to same as type returned (TransactionPropagation)



		/// <summary>
		/// Gets / Sets the <see cref="Spring.Transaction.TransactionPropagation">propagation</see>
		/// behavior.
		/// </summary>
		public TransactionPropagation PropagationBehavior
		{
			get { return _transactionPropagation; }
			set { _transactionPropagation = value; }
		}


        // TODO change method name to same as type returned (TransactionPropagation)

        
		/// <summary>
		/// Return the isolation level of type <see cref="System.Data.IsolationLevel"/>.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Only makes sense in combination with
		/// <see cref="Spring.Transaction.TransactionPropagation.Required"/> and
		/// <see cref="Spring.Transaction.TransactionPropagation.RequiresNew"/>.
		/// </p>
		/// <p>
		/// Note that a transaction manager that does not support custom isolation levels
		/// will throw an exception when given any other level than
		/// <see cref="System.Data.IsolationLevel.Unspecified"/>.
		/// </p>
		/// </remarks>
		public IsolationLevel TransactionIsolationLevel
		{
			get { return _transactionIsolationLevel; }
			set { _transactionIsolationLevel = value; }
		}

		/// <summary>
		/// Return the transaction timeout.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Must return a number of seconds, or -1.
		/// Only makes sense in combination with
		/// <see cref="Spring.Transaction.TransactionPropagation.Required"/> and
		/// <see cref="Spring.Transaction.TransactionPropagation.RequiresNew"/>.
		/// Note that a transaction manager that does not support timeouts will
		/// throw an exception when given any other timeout than -1.
		/// </p>
		/// </remarks>
		public int TransactionTimeout
		{
			get { return _timeout; }
			set 
			{ 
				if ( value < DefaultTransactionDefinition.TIMEOUT_DEFAULT ) 
				{	
					throw new ArgumentException( "Timeout must be a positive integer or DefaultTransactionDefinition.TIMEOUT_DEFAULT" );
				}
				_timeout = value;
			}
		}
		
		/// <summary>
		/// Get whether to optimize as read-only transaction.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This just serves as hint for the actual transaction subsystem,
		/// it will <i>not necessarily</i> cause failure of write accesses.
		/// </p>
		/// <p>
		/// Only makes sense in combination with
		/// <see cref="Spring.Transaction.TransactionPropagation.Required"/> and
		/// <see cref="Spring.Transaction.TransactionPropagation.RequiresNew"/>.
		/// </p>
		/// <p>
		/// A transaction manager that cannot interpret the read-only hint
		/// will <i>not</i> throw an exception when given <c>ReadOnly=true</c>.
		/// </p>
		/// </remarks>
		public bool ReadOnly
		{
			get { return _readOnly; }
			set { _readOnly = value; }
		}

        /// <summary>
        /// Return the name of this transaction.  Can be null.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// This will be used as a transaction name to be shown in a
        /// transaction monitor, if applicable.  In the case of Spring
        /// declarative transactions, the exposed name will be the fully
        /// qualified type name + "." method name + assembly (by default).
        /// </remarks>
	    public string Name
	    {
	        get { return _name; }
            set { _name = value;}
	    }

#if NET_2_0
        /// <summary>
        /// Gets the enterprise services interop option.
        /// </summary>
        /// <value>The enterprise services interop option.</value>
        public System.Transactions.EnterpriseServicesInteropOption EnterpriseServicesInteropOption
        {
            get { return _esInteropOption; }
            set { _esInteropOption = value; }
        }
#endif

	    #endregion

		/// <summary>
        /// An override of the default <see cref="System.Object.Equals(object)"/> method.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare to.</param>
		/// <returns>True if the objects are equal.</returns>
		public override bool Equals(object obj)
		{
			return ( obj is ITransactionDefinition ) && ToString().Equals( obj.ToString() );
		}

		/// <summary>
        /// An override of the default <see cref="System.Object.GetHashCode"/> method that returns the
        /// hashcode of the
        /// <see cref="Spring.Transaction.Support.DefaultTransactionDefinition.DefinitionDescription"/>
        /// property.
		/// </summary>
		/// <returns>
		/// The hashcode of the
		/// <see cref="Spring.Transaction.Support.DefaultTransactionDefinition.DefinitionDescription"/>
		/// property.
        /// </returns>
		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		/// <summary>
		/// An override of the default <see cref="System.Object.ToString"/> method that returns a string
		/// representation of the
		/// <see cref="Spring.Transaction.Support.DefaultTransactionDefinition.DefinitionDescription"/>
		/// property.
		/// </summary>
        /// <returns>
        /// A string representation of the
        /// <see cref="Spring.Transaction.Support.DefaultTransactionDefinition.DefinitionDescription"/>
        /// property.
        /// </returns>
		public override string ToString()
		{
			return DefinitionDescription;
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> representation of the
		/// <see cref="Spring.Transaction.Support.DefaultTransactionDefinition.DefinitionDescription"/>
		/// property.
		/// </summary>
		protected string DefinitionDescription
		{
			get 
			{ 
				StringBuilder builder = new StringBuilder();
				builder.Append( PROPAGATION_CONSTANT_PREFIX +"_" + PropagationBehavior );
				builder.Append( "," );
				builder.Append( ISOLATION_CONSTANT_PREFIX + "_" +TransactionIsolationLevel );
				if ( TransactionTimeout != DefaultTransactionDefinition.TIMEOUT_DEFAULT ) 
				{
					builder.Append( ",timeout_" + _timeout );
				}	
				if ( ReadOnly ) 
				{
					builder.Append( ",readOnly" );
				}
				return builder.ToString();
			}
		}
	}
}
