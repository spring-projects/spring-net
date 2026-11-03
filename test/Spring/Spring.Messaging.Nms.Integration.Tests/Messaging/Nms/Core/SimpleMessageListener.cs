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

    private readonly SemaphoreSlim received = new(0);

    private int messageCount;

    public int MessageCount
    {
        get { return Volatile.Read(ref messageCount); }
    }

    /// <summary>
    /// Waits until a message has been delivered that no earlier call has consumed.
    /// </summary>
    /// <returns><c>true</c> if a message arrived within <paramref name="timeout"/>.</returns>
    public bool WaitForMessage(TimeSpan timeout)
    {
        return received.Wait(timeout);
    }

    public void OnMessage(IMessage message)
    {
        int count = Interlocked.Increment(ref messageCount);
        LOG.LogDebug("Message listener count = {MessageCount}", count);
        ITextMessage textMessage = message as ITextMessage;
        if (textMessage != null)
        {
            LOG.LogInformation("Message Text = {Text}", textMessage.Text);
        }
        else
        {
            LOG.LogWarning("Can not process message of type {MessageType}", message.GetType());
        }

        received.Release();
    }
}
