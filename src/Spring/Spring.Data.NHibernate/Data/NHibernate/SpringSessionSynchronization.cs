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

using Common.Logging;
using NHibernate;
using NHibernate.Engine;
using Spring.Core;
using Spring.Data.Support;
using Spring.Transaction.Support;

namespace Spring.Data.NHibernate
{
    /// <summary>
    /// NHibnerations actions taken during the transaction lifecycle.
    /// </summary>
    /// <author>Mark Pollack (.NET)</author>
    public class SpringSessionSynchronization : TransactionSynchronizationAdapter, IOrdered
    {
        /// <summary>
        /// The <see cref="ILog"/> instance for this class. 
        /// </summary>
        private readonly ILog log = LogManager.GetLogger(typeof(SpringSessionSynchronization));

        private readonly SessionHolder sessionHolder;

        private readonly ISessionFactory sessionFactory;

        private readonly IAdoExceptionTranslator adoExceptionTranslator;

        private readonly bool newSession;

        private bool holderActive = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpringSessionSynchronization"/> class.
        /// </summary>
        public SpringSessionSynchronization(SessionHolder sessionHolder, ISessionFactory sessionFactory,
            IAdoExceptionTranslator adoExceptionTranslator, bool newSession)
        {
            this.sessionHolder = sessionHolder;
            this.sessionFactory = sessionFactory;
            this.adoExceptionTranslator = adoExceptionTranslator;
            this.newSession = newSession;
        }

        /// <summary>
        /// Return the order value of this object, where a higher value means greater in
        /// terms of sorting.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Normally starting with 0 or 1, with <see cref="System.Int32.MaxValue"/> indicating
        /// greatest. Same order values will result in arbitrary positions for the affected
        /// objects.
        /// </p>
        /// <p>
        /// Higher value can be interpreted as lower priority, consequently the first object
        /// has highest priority.
        /// </p>
        /// </remarks>
        /// <returns>The order value.</returns>
        public int Order
        {
            get { return SessionFactoryUtils.SESSION_SYNCHRONIZATION_ORDER; }
        }

        /// <summary>
        /// Suspend this synchronization. 
        /// </summary>
        /// <remarks>
        /// <p>
        /// Unbind Hibernate resources (SessionHolder) from
        /// <see cref="Spring.Transaction.Support.TransactionSynchronizationManager"/>
        /// if managing any.
        /// </p>
        /// </remarks>
        public override void Suspend()
        {
            if (this.holderActive)
            {
                TransactionSynchronizationManager.UnbindResource(this.sessionFactory);
            }
        }

        /// <summary>
        /// Resume this synchronization.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Rebind Hibernate resources from 
        /// <see cref="Spring.Transaction.Support.TransactionSynchronizationManager"/>
        /// if managing any.
        /// </p>
        /// </remarks>
        public override void Resume()
        {
            if (this.holderActive)
            {
                TransactionSynchronizationManager.BindResource(this.sessionFactory, this.sessionHolder);
            }
        }

        /// <summary>
        /// Invoked before transaction commit (before
        /// <see cref="Spring.Transaction.Support.ITransactionSynchronization.BeforeCompletion"/>)
        /// </summary>
        /// <param name="readOnly">
        /// If the transaction is defined as a read-only transaction.
        /// </param>
        /// <remarks>
        /// <p>
        /// Can flush transactional sessions to the database.
        /// </p>
        /// <p>
        /// Note that exceptions will get propagated to the commit caller and 
        /// cause a rollback of the transaction.
        /// </p>
        /// </remarks>
        public override void BeforeCommit(bool readOnly)
        {
            if (!readOnly)
            {
                // read-write transaction -> flush the Hibernate Session
                log.Debug("Flushing Hibernate Session on transaction synchronization");
                ISession session = this.sessionHolder.Session;
                //Further check: only flush when not FlushMode.NEVER
                if (session.FlushMode != FlushMode.Never)
                {
                    try
                    {
                        session.Flush();
                        //TODO can throw System.ObjectDisposedException...
                    }
                    catch (ADOException ex)
                    {
                        if (this.adoExceptionTranslator != null)
                        {
                            //TODO investigate how ADOException wraps inner exception.
                            throw this.adoExceptionTranslator.Translate(
                                "Hibernate transaction synchronization: " + ex.Message, null, ex.InnerException);
                        }
                        else
                        {
                            throw new HibernateAdoException("ADO.NET Exception", ex);
                        }
                    }
                    catch (HibernateException ex)
                    {
                        throw SessionFactoryUtils.ConvertHibernateAccessException(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Invoked before transaction commit (before
        /// <see cref="Spring.Transaction.Support.ITransactionSynchronization.BeforeCompletion"/>)
        /// Can e.g. flush transactional O/R Mapping sessions to the database
        /// </summary>
        /// <remarks>
        /// <para>
        /// This callback does not mean that the transaction will actually be
        /// commited.  A rollback decision can still occur after this method
        /// has been called.  This callback is rather meant to perform work 
        /// that's only relevant if a commit still has a chance
        /// to happen, such as flushing SQL statements to the database.
        /// </para>
        /// <para>
        /// Note that exceptions will get propagated to the commit caller and cause a
        /// rollback of the transaction.</para>
        /// <para>
        /// (note: do not throw TransactionException subclasses here!)
        /// </para>
        /// </remarks>
        public override void BeforeCompletion()
        {
            if (this.newSession)
            {
                // Default behavior: unbind and close the thread-bound Hibernate Session.
                TransactionSynchronizationManager.UnbindResource(this.sessionFactory);
                this.holderActive = false;
            }
            else if (this.sessionHolder.AssignedPreviousFlushMode == true)
            {
                // In case of pre-bound Session, restore previous flush mode.
                this.sessionHolder.Session.FlushMode = (this.sessionHolder.PreviousFlushMode);
            }
        }

        /// <summary>
        /// Invoked after transaction commit/rollback.
        /// </summary>
        /// <param name="status">
        /// Status according to <see cref="Spring.Transaction.Support.TransactionSynchronizationStatus"/>
        /// </param>
        /// <remarks>
        /// Can e.g. perform resource cleanup, in this case after transaction completion.
        /// <p>
        /// Note that exceptions will get propagated to the commit or rollback
        /// caller, although they will not influence the outcome of the transaction.
        /// </p>
        /// </remarks>
        public override void AfterCompletion(TransactionSynchronizationStatus status)
        {
            if (!newSession)
            {
                ISession session = sessionHolder.Session;

                // Provide correct transaction status for releasing the Session's cache locks,
                // if possible. Else, closing will release all cache locks assuming a rollback.
                ISessionImplementor sessionImplementor = session as ISessionImplementor;
                if (sessionImplementor != null)
                {
                    sessionImplementor.AfterTransactionCompletion(status == TransactionSynchronizationStatus.Committed,
                        sessionHolder.Transaction);
                }

                if (newSession)
                {
                    SessionFactoryUtils.CloseSessionOrRegisterDeferredClose(session, sessionFactory);
                }
            }

            if (!newSession && status != TransactionSynchronizationStatus.Committed)
            {
                // Clear all pending inserts/updates/deletes in the Session.
                // Necessary for pre-bound Sessions, to avoid inconsistent state.
                sessionHolder.Session.Clear();
            }

            if (this.sessionHolder.DoesNotHoldNonDefaultSession)
            {
                sessionHolder.SynchronizedWithTransaction = false;
            }
        }
    }
}