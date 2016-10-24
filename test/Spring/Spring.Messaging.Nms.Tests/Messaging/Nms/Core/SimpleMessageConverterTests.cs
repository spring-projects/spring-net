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

using System;
using System.Collections;
using System.Text;

using Apache.NMS;
using Apache.NMS.Util;

using NUnit.Framework;

using Rhino.Mocks;

using Spring.Messaging.Nms.Support.Converter;

#endregion

namespace Spring.Messaging.Nms.Core
{
    /// <summary>
    /// This class contains tests for SimpleMessageConverer
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class SimpleMessageConverterTests
    {
        private MockRepository mocks;
        private SimpleMessageConverter converter;
        private ISession session;


        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            session = mocks.StrictMock<ISession>();
            converter = new SimpleMessageConverter();
        }

        [Test]
        public void StringConversion()
        {
            ITextMessage message = mocks.StrictMock<ITextMessage>();
            string content = "test";

            Expect.Call(session.CreateTextMessage(content)).Return(message).Repeat.Once();
            string txt = message.Text;
            LastCall.On(message).Return(content).Repeat.Once();

            mocks.ReplayAll();


            IMessage msg = converter.ToMessage(content, session);
            Assert.AreEqual(content, converter.FromMessage(msg));

            mocks.VerifyAll();


        }

        [Test]
        public void ByteArrayConversion()
        {
            IBytesMessage message = mocks.StrictMock<IBytesMessage>();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] content =  encoding.GetBytes("test");

            Expect.Call(session.CreateBytesMessage()).Return(message);
            Expect.Call(message.Content = content).Repeat.Once();


            Expect.Call(message.Content).Return(content).Repeat.Once();
            
            mocks.ReplayAll();

            IMessage msg = converter.ToMessage(content, session);
            Assert.AreEqual(content.Length, ((byte[])converter.FromMessage(msg)).Length);

            mocks.VerifyAll();
        }

        [Test]
        public void MapConversion()
        {
            IMapMessage message = mocks.StrictMock<IMapMessage>();
            IPrimitiveMap primitiveMap = new PrimitiveMap();
            IDictionary content = new Hashtable();
            content["key1"] = "value1";
            content["key2"] = "value2";

            Expect.Call(session.CreateMapMessage()).Return(message).Repeat.Once();
            Expect.Call(message.Body).Return(primitiveMap).Repeat.Any();
            //can't seem to mock indexer...
            
            mocks.ReplayAll();

            IMessage msg = converter.ToMessage(content, session);
            Assert.AreEqual(content, converter.FromMessage(msg));

            mocks.VerifyAll();


        }

        [Test]
        public void Serializable()
        {
            IObjectMessage message = mocks.StrictMock<IObjectMessage>();

            SerializableWithAttribute content = new SerializableWithAttribute();

            Expect.Call(session.CreateObjectMessage(content)).Return(message).Repeat.Once();

            Expect.Call(message.Body).Return(content).Repeat.Once();

            mocks.ReplayAll();


            IMessage msg = converter.ToMessage(content, session);
            Assert.AreEqual(content, converter.FromMessage(message));
            
            mocks.VerifyAll();
        }

        [Test]
        public void ToMessageThrowsExceptionIfGivenNullObjectToConvert()
        {
            Assert.Throws<MessageConversionException>(() => converter.ToMessage(null, null));
        }

        [Test]
        public void ToMessageThrowsExceptionIfGivenIncompatibleObjectToConvert()
        {
            Assert.Throws<MessageConversionException>(() => converter.ToMessage(new Cafe(), null));
        }

        [Test]
        public void ToMessageSimplyReturnsMessageAsIsIfSuppliedWithMessage()
        {
            IObjectMessage message = mocks.StrictMock<IObjectMessage>();

            mocks.ReplayAll();

            IMessage msg = converter.ToMessage(message, session);
            Assert.AreSame(message, msg);

            mocks.VerifyAll();
        }

        [Test]
        public void FromMessageSimplyReturnsMessageAsIsIfSuppliedWithMessage()
        {
            IMessage message = mocks.StrictMock<IMessage>();

            mocks.ReplayAll();

            Object msg = converter.FromMessage(message);
            Assert.AreSame(message, msg);

            mocks.VerifyAll();
        }

        [Test]
        public void DictionaryConversionWhereMapHasNonStringTypesForKeys()
        {
            IMapMessage message = mocks.StrictMock<IMapMessage>();


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
        }

        public class Cafe
        {
            
        }
    }
}