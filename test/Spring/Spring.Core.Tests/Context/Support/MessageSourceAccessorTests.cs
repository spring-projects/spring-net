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

using System.Globalization;

using FakeItEasy;

using NUnit.Framework;
using Spring.Globalization;

namespace Spring.Context.Support
{
    [TestFixture]
    public class MessageSourceAccessorTests
    {
        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            CultureTestScope.Set();
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            CultureTestScope.Reset();
        }

        private readonly string MSGCODE = "code1";
        private readonly CultureInfo MSGCULTURE = new CultureInfo("fr");
        private readonly object[] MSGARGS = new object[] { "argument1" };
        private readonly string MSGRESULT = "my message";


        private IMessageSource mockMsgSource;
        private IMessageSourceResolvable mockMsgSourceResolvable;

        [SetUp]
        public void SetUp()
        {
            mockMsgSource = A.Fake<IMessageSource>();
            mockMsgSourceResolvable = A.Fake<IMessageSourceResolvable>();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void GetMessageCodeCultureArgs()
        {
            A.CallTo(() => mockMsgSource.GetMessage(MSGCODE, MSGCULTURE, MSGARGS)).Returns(MSGRESULT);

            MessageSourceAccessor msgSourceAccessor = new MessageSourceAccessor(mockMsgSource);
            Assert.AreEqual(MSGRESULT, msgSourceAccessor.GetMessage(MSGCODE, MSGCULTURE, MSGARGS));
        }

        [Test]
        public void GetMessageCodeArgs()
        {
            A.CallTo(() => mockMsgSource.GetMessage(MSGCODE, MSGCULTURE, MSGARGS)).Returns(MSGRESULT);

            MessageSourceAccessor msgSourceAccessor = new MessageSourceAccessor(mockMsgSource, MSGCULTURE);
            Assert.AreEqual(MSGRESULT, msgSourceAccessor.GetMessage(MSGCODE, MSGARGS));
        }

        [Test]
        public void GetMessageCodeArgsDefaultsToCurrentUICulture()
        {
            A.CallTo(() => mockMsgSource.GetMessage(MSGCODE, CultureInfo.CurrentUICulture, MSGARGS)).Returns(MSGRESULT);

            MessageSourceAccessor msgSourceAccessor = new MessageSourceAccessor(mockMsgSource);
            Assert.AreEqual(MSGRESULT, msgSourceAccessor.GetMessage(MSGCODE, MSGARGS));
        }

        [Test]
        public void GetMessageCodeCulture()
        {
            A.CallTo(() => mockMsgSource.GetMessage(MSGCODE, MSGCULTURE)).Returns(MSGRESULT);

            MessageSourceAccessor msgSourceAccessor = new MessageSourceAccessor(mockMsgSource);
            Assert.AreEqual(MSGRESULT, msgSourceAccessor.GetMessage(MSGCODE, MSGCULTURE));
        }

        [Test]
        public void GetMessageCodeDefaultsToCurrentUICulture()
        {
            A.CallTo(() => mockMsgSource.GetMessage(MSGCODE, CultureInfo.CurrentUICulture)).Returns(MSGRESULT);

            MessageSourceAccessor msgSourceAccessor = new MessageSourceAccessor(mockMsgSource);
            Assert.AreEqual(MSGRESULT, msgSourceAccessor.GetMessage(MSGCODE));
        }

        [Test]
        public void GetMessageResolvableCulture()
        {
            A.CallTo(() => mockMsgSource.GetMessage(mockMsgSourceResolvable, MSGCULTURE)).Returns(MSGRESULT);

            MessageSourceAccessor msgSourceAccessor = new MessageSourceAccessor(mockMsgSource);
            Assert.AreEqual(MSGRESULT, msgSourceAccessor.GetMessage(mockMsgSourceResolvable, MSGCULTURE));
        }

        [Test]
        public void GetMessageResolvableDefaultsToCurrentUICulture()
        {
            A.CallTo(() => mockMsgSource.GetMessage(mockMsgSourceResolvable, CultureInfo.CurrentUICulture)).Returns(MSGRESULT);

            MessageSourceAccessor msgSourceAccessor = new MessageSourceAccessor(mockMsgSource);
            Assert.AreEqual(MSGRESULT, msgSourceAccessor.GetMessage(mockMsgSourceResolvable));
        }
    }
}