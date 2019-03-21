 

#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

using System.Collections;

namespace Spring.Messaging.Nms.Listener.Adapter
{
    /// <summary>
    /// Used in MessageListenerAdapterTests
    /// </summary>
    /// <author>Mark Pollack</author>
    public interface IMessageContentsHandler
    {
        void HandleMessage(IDictionary message);

        void HandleMessage(byte[] message);

        void HandleMessage(int message);

        void HandleMessage(object message);

        void HandleMessage(string message);
    }
}