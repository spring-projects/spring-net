#region License

/*
 * Copyright 2002-2009 the original author or authors.
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

using Spring.Messaging.Ems.Common;
using Spring.Messaging.Ems.Core;
using TIBCO.EMS;

namespace Spring.EmsQuickStart.Client.Gateways
{
    /// <summary>
    /// Not used in the example application but included to show how one could layer on top of 
    /// EMS a synchronous request reply interaction.  This implementation uses temporary queues but 
    /// another approach would be to use a permanent queue with a message selector unique to the request.
    /// </summary>
    public class RequestReplyEmsTemplate : EmsTemplate
    {
        public object ConvertAndSendRequestReply(object objectToSend)
        {
            
            return Execute(delegate(ISession session, IMessageProducer producer)
                               {
                                   TemporaryQueue tempQueue = null;
                                   try
                                   {
                                       tempQueue = session.CreateTemporaryQueue();
                                       this.ConvertAndSendWithDelegate(objectToSend, delegate(Message message)
                                                                                         {
                                                                                             message.ReplyTo = tempQueue;
                                                                                             return message;
                                                                                         });
                                       return ReceiveAndConvert(tempQueue);
                                   }
                                   finally
                                   {
                                       //TODO use Admin API
                                       //if (tempQueue != null) session.DeleteDestination(tempQueue);
                                   }
                               });
        }
    }
}