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

using System.Runtime.Remoting.Lifetime;

namespace Spring.Remoting.Support
{
    /// <summary>
    /// This class extends <see cref="MarshalByRefObject"/> to allow users
    /// to define object lifecycle details by simply setting its properties.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Remoting exporters uses this class as a base proxy class
    /// in order to support lifecycle configuration when exporting
    /// a remote object.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public abstract class BaseRemoteObject : MarshalByRefObject
    {
		#region Fields

        private bool isInfinite = false;
        private TimeSpan initialLeaseTime = TimeSpan.Zero;
        private TimeSpan renewOnCallTime = TimeSpan.Zero;
        private TimeSpan sponsorshipTimeout = TimeSpan.Zero;

		#endregion

		#region Constructor(s) / Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseRemoteObject"/> class.
        /// </summary>
        public BaseRemoteObject()
        {}

		#endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance has infinite lifetime.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance has infinite lifetime;
        /// otherwise, <see langword="false" /> .
        /// </value>
        public bool IsInfinite
        {
            get { return isInfinite; }
            set { isInfinite = value; }
        }

        /// <summary>
        /// Gets or sets the initial lease time.
        /// </summary>
        /// <value>The initial lease time.</value>
        public TimeSpan InitialLeaseTime
        {
            get { return initialLeaseTime; }
            set { initialLeaseTime = value; }
        }

        /// <summary>
        /// Gets or sets the amount of time lease should be
        /// extended for on each call to this object.
        /// </summary>
        /// <value>The amount of time lease should be
        /// extended for on each call to this object.</value>
        public TimeSpan RenewOnCallTime
        {
            get { return renewOnCallTime; }
            set { renewOnCallTime = value; }
        }

        /// <summary>
        /// Gets or sets the amount of time lease manager will for this object's
        /// sponsors to respond.
        /// </summary>
        /// <value>The amount of time lease manager will for this object's
        /// sponsors to respond.</value>
        public TimeSpan SponsorshipTimeout
        {
            get { return sponsorshipTimeout; }
            set { sponsorshipTimeout = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Obtains a lifetime service object to control the lifetime policy for this instance.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method uses property values to configure <see cref="ILease"/> for this object.
        /// </p>
        /// <p>
        /// It is very much inspired by Ingo Rammer's example in Chapter 6 of "Advanced .NET Remoting",
        /// but is modified slightly to make it more "Spring-friendly". Basically, the main difference is that
        /// instead of pulling lease configuration from the .NET config file, this implementation relies
        /// on Spring DI to get appropriate values injected, which makes it much more flexible.
        /// </p>
        /// </remarks>
        /// <returns>
        /// An object of type <see cref="T:System.Runtime.Remoting.Lifetime.ILease"/> used to control the
        /// lifetime policy for this instance. This is the current lifetime service object for
        /// this instance if one exists; otherwise, a new lifetime service object initialized to the value
        /// of the <see cref="P:System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime" qualify="true"/> property.
        /// </returns>
        /// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
        public override object InitializeLifetimeService()
        {
            if (this.isInfinite)
            {
                return null;
            }

            ILease lease = (ILease) base.InitializeLifetimeService();
            if (this.initialLeaseTime != TimeSpan.Zero)
            {
                lease.InitialLeaseTime = this.initialLeaseTime;
            }
            if (this.renewOnCallTime != TimeSpan.Zero)
            {
                lease.RenewOnCallTime = this.renewOnCallTime;
            }
            if (this.sponsorshipTimeout != TimeSpan.Zero)
            {
                lease.SponsorshipTimeout = this.sponsorshipTimeout;
            }

            return lease;
        }

        #endregion
    }
}
