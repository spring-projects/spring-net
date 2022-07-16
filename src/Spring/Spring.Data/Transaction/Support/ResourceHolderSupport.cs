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

namespace Spring.Transaction.Support
{
	/// <summary>
	/// Convenient base class for resource holders.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Features rollback-only support transactions. Can expire after a certain number of
	/// seconds or milliseconds, to determine transactional timeouts.
	/// </p>
	/// </remarks>
	/// <author>Juergen Hoeller</author>
	/// <author>Griffin Caprio (.NET)</author>
	public abstract class ResourceHolderSupport
	{
		private bool synchronizedWithTransaction = false;
		private bool rollbackOnly = false;
		private DateTime deadline;
        private int referenceCount = 0;

		/// <summary>
		/// Mark the resource as synchronized with a transaction.
		/// </summary>
		public bool SynchronizedWithTransaction
		{
			get { return synchronizedWithTransaction; }
			set { synchronizedWithTransaction = value; }
		}

        /// <summary>
        /// Get or set whether the resource is synchronized with a transaction.
        /// </summary>
        /// <value><c>true</c> if synchronized; otherwise, <c>false</c>.</value>
		public bool RollbackOnly
		{
			get { return rollbackOnly; }
            set { rollbackOnly = value;}
		}

		/// <summary>
		/// Return the expiration deadline of this object.
		/// </summary>
		public DateTime Deadline
		{
			get { return deadline; }
		}

		/// <summary>
		/// Return whether this object has an associated timeout.
		/// </summary>
		public bool HasTimeout
		{
			get { return ( deadline != DateTime.MinValue ); }
		}

		/// <summary>
		/// Return the time to live for this object in seconds.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Rounds up eagerly, e.g. '9.00001' to '10'.
		/// </p>
		/// </remarks>
		/// <exception cref="System.ArgumentException">
		/// If no deadline has been set.
		/// </exception>
		public int TimeToLiveInSeconds
		{
			get
			{
			    int secs = (int)Math.Ceiling( TimeToLiveInMilliseconds / 1000 );
                checkTransactionTimeout(secs <= 0);
                return secs;
			}
		}

		/// <summary>
		/// Return the time to live for this object in milliseconds.
		/// </summary>
		/// <exception cref="System.ArgumentException">
		/// If no deadline has been set.
		/// </exception>
		public double TimeToLiveInMilliseconds
		{
			get
			{
				if ( deadline == DateTime.MinValue )
				{
					throw new ArgumentException( "No deadline specified for this resource holder.");
				}
                TimeSpan duration = deadline - DateTime.Now;
                checkTransactionTimeout(duration.TotalMilliseconds <= 0);
			    if (duration.TotalMilliseconds > 0)
			    {
                    return duration.TotalMilliseconds;
			    }
			    else
			    {
                    return 0;
			    }
			}
		}

        /// <summary>
        /// Sets the timeout for this object in milliseconds.
        /// </summary>
        /// <value>Number of milliseconds until expiration.</value>
        public long TimeoutInMillis
        {
            set
            {
                deadline = DateTime.Now.AddMilliseconds(value);
            }
        }

        /// <summary>
        /// Sets the timeout for this object in seconds.
        /// </summary>
        /// <value>Number of seconds until expiration.</value>
        public int TimeoutInSeconds
        {
            set
            {
                TimeoutInMillis = value * 1000;
            }
        }

        private void checkTransactionTimeout(bool deadlineReached)
        {
            if (deadlineReached)
            {
                RollbackOnly = true;
                throw new TransactionTimedOutException("Transaction timed out: deadline was " + Deadline);
            }
        }


		/// <summary>
		/// Clear the transaction state of this resource holder.
		/// </summary>
		public virtual void Clear()
		{
		    synchronizedWithTransaction = false;
		    rollbackOnly = false;
		    deadline = DateTime.MinValue;
		}

        /// <summary>
        /// Increase the reference count by one because the holder has been requested.
        /// </summary>
        public void Requested()
        {
            referenceCount++;
        }

        /// <summary>
        /// Decrease the reference count by one because the holder has been released.
        /// </summary>
        public void Released()
        {
            referenceCount--;
        }

        /// <summary>
        /// Return wheterh there are still open references to this holder
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return (referenceCount > 0);
            }
        }

	}
}
