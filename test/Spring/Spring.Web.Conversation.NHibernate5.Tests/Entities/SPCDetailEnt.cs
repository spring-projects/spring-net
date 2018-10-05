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

using System;

using Spring.Web.Conversation;

namespace Spring.Entities
{
    /// <summary>
    /// Detail Entity for 'session-per-conversation' tests: 
    /// <see cref="WebConversationStateTest.SPCLazyLoadTest()"/>, 
    /// <see cref="WebConversationStateTest.SPCSwitchConversationSameRequestTest()"/>
    /// </summary>
    /// <author>Hailton de Castro</author>
    [Serializable]
    public class SPCDetailEnt
    {
        private Int32? id;
        /// <summary>
        /// Entity key
        /// </summary>
        public virtual Int32? Id
        {
            get { return id; }
            set { id = value; }
        }

        private String description;
        /// <summary>
        /// Description
        /// </summary>
        public virtual String Description
        {
            get { return description; }
            set { description = value; }
        }
    }
}
