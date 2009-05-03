#region License

/*
 * Copyright © 2002-2006 the original author or authors.
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

using Spring.Messaging.Ems.Common;

namespace Spring.Messaging.Ems.Core
{
    /// <summary> Callback for executing any number of operations on a provided
    /// Session
    /// </summary>
    /// <remarks>
    /// <p>To be used with the EmsTemplate.Execute(ISessionCallback)}
    /// method, often implemented as an anonymous inner class.</p>
    /// </remarks>
    /// <author>Mark Pollack</author>
    /// <seealso cref="EmsTemplate.Execute(ISessionCallback,bool)">
    /// </seealso>
    public interface ISessionCallback
    {
        /// <summary> Execute any number of operations against the supplied EMS
        /// Session, possibly returning a result.
        /// </summary>
        /// <param name="session">the EMS <code>Session</code>
        /// </param>
        /// <returns> a result object from working with the <code>Session</code>, if any (so can be <code>null</code>) 
        /// </returns>
        /// <throws>EMSException if there is any problem </throws>
        object DoInEms(ISession session);
    }
}
