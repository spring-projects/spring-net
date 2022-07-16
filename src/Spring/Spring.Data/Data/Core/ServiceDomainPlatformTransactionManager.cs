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

using System.EnterpriseServices;
using Spring.Data.Support;
using Spring.Objects.Factory;
using Spring.Transaction;
using Spring.Transaction.Support;

namespace Spring.Data.Core
{
    /// <summary>
    /// Transaction Manager that uses EnterpriseServices to access the
    /// MS-DTC.  It requires the support of 'Services without Components'
    /// functionality which is available on Win 2003 and Win XP SP2.
    /// </summary>
    /// <author>Mark Pollack (.NET)</author>
    public class ServiceDomainPlatformTransactionManager : AbstractPlatformTransactionManager, IInitializingObject
    {
        private IServiceDomainAdapter txAdapter;

        private bool trackingEnabled = true;
        private string trackingAppName = "Spring.NET";
        private string trackingComponentName = "ServiceDomainPlatformTransactionManager";

        #region Constructor (s)
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceDomainPlatformTransactionManager"/> class.
        /// </summary>
        public 	ServiceDomainPlatformTransactionManager()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceDomainPlatformTransactionManager"/> class.
        /// </summary>
        /// <remarks>This is indented only for unit testing purposes and should not be
        /// called by production application code.</remarks>
        /// <param name="txAdapter">The tx adapter.</param>
        public ServiceDomainPlatformTransactionManager(IServiceDomainAdapter txAdapter)
        {
            this.txAdapter = txAdapter;
        }

        #endregion


        /// <summary>
        /// Gets or sets a value indicating whether tracking is enabled.
        /// </summary>
        /// <value><c>true</c> if tracking is enabled; otherwise, <c>false</c>.</value>
        public bool TrackingEnabled
        {
            get { return trackingEnabled; }
            set { trackingEnabled = value; }
        }

        /// <summary>
        /// Gets or sets a text string that corresponds to the application ID under which tracker information is reported.
        /// The default value is 'Spring.NET'
        /// </summary>
        /// <value>The name of the tracking app.</value>
        public string TrackingAppName
        {
            get { return trackingAppName; }
            set { trackingAppName = value; }
        }

        /// <summary>
        /// Gets or sets a text string that corresponds to the context name under which tracker information is reported.
        /// </summary>
        /// <value>The name of the tracking component.</value>
        public string TrackingComponentName
        {
            get { return trackingComponentName; }
            set { trackingComponentName = value; }
        }

        public void AfterPropertiesSet()
        {
            //TODO decide what should be validated for advanced configurations.
        }

        protected override object DoGetTransaction()
        {
            ServiceDomainTransactionObject txObject = new ServiceDomainTransactionObject();
            if (txAdapter != null)
            {
                txObject.ServiceDomainAdapter = txAdapter;
            }
            return txObject;
        }



        protected override bool IsExistingTransaction(object transaction)
        {
            ServiceDomainTransactionObject txObject =  (ServiceDomainTransactionObject)transaction;
            if (txObject.ServiceDomainAdapter.IsInTransaction)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override void DoBegin(object transaction, ITransactionDefinition definition)
        {
            try
            {
                ServiceDomainTransactionObject txObject = (ServiceDomainTransactionObject)transaction;

                DoServiceDomainBegin(txObject, definition);
            }
            catch (PlatformNotSupportedException ex)
            {
                throw new TransactionSystemException("ServiceDomain failure on begin of transaction.  Platform does not support EnterpriseServices 'Services without Components'", ex);
            }
            catch (Exception e)
            {
                throw new CannotCreateTransactionException("ServiceDomain failure on begin of transaction", e);
            }
        }

        private void DoServiceDomainBegin(ServiceDomainTransactionObject serviceDomainTxObject, ITransactionDefinition definition)
        {
            SimpleServiceConfig serviceConfig = CreateServiceConfig(definition);
            //The context is created when we call Enter.
            serviceDomainTxObject.ServiceDomainAdapter.Enter(serviceConfig);
            if (log.IsDebugEnabled)
            {
                log.Debug("Context created. TransactionId = " + ContextUtil.TransactionId
                          + ", ActivityId = " + ContextUtil.ActivityId);
            }
        }

        protected override object DoSuspend(object transaction)
        {
            // Passing the current transaction object, literally an 'object' as the 'suspended resource',
            // even though it is not used just to avoid passing null
            // ServiceDomainPlatformTransactionManager is not binding any resources to the local thread, instead delegating to
            // System.EnterpriseServices to handle thread local resources.
            return transaction;
        }

        protected override void DoResume(object transaction, object suspendedResources)
        {
        }

        private SimpleServiceConfig CreateServiceConfig(ITransactionDefinition definition)
        {
            SimpleServiceConfig serviceConfig = new SimpleServiceConfig();

            //TODO investigate BringYourOwnTransaction and TipUrl properties of ServiceConfig
            serviceConfig.TransactionDescription = definition.Name;

            serviceConfig.TrackingEnabled = TrackingEnabled;
            serviceConfig.TrackingAppName = TrackingAppName;
            serviceConfig.TrackingComponentName = TrackingComponentName;

            ApplyPropagationBehavior(serviceConfig, definition);

            ApplyIsolationLevel(serviceConfig, definition);

            // infinite==-1 would cause transactions to be aborted immediately!
            if(definition.TransactionTimeout != Timeout.Infinite)
            {
                serviceConfig.TransactionTimeout = definition.TransactionTimeout;
            }
            return serviceConfig;
        }

        protected void ApplyIsolationLevel(SimpleServiceConfig serviceConfig, ITransactionDefinition definition)
        {
            switch (definition.TransactionIsolationLevel)
            {

                case System.Data.IsolationLevel.Chaos:
                    if (log.IsInfoEnabled)
                    {
                        log.Info("IsolationLevel Chaos does not have a direct counterpart in EnterpriseServices, using Any");
                    }
                    serviceConfig.IsolationLevel = TransactionIsolationLevel.Any;
                    break;
                case System.Data.IsolationLevel.ReadCommitted:
                    serviceConfig.IsolationLevel = TransactionIsolationLevel.ReadCommitted;
                    break;
                case System.Data.IsolationLevel.ReadUncommitted:
                    serviceConfig.IsolationLevel = TransactionIsolationLevel.ReadUncommitted;
                    break;
                case System.Data.IsolationLevel.RepeatableRead:
                    serviceConfig.IsolationLevel = TransactionIsolationLevel.RepeatableRead;
                    break;
                case System.Data.IsolationLevel.Serializable:
                    serviceConfig.IsolationLevel = TransactionIsolationLevel.Serializable;
                    break;
                case System.Data.IsolationLevel.Snapshot:
                    if (log.IsInfoEnabled)
                    {
                        log.Info("IsolationLevel Snapshot does not have a direct counterpart in EnterpriseServices, using ReadCommitted.  Introduced in SqlServer 2005.  Consider using System.Transactions for transaction management instead.");
                    }
                    serviceConfig.IsolationLevel = TransactionIsolationLevel.ReadCommitted;  //err on the side of consistency
                    break;
                case System.Data.IsolationLevel.Unspecified:
                    serviceConfig.IsolationLevel = TransactionIsolationLevel.Any;
                    break;
            }

        }

        protected void ApplyPropagationBehavior(SimpleServiceConfig serviceConfig, ITransactionDefinition definition)
        {
            if (definition.PropagationBehavior == TransactionPropagation.Required)
            {
                serviceConfig.TransactionOption = TransactionOption.Required;
            }
            else if (definition.PropagationBehavior == TransactionPropagation.RequiresNew)
            {
                serviceConfig.TransactionOption = TransactionOption.RequiresNew;
            }
            else if (definition.PropagationBehavior == TransactionPropagation.Supports)
            {
                serviceConfig.TransactionOption = TransactionOption.Supported;
            }
            else if (definition.PropagationBehavior == TransactionPropagation.NotSupported)
            {
                serviceConfig.TransactionOption = TransactionOption.NotSupported;
            }
            else if (definition.PropagationBehavior == TransactionPropagation.Never)
            {
                //TODO check the validity of this mapping
                serviceConfig.TransactionOption = TransactionOption.Disabled;
            } else
            {
                //TODO Should we throw an exception instead?
                log.Warn("The requested transaction propagation option " +
                         definition.PropagationBehavior + " is not supported.  " +
                         "Defaulting to Never(Disabled) ");
            }
        }




        protected override void DoCommit(DefaultTransactionStatus status)
        {
            ServiceDomainTransactionObject txObject = (ServiceDomainTransactionObject) status.Transaction;
            bool globalRollbackOnly = status.GlobalRollbackOnly;
            try
            {
                if (txObject.ServiceDomainAdapter.IsInTransaction)
                {

                    if (txObject.ServiceDomainAdapter.MyTransactionVote == TransactionVote.Commit)
                    {
                        txObject.ServiceDomainAdapter.SetComplete();
                    }
                    else
                    {
                        txObject.ServiceDomainAdapter.SetAbort();
                    }
                }
                TransactionStatus serviceDomainTxstatus = txObject.ServiceDomainAdapter.Leave();
                if (log.IsDebugEnabled)
                {
                    log.Debug("ServiceDomain Transaction Status upon leaving ServiceDomain = " + serviceDomainTxstatus);
                }
                txObject.TransactionStatus = serviceDomainTxstatus;
                if (!globalRollbackOnly && serviceDomainTxstatus == TransactionStatus.Aborted)
                {
                    throw new UnexpectedRollbackException("Transaction unexpectedly rolled-back (maybe due to a timeout)");
                }
            }
            catch (PlatformNotSupportedException ex)
            {
                throw new TransactionSystemException("Failure on Commit.  Platform does not support EnterpriseServices 'Services without Components'", ex);
            }
            catch (Exception e)
            {
                throw new TransactionSystemException("Failure upon Leaving ServiceDomain (for Commit)", e);
            }

        }

        protected override void DoRollback(DefaultTransactionStatus status)
        {
            ServiceDomainTransactionObject txObject = (ServiceDomainTransactionObject)status.Transaction;
            if (txObject.ServiceDomainAdapter.IsInTransaction)
            {
                try
                {
                    txObject.ServiceDomainAdapter.SetAbort();
                    txObject.ServiceDomainAdapter.Leave();
                }
                catch (PlatformNotSupportedException ex)
                {
                    throw new TransactionSystemException("Failure on Rollback.  Platform does not support EnterpriseServices 'Services without Components'", ex);
                }
                catch (Exception e)
                {
                    throw new Spring.Transaction.TransactionSystemException("Failure upon Leaving ServiceDomain (for Rollback)", e);
                }
            }
        }

        protected override void DoSetRollbackOnly(DefaultTransactionStatus status)
        {
            ServiceDomainTransactionObject txObject = (ServiceDomainTransactionObject)status.Transaction;
            if (status.Debug)
            {
                log.Debug("Setting transaction rollback-only");
            }
            try
            {
                txObject.ServiceDomainAdapter.MyTransactionVote = TransactionVote.Abort;
            }
            catch (Exception ex)
            {
                throw new TransactionSystemException("Failure on System.Transactions.Transaction.Current.Rollback", ex);
            }

        }

        protected override bool ShouldCommitOnGlobalRollbackOnly
        {
            get { return true; }
        }


        public class ServiceDomainTransactionObject : ISmartTransactionObject
        {
            private TransactionStatus transactionStatus;
            private IServiceDomainAdapter serviceDomainAdapter;


            public ServiceDomainTransactionObject()
            {
                serviceDomainAdapter = new DefaultServiceDomainAdapter();
            }

            public IServiceDomainAdapter ServiceDomainAdapter
            {
                get { return serviceDomainAdapter; }
                set { serviceDomainAdapter = value; }
            }

            public TransactionStatus TransactionStatus
            {
                get { return transactionStatus; }
                set { transactionStatus = value; }
            }

            public bool RollbackOnly
            {
                get
                {
                    if ( serviceDomainAdapter.MyTransactionVote == TransactionVote.Abort)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
    }
}
