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

namespace Spring.Remoting.Support
{
    /// <summary>
    /// Configurable implementation of the <see cref="ILifetime"/> interface.
    /// </summary>
    /// <author>Bruno Baia</author>
    public class ConfigurableLifetime : ILifetime
    {
        #region Fields

        private bool infinite = true;
        private TimeSpan initialLeaseTime = TimeSpan.Zero;
        private TimeSpan renewOnCallTime = TimeSpan.Zero;
        private TimeSpan sponsorshipTimeout = TimeSpan.Zero;

        #endregion

        #region ILifetime Members

        /// <summary>
        /// Gets or sets a value indicating whether this instance has infinite lifetime.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has infinite lifetime; otherwise, <c>false</c>.
        /// </value>
        public bool Infinite
        {
            get { return infinite; }
            set { infinite = value; }
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
        /// Gets or sets the amount of time lease
        /// should be extended for on each call to this object.
        /// </summary>
        /// <value>The amount of time lease should be
        /// extended for on each call to this object.</value>
        public TimeSpan RenewOnCallTime
        {
            get { return renewOnCallTime; }
            set { renewOnCallTime = value; }
        }

        /// <summary>
        /// Gets or sets the amount of time lease manager
        /// will for this object's sponsors to respond.
        /// </summary>
        /// <value>The amount of time lease manager will for this object's
        /// sponsors to respond.</value>
        public TimeSpan SponsorshipTimeout
        {
            get { return sponsorshipTimeout; }
            set { sponsorshipTimeout = value; }
        }

        #endregion
    }
}
