#region License

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

#endregion

using System.Collections;
using Apache.NMS;

namespace Spring.Messaging.Nms.Core
{
    public class SimpleGateway: NmsGatewaySupport
    {
        public void Publish(string ticker, double price)
        {
            NmsTemplate.SendWithDelegate("APP.STOCK.MARKETDATA",
                          delegate(ISession session)
                          {
                              IMapMessage message = session.CreateMapMessage();
                              message.Body.SetString("TICKER", ticker);
                              message.Body.SetDouble("PRICE", price);
                              message.NMSPriority = MsgPriority.Normal;
                              return message;
                          });
        }

        public void PublishUsingDict(string ticker, double price)
        {
            IDictionary marketData = new Hashtable();
            marketData.Add("TICKER", ticker);
            marketData.Add("PRICE", price);
            NmsTemplate.ConvertAndSendWithDelegate("APP.STOCK.MARKETDATA", marketData,
                     delegate(IMessage message)
                     {
                         message.NMSPriority = MsgPriority.Normal;
                         message.NMSCorrelationID = new Guid().ToString();
                         return message;
                     });
        }
    }

}
