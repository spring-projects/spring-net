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

using System.Collections.Specialized;
using System.Reflection;
using Common.Logging;
using Spring.Objects.Factory;
using Spring.Threading;
using Spring.Util;

namespace Spring.Transaction.Interceptor
{
	/// <summary>
	/// Superclass for transaction aspects, such as the AOP Alliance-compatible
	/// <see cref="Spring.Transaction.Interceptor.TransactionInterceptor"/>.
	/// </summary>
	/// <remarks>
	/// <p>
	///  This enables the underlying Spring transaction infrastructure to be used
	///  to easily implement an aspect for any aspect system.
	/// </p>
	/// <p>
	/// Subclasses are responsible for calling methods in this class in the correct order.
	/// </p>
	/// <p>
	/// Uses the Strategy design pattern. A <see cref="Spring.Transaction.IPlatformTransactionManager"/>
	/// implementation will perform the actual transaction management
	/// </p>
	/// <p>
	/// A transaction aspect is serializable if its
	/// <see cref="Spring.Transaction.IPlatformTransactionManager"/> and
	/// <see cref="Spring.Transaction.Interceptor.ITransactionAttributeSource"/> are serializable.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Juergen Hoeller</author>
	/// <author>Griffin Caprio (.NET)</author>
	/// <author>Mark Pollack (.NET)</author>
	[Serializable]
	public class TransactionAspectSupport : IInitializingObject
	{
        //TODO work on serialization support.

		#region TransactionInfo Class
		/// <summary>
		/// Opaque object used to hold transaction information.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Subclasses must pass it back to method on this class, but not see its internals.
		/// </p>
		/// </remarks>
		internal protected class TransactionInfo
		{

			private ITransactionAttribute _transactionAttribute;
		    private string _joinpointIdentification;
            private ITransactionStatus _transactionStatus;
			private TransactionInfo _oldTransactionInfo;

            /// <summary>
            /// Creates a new instance of the
            /// <see cref="Spring.Transaction.Interceptor.TransactionAspectSupport.TransactionInfo"/>
            /// class for the supplied <paramref name="transactionAttribute"/>.
            /// </summary>
            /// <param name="transactionAttribute">The transaction attributes to associate with any transaction.</param>
            /// <param name="joinpointIdentification">The info for diagnostic display of joinpoint</param>
			public TransactionInfo( ITransactionAttribute transactionAttribute, string joinpointIdentification)
			{
				_transactionAttribute = transactionAttribute;
                _joinpointIdentification = joinpointIdentification;
			}

			/// <summary>
			/// Does this instance currently have a transaction?
			/// </summary>
			/// <value><b>True</b> if this instance has a transaction.</value>
			public bool HasTransaction
			{
				get { return _transactionStatus != null; }
			}

			/// <summary>
			/// Gets and sets the <see cref="Spring.Transaction.ITransactionStatus"/> for this object.
			/// </summary>
			public ITransactionStatus TransactionStatus
			{
				set { _transactionStatus = value; }
				get { return _transactionStatus; }
			}

			/// <summary>
			/// Binds this
			/// <see cref="Spring.Transaction.Interceptor.TransactionAspectSupport.TransactionInfo"/>
			/// instance to the thread local storage variable for the current thread and
			/// backs up the existing
			/// <see cref="Spring.Transaction.Interceptor.TransactionAspectSupport.TransactionInfo"/>
			/// object for the current thread.
			/// </summary>
			public void BindToThread()
			{
                // Expose current TransactionStatus, preserving any existing TransactionStatus
                // for restoration after this transaction is complete.
                TransactionInfo currentTransactionInfo = LogicalThreadContext.GetData(CURRENT_TRANSACTIONINFO_SLOTNAME) as TransactionInfo;
                _oldTransactionInfo = currentTransactionInfo;
                LogicalThreadContext.SetData(CURRENT_TRANSACTIONINFO_SLOTNAME, this);
			}

			/// <summary>
			/// Restores the previous
			/// <see cref="Spring.Transaction.Interceptor.TransactionAspectSupport.TransactionInfo"/>
			/// object to the current thread.
			/// </summary>
			public void RestoreThreadLocalStatus()
			{
                // Use stack to restore old transaction TransactionInfo.
                // Will be null if none was set.
                LogicalThreadContext.SetData(CURRENT_TRANSACTIONINFO_SLOTNAME, _oldTransactionInfo);
			}

			/// <summary>
			/// Gets the current
			/// <see cref="Spring.Transaction.Interceptor.ITransactionAttribute"/> for this
			/// <see cref="Spring.Transaction.Interceptor.TransactionAspectSupport.TransactionInfo"/>
			/// object.
			/// </summary>
			public ITransactionAttribute TransactionAttribute
			{
				get { return _transactionAttribute; }
			}

            /// <summary>
            /// Gets the joinpoint identification.
            /// </summary>
            /// <value>The joinpoint identification.</value>
		    public string JoinpointIdentification
		    {
		        get { return _joinpointIdentification; }
		    }
		}
		#endregion

	    #region Logging Definition

        [NonSerialized]
	    protected ILog log = LogManager.GetLogger(typeof (TransactionAspectSupport));

	    #endregion


        /// <summary>
        /// The name in thread local storage where the TransactionInfo object is located
        /// </summary>
        public const string CURRENT_TRANSACTIONINFO_SLOTNAME = "TransactionAspectSupport.CurrentTransactionInfoSlotName";

		/// <summary>
		/// The <see cref="Spring.Transaction.IPlatformTransactionManager"/> currently referenced by
		/// this aspect
		/// </summary>
		private IPlatformTransactionManager _transactionManager;

		/// <summary>
		/// The <see cref="Spring.Transaction.Interceptor.ITransactionAttributeSource"/> currently
		/// referenced by this aspect.
		/// </summary>
		private ITransactionAttributeSource _transactionAttributeSource;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.Interceptor.TransactionAspectSupport"/> class.
		/// </summary>
		public TransactionAspectSupport() {}

		/// <summary>
		/// Gets and sets the <see cref="Spring.Transaction.IPlatformTransactionManager"/> for
		/// this aspect.
		/// </summary>
		public IPlatformTransactionManager TransactionManager
		{
			get { return _transactionManager; }
			set { _transactionManager = value; }
		}

		/// <summary>
		/// Gets and sets the
		/// <see cref="Spring.Transaction.Interceptor.ITransactionAttributeSource"/> for
		/// this aspect.
		/// </summary>
		public ITransactionAttributeSource TransactionAttributeSource
		{
			get { return _transactionAttributeSource; }
			set { _transactionAttributeSource = value; }
		}

		/// <summary>
		/// Returns the <see cref="Spring.Transaction.ITransactionStatus"/>
		/// of the current method invocation.
		/// </summary>
		/// <remarks>
		/// Mainly intended for code that wants to set the current transaction
		/// rollback-only but not throw an application exception.
		/// </remarks>
		/// <exception cref="Spring.Transaction.NoTransactionException">
		/// If the transaction info cannot be found, because the method was invoked
		/// outside of an AOP invocation context.
		/// </exception>
		public static ITransactionStatus CurrentTransactionStatus
		{
			get { return CurrentTransactionInfo.TransactionStatus; }
		}

		/// <summary>
		/// Subclasses can use this to return the current
		/// <see cref="Spring.Transaction.Interceptor.TransactionAspectSupport.TransactionInfo"/>.
		/// </summary>
		/// <remarks>
		/// Only subclasses that cannot handle all operations in one method
		/// need to use this mechanism to get at the current
		/// <see cref="Spring.Transaction.Interceptor.TransactionAspectSupport.TransactionInfo"/>. 
		/// An around advice such as an AOP Alliance
		/// <see cref="AopAlliance.Intercept.IMethodInterceptor"/> can hold a reference to the 
		/// <see cref="Spring.Transaction.Interceptor.TransactionAspectSupport.TransactionInfo"/>
		/// throughout the aspect method.
		/// A <see cref="Spring.Transaction.Interceptor.TransactionAspectSupport.TransactionInfo"/>
		/// will be returned even if no transaction was created. The
		/// <see cref="Spring.Transaction.Interceptor.TransactionAspectSupport.TransactionInfo.HasTransaction"/>
		/// property can be used to query this.
		/// </remarks>
		/// <exception cref="Spring.Transaction.NoTransactionException">
		/// If no transaction has been created by an aspect.
		/// </exception>
		protected static TransactionInfo CurrentTransactionInfo
		{
			get
			{
			    TransactionInfo currentTransactionInfo =
			        LogicalThreadContext.GetData(CURRENT_TRANSACTIONINFO_SLOTNAME) as TransactionInfo;
                if (currentTransactionInfo == null)
				{
					throw new NoTransactionException("No transaction aspect-managed ITransactionStatus in scope");
				}
                return currentTransactionInfo;
			}
		}
		/// <summary>
		/// Set properties with method names as keys and transaction attribute
		/// descriptors (parsed via <see cref="Spring.Transaction.Interceptor.TransactionAttributeEditor"/>) as values:
		/// e.g. key = "MyMethod", value = "PROPAGATION_REQUIRED,readOnly".
		/// </summary>
		/// <remarks>
		/// <note>
		/// Method names are always applied to the target class, no matter if defined in an
		/// interface or the class itself.
		/// </note>
		/// <p>
		/// Internally, a
		/// <see cref="Spring.Transaction.Interceptor.NameMatchTransactionAttributeSource"/>
		/// will be created from the given properties.
		/// </p>	
		/// </remarks>
		public NameValueCollection TransactionAttributes
		{
			set
			{
				NameMatchTransactionAttributeSource attributeSource = new NameMatchTransactionAttributeSource();
			    attributeSource.NameProperties = value;
				_transactionAttributeSource = attributeSource;
			}
		}

		/// <summary>
		/// Checks that the required properties are set.
		/// </summary>
		public void AfterPropertiesSet()
		{
			if ( null == _transactionManager)
			{
				throw new ArgumentException("IPlatformTransactionManager is required");
			}
			if ( null == _transactionAttributeSource )
			{
				throw new ArgumentException("Either 'TransactionAttributeSource' or 'TransactionAttribute' property is required: " +
					"If there are no transactional methods, don't use a TransactionInterceptor " + 
					"or TransactionProxyFactoryObject." );
			}
		}

		/// <summary>
		/// Create a transaction if necessary
		/// </summary>
		/// <param name="method">Method about to execute</param>
		/// <param name="targetType">Type that the method is on</param>
		/// <returns>
		/// A <see cref="Spring.Transaction.Interceptor.TransactionAspectSupport.TransactionInfo"/> object,
		/// whether or not a transaction was created.
		/// <p>
		/// The
		/// <see cref="Spring.Transaction.Interceptor.TransactionAspectSupport.TransactionInfo.HasTransaction"/>
		/// property on the
		/// <see cref="Spring.Transaction.Interceptor.TransactionAspectSupport.TransactionInfo"/>
		/// class can be used to tell if there was a transaction created.
		/// </p>
		/// </returns>
		protected TransactionInfo CreateTransactionIfNecessary( MethodInfo method, Type targetType )
		{
            // If the transaction attribute is null, the method is non-transactional.
			ITransactionAttribute sourceAttr = _transactionAttributeSource.ReturnTransactionAttribute( method, targetType );

		    return CreateTransactionIfNecessary(sourceAttr, MethodIdentification(method));
		}

        /// <summary>
        /// Creates the transaction if necessary.
        /// </summary>
        /// <param name="sourceAttr">The source transaction attribute.</param>
        /// <param name="joinpointIdentification">The joinpoint identification.</param>
        /// <returns>Transaction Info for declarative transaction management.</returns>
	    protected TransactionInfo CreateTransactionIfNecessary(ITransactionAttribute sourceAttr, string joinpointIdentification)
	    {
	        ITransactionAttribute txAttr = sourceAttr;

            // If no name specified, apply method identification as transaction name.
	        if (txAttr != null && txAttr.Name == null)
	        {
	            txAttr = new DelegatingTransactionAttributeWithName(txAttr, joinpointIdentification);
	        }

            TransactionInfo transactionInfo = new TransactionInfo(txAttr, joinpointIdentification);            
	        if ( txAttr != null )
	        {
	            // We need a transaction for this method
	            #region Instrumentation

	            if (log.IsDebugEnabled)
	            {
	                log.Debug("Getting transaction for " + transactionInfo.JoinpointIdentification);
	            }

	            #endregion
	            // The transaction manager will flag an error if an incompatible tx already exists
	            transactionInfo.TransactionStatus = _transactionManager.GetTransaction(txAttr);
	        } 
	        else
	        {
	            // The TransactionInfo.HasTransaction property will return
	            // false. We created it only to preserve the integrity of
	            // the ThreadLocal stack maintained in this class.
	            if (log.IsDebugEnabled)
	            {
                    log.Debug("Skipping transactional joinpoint [" + joinpointIdentification +
                              "] because no transaction manager has been configured");
	            }
	        }

	        // We always bind the TransactionInfo to the thread, even if we didn't create
	        // a new transaction here. This guarantees that the TransactionInfo stack
	        // will be managed correctly even if no transaction was created by this aspect.
	        transactionInfo.BindToThread( );
	        return transactionInfo;
	    }

        /// <summary>
        /// Identifies the method by providing the qualfied method name.
        /// </summary>
        /// <param name="methodInfo">The method info.</param>
        /// <returns>qualified mehtod name.</returns>
        protected string MethodIdentification(MethodInfo methodInfo)
        {
            return ObjectUtils.GetQualifiedMethodName(methodInfo);
        }

	    /// <summary>
		/// Execute after the successful completion of call, but not after an exception was handled.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Do nothing if we didn't create a transaction.
		/// </p>
		/// </remarks>
		/// <param name="transactionInfo">
		/// The
		/// <see cref="Spring.Transaction.Interceptor.TransactionAspectSupport.TransactionInfo"/>
		/// about the current transaction.
		/// </param>
		protected void CommitTransactionAfterReturning( TransactionInfo transactionInfo )
		{
			if ( transactionInfo != null && transactionInfo.HasTransaction )
			{
                #region Instrumentation

			    if (log.IsDebugEnabled) 
			    {
                    log.Debug("Completing transaction for [" + transactionInfo.JoinpointIdentification + "]");
			    }

			    #endregion

			    _transactionManager.Commit( transactionInfo.TransactionStatus );
			}
		}

		/// <summary>
		/// Handle a exception, closing out the transaction.
		/// </summary>
		/// <remarks>
		/// <p>
		/// We may commit or roll back, depending on our configuration.
		/// </p>
		/// </remarks>
		/// <param name="transactionInfo">
		/// The
		/// <see cref="Spring.Transaction.Interceptor.TransactionAspectSupport.TransactionInfo"/>
		/// about the current transaction.
		/// </param>
		/// <param name="exception">The <see cref="System.Exception"/> encountered.</param>
		protected void CompleteTransactionAfterThrowing(
			TransactionInfo transactionInfo, Exception exception )
		{
			if ( transactionInfo != null &&  transactionInfo.HasTransaction )
			{
                if (log.IsDebugEnabled)
                {                    
                    log.Debug("Completing transaction for [" + transactionInfo.JoinpointIdentification + "] after exception: " + exception);
                }

				if ( transactionInfo.TransactionAttribute.RollbackOn( exception ))
				{
				    try
					{
						_transactionManager.Rollback( transactionInfo.TransactionStatus );
					}
					catch (Exception e)
					{
					    log.Error("Application exception overridden by rollback exception", e);
						throw;
					}
				} 
				else
				{
                    // We don't roll back on this exception.
				    // Will still roll back if TransactionStatus.RollbackOnly is true.
                    try
                    {
                        _transactionManager.Commit(transactionInfo.TransactionStatus);
                    } catch (Exception e)
                    {
                        log.Error("Application exception overriden by commit exception", e);
                        throw;
                    }

				}
			} 
		}
		/// <summary>
		/// Resets the 
		/// <see cref="Spring.Transaction.Interceptor.TransactionAspectSupport.TransactionInfo"/>
		/// for this thread.
		/// </summary>
		/// <remarks>
		/// <note>
		/// Call this in all cases: exceptions or normal return.
		/// </note>
		/// </remarks>
		/// <param name="transactionInfo">
		/// The 
		/// <see cref="Spring.Transaction.Interceptor.TransactionAspectSupport.TransactionInfo"/>
		/// about the current transaction. May be null.
		/// </param>
		protected void CleanupTransactionInfo( TransactionInfo transactionInfo )
		{
			if ( transactionInfo != null )
			{
				transactionInfo.RestoreThreadLocalStatus( );
			}
		}
	}
}
