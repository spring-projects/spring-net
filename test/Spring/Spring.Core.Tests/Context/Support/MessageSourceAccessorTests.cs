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

using NUnit.Framework;
using Rhino.Mocks;
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


        private MockRepository mocks;
        private IMessageSource mockMsgSource;
        private IMessageSourceResolvable mockMsgSourceResolvable;

        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();
            mockMsgSource = mocks.StrictMock<IMessageSource>();
            mockMsgSourceResolvable = mocks.StrictMock<IMessageSourceResolvable>();
        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
        }

        [Test]
        public void GetMessageCodeCultureArgs()
        {
            Expect.Call(mockMsgSource.GetMessage(MSGCODE, MSGCULTURE, MSGARGS)).Return(MSGRESULT);
            mocks.ReplayAll();

            MessageSourceAccessor msgSourceAccessor = new MessageSourceAccessor(mockMsgSource);
            Assert.AreEqual(MSGRESULT, msgSourceAccessor.GetMessage(MSGCODE, MSGCULTURE, MSGARGS));
        }

        [Test]
        public void GetMessageCodeArgs()
        {
            Expect.Call(mockMsgSource.GetMessage(MSGCODE, MSGCULTURE, MSGARGS)).Return(MSGRESULT);
            mocks.ReplayAll();

            MessageSourceAccessor msgSourceAccessor = new MessageSourceAccessor(mockMsgSource, MSGCULTURE);
            Assert.AreEqual(MSGRESULT, msgSourceAccessor.GetMessage(MSGCODE, MSGARGS));
        }

        [Test]
        public void GetMessageCodeArgsDefaultsToCurrentUICulture()
        {
            Expect.Call(mockMsgSource.GetMessage(MSGCODE, CultureInfo.CurrentUICulture, MSGARGS)).Return(MSGRESULT);
            mocks.ReplayAll();

            MessageSourceAccessor msgSourceAccessor = new MessageSourceAccessor(mockMsgSource);
            Assert.AreEqual(MSGRESULT, msgSourceAccessor.GetMessage(MSGCODE, MSGARGS));
        }

        [Test]
        public void GetMessageCodeCulture()
        {
            Expect.Call(mockMsgSource.GetMessage(MSGCODE, MSGCULTURE)).Return(MSGRESULT);
            mocks.ReplayAll();

            MessageSourceAccessor msgSourceAccessor = new MessageSourceAccessor(mockMsgSource);
            Assert.AreEqual(MSGRESULT, msgSourceAccessor.GetMessage(MSGCODE, MSGCULTURE));
        }

        [Test]
        public void GetMessageCodeDefaultsToCurrentUICulture()
        {
            Expect.Call(mockMsgSource.GetMessage(MSGCODE, CultureInfo.CurrentUICulture)).Return(MSGRESULT);
            mocks.ReplayAll();

            MessageSourceAccessor msgSourceAccessor = new MessageSourceAccessor(mockMsgSource);
            Assert.AreEqual(MSGRESULT, msgSourceAccessor.GetMessage(MSGCODE));
        }

        [Test]
        public void GetMessageResolvableCulture()
        {
            Expect.Call(mockMsgSource.GetMessage(mockMsgSourceResolvable, MSGCULTURE)).Return(MSGRESULT);            
            mocks.ReplayAll();

            MessageSourceAccessor msgSourceAccessor = new MessageSourceAccessor(mockMsgSource);
            Assert.AreEqual(MSGRESULT, msgSourceAccessor.GetMessage(mockMsgSourceResolvable, MSGCULTURE));
        }

        [Test]
        public void GetMessageResolvableDefaultsToCurrentUICulture()
        {
            Expect.Call(mockMsgSource.GetMessage(mockMsgSourceResolvable, CultureInfo.CurrentUICulture)).Return(MSGRESULT);
            mocks.ReplayAll();

            MessageSourceAccessor msgSourceAccessor = new MessageSourceAccessor(mockMsgSource);
            Assert.AreEqual(MSGRESULT, msgSourceAccessor.GetMessage(mockMsgSourceResolvable));
        }
    }
}