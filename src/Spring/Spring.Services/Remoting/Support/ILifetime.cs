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
    /// Defines lifetime's properties of remote objects that is used by Spring.
    /// </summary>
    /// <author>Bruno Baia</author>
    public interface ILifetime
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance has infinite lifetime.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has infinite lifetime; otherwise, <c>false</c>.
        /// </value>
        bool Infinite { get; }

        /// <summary>
        /// Gets the initial lease time.
        /// </summary>
        /// <value>The initial lease time.</value>
        TimeSpan InitialLeaseTime { get; }

        /// <summary>
        /// Gets the amount of time lease
        /// should be extended for on each call to this object.
        /// </summary>
        /// <value>The amount of time lease should be
        /// extended for on each call to this object.</value>
        TimeSpan RenewOnCallTime { get; }

        /// <summary>
        /// Gets the amount of time lease manager
        /// will for this object's sponsors to respond.
        /// </summary>
        /// <value>The amount of time lease manager will for this object's
        /// sponsors to respond.</value>
        TimeSpan SponsorshipTimeout { get; }
    }
}
