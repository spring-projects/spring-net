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

using Apache.NMS;

namespace Spring.Messaging.Nms
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks>
    ///
    /// </remarks>
    /// <author>Mark Pollack</author>
    public class StubQueue : IQueue
    {
        public static string DEFAULT_QUEUE_NAME = "banjo";


	    private string queueName = DEFAULT_QUEUE_NAME;

        public string QueueName
        {
            get { return queueName; }
        }

        public DestinationType DestinationType
        {
            get { return DestinationType.Queue; }
        }

        public bool IsTopic
        {
            get { return false; }
        }

        public bool IsQueue
        {
            get { return true; }
        }

        public bool IsTemporary
        {
            get { return false; }
        }

        public void Dispose()
        {
        }
    }
}