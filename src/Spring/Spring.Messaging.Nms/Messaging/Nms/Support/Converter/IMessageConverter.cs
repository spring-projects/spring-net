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

using Apache.NMS;

namespace Spring.Messaging.Nms.Support.Converter
{
    /// <summary> Strategy interface that specifies a IMessageConverter
    /// between .NET objects and NMS messages.
    /// 
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public interface IMessageConverter
    {
        /// <summary> Convert a .NET object to a NMS Message using the supplied session
        /// to create the message object.
        /// </summary>
        /// <param name="objectToConvert">the object to convert
        /// </param>
        /// <param name="session">the Session to use for creating a NMS Message
        /// </param>
        /// <returns> the NMS Message
        /// </returns>
        /// <throws>NMSException if thrown by NMS API methods </throws>
        /// <throws>MessageConversionException in case of conversion failure </throws>
        IMessage ToMessage(object objectToConvert, ISession session);

        /// <summary> Convert from a NMS Message to a .NET object.</summary>
        /// <param name="messageToConvert">the message to convert
        /// </param>
        /// <returns> the converted .NET object
        /// </returns>
        /// <throws>MessageConversionException in case of conversion failure </throws>
        object FromMessage(IMessage messageToConvert);
    }
}
