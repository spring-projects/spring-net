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

namespace Spring.Messaging.Core
{
    /// <summary>
    /// Encapsulates additional metadata information about the MessageQueue that can not be easily obtained
    /// from the MessageQueue itself.
    /// </summary>
    public class MessageQueueMetadata
    {
        private bool remoteQueue;

        private bool remoteQueueIsTransactional;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageQueueMetadata"/> class.
        /// </summary>
        /// <param name="remoteQueue">if set to <c>true</c> [remote queue].</param>
        /// <param name="remoteQueueIsTransactional">if set to <c>true</c> [remote queue is transactional].</param>
        public MessageQueueMetadata(bool remoteQueue, bool remoteQueueIsTransactional)
        {
            this.remoteQueue = remoteQueue;
            this.remoteQueueIsTransactional = remoteQueueIsTransactional;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the queue is a remote queue. 
        /// </summary>
        /// <remarks>
        /// The operations that one can perform on the MessageQueue depend on if it is local or remote, for 
        /// example checking if it is transactional.  This is very difficult to determine programmatically.
        /// The property was made virtual so it can be overridden to take into account custom heuristics you
        /// may want to use to determine this programmatically.
        /// </remarks>
        /// <value><c>true</c> if remote queue; otherwise, <c>false</c>.</value>
        public virtual bool RemoteQueue
        {
            get { return remoteQueue; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the remote queue is transactional.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the remote queue is transactional; otherwise, <c>false</c>.
        /// </value>
        public virtual bool RemoteQueueIsTransactional
        {
            get { return remoteQueueIsTransactional; }
        }
    }
}