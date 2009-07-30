#region License

/*
 * Copyright © 2002-2007 the original author or authors.
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

using System;
using System.Collections;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Messaging.Ems.Common;
using Spring.Messaging.Ems.Support.Converter;
using Spring.Util;
using TIBCO.EMS;

#endregion

namespace Spring.Messaging.Ems.Core
{
    /// <summary>
    /// This class contains integration tests for SimpleMessageConverer
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class SimpleMessageConverterTests
    {
        private MockRepository mocks;
        private SimpleMessageConverter converter;
        private ISession session;
        private TIBCO.EMS.Session tibcoSession = null;


        [SetUp]
        public void Setup()
        {
            //ConnectionFactory cf = new ConnectionFactory("tcp://localhost:7222");
            //Connection c = cf.CreateConnection();
            //Session tibcoSession = null;// c.CreateSession(false, SessionMode.AutoAcknowledge);
            mocks = new MockRepository();
            session = (ISession) mocks.CreateMock(typeof (ISession));
            converter = new SimpleMessageConverter();
        }

        [Test]
        public void StringConversion()
        {
            
            string content = "test";
            TextMessage message = new TextMessage(tibcoSession, content);

            Expect.Call(session.CreateTextMessage(content)).Return(message).Repeat.Once();
            string txt = message.Text;

            mocks.ReplayAll();


            Message msg = converter.ToMessage(content, session);

            Assert.AreEqual(content, converter.FromMessage(msg));

            mocks.VerifyAll();


        }

        [Test]
        public void ByteArrayConversion()
        {
            BytesMessage message = new BytesMessage(tibcoSession);
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] content =  encoding.GetBytes("test");

            Expect.Call(session.CreateBytesMessage()).Return(message);
            

            mocks.ReplayAll();

            Message msg = converter.ToMessage(content, session);
            message.Reset();
            Assert.AreEqual(content.Length, ((byte[])converter.FromMessage(msg)).Length);

            mocks.VerifyAll();
        }

        [Test]
        public void MapConversion()
        {

            MapMessage message = new MapMessage(tibcoSession);
            IDictionary content = new Hashtable();
            content["key1"] = "value1";
            content["key2"] = "value2";

            Expect.Call(session.CreateMapMessage()).Return(message).Repeat.Once();

            mocks.ReplayAll();

            Message msg = converter.ToMessage(content, session);
            Assert.AreEqual(content, converter.FromMessage(msg));

            mocks.VerifyAll();


        }

        [Test]
        public void Serializable()
        {
            //IObjectMessage message = (IObjectMessage)mocks.CreateMock(typeof(IObjectMessage));

            SerializableWithAttribute content = new SerializableWithAttribute();

            ObjectMessage message = new ObjectMessage(tibcoSession, content);
            Expect.Call(session.CreateObjectMessage(content)).Return(message).Repeat.Once();

            //Expect.Call(message.TheObject).Return(content).Repeat.Once();

            mocks.ReplayAll();


            Message msg = converter.ToMessage(content, session);
            Assert.AreEqual(content, converter.FromMessage(message));
            
            mocks.VerifyAll();
        }

        [Test]
        [ExpectedException(typeof(MessageConversionException))]
        public void ToMessageThrowsExceptionIfGivenNullObjectToConvert()
        {
            converter.ToMessage(null, null);
        }

        [Test]
        [ExpectedException(typeof(MessageConversionException))]
        public void ToMessageThrowsExceptionIfGivenIncompatibleObjectToConvert()
        {
            converter.ToMessage(new Cafe(), null);
        }
        
        [Test]
        public void ToMessageSimplyReturnsMessageAsIsIfSuppliedWithMessage()
        {
            SerializableWithAttribute content = new SerializableWithAttribute();

            ObjectMessage message = new ObjectMessage(tibcoSession, content);

            mocks.ReplayAll();

            Message msg = converter.ToMessage(message, session);
            Assert.AreSame(message, msg);

            mocks.VerifyAll();
        }
        
        [Test]
        public void FromMessageSimplyReturnsMessageAsIsIfSuppliedWithMessage()
        {
            //IMessage message = (IMessage)mocks.CreateMock(typeof(IMessage));

            StreamMessage message = new StreamMessage(tibcoSession);
            mocks.ReplayAll();

            Object msg = converter.FromMessage(message);
            Assert.AreSame(message, msg);

            mocks.VerifyAll();
        }

        [Test]
        public void DictionaryConversionWhereMapHasNonStringTypesForKeys()
        {
            //IMapMessage message = (IMapMessage)mocks.CreateMock(typeof(IMapMessage));
            MapMessage message = new MapMessage(tibcoSession);

            Expect.Call(session.CreateMapMessage()).Return(message);
            mocks.ReplayAll();

            IDictionary content = new Hashtable();
            content.Add(new Cafe(), "value1");
    
            try
            {
                converter.ToMessage(content, session);
                Assert.Fail("Should have thrown MessageConversionException");
            } catch (MessageConversionException)
            {
        
            }

            mocks.VerifyAll();
        }
        

        [Serializable]
        public class SerializableWithAttribute
        {
            public bool Equals(SerializableWithAttribute other)
            {
                return !ReferenceEquals(null, other);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof (SerializableWithAttribute)) return false;
                return Equals((SerializableWithAttribute) obj);
            }

            public override int GetHashCode()
            {
                return 0;
            }
        }

        public class Cafe
        {
            
        }

    }
}