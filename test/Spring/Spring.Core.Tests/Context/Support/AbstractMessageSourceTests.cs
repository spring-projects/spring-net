#region License

/*
 * Copyright 2002-2005 the original author or authors.
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
		[ExpectedException(typeof (NoSuchMessageException))]
		public void GetResolvableNullCodes()
		{
			MockMessageResolvable res = new MockMessageResolvable();
			res.SetExpectedCodesCalls(1);
			GetMessage(res, CultureInfo.CurrentCulture);
			res.Verify();
		}

		[Test]
		public void GetResolvableDefaultsToParentMessageSource()
		{
		    string MSGCODE = "nullCode";
            object[] MSGARGS = new object[] { "arg1", "arg2" };

            MockMessageResolvable res = new MockMessageResolvable();
            res.SetExpectedCodesCalls(1);
            res.SetArguments(MSGARGS);
            res.SetCode(MSGCODE);

			MockMessageSource parentSource = new MockMessageSource();
			parentSource.SetExpectedGetMessageCalls(1);
            parentSource.SetExpectedGetMessageCode(MSGCODE);
            parentSource.SetExpectedGetMessageDefaultMessage(null);
            parentSource.SetExpectedGetMessageArguments(res.GetArguments());
			parentSource.SetExpectedGetMessageReturn("MockMessageSource");
			ParentMessageSource = parentSource;

			Assert.AreEqual("MockMessageSource", GetMessage(res, CultureInfo.CurrentCulture), "My Message");
			parentSource.Verify();
			res.Verify();
		}

		[Test]
		public void GetMessageParentMessageSource()
		{
		    object[] args = new object[] {"arguments"};
			MockMessageSource parentSource = new MockMessageSource();
			parentSource.SetExpectedGetMessageCalls(1);
            parentSource.SetExpectedGetMessageCode("null");
            parentSource.SetExpectedGetMessageDefaultMessage(null);
            parentSource.SetExpectedGetMessageArguments(args);
			parentSource.SetExpectedGetMessageReturn("my parent message");
			ParentMessageSource = parentSource;
			Assert.AreEqual("my parent message", GetMessage("null", "message", CultureInfo.CurrentCulture, args[0]));
			parentSource.Verify();
		}

		[Test]
		public void GetMessageResolvableDefaultMessage()
		{
			MockMessageResolvable res = new MockMessageResolvable();
			res.SetDefaultMessage("MyDefaultMessage");
			res.SetCode(null);
			Assert.AreEqual("MyDefaultMessage", GetMessage(res, CultureInfo.CurrentCulture), "Default");
			res.Verify();
		}

		[Test]
		public void GetMessageResolvableReturnsFirstCode()
		{
			MockMessageResolvable res = new MockMessageResolvable();
			UseCodeAsDefaultMessage = true;
			res.SetCode("null");
			Assert.AreEqual("null", GetMessage(res, CultureInfo.CurrentCulture), "Code");
			res.Verify();
		}

		[Test]
		[ExpectedException(typeof (NoSuchMessageException))]
		public void GetMessageResolvableNoValidMessage()
		{
			MockMessageResolvable res = new MockMessageResolvable();
			res.SetCode(null);
			GetMessage(res, CultureInfo.CurrentCulture);
		}

		[Test]
		public void GetMessageResolvableValidMessageAndCode()
		{
			MockMessageResolvable res = new MockMessageResolvable();
			res.SetCode("code1");
			res.SetArguments(new object[] {"my", "arguments"});
			Assert.AreEqual("my arguments", GetMessage(res, CultureInfo.CurrentCulture), "Resolve");
			res.Verify();
		}

		[Test]
		public void GetMessageResolvableValidMessageAndCodeNullCulture()
		{
			MockMessageResolvable res = new MockMessageResolvable();
			res.SetCode("code1");
			res.SetArguments(new object[] {"my", "arguments"});
			Assert.AreEqual("my arguments", GetMessage(res, null), "Resolve");
			res.Verify();
		}

		[Test]
        [ExpectedException(typeof(NoSuchMessageException))]
		public void GetMessageNullCode()
		{
			Assert.IsNull(GetMessage(null));
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
		[ExpectedException(typeof (NoSuchMessageException))]
		public void GetMessageNoValidMessage()
		{
			GetMessage("null", new object[] {"arguments"});
		}

		[Test]
		public void GetMessageWithResolvableArguments()
		{
			MockMessageResolvable res = new MockMessageResolvable();
			res.SetCode("code1");
			res.SetArguments(new object[] {"my", "resolvable"});
			Assert.AreEqual("spring my resolvable", GetMessage("code2", CultureInfo.CurrentCulture, new object[] {"spring", res}), "Resolve");
			res.Verify();
		}

		[Test]
		public void GetMessageResolvableValidMessageAndCodNullMessageFormat()
		{
			MockMessageResolvable res = new MockMessageResolvable();
			res.SetDefaultMessage("myDefaultMessage");
			res.SetCode("nullCode");
			Assert.AreEqual("myDefaultMessage", GetMessage(res, null), "Resolve");
			res.Verify();
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