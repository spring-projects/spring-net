#region License

/*
 * Copyright � 2002-2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using Apache.NMS;

namespace Spring.Messaging.Nms.Core
{
    /// <summary> Callback for executing any number of operations on a provided
    /// Session
    /// </summary>
    /// <remarks>
    /// <para>To be used with the NmsTemplate.Execute(ISessionCallback)}
    /// method. See <see cref="SessionDelegate"/> for the equivalent callback
    /// that can be used as a (anonymous) delegate.</para>
    /// </remarks>
    /// <author>Mark Pollack</author>
    /// <seealso cref="NmsTemplate.Execute(ISessionCallback,bool)">
    /// </seealso>
    public interface ISessionCallback
    {
        /// <summary> Execute any number of operations against the supplied NMS
        /// Session, possibly returning a result.
        /// </summary>
        /// <param name="session">the NMS <code>Session</code>
        /// </param>
        /// <returns> a result object from working with the <code>Session</code>, if any (so can be <code>null</code>) 
        /// </returns>
        /// <throws>NMSException if there is any problem </throws>
        object DoInNms(ISession session);
    }
}
