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

using Common.Logging;
using Spring.Objects.Factory;

namespace Spring.Transaction.Support
{
    /// <summary>
    /// Helper class that simplifies programmatic transaction demarcation and
    /// transaction exception handling.
    /// </summary>
    /// <remarks>
    /// <p>
    /// The central methods are
    /// <see cref="Spring.Transaction.Support.TransactionTemplate.Execute(ITransactionCallback)"/>
    /// and <see cref="Spring.Transaction.Support.TransactionTemplate.Execute(TransactionDelegate)"/>
    /// supporting transactional code wrapped in the delegate instance. It handles the
    /// transaction lifecycle and possible exceptions such that neither the delegate
    /// implementation nor the calling code needs to explicitly handle transactions.
    /// </p>
    /// <p>
    /// Can be used within a service implementation via direct instantiation with
    /// a transaction manager reference, or get prepared in an application context
    /// and given to services as object reference.
    /// </p>
    /// <note>
    /// The transaction manager should always be configured as an object in the application
    /// context, in the first case given to the service directly, in the second case to the
    /// prepared template.
    /// </note>
    /// <p>
    /// Supports setting the propagation behavior and the isolation level by name,
    /// for convenient configuration in context definitions.
    /// </p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    /// <author>Griffin Caprio (.NET)</author>
    public class TransactionTemplate : DefaultTransactionDefinition, ITransactionOperations, IInitializingObject
    {
        private IPlatformTransactionManager _platformTransactionManager;

        #region Logging Definition

        protected readonly ILog log = LogManager.GetLogger(typeof(TransactionTemplate));

        #endregion
        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Transaction.Support.TransactionTemplate"/> class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Mainly targeted at configuration by an object factory.
        /// </p>
        /// <note>
        /// The
        /// <see cref="Spring.Transaction.Support.TransactionTemplate.PlatformTransactionManager"/>
        /// property must be set before any calls to the
        /// <see cref="Spring.Transaction.Support.TransactionTemplate.Execute(ITransactionCallback)"/>
        /// or <see cref="Spring.Transaction.Support.TransactionTemplate.Execute(TransactionDelegate)"/>
        /// method.
        /// </note>
        /// </remarks>
        /// <seealso cref="Spring.Transaction.IPlatformTransactionManager"/>
        public TransactionTemplate() {}

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Transaction.Support.TransactionTemplate"/> class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Mainly targeted at configuration by an object factory.
        /// </p>
        /// </remarks>
        /// <param name="platformTransactionManager">
        /// The transaction management strategy to be used.
        /// </param>
        public TransactionTemplate( IPlatformTransactionManager platformTransactionManager )
        {
            _platformTransactionManager = platformTransactionManager;
        }

        /// <summary>
        /// Gets and sets the <see cref="Spring.Transaction.IPlatformTransactionManager"/> to
        /// be used.
        /// </summary>
        public IPlatformTransactionManager PlatformTransactionManager
        {
            get { return _platformTransactionManager; }
            set { _platformTransactionManager = value; }
        }

        #region IInitializingObject Members
        /// <summary>
        /// Ensures that the
        /// <see cref="Spring.Transaction.Support.TransactionTemplate.PlatformTransactionManager"/>
        /// has been set.
        /// </summary>
        public void AfterPropertiesSet()
        {
            if ( _platformTransactionManager == null )
            {
                throw new ArgumentException( "IPlatformTransactionManager instance is required." );
            }
        }
        #endregion


        /// <summary>
        /// Executes the the action specified by the given delegate callback within a transaction.
        /// </summary>
        /// <param name="transactionMethod">The delegate that specifies the transactional action.</param>
        /// <returns>
        /// A result object returned by the callback, or <code>null</code> if one
        /// </returns>
        /// <remarks>Allows for returning a result object created within the transaction, that is,
        /// a domain object or a collection of domain objects.  An exception thrown by the callback
        /// is treated as a fatal exception that enforces a rollback.  Such an exception gets
        /// propagated to the caller of the template.
        /// </remarks>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// In case of initialization or system errors.
        /// </exception>
        public object Execute( TransactionDelegate transactionMethod )
        {
            ITransactionStatus status = _platformTransactionManager.GetTransaction( this );
            object result;
            try
            {
                result = transactionMethod( status );
            }
            catch ( Exception ex )
            {
                rollbackOnException( status, ex );
                throw;
            }
            _platformTransactionManager.Commit( status );
            return result;
        }


        /// <summary>
        /// Executes the action specified by the given callback object within a transaction.
        /// </summary>
        /// <param name="action">The callback object that specifies the transactional action.</param>
        /// <returns>
        /// A result object returned by the callback, or <code>null</code> if one
        /// </returns>
        /// <remarks>Allows for returning a result object created within the transaction, that is,
        /// a domain object or a collection of domain objects.  An exception thrown by the callback
        /// is treated as a fatal exception that enforces a rollback.  Such an exception gets
        /// propagated to the caller of the template.
        /// </remarks>
        /// <exception cref="Spring.Transaction.TransactionException">
        /// In case of initialization or system errors.
        /// </exception>
        public object Execute(ITransactionCallback action)
        {
            ITransactionStatus status = _platformTransactionManager.GetTransaction( this );
            object result;
            try
            {
                result = action.DoInTransaction(status);
            }
            catch ( Exception ex )
            {
                rollbackOnException( status, ex );
                throw;
            }
            _platformTransactionManager.Commit( status );
            return result;
        }

        /// <summary>
        /// Perform a rollback, handling rollback exceptions properly.
        /// </summary>
        /// <param name="status">The object representing the transaction.</param>
        /// <param name="exception">The thrown application exception or error.</param>
        private void rollbackOnException( ITransactionStatus status, Exception exception )
        {
            if (log.IsDebugEnabled)
            {
                log.Debug("Initiating transaction rollback on application exception", exception);
            }
            try
            {
                _platformTransactionManager.Rollback( status );
            }
            catch ( Exception ex )
            {
                log.Error("Application exception overridden by rollback exception", ex);
                throw;
            }
        }
    }
}
