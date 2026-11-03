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

using System.Globalization;
using NUnit.Framework;
using Spring.Globalization;
using Spring.Objects;

namespace Spring.Context.Support;

/// <summary>
/// Unit tests for the StaticMessageSource class.
/// </summary>
[TestFixture]
public sealed class StaticMessageSourceTests
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

    [Test]
    [SetUICulture("de-DE")]
    public void GetMessageDefaultsToCurrentUICulture()
    {
        StaticMessageSource msgSource = new StaticMessageSource();
        msgSource.AddMessage("code", CultureInfo.CurrentUICulture, "{0}");
        Assert.That(msgSource.GetMessage("code", "my message"), Is.EqualTo("my message"), "message");

        try
        {
            msgSource.GetMessage("code", CultureInfo.CurrentCulture);
            Assert.Fail("message");
        }
        catch (NoSuchMessageException)
        {
        }
    }

    [Test]
    public void GetMessageCode()
    {
        StaticMessageSource msgSource = new StaticMessageSource();
        msgSource.AddMessage("code", CultureInfo.CurrentUICulture, "{0} {1}");
        Assert.That(msgSource.GetMessage("code", CultureInfo.CurrentUICulture, new object[] { "my", "message" }),
            Is.EqualTo("my message"),
            "message");
    }

    [Test]
    public void StaticMessageSourceToString()
    {
        StaticMessageSource msgSource = new StaticMessageSource();
        msgSource.AddMessage("code1", CultureInfo.CurrentUICulture, "{0} {1}");
        Assert.That(msgSource.ToString(),
            Is.EqualTo("StaticMessageSource : ['code1_" + CultureInfo.CurrentUICulture.Name + "' : '{0} {1}']"));
    }

#if !NETCOREAPP
        [Test]
        public void ApplyResources()
        {
            TestObject value = new TestObject();
            StaticMessageSource msgSource = new StaticMessageSource();
            msgSource.ApplyResources(value, "testObject", CultureInfo.InvariantCulture);
        Assert.That(value.Name, Is.EqualTo("Mark"), "Name property value not applied.");
        Assert.That(value.Age, Is.EqualTo(35), "Age property value not applied.");
        }
#endif

    [Test]
    public void ApplyResourcesWithNullObject()
    {
        TestObject value = new TestObject();
        StaticMessageSource msgSource = new StaticMessageSource();
        msgSource.ApplyResources(null, "testObject", CultureInfo.InvariantCulture);
        Assert.That(value.Name, Is.EqualTo(null));
        Assert.That(value.Age, Is.EqualTo(0));
    }

    [Test]
    public void ApplyResourcesWithNullLookupKey()
    {
        TestObject value = new TestObject();
        StaticMessageSource msgSource = new StaticMessageSource();

        try
        {
            msgSource.ApplyResources(value, null, CultureInfo.InvariantCulture);
            Assert.Fail("ArgumentNullException was expected");
        }
        catch (ArgumentNullException e)
        {
            Assert.That(e, Is.Not.Null);
        }

        Assert.That(value.Name, Is.EqualTo(null));
        Assert.That(value.Age, Is.EqualTo(0));
    }

    [Test]
    public void GetResourceObjectWithCodeAndCulture()
    {
        TestObject value = new TestObject("Rick", 30);
        StaticMessageSource msgSource = new StaticMessageSource();
        msgSource.AddObject("rick", CultureInfo.InstalledUICulture, value);

        TestObject retrieved = (TestObject)
            msgSource.GetResourceObject("rick", CultureInfo.InstalledUICulture);
        Assert.That(retrieved,
            Is.Not.Null,
            "Object previously added to StaticMessageSource was not retrieved " +
            "when using same lookup code and CultureInfo.");
        Assert.That(ReferenceEquals(value, retrieved),
            Is.True,
            "Object returned from StaticMessageSource was not the same one " +
            "that was previously added (it must be).");
    }

    [Test]
    public void GetResourceObjectWithCode()
    {
        TestObject value = new TestObject("Rick", 30);
        StaticMessageSource msgSource = new StaticMessageSource();
        msgSource.AddObject("rick", CultureInfo.CurrentUICulture, value);

        TestObject retrieved = (TestObject)
            msgSource.GetResourceObject("rick");
        Assert.That(retrieved,
            Is.Not.Null,
            "Object previously added to StaticMessageSource was not retrieved " +
            "when using same lookup code.");
        Assert.That(ReferenceEquals(value, retrieved),
            Is.True,
            "Object returned from StaticMessageSource was not the same one " +
            "that was previously added (it must be).");
    }

    [Test]
    public void GetResourceObjectThatAintPreviouslyBeenAddedDoesntYieldAnything()
    {
        StaticMessageSource msgSource = new StaticMessageSource();
        TestObject retrieved = (TestObject)
            msgSource.GetResourceObject("rick");
        Assert.That(retrieved,
            Is.Null,
            "Getting 'some (?) weird object out of an empty StaticMessageSource");
    }

    [Test]
    public void GetResourceObjectWithCodeAssumesCurrentUICulture()
    {
        TestObject value = new TestObject("Rick", 30);
        StaticMessageSource msgSource = new StaticMessageSource();
        msgSource.AddObject("rick", CultureInfo.InvariantCulture, value);

        // assumes object was previously added using CultureInfo.CurrentUICulture
        TestObject retrieved = (TestObject)
            msgSource.GetResourceObject("rick");
        Assert.That(retrieved,
            Is.Null,
            "Object previously added to StaticMessageSource " +
            "(using CultureInfo.InvariantCulture) was (wrongly) retrieved " +
            "when using same lookup code.");
    }
}
