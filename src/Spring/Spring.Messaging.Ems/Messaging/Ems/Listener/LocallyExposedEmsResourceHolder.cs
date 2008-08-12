#region License

/*
 * Copyright 2002-2008 the original author or authors.
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

using Spring.Messaging.Ems.Connections;
using TIBCO.EMS;

namespace Spring.Messaging.Ems.Listener
{
    /// <summary>
    /// EmsResourceHolder marker subclass that indicates local exposure,
    /// i.e. that does not indicate an externally managed transaction.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public class LocallyExposedEmsResourceHolder : EmsResourceHolder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocallyExposedEmsResourceHolder"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        public LocallyExposedEmsResourceHolder(Session session) : base(session)
        {
            
        }
    }
}