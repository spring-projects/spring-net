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

using System.Collections;
using System.Text;
using NUnit.Framework;
using NVelocity.App;
using Spring.Context.Support;
using Spring.Objects.Factory.Xml;

namespace Spring.Template.Velocity;

/// <summary>
/// Base class for Velocity engine tests.
/// </summary>
public class VelocityEngineTestBase
{
    /// <summary>
    /// Shared application context instance.
    /// </summary>
    protected XmlApplicationContext appContext;

    /// <summary>
    /// Model used in templating.
    /// </summary>
    protected readonly Hashtable model = new Hashtable();

    /// <summary>
    /// Simple test value.
    /// </summary>
    protected const string TEST_VALUE = "TEST_VALUE";

    /// <summary>
    /// Test setup.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        appContext = new XmlApplicationContext(false,
            ReadOnlyXmlTestResource.GetFilePath(
                "VelocityEngineFactoryObjectTests.xml",
                typeof(VelocityEngineFactoryObjectTests)));
        model.Add("var1", TEST_VALUE);
    }

    /// <summary>
    /// Test cleanup.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        appContext.Dispose();
        model.Clear();
    }

    /// <summary>
    /// Basic method for asserting the expected TEST_VALUE in the merged template
    /// </summary>
    protected void AssertMergedValue(VelocityEngine velocityEngine, string template)
    {
        string mergedTemplate = VelocityEngineUtils.MergeTemplateIntoString(velocityEngine, template, Encoding.UTF8.WebName, model);
        Assert.AreEqual(string.Format("value={0}", TEST_VALUE), mergedTemplate);
    }
}
