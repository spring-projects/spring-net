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
using Spring.Messaging.Support;
using Spring.Messaging.Support.Converters;

namespace Spring.Messaging.Core
{
    /// <summary>
    /// An interface for creating MessageQueue and IMessageConverter objects from object definitions 
    /// defined in the application context.
    /// </summary>
    /// <remarks>
    /// MessageQueue and IMessageConverter objects have methods that are generally not thread safe, 
    /// (IMessageConverter classes rely on IMessageFormatter objects that are not thread safe). 
    /// As such, a major reason to for this interface is to provide thread-local instances such that 
    /// appliation code need not be concerned with these resource management issues.
    /// </remarks>
    /// <author>Mark Pollack</author>
    public interface IMessageQueueFactory
    {
        void RegisterMessageQueue(string messageQueueObjectName,
                                  MessageQueueCreatorDelegate messageQueueCreatorDelegate);

        MessageQueue CreateMessageQueue(string messageQueueObjectName);

        bool ContainsMessageQueue(string messageQueueObjectName);
        
        
        
        void RegisterMessageConverter(string messageConverterName,
                                      MessageConverterCreatorDelegate MessageConverterCreatorDelegate);


        IMessageConverter CreateMessageConverter(string messageConverterObjectName);

        bool ContainsMessageConverter(string messageConverterObjectName);



    }
}