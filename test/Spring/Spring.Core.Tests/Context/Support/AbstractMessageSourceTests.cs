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

namespace Spring.Context.Support
{
    [TestFixture]
	public sealed class AbstractMessageSourceTests : AbstractMessageSource
	{
		[SetUp]
		public void Init()
		{
			ResetMe();
		}

		[Test]
		public void GetResolvableNullCodes()
		{
		    var res = A.Fake<IMessageSourceResolvable>();
			A.CallTo(() => res.DefaultMessage).Returns(null);

            Assert.Throws<NoSuchMessageException>(() => GetMessage(res, CultureInfo.CurrentCulture));
		}

		[Test]
		public void GetResolvableDefaultsToParentMessageSource()
		{
		    string MSGCODE = "nullCode";
            object[] MSGARGS = new object[] { "arg1", "arg2" };

            IMessageSourceResolvable res = A.Fake<IMessageSourceResolvable>();
            A.CallTo(() => res.GetArguments()).Returns(MSGARGS);
		    A.CallTo(() => res.GetCodes()).Returns(new string[] {MSGCODE}).Once();

            IMessageSource parentSource = A.Fake<IMessageSource>();
            A.CallTo(() => parentSource.GetMessage(MSGCODE, null, CultureInfo.CurrentCulture, A<object[]>._)).Returns("MockMessageSource");
			ParentMessageSource = parentSource;

			Assert.AreEqual("MockMessageSource", GetMessage(res, CultureInfo.CurrentCulture), "My Message");
		}

		[Test]
		public void GetMessageParentMessageSource()
		{
		    object[] args = new object[] {"arguments"};
            IMessageSource parentSource = A.Fake<IMessageSource>();
            A.CallTo(() => parentSource.GetMessage("null", null, CultureInfo.CurrentCulture, A<object[]>._)).Returns("my parent message");
			ParentMessageSource = parentSource;
			Assert.AreEqual("my parent message", GetMessage("null", "message", CultureInfo.CurrentCulture, args[0]));
		}

		[Test]
		public void GetMessageResolvableDefaultMessage()
		{
            IMessageSourceResolvable res = A.Fake<IMessageSourceResolvable>();
            A.CallTo(() => res.DefaultMessage).Returns("MyDefaultMessage");
		    A.CallTo(() => res.GetCodes()).Returns(null);
            A.CallTo(() => res.GetArguments()).Returns(null);

			Assert.AreEqual("MyDefaultMessage", GetMessage(res, CultureInfo.CurrentCulture), "Default");
		}

		[Test]
		public void GetMessageResolvableReturnsFirstCode()
		{
            IMessageSourceResolvable res = A.Fake<IMessageSourceResolvable>();
            A.CallTo(() => res.DefaultMessage).Returns(null);
            A.CallTo(() => res.GetCodes()).Returns(new string[] {"null"});
            A.CallTo(() => res.GetArguments()).Returns(null);

			UseCodeAsDefaultMessage = true;
		    Assert.AreEqual("null", GetMessage(res, CultureInfo.CurrentCulture), "Code");
		}

		[Test]
		public void GetMessageResolvableNoValidMessage()
		{
            IMessageSourceResolvable res = A.Fake<IMessageSourceResolvable>();
            A.CallTo(() => res.DefaultMessage).Returns(null);
            A.CallTo(() => res.GetCodes()).Returns(null);
            A.CallTo(() => res.GetArguments()).Returns(null);

            Assert.Throws<NoSuchMessageException>(() => GetMessage(res, CultureInfo.CurrentCulture));
		}

		[Test]
		public void GetMessageResolvableValidMessageAndCode()
		{
            IMessageSourceResolvable res = A.Fake<IMessageSourceResolvable>();
            A.CallTo(() => res.GetCodes()).Returns(new string[] {"code1"});
            A.CallTo(() => res.GetArguments()).Returns(new object[] { "my", "arguments" });

			Assert.AreEqual("my arguments", GetMessage(res, CultureInfo.CurrentCulture), "Resolve");
		}

		[Test]
		public void GetMessageResolvableValidMessageAndCodeNullCulture()
		{
            IMessageSourceResolvable res = A.Fake<IMessageSourceResolvable>();
            A.CallTo(() => res.GetCodes()).Returns(new string[] { "code1" });
            A.CallTo(() => res.GetArguments()).Returns(new object[] { "my", "arguments" });

            Assert.AreEqual("my arguments", GetMessage(res, null), "Resolve");
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
            IMessageSourceResolvable res = A.Fake<IMessageSourceResolvable>();
            A.CallTo(() => res.GetCodes()).Returns(new string[] { "code1" });
            A.CallTo(() => res.GetArguments()).Returns(new object[] { "my", "resolvable" });

			Assert.AreEqual("spring my resolvable", GetMessage("code2", CultureInfo.CurrentCulture, new object[] {"spring", res}), "Resolve");
		}

		[Test]
		public void GetMessageResolvableValidMessageAndCodNullMessageFormat()
		{
            IMessageSourceResolvable res = A.Fake<IMessageSourceResolvable>();
            A.CallTo(() => res.DefaultMessage).Returns("myDefaultMessage");
            A.CallTo(() => res.GetCodes()).Returns(new string[] { "nullCode" });
            A.CallTo(() => res.GetArguments()).Returns(null);

			Assert.AreEqual("myDefaultMessage", GetMessage(res, null), "Resolve");
		}

		private void ResetMe()
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