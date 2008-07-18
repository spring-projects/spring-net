#region License

/*
 * Copyright 2002-2008 the original author or authors.
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

using System;
using Apache.NMS;

namespace Spring.Messaging.Nms.Connection
{
    /// <summary>
    /// MessageProducer decorator that adapts specific settings
    /// to a shared MessageProducer instance underneath.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public class CachedMessageProducer : IMessageProducer
    {
        private IMessageProducer target;

        private bool disableMessageID;

        private object originalDisableMessageID = null;

        private bool disableMessageTimestamp;

        private object originalDisableMessageTimestamp = null;

        //Not part of NMS spce
        //private int deliveryMode;

        private bool persistent;

        private byte priority;

        private TimeSpan timeToLive;


        public CachedMessageProducer(IMessageProducer target)
        {
            this.target = target;
        }


        public IMessageProducer Target
        {
            get { return target; }
        }

        public void Send(IMessage message)
        {
            target.Send(message);
        }

        public void Send(IMessage message, bool persistent, byte priority, TimeSpan timeToLive)
        {
           target.Send(message, persistent, priority, timeToLive);
        }

        public void Send(IDestination destination, IMessage message)
        {
            target.Send(destination, message);
        }

        public void Send(IDestination destination, IMessage message, bool persistent, byte priority, TimeSpan timeToLive)
        {
            target.Send(destination, message, persistent, priority, timeToLive);
        }

        #region Odd Message Creationg Methods on IMessageProducer - not in-line with JMS APIs.
        public IMessage CreateMessage()
        {
            return target.CreateMessage();
        }

        public ITextMessage CreateTextMessage()
        {
            return target.CreateTextMessage();
        }

        public ITextMessage CreateTextMessage(string text)
        {
            return target.CreateTextMessage(text);
        }

        public IMapMessage CreateMapMessage()
        {
            return target.CreateMapMessage();
        }

        public IObjectMessage CreateObjectMessage(object body)
        {
            return target.CreateObjectMessage(body);
        }

        public IBytesMessage CreateBytesMessage()
        {
            return target.CreateBytesMessage();
        }

        public IBytesMessage CreateBytesMessage(byte[] body)
        {
            return target.CreateBytesMessage(body);
        }
        #endregion

        public bool Persistent
        {
            get { return persistent; }
            set { persistent = value; }
        }

        public TimeSpan TimeToLive
        {
            get { return timeToLive; }
            set { timeToLive = value; }
        }

        public byte Priority
        {
            get { return priority; }
            set { priority = value;}
        }

        public bool DisableMessageID
        {
            get
            {
                return disableMessageID;
            }
            set
            {
                if (originalDisableMessageID == null)
                {
                    originalDisableMessageID = value;
                }
                disableMessageID = value;
            }
        }

        public bool DisableMessageTimestamp
        {
            get
            {
                return disableMessageTimestamp;
            }
            set
            {
                if (originalDisableMessageTimestamp == null)
                {
                    originalDisableMessageTimestamp = value;
                }
                disableMessageTimestamp = value;
            }
        }

        public void Dispose()
        {
            // It's a cached MessageProducer... reset properties only.
            if (originalDisableMessageID != null)
            {
                target.DisableMessageID = (bool) originalDisableMessageID;
                originalDisableMessageID = null;
            }
            if (originalDisableMessageTimestamp != null)
            {
                target.DisableMessageTimestamp = (bool) originalDisableMessageTimestamp;
                originalDisableMessageTimestamp = null;
            }
        }
    }
}