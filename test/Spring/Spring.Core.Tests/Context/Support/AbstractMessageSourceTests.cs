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

#region Imports

using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;

#endregion

namespace Spring.Context.Support
{
    [TestFixture]
	public sealed class AbstractMessageSourceTests : AbstractMessageSource
	{
        private MockRepository mocks;
		[SetUp]
		public void Init()
		{
            mocks = new MockRepository();
			resetMe();
		}

		[Test]
		public void GetResolvableNullCodes()
		{            
		    IMessageSourceResolvable res = mocks.StrictMock<IMessageSourceResolvable>();
		    Expect.Call(res.GetCodes()).Return(null);
		    Expect.Call(res.DefaultMessage).Return(null);
            mocks.ReplayAll();
            Assert.Throws<NoSuchMessageException>(() => GetMessage(res, CultureInfo.CurrentCulture));
            mocks.VerifyAll();		    
		}

		[Test]
		public void GetResolvableDefaultsToParentMessageSource()
		{
		    string MSGCODE = "nullCode";
            object[] MSGARGS = new object[] { "arg1", "arg2" };

            IMessageSourceResolvable res = mocks.StrictMock<IMessageSourceResolvable>();
            Expect.Call(res.GetArguments()).Return(MSGARGS);
		    Expect.Call(res.GetCodes()).Return(new string[] {MSGCODE}).Repeat.Once();
		                           
            IMessageSource parentSource = mocks.StrictMock<IMessageSource>();
            Expect.Call(parentSource.GetMessage(MSGCODE, null, CultureInfo.CurrentCulture, MSGARGS)).Return(
		        "MockMessageSource");
			ParentMessageSource = parentSource;

            mocks.ReplayAll();
			Assert.AreEqual("MockMessageSource", GetMessage(res, CultureInfo.CurrentCulture), "My Message");
			mocks.VerifyAll();
		}

		[Test]
		public void GetMessageParentMessageSource()
		{
		    object[] args = new object[] {"arguments"};
            IMessageSource parentSource = mocks.StrictMock<IMessageSource>();
            Expect.Call(parentSource.GetMessage("null", null, CultureInfo.CurrentCulture, args)).Return(
                "my parent message");
			ParentMessageSource = parentSource;
            mocks.ReplayAll();
			Assert.AreEqual("my parent message", GetMessage("null", "message", CultureInfo.CurrentCulture, args[0]));
            mocks.VerifyAll();
		}

		[Test]
		public void GetMessageResolvableDefaultMessage()
		{
            IMessageSourceResolvable res = mocks.StrictMock<IMessageSourceResolvable>();
		    Expect.Call(res.DefaultMessage).Return("MyDefaultMessage").Repeat.AtLeastOnce();
		    Expect.Call(res.GetCodes()).Return(null);
		    Expect.Call(res.GetArguments()).Return(null);
            mocks.ReplayAll();
			
			Assert.AreEqual("MyDefaultMessage", GetMessage(res, CultureInfo.CurrentCulture), "Default");
			
            mocks.VerifyAll();
		}

		[Test]
		public void GetMessageResolvableReturnsFirstCode()
		{

            IMessageSourceResolvable res = mocks.StrictMock<IMessageSourceResolvable>();
            Expect.Call(res.DefaultMessage).Return(null).Repeat.AtLeastOnce();
            Expect.Call(res.GetCodes()).Return(new string[] {"null"});
		    Expect.Call(res.GetArguments()).Return(null).Repeat.AtLeastOnce();
            mocks.ReplayAll();
			UseCodeAsDefaultMessage = true;
		    Assert.AreEqual("null", GetMessage(res, CultureInfo.CurrentCulture), "Code");			
            mocks.VerifyAll();
		}

		[Test]
		public void GetMessageResolvableNoValidMessage()
		{
            IMessageSourceResolvable res = mocks.StrictMock<IMessageSourceResolvable>();
            Expect.Call(res.DefaultMessage).Return(null).Repeat.AtLeastOnce();
            Expect.Call(res.GetCodes()).Return(null);
            Expect.Call(res.GetArguments()).Return(null).Repeat.AtLeastOnce();
            mocks.ReplayAll();
            Assert.Throws<NoSuchMessageException>(() => GetMessage(res, CultureInfo.CurrentCulture));
		}

		[Test]
		public void GetMessageResolvableValidMessageAndCode()
		{
            IMessageSourceResolvable res = mocks.StrictMock<IMessageSourceResolvable>();
            Expect.Call(res.GetCodes()).Return(new string[] {"code1"});
            Expect.Call(res.GetArguments()).Return(new object[] { "my", "arguments" }).Repeat.AtLeastOnce();
            mocks.ReplayAll();
			Assert.AreEqual("my arguments", GetMessage(res, CultureInfo.CurrentCulture), "Resolve");
			mocks.VerifyAll();
		}

		[Test]
		public void GetMessageResolvableValidMessageAndCodeNullCulture()
		{
            IMessageSourceResolvable res = mocks.StrictMock<IMessageSourceResolvable>();
            Expect.Call(res.GetCodes()).Return(new string[] { "code1" });
            Expect.Call(res.GetArguments()).Return(new object[] { "my", "arguments" }).Repeat.AtLeastOnce();
            mocks.ReplayAll();
			Assert.AreEqual("my arguments", GetMessage(res, null), "Resolve");
			mocks.VerifyAll();
		}

		[Test]
		public void GetMessageNullCode()
		{
            Assert.Throws<NoSuchMessageException>(() => GetMessage(null));
		}

		[Test]
		public void GetMessageValidMessageAndCode()
		{
			Assert.AreEqual("my arguments", GetMessage("code1", new object[] {"my", "arguments"}), "Resolve");
		}

		[Test]
		public void GetMessageValidMessageAndCodeNullCulture()
		{
			Assert.AreEqual("my arguments", GetMessage("code1", null, new object[] {"my", "arguments"}), "Resolve");
		}

		[Test]
		public void GetMessageUseDefaultCode()
		{
			UseCodeAsDefaultMessage = true;
			Assert.AreEqual("null", GetMessage("null", new object[] {"arguments"}), "message");
			Assert.IsTrue(UseCodeAsDefaultMessage, "default");
		}

		[Test]
		public void GetMessageNoValidMessage()
		{
            Assert.Throws<NoSuchMessageException>(() => GetMessage("null", new object[] {"arguments"}));
		}

		[Test]
		public void GetMessageWithResolvableArguments()
		{
            IMessageSourceResolvable res = mocks.StrictMock<IMessageSourceResolvable>();
            Expect.Call(res.GetCodes()).Return(new string[] { "code1" });
            Expect.Call(res.GetArguments()).Return(new object[] { "my", "resolvable" }).Repeat.AtLeastOnce();
            mocks.ReplayAll();
			Assert.AreEqual("spring my resolvable", GetMessage("code2", CultureInfo.CurrentCulture, new object[] {"spring", res}), "Resolve");
			mocks.VerifyAll();
		}

		[Test]
		public void GetMessageResolvableValidMessageAndCodNullMessageFormat()
		{
            IMessageSourceResolvable res = mocks.StrictMock<IMessageSourceResolvable>();
		    Expect.Call(res.DefaultMessage).Return("myDefaultMessage").Repeat.AtLeastOnce();
            Expect.Call(res.GetCodes()).Return(new string[] { "nullCode" });
            Expect.Call(res.GetArguments()).Return(null).Repeat.AtLeastOnce();
            mocks.ReplayAll();

			Assert.AreEqual("myDefaultMessage", GetMessage(res, null), "Resolve");
			mocks.VerifyAll();
		}

		private void resetMe()
		{
			ParentMessageSource = null;
			UseCodeAsDefaultMessage = false;
		}

		protected override string ResolveMessage(string code, CultureInfo cultureInfo)
		{
			if (code.Equals("null"))
			{
				return null;
			}
			else if (code.Equals("nullCode"))
			{
				return null;
			}            
			else
			{
				return "{0} {1}";
			}
		}

		protected override object ResolveObject(string code, CultureInfo cultureInfo)
		{
			return null;
		}

		protected override void ApplyResourcesToObject(object value, string objectName, CultureInfo cultureInfo)
		{
		}
	}
}