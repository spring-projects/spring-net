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

using System.Text;

using FakeItEasy;

using NUnit.Framework;

namespace Spring.Context.Support
{
	/// <summary>
	/// Unit tests for the DefaultMessageSourceResolvable class.
	/// </summary>
	[TestFixture]
	public sealed class DefaultMessageSourceResolvableTests
	{
        [SetUp]
        public void Init()
        {
        }

		[Test]
		public void InstantiationWithASingleCodeDefaultsToEmptyDefaultMessage()
		{
			DefaultMessageSourceResolvable dmr = new DefaultMessageSourceResolvable("foo");
			Assert.AreEqual(string.Empty, dmr.DefaultMessage,
							"Not defaulting to non null empty string value (it must).");
		}

		[Test]
		public void NullLastCode()
		{
			DefaultMessageSourceResolvable dmr = new DefaultMessageSourceResolvable(null, null);
			Assert.IsNull(dmr.LastCode);
		}

		[Test]
		public void LastCode()
		{
			DefaultMessageSourceResolvable dmr = new DefaultMessageSourceResolvable(new string[] {"code1"}, null);
			Assert.AreEqual("code1", dmr.LastCode);
		}

		[Test]
		public void ResolvableToString()
		{
			string[] codes = new string[] {"code1", "code2"};
			object[] arguments = new object[] {"argument1", "argument2"};
			string defaultMessage = "defaultMessage";
			DefaultMessageSourceResolvable dmr = new DefaultMessageSourceResolvable(codes, arguments, defaultMessage);
			Assert.AreEqual(getResolvableString(), dmr.ToString(), "to string");
		}

		[Test]
		public void ResolvableToStringNullArguments()
		{
			string[] codes = new string[] {"code1", "code2"};
			string defaultMessage = "defaultMessage";
			DefaultMessageSourceResolvable dmr = new DefaultMessageSourceResolvable(codes, null, defaultMessage);
			Assert.AreEqual(getResolvableStringNull(), dmr.ToString(), "to string");
		}

		[Test]
		public void DefaultResolvableFromExistingResolvable()
		{
            IMessageSourceResolvable res = A.Fake<IMessageSourceResolvable>();
            A.CallTo(() => res.DefaultMessage).Returns("defaultMessageFromMock");
            A.CallTo(() => res.GetCodes()).Returns(new string[] { "code1FromMock" });
            A.CallTo(() => res.GetArguments()).Returns(new object[] { "ArgumentFromMock" });

            DefaultMessageSourceResolvable dmr = new DefaultMessageSourceResolvable(res);
			Assert.AreEqual("defaultMessageFromMock", dmr.DefaultMessage, "default");
			Assert.AreEqual("code1FromMock", dmr.LastCode, "codes");
			Assert.AreEqual("ArgumentFromMock", (dmr.GetArguments())[0], "arguments");
		}

		private string getResolvableString()
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("codes=[code1,code2]; arguments=[(String)[argument1], (String)[argument2]]; ");
			builder.Append("defaultMessage=[defaultMessage]");
			return builder.ToString();
		}

		private string getResolvableStringNull()
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("codes=[code1,code2]; arguments=[null]; ");
			builder.Append("defaultMessage=[defaultMessage]");
			return builder.ToString();
		}
	}
}