/*
 * Copyright � 2002-2011 the original author or authors.
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

using System.Text;
using FakeItEasy;
using NUnit.Framework;

namespace Spring.Context.Support;

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
        Assert.That(dmr.DefaultMessage, Is.Empty,
            "Not defaulting to non null empty string value (it must).");
    }

    [Test]
    public void NullLastCode()
    {
        DefaultMessageSourceResolvable dmr = new DefaultMessageSourceResolvable(null, null);
        Assert.That(dmr.LastCode, Is.Null);
    }

    [Test]
    public void LastCode()
    {
        DefaultMessageSourceResolvable dmr = new DefaultMessageSourceResolvable(new string[] { "code1" }, null);
        Assert.That(dmr.LastCode, Is.EqualTo("code1"));
    }

    [Test]
    public void ResolvableToString()
    {
        string[] codes = new string[] { "code1", "code2" };
        object[] arguments = new object[] { "argument1", "argument2" };
        string defaultMessage = "defaultMessage";
        DefaultMessageSourceResolvable dmr = new DefaultMessageSourceResolvable(codes, arguments, defaultMessage);
        Assert.That(dmr.ToString(), Is.EqualTo(getResolvableString()), "to string");
    }

    [Test]
    public void ResolvableToStringNullArguments()
    {
        string[] codes = new string[] { "code1", "code2" };
        string defaultMessage = "defaultMessage";
        DefaultMessageSourceResolvable dmr = new DefaultMessageSourceResolvable(codes, null, defaultMessage);
        Assert.That(dmr.ToString(), Is.EqualTo(getResolvableStringNull()), "to string");
    }

    [Test]
    public void DefaultResolvableFromExistingResolvable()
    {
        IMessageSourceResolvable res = A.Fake<IMessageSourceResolvable>();
        A.CallTo(() => res.DefaultMessage).Returns("defaultMessageFromMock");
        A.CallTo(() => res.GetCodes()).Returns(new string[] { "code1FromMock" });
        A.CallTo(() => res.GetArguments()).Returns(new object[] { "ArgumentFromMock" });

        DefaultMessageSourceResolvable dmr = new DefaultMessageSourceResolvable(res);
        Assert.That(dmr.DefaultMessage, Is.EqualTo("defaultMessageFromMock"), "default");
        Assert.That(dmr.LastCode, Is.EqualTo("code1FromMock"), "codes");
        Assert.That((dmr.GetArguments())[0], Is.EqualTo("ArgumentFromMock"), "arguments");
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
