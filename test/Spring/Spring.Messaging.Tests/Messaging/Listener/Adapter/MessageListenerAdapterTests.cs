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

#region Imports

using NUnit.Framework;

#endregion

namespace Spring.Messaging.Listener.Adapter
{
    /// <summary>
    /// To properly test the MessageListnerAdapter one needs to use TypeMock as MessageQueue and Message are not 
    /// interface based. 
    /// </summary>
    [TestFixture]
    public class MessageListenerAdapterTests
    {
        [Test]
        public void DefaultMessageHandlerMethodNameIsTheConstantDefault()
        {
            MessageListenerAdapter adapter = new MessageListenerAdapter();
            Assert.AreSame(MessageListenerAdapter.ORIGINAL_DEFAULT_LISTENER_METHOD, adapter.DefaultHandlerMethod);
        }
        
        [Test]
        public void WhenNoHandlerMethodIsSuppliedTheHandlerIsAssumedToBeTheMessageListenerAdapterItself()
        {
            MessageListenerAdapter adapter = new MessageListenerAdapter();
            Assert.AreSame(adapter, adapter.HandlerObject);
                 
        }
    }
}