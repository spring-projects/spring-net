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

using Spring.Transaction;
using Spring.Transaction.Support;

namespace Spring.Testing.Microsoft
{
    /// <summary>
    /// Convenient superclass for tests that should occur in a transaction, but normally
    /// will roll the transaction back on the completion of each test.
    /// </summary>
    /// <remarks>
    /// <p>This is useful in a range of circumstances, allowing the following benefits:</p>
    /// <ul>
    /// <li>Ability to delete or insert any data in the database, without affecting other tests</li>
    /// <li>Providing a transactional context for any code requiring a transaction</li>
    /// <li>Ability to write anything to the database without any need to clean up.</li>
    /// </ul>
    ///
    /// <p>This class is typically very fast, compared to traditional setup/teardown scripts.</p>
    ///
    /// <p>If data should be left in the database, call the <code>SetComplete()</code>
    /// method in each test. The "DefaultRollback" property, which defaults to "true",
    /// determines whether transactions will complete by default.</p>
    ///
    /// <p>It is even possible to end the transaction early; for example, to verify lazy
    /// loading behavior of an O/R mapping tool. (This is a valuable away to avoid
    /// unexpected errors when testing a web UI, for example.)  Simply call the
    /// <code>endTransaction()</code> method. Execution will then occur without a
    /// transactional context.</p>
    ///
    /// <p>The <code>StartNewTransaction()</code> method may be called after a call to
    /// <code>EndTransaction()</code> if you wish to create a new transaction, quite
    /// independent of the old transaction. The new transaction's default fate will be to
    /// roll back, unless <code>setComplete()</code> is called again during the scope of the
    /// new transaction. Any number of transactions may be created and ended in this way.
    /// The final transaction will automatically be rolled back when the test case is
    /// torn down.</p>
    ///
    /// <p>Transactional behavior requires a single object in the context implementing the
    /// IPlatformTransactionManager interface. This will be set by the superclass's
    /// Dependency Injection mechanism. If using the superclass's Field Injection mechanism,
    /// the implementation should be named "transactionManager". This mechanism allows the
    /// use of this superclass even when there's more than one transaction manager in the context.</p>
    /// 
    /// <p><i>This superclass can also be used without transaction management, if no
    /// IPlatformTransactionManager object is found in the context provided. Be careful about
    /// using this mode, as it allows the potential to permanently modify data.
    /// This mode is available only if dependency checking is turned off in
    /// the AbstractDependencyInjectionSpringContextTests superclass. The non-transactional
    /// capability is provided to enable use of the same subclass in different environments.</i></p>
    /// 
    /// </remarks>
    ///     
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans</author>
    /// <author>Mark Pollack (.NET)</author>
    public abstract class AbstractTransactionalSpringContextTests : AbstractDependencyInjectionSpringContextTests
    {
        /// <summary>
        /// The transaction manager to use
        /// </summary>
        private IPlatformTransactionManager transactionManager;

        /// <summary>
        /// Should we roll back by default?
        /// </summary>
        private bool defaultRollback = true;

        /// <summary>
        /// Should we commit the current transaction?
        /// </summary>
        private bool complete = false;

        /// <summary>
        /// Number of transactions started
        /// </summary>
        private int transactionsStarted = 0;

        /// <summary>
        /// Default transaction definition is used.
        /// Subclasses can change this to cause different behaviour.
        /// </summary>
        private ITransactionDefinition transactionDefinition = new DefaultTransactionDefinition();

        /// <summary>
        /// TransactionStatus for this test. Typical subclasses won't need to use it.
        /// </summary>
        private ITransactionStatus transactionStatus;


        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractTransactionalSpringContextTests"/> class.
        /// </summary>
        public AbstractTransactionalSpringContextTests()
        {
        }

        /// <summary>
        /// Sets the transaction manager to use.
        /// </summary>
        public IPlatformTransactionManager TransactionManager
        {
            protected get { return transactionManager; }
            set { transactionManager = value; }
        }

        /// <summary>
        /// Sets the default rollback flag.
        /// </summary>
        public bool DefaultRollback
        {
            set { defaultRollback = value; }
        }

        /// <summary>
        /// Set the <see cref="ITransactionDefinition"/> to be used
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="DefaultTransactionDefinition"/>
        /// </remarks>
        protected ITransactionDefinition TransactionDefinition
        {
            set { transactionDefinition = value; }
        }

        /// <summary>
        /// TransactionStatus for this test. Typical subclasses won't need to use it.
        /// </summary>
        protected ITransactionStatus TransactionStatus
        {
            get { return transactionStatus; }
            set { transactionStatus = value; }
        }

        /// <summary>
        /// Prevents the transaction.
        /// </summary>
        protected virtual void PreventTransaction()
        {
            this.transactionDefinition = null;
        }


        /// <summary>
        /// Creates a transaction
        /// </summary>
        protected override void OnTestInitialize()
        {
            this.complete = !this.defaultRollback;

            if (this.transactionManager == null)
            {
                logger.Info("No transaction manager set: test will NOT run within a transaction");
            }
            else if (this.transactionDefinition == null)
            {
                logger.Info("No transaction definition set: test will NOT run within a transaction");
            }
            else
            {
                OnSetUpBeforeTransaction();
                StartNewTransaction();
                try
                {
                    OnSetUpInTransaction();
                }
                catch (Exception)
                {
                    EndTransaction();
                    throw;
                }
            }
        }


        /// <summary>
        /// Callback method called before transaction is setup.
        /// </summary>
        protected virtual void OnSetUpBeforeTransaction()
        {
        }

        /// <summary>
        /// Callback method called after transaction is setup.
        /// </summary>
        protected virtual void OnSetUpInTransaction()
        {
        }

        /// <summary>
        /// rollback the transaction.
        /// </summary>
        protected override void OnTestCleanup()
        {
            // Call onTearDownInTransaction and end transaction if the transaction is still active.
            if (this.transactionStatus != null && !this.transactionStatus.Completed)
            {
                try
                {
                    OnTearDownInTransaction();
                }
                finally
                {
                    EndTransaction();
                }
            }
            // Call onTearDownAfterTransaction if there was at least one transaction,
            // even if it has been completed early through an endTransaction() call.
            if (this.transactionsStarted > 0)
            {
                OnTearDownAfterTransaction();
            }
        }

        /// <summary>
        /// Callback before rolling back the transaction.
        /// </summary>
        protected virtual void OnTearDownInTransaction()
        {
        }

        /// <summary>
        /// Callback after rolling back the transaction.
        /// </summary>
        protected virtual void OnTearDownAfterTransaction()
        {
        }

        /// <summary>
        /// Set the complete flag..
        /// </summary>
        protected virtual void SetComplete()
        {
            if (this.transactionManager == null)
            {
                throw new InvalidOperationException("No transaction manager set");
            }
            this.complete = true;
        }

        /// <summary>
        /// Ends the transaction.
        /// </summary>
        protected virtual void EndTransaction()
        {
            if (this.transactionStatus != null)
            {
                try
                {
                    if (!this.complete)
                    {
                        this.transactionManager.Rollback(this.transactionStatus);
                        logger.Info("Rolled back transaction after test execution");
                    }
                    else
                    {
                        this.transactionManager.Commit(this.transactionStatus);
                        logger.Info("Committed transaction after test execution");
                    }
                }
                finally
                {
                    this.transactionStatus = null;
                }
            }
        }

        /// <summary>
        /// Starts the new transaction.
        /// </summary>
        protected void StartNewTransaction()
        {
            if (this.transactionStatus != null)
            {
                throw new InvalidOperationException("Cannot start new transaction without ending existing transaction: " +
                                                "Invoke endTransaction() before startNewTransaction()");
            }
            if (this.transactionManager == null)
            {
                throw new InvalidOperationException("No transaction manager set");
            }

            this.transactionStatus = this.transactionManager.GetTransaction(this.transactionDefinition);
            ++this.transactionsStarted;
            this.complete = !this.defaultRollback;

            if (logger.IsInfoEnabled)
            {
                logger.Info("Began transaction (" + this.transactionsStarted + "): transaction manager [" +
                            this.transactionManager + "]; default rollback = " + this.defaultRollback);
            }
        }
    }
}
