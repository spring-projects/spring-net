#region License

/*
 * Copyright © 2002-2008 the original author or authors.
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

using System.Messaging;
using Spring.Messaging.Support.Converters;

namespace Spring.Messaging.Core
{
    /// <summary>
    /// An interface for creating MessageQueue and IMessageConverter objects. 
    /// </summary>
    /// <remarks>
    /// These objects have methods that are generally not thread safe, (IMessageConverter classes
    /// rely on IMessageFormatter objects that are not thread safe).  A major reason to
    /// for this interface is to provide thread-local instances such that appliation code need
    /// not be concerned with these resource management issues.
    /// </remarks>
    public interface IMessageQueueFactory
    {
        MessageQueue CreateMessageQueue(string messageQueueObjectName);

        IMessageConverter CreateMessageConverter(string messageConverterObjectName);
    }
}