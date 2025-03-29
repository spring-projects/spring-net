/*
 * Copyright � 2002-2011 the original author or authors.
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

namespace Spring.Messaging.Nms.Core;

/// <summary> To be used with NmsTemplate's send method that
/// convert an object to a message.
/// </summary>
/// <remarks>
/// It allows for further modification of the message after it has been processed
/// by the converter. This is useful for setting of NMS Header and Properties.
/// </remarks>
/// <author>Mark Pollack</author>
public interface IMessagePostProcessor
{
    /// <summary> Apply a IMessagePostProcessor to the message. The returned message is
    /// typically a modified version of the original.
    /// </summary>
    /// <param name="message">the NMS message from the IMessageConverter
    /// </param>
    /// <returns> the modified version of the Message
    /// </returns>
    /// <throws>NMSException if thrown by NMS API methods </throws>
    IMessage PostProcessMessage(IMessage message);
}
