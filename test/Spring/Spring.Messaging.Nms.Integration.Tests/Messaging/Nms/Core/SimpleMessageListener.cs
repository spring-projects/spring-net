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

using Apache.NMS;
using Microsoft.Extensions.Logging;

namespace Spring.Messaging.Nms.Core;

public class SimpleMessageListener : IMessageListener
{
    private static readonly ILogger<SimpleMessageListener> LOG = LogManager.GetLogger<SimpleMessageListener>();

    private int messageCount;

    public int MessageCount
    {
        get { return messageCount; }
    }

    public void OnMessage(IMessage message)
    {
        messageCount++;
        LOG.LogDebug("Message listener count = " + messageCount);
        ITextMessage textMessage = message as ITextMessage;
        if (textMessage != null)
        {
            LOG.LogInformation("Message Text = " + textMessage.Text);
        }
        else
        {
            LOG.LogWarning("Can not process message of type " + message.GetType());
        }
    }
}
