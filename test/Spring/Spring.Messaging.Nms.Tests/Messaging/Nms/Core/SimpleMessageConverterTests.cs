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

using FakeItEasy;

using NUnit.Framework;

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
        private SimpleMessageConverter converter;
        private ISession session;

        [SetUp]
        public void Setup()
        {
            session = A.Fake<ISession>();
            converter = new SimpleMessageConverter();
        }

        [Test]
        public void StringConversion()
        {
            ITextMessage message = A.Fake<ITextMessage>();
            string content = "test";

            A.CallTo(() => session.CreateTextMessage(content)).Returns(message).Once();
            A.CallTo(() => message.Text).Returns(content).Once();

            IMessage msg = converter.ToMessage(content, session);
            Assert.AreEqual(content, converter.FromMessage(msg));
        }

        [Test]
        public void ByteArrayConversion()
        {
            IBytesMessage message = A.Fake<IBytesMessage>();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] content =  encoding.GetBytes("test");

            A.CallTo(() => session.CreateBytesMessage()).Returns(message);
            A.CallTo(() => message.Content).Returns(content).Once();
            
            IMessage msg = converter.ToMessage(content, session);
            Assert.AreEqual(content.Length, ((byte[])converter.FromMessage(msg)).Length);
            A.CallTo(() => message.Content).WithAnyArguments().MustHaveHappened();
        }

        [Test]
        public void MapConversion()
        {
            IMapMessage message = A.Fake<IMapMessage>();
            IPrimitiveMap primitiveMap = new PrimitiveMap();
            IDictionary content = new Hashtable();
            content["key1"] = "value1";
            content["key2"] = "value2";

            A.CallTo(() => session.CreateMapMessage()).Returns(message).Once();
            A.CallTo(() => message.Body).Returns(primitiveMap);
            //can't seem to mock indexer...
            
            IMessage msg = converter.ToMessage(content, session);
            Assert.AreEqual(content, converter.FromMessage(msg));
        }

        [Test]
        public void Serializable()
        {
            IObjectMessage message = A.Fake<IObjectMessage>();

            SerializableWithAttribute content = new SerializableWithAttribute();

            A.CallTo(() => session.CreateObjectMessage(content)).Returns(message).Once();
            A.CallTo(() => message.Body).Returns(content).Once();

            IMessage msg = converter.ToMessage(content, session);
            Assert.AreEqual(content, converter.FromMessage(message));
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
            IObjectMessage message = A.Fake<IObjectMessage>();
            IMessage msg = converter.ToMessage(message, session);
            Assert.AreSame(message, msg);
        }

        [Test]
        public void FromMessageSimplyReturnsMessageAsIsIfSuppliedWithMessage()
        {
            IMessage message = A.Fake<IMessage>();
            Object msg = converter.FromMessage(message);
            Assert.AreSame(message, msg);
        }

        [Test]
        public void DictionaryConversionWhereMapHasNonStringTypesForKeys()
        {
            IMapMessage message = A.Fake<IMapMessage>();

            A.CallTo(() => session.CreateMapMessage()).Returns(message);

            var content = new Hashtable();
            content.Add(new Cafe(), "value1");

            Assert.Throws<MessageConversionException>(() => converter.ToMessage(content, session));
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