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

using System.Reflection;
using NUnit.Framework;
using Spring.Context.Support;

namespace Spring.Objects.Factory.Attributes;

/// <summary>
/// This class contains tests for RequiredAttributeObjectPostProcessor.
/// </summary>
/// <author>Mark Pollack</author>
[TestFixture]
public class RequiredAttributeObjectPostProcessorTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void WithRequiredPropertyOmitted()
    {
        try
        {
            XmlApplicationContext ctx =
                new XmlApplicationContext(false,
                    "assembly://Spring.Core.Tests/Spring.Objects.Factory.Attributes/RequiredWithOneRequiredPropertyOmitted.xml");
            Assert.Fail("Should have thrown ObjectCreationException");
        }
        catch (ObjectCreationException ex)
        {
            string message = ex.InnerException.Message;
            Assert.IsTrue(message.IndexOf("Property") > -1);
            Assert.IsTrue(message.IndexOf("Age") > -1);
            Assert.IsTrue(message.IndexOf("testObject") > -1);
        }
    }

    [Test]
    public void WithThreeRequiredPropertiesOmitted()
    {
        try
        {
            XmlApplicationContext ctx =
                new XmlApplicationContext(false,
                    "assembly://Spring.Core.Tests/Spring.Objects.Factory.Attributes/RequiredWithThreeRequiredPropertiesOmitted.xml");
            Assert.Fail("Should have thrown ObjectCreationException");
        }
        catch (ObjectCreationException ex)
        {
            string message = ex.InnerException.Message;
            Assert.IsTrue(message.IndexOf("Properties") > -1);
            Assert.IsTrue(message.IndexOf("Age") > -1);
            Assert.IsTrue(message.IndexOf("FavoriteColor") > -1);
            Assert.IsTrue(message.IndexOf("JobTitle") > -1);
            Assert.IsTrue(message.IndexOf("testObject") > -1);
        }
    }

    [Test]
    public void WithOnlyRequiredPropertiesSpecified()
    {
        XmlApplicationContext ctx =
            new XmlApplicationContext(false,
                "assembly://Spring.Core.Tests/Spring.Objects.Factory.Attributes/RequiredWithAllRequiredPropertiesProvided.xml");
        RequiredTestObject to = (RequiredTestObject) ctx.GetObject("testObject");
        Assert.AreEqual(24, to.Age);
        Assert.AreEqual("Blue", to.GetFavoriteColor());
    }

    [Test]
    public void Reflection()
    {
        foreach (PropertyInfo pi in typeof(RequiredTestObject).GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (pi.Name.Equals("Age"))
            {
                object[] attribs = pi.GetCustomAttributes(typeof(RequiredAttribute), true);
                Assert.Greater(attribs.Length, 0);
            }
        }
    }

    [Test]
    public void WithCustomAttribute()
    {
        try
        {
            XmlApplicationContext ctx =
                new XmlApplicationContext(false,
                    "assembly://Spring.Core.Tests/Spring.Objects.Factory.Attributes/RequiredWithCustomAttribute.xml");
            Assert.Fail("Should have thrown ObjectCreationException");
        }
        catch (ObjectCreationException ex)
        {
            string message = ex.InnerException.Message;
            Assert.IsTrue(message.IndexOf("Property") > -1);
            Assert.IsTrue(message.IndexOf("Name") > -1);
            Assert.IsTrue(message.IndexOf("testObject") > -1);
        }
    }
}
