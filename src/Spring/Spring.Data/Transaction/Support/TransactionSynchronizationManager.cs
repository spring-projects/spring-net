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

using System.Collections;
using System.Data;

using Spring.Core;
using Spring.Threading;
using Spring.Util;

namespace Spring.Transaction.Support
{
	/// <summary>
	/// Internal class that manages resources and transaction synchronizations per thread.
	/// </summary>
	/// <remarks>
	/// Supports one resource per key without overwriting, i.e. a resource needs to
	/// be removed before a new one can be set for the same key.
	/// Supports a list of transaction synchronizations if synchronization is active.
	/// <p>
	/// Resource management code should check for thread-bound resources via GetResource().
	/// It is normally not supposed
	/// to bind resources to threads, as this is the responsiblity of transaction managers.
	/// A further option is to lazily bind on first use if transaction synchronization
	/// is active, for performing transactions that span an arbitrary number of resources.
	/// </p>
	/// <p>
	/// Transaction synchronization must be activated and deactivated by a transaction
	/// manager via
	/// <see cref="Spring.Transaction.Support.TransactionSynchronizationManager.InitSynchronization">InitSynchronization</see>
	/// and
	/// <see cref="Spring.Transaction.Support.TransactionSynchronizationManager.ClearSynchronization">ClearSynchronization</see>.
	/// This is automatically supported by
	/// <see cref="Spring.Transaction.Support.AbstractPlatformTransactionManager"/>.
	/// </p>
	/// <p>
	/// Resource management code should only register synchronizations when this
	/// manager is active, and perform resource cleanup immediately else.
	/// If transaction synchronization isn't active, there is either no current
	/// transaction, or the transaction manager doesn't support synchronizations.
	/// </p>
	/// Note that this class uses following naming convention for the
	/// named 'data slots' for storage of thread local data, 'Spring.Transaction:Name'
	/// where Name is either
	/// </remarks>
	/// <author>Juergen Hoeller</author>
    /// <author>Griffin Caprio (.NET)</author>
    /// <author>Mark Pollack (.NET)</author>
	public sealed class TransactionSynchronizationManager
	{
	    #region Logging

	    private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof (TransactionSynchronizationManager));

	    #endregion

        #region Fields
        private static readonly string syncsDataSlotName = "Spring.Transactions:syncList";

        private static readonly string resourcesDataSlotName = "Spring.Transactions:resources";

        private static readonly string currentTxReadOnlyDataSlotName = "Spring.Transactions:currentTxReadOnly";

	    private static readonly string currentTxNameDataSlotName = "Spring.Transactions:currentTxName";

        private static readonly string currentTxIsolationLevelDataSlotName = "Spring.Transactions:currentTxIsolationLevel";

        private static readonly string actualTxActiveDataSlotName = "Spring.Transactions:actualTxActive";

	    private static IComparer syncComparer = new OrderComparator();

        #endregion

        #region Management of transaction-associated resource handles
        /// <summary>
        /// Return all resources that are bound to the current thread.
        /// </summary>
        /// <remarks>Main for debugging purposes.  Resource manager should always
        /// invoke HasResource for a specific resource key that they are interested in.
        /// </remarks>
        /// <returns>IDictionary with resource keys and resource objects or empty
        /// dictionary if none is bound.</returns>
        public static IDictionary ResourceDictionary
        {
            get
            {
                IDictionary resources = LogicalThreadContext.GetData(resourcesDataSlotName) as IDictionary;
                if (resources != null)
                {
                    //TODO add readonly wrapper in Spring.Collections.
                    return resources;
                }
                else
                {
                    return new Hashtable();
                }
            }
        }

        /// <summary>
        /// Check if there is a resource for the given key bound to the current thread.
        /// </summary>
        /// <param name="key">key to check</param>
        /// <returns>if there is a value bound to the current thread</returns>
        public static bool HasResource(Object key)
        {
            AssertUtils.ArgumentNotNull(key, "Key must not be null");
            return ResourceDictionary.Contains(key);
        }

        /// <summary>
        /// Retrieve a resource for the given key that is bound to the current thread.
        /// </summary>
        /// <param name="key">key to check</param>
        /// <returns>a value bound to the current thread, or null if none.</returns>
        public static object GetResource(Object key)
        {
            AssertUtils.ArgumentNotNull(key, "Key must not be null");
            IDictionary resources = LogicalThreadContext.GetData(resourcesDataSlotName) as IDictionary;
            if (resources == null)
            {
                return null;
            }
            //Check for contains since indexer returning null behavior changes in 2.0
            if (!resources.Contains(key))
            {
                return null;
            }
            object val = resources[key];

            if (val != null && LOG.IsDebugEnabled)
            {
                LOG.Debug("Retrieved value [" + Describe(val) + "] for key [" + Describe(key) + "] bound to thread [" +
                    SystemUtils.ThreadId + "]");
            }
            return val;
        }

        /// <summary>
        /// Bind the given resource for teh given key to the current thread
        /// </summary>
        /// <param name="key">key to bind the value to</param>
        /// <param name="value">value to bind</param>
        public static void BindResource(Object key, Object value)
        {
            AssertUtils.ArgumentNotNull(key, "Key value for thread local storage of transactional resources must not be null");
            AssertUtils.ArgumentNotNull(value, "Transactional resource to bind to thread local storage must not be null" );

            IDictionary resources = LogicalThreadContext.GetData(resourcesDataSlotName) as IDictionary;
            //Set thread local resource storage if not found
            if (resources == null)
            {
                resources = new Hashtable();
                LogicalThreadContext.SetData(resourcesDataSlotName, resources);
            }
            if (resources.Contains(key))
            {
                throw new InvalidOperationException("Already value [" + resources[key] + "] for key [" + key +
                        "] bound to thread [" + SystemUtils.ThreadId + "]");
            }
            resources.Add(key, value);
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug("Bound value [" + Describe(value) + "] for key [" + Describe(key) + "] to thread [" +
                    SystemUtils.ThreadId + "]");
            }
        }


        /// <summary>
        /// Unbind a resource for the given key from the current thread
        /// </summary>
        /// <param name="key">key to check</param>
        /// <returns>the previously bound value</returns>
        /// <exception cref="InvalidOperationException">if there is no value bound to the thread</exception>
        public static object UnbindResource(Object key)
        {
            AssertUtils.ArgumentNotNull(key, "Key must not be null");

            IDictionary resources = LogicalThreadContext.GetData(resourcesDataSlotName) as IDictionary;
            if (resources == null || !resources.Contains(key))
            {
                throw new InvalidOperationException("No value for key [" + key +  "] bound to thread [" +
                    SystemUtils.ThreadId + "]");
            }
            Object val = resources[key];
            resources.Remove(key);
            if (resources.Count == 0)
            {
                LogicalThreadContext.FreeNamedDataSlot(resourcesDataSlotName);
            }
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug("Removed value [" + Describe(val) + "] for key [" + Describe(key) + "] from thread [" +
                    SystemUtils.ThreadId + "]");
            }
            return val;
        }

        #endregion

	    /// <summary>
		/// Activate transaction synchronization for the current thread.
		/// </summary>
		/// <remarks>
		/// Called by transaction manager at the beginning of a transaction.
		/// </remarks>
		/// <exception cref="System.InvalidOperationException">
		/// If synchronization is already active.
		/// </exception>
		public static void InitSynchronization()
		{
			if ( SynchronizationActive )
			{
				throw new InvalidOperationException( "Cannot activate transaction synchronization - already active" );
			}
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug("Initializing transaction synchronization");
            }
            ArrayList syncs = new ArrayList();
            LogicalThreadContext.SetData(syncsDataSlotName, syncs);
		}

		/// <summary>
		/// Deactivate transaction synchronization for the current thread.
		/// </summary>
		/// <remarks>
		/// Called by transaction manager on transaction cleanup.
		/// </remarks>
		/// <exception cref="System.InvalidOperationException">
		/// If synchronization is not active.
		/// </exception>
		public static void ClearSynchronization()
		{
			if ( !SynchronizationActive )
			{
				throw new InvalidOperationException( "Cannot deactivate transaction synchronization - not active" );
			}
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug("Clearing transaction synchronization");
            }
            LogicalThreadContext.FreeNamedDataSlot(syncsDataSlotName);
		}

        /// <summary>
        /// Clears the entire transaction synchronization state for the current thread, registered
        /// synchronizations as well as the various transaction characteristics.
        /// </summary>
        public static void Clear()
        {
            ClearSynchronization();
            CurrentTransactionName = null;
            CurrentTransactionReadOnly = false;
            CurrentTransactionIsolationLevel = IsolationLevel.Unspecified;
            ActualTransactionActive = false;
        }

		/// <summary>
		/// Register a new transaction synchronization for the current thread.
		/// </summary>
		/// <remarks>
		/// Typically called by resource management code.
		/// </remarks>
		/// <exception cref="System.InvalidOperationException">
		/// If synchronization is not active.
		/// </exception>
		public static void RegisterSynchronization( ITransactionSynchronization synchronization )
		{
            AssertUtils.ArgumentNotNull(synchronization, "TransactionSynchronization must not be null");
			if ( !SynchronizationActive )
			{
				throw new InvalidOperationException( "Transaction synchronization is not active" );
			}
            ArrayList syncs = LogicalThreadContext.GetData(syncsDataSlotName) as ArrayList;
            if (syncs != null)
            {
                object root = syncs.SyncRoot;
                lock (root)
                {
                    syncs.Add(synchronization);
                }
            }
        }

        private static string Describe(object obj)
        {
            return obj == null ? "" : obj + "@" + obj.GetHashCode().ToString("X");
        }

	    #region Properties

	    /// <summary>
	    /// Return an unmodifiable list of all registered synchronizations
	    /// for the current thread.
	    /// </summary>
	    /// <returns>
	    /// A list of <see cref="Spring.Transaction.Support.ITransactionSynchronization"/>
	    /// instances.
	    /// </returns>
	    /// <exception cref="System.InvalidOperationException">
	    /// If synchronization is not active.
	    /// </exception>
	    public static IList Synchronizations
	    {
	        get
	        {
	            if (!SynchronizationActive)
	            {
	                throw new InvalidOperationException("Transaction synchronization is not active");
	            }
	            ArrayList syncs = LogicalThreadContext.GetData(syncsDataSlotName) as ArrayList;
	            if (syncs != null)
	            {
	                // Sort lazily here, not in registerSynchronization.
	                object root = syncs.SyncRoot;
	                lock (root)
	                {
	                    // #SPRNET-1160, tx Ben Rowlands
	                    CollectionUtils.StableSortInPlace(syncs, syncComparer);
	                }

	                // Return unmodifiable snapshot, to avoid exceptions
	                // while iterating and invoking synchronization callbacks that in turn
	                // might register further synchronizations.
	                return ArrayList.ReadOnly(syncs);
	            }
	            else
	            {
	                return ArrayList.ReadOnly(new ArrayList());
	            }
	        }
	    }

	    /// <summary>
		/// Return if transaction synchronization is active for the current thread.
		/// </summary>
		/// <remarks>
		/// Can be called before
		/// <see cref="Spring.Transaction.Support.TransactionSynchronizationManager.RegisterSynchronization">InitSynchronization</see>
		/// to avoid unnecessary instance creation.
		/// </remarks>
		public static bool SynchronizationActive
		{
			get
			{
                IList syncs = LogicalThreadContext.GetData(syncsDataSlotName) as IList;
                return syncs != null;
			}
		}

        /// <summary>
        /// Gets or sets a value indicating whether the
        /// current transaction is read only.
        /// </summary>
        /// <remarks>
        /// Called by transaction manager on transaction begin and on cleanup.
        /// Return whether the current transaction is marked as read-only.
        /// To be called by resource management code when preparing a newly
        /// created resource (for example, a Hibernate Session).
        /// <p>Note that transaction synchronizations receive the read-only flag
        /// as argument for the <code>beforeCommit</code> callback, to be able
        /// to suppress change detection on commit. The present method is meant
        /// to be used for earlier read-only checks, for example to set the
        /// flush mode of a Hibernate Session to FlushMode.Never upfront.
        /// </p>
        /// </remarks>
        /// <value>
        /// 	<c>true</c> if current transaction read only; otherwise, <c>false</c>.
        /// </value>
        public static bool CurrentTransactionReadOnly
        {
            get
            {
                return LogicalThreadContext.GetData(currentTxReadOnlyDataSlotName) != null;
            }
            set
            {
                if (value)
                {
                    LogicalThreadContext.SetData(currentTxReadOnlyDataSlotName, true);
                }
                else
                {
                    LogicalThreadContext.FreeNamedDataSlot(currentTxReadOnlyDataSlotName);
                }

            }
        }

        /// <summary>
        /// Gets or sets the name of the current transaction, if any.
        /// </summary>
        /// <remarks>Called by the transaction manager on transaction begin and on cleanup.
        /// To be called by resource management code for optimizations per use case, for
        /// example to optimize fetch strategies for specific named transactions.</remarks>
        /// <value>The name of the current transactio or null if none set.</value>
        public static string CurrentTransactionName
        {
            get
            {
                return LogicalThreadContext.GetData(currentTxNameDataSlotName) as string;
            }
            set
            {
                LogicalThreadContext.SetData(currentTxNameDataSlotName, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether there currently is an actual transaction
        /// active.
        /// </summary>
        /// <remarks>This indicates wheter the current thread is associated with an actual
        /// transaction rather than just with active transaction synchronization.
        /// <para>Called by the transaction manager on transaction begin and on cleanup.</para>
        /// <para>To be called by resource management code that wants to discriminate between
        /// active transaction synchronization (with or without backing resource transaction;
        /// also on PROPAGATION_SUPPORTS) and an actual transaction being active; on
        /// PROPAGATION_REQUIRES, PROPAGATION_REQUIRES_NEW, etC)</para></remarks>
        /// <value>
        /// 	<c>true</c> if [actual transaction active]; otherwise, <c>false</c>.
        /// </value>
	    public static bool ActualTransactionActive
	    {
	        get
	        {
	            return LogicalThreadContext.GetData(actualTxActiveDataSlotName) != null;
	        }
            set
            {
                if (value)
                {
                    LogicalThreadContext.SetData(actualTxActiveDataSlotName, value);
                }
                else
                {
                    LogicalThreadContext.FreeNamedDataSlot(actualTxActiveDataSlotName);
                }
            }
	    }


        /// <summary>
        /// Gets or sets the current transaction isolation level, if any.
        /// </summary>
        /// <remarks>Called by the transaction manager on transaction begin and on cleanup.</remarks>
        /// <value>The current transaction isolation level.  If no current transaction is
        /// active, retrun IsolationLevel.Unspecified</value>
        public static IsolationLevel CurrentTransactionIsolationLevel
        {
            get
            {
                object data =
                    LogicalThreadContext.GetData(currentTxIsolationLevelDataSlotName);
                if (data != null)
                {
                    return (IsolationLevel) data;
                }
                else
                {
                    return IsolationLevel.Unspecified;
                }
            }
            set
            {
                LogicalThreadContext.SetData(currentTxIsolationLevelDataSlotName, value);
            }
        }
        #endregion
    }
}
