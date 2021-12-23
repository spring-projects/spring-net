#region License

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

#endregion

using Apache.NMS;

namespace Spring.Messaging.Nms.Core
{
    /// <summary>
    /// Callback delegate for code that operates on a Session. 
    /// </summary>
    /// <param name="session">The NMS ISession object.</param>
    /// <remarks>
    /// <para>Allows you to execute any number of operations
    /// on a single ISession, possibly returning a result a result.
    /// </para>
    /// </remarks>
    /// <returns>A result object from working with the <code>Session</code>, if any (so can be <code>null</code>) 
    /// </returns>
    /// <throws>NMSException if there is any problem </throws>
    /// <author>Mark Pollack</author>
    public delegate object SessionDelegate(ISession session);
    
    // async version
    public delegate Task<object> SessionDelegateAsync(ISession session);
}
