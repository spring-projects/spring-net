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

using System.Messaging;

namespace Spring.Messaging.Core;

/// <summary>
/// Utility class to recreate message queues if they do not exist. 
/// </summary>
/// <author>Mark Pollack</author>
public class MessageQueueUtils
{
    public static void RecreateMessageQueue(string path, bool transactional)
    {
        bool defaultCacheEnabled = MessageQueue.EnableConnectionCache;
        MessageQueue.ClearConnectionCache();
        MessageQueue.EnableConnectionCache = false;
        if (MessageQueue.Exists(path))
        {
            // TODO (EE): delete/create doesn't work for some reason
            //                MessageQueue.Delete(path);
            //                queue = MessageQueue.Create(path, transactional);
            using (MessageQueue queue = new MessageQueue(path))
            {
                queue.Purge();
            }
        }
        else
        {
            /*
             * MSDN docs indicate that calls to the static .Create() method should include
             *   an explicit call to .Dispose() b/c unmanaged resources are involved
             * Here this req'ment is handled implicitly with the using() statement
             *  even though the empty using() block seems odd at first glance b/c it
             *  encloses a static method call
             */
            using (MessageQueue.Create(path, transactional))
            {
            }
        }

        MessageQueue.ClearConnectionCache();
        MessageQueue.EnableConnectionCache = defaultCacheEnabled; // set to default
    }
}