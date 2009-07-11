#region License

/*
 * Copyright © 2002-2009 the original author or authors.
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
using System.Collections;
using System.Text;
using NUnit.Framework;
using NVelocity.App;
using Spring.Context.Support;
using Spring.Objects.Factory.Xml;

#endregion

namespace Spring.Template.Velocity.Tests.Template.Velocity {
    /// <summary>
    /// This class contains tests for VelocityEngineFactoryObject
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <author>Erez Mazor</author>
    [TestFixture]
    public class VelocityEngineFactoryObjectTests {
        private const string TEST_VALUE = "TEST_VALUE";
        private XmlApplicationContext appContext;
        private readonly Hashtable model = new Hashtable();

        [SetUp]
        public void Setup() {
            appContext = new XmlApplicationContext(false,
                                                                          ReadOnlyXmlTestResource.GetFilePath(
                                                                              "VelocityEngineFactoryObjectTests.xml",
                                                                              typeof(VelocityEngineFactoryObjectTests)));
            model.Add("var1", TEST_VALUE);
        }

        [TearDown]
        public void TearDown() {
            appContext.Dispose();
            model.Clear();
        }

        /// <summary>
        /// Test the assemblyBasedVelocityEngine bean configuration from VelocityEngineFactoryObjectTests.xml
        /// </summary>
        [Test]
        public void TestMergeUsingAssembly() {
            VelocityEngine velocityEngine = appContext.GetObject("assemblyBasedVelocityEngine") as VelocityEngine;
            Assert.IsNotNull(velocityEngine, "velocityEngine is null");
            string mergedTemplate = VelocityEngineUtils.MergeTemplateIntostring(velocityEngine, "Spring.Template.Velocity.Tests.Template.Velocity.SimpleTemplate.vm", Encoding.UTF8.WebName, model);
            Assert.AreEqual(string.Format("value={0}", TEST_VALUE), mergedTemplate);
        }

        /// <summary>
        /// Test the fileBasedVelocityEngine bean configuration from VelocityEngineFactoryObjectTests.xml
        /// </summary>
        [Test]
        public void TestMergeUsingFile() {
            VelocityEngine velocityEngine = appContext.GetObject("fileBasedVelocityEngine") as VelocityEngine;
            Assert.IsNotNull(velocityEngine, "velocityEngine is null");
            string mergedTemplate = VelocityEngineUtils.MergeTemplateIntostring(
                velocityEngine, "Template/Velocity/SimpleTemplate.vm", Encoding.UTF8.WebName, model);
            Assert.AreEqual(string.Format("value={0}", TEST_VALUE), mergedTemplate);
        }

        /// <summary>
        /// Test the fileBasedVelocityEngine bean configuration from VelocityEngineFactoryObjectTests.xml
        /// </summary>
        [Test]
        public void TestMergeUsingCustomNamespaceDefinition() {
            VelocityEngine velocityEngine =
                appContext.GetObject("customNamespaceVelocityTemplate") as VelocityEngine;
            Assert.IsNotNull(velocityEngine, "velocityEngine is null");
            string mergedTemplate = VelocityEngineUtils.MergeTemplateIntostring(velocityEngine, "SimpleTemplate.vm",
                                                                                Encoding.UTF8.WebName, model);
            Assert.AreEqual(string.Format("value={0}", "TEST_VALUE"), mergedTemplate);
        }

        /// <summary>
        /// Test using definition of ResourceLoaderPath (file-based configuration) referencing just the template name
        /// </summary>
        [Test]
        public void TestMergeUsingResourceLoaderPath() {
            VelocityEngine velocityEngine =
                appContext.GetObject("pathBasedVelocityEngine") as VelocityEngine;
            Assert.IsNotNull(velocityEngine, "velocityEngine is null");
            string mergedTemplate = VelocityEngineUtils.MergeTemplateIntostring(velocityEngine, "SimpleTemplate.vm",
                                                                                Encoding.UTF8.WebName, model);
            Assert.AreEqual(string.Format("value={0}", "TEST_VALUE"), mergedTemplate);
        }

        /// <summary>
        /// Test using a custom properties file (assembly-based configuration)
        /// </summary>
        [Test]
        public void TestMergeUsingConfigPropertiesFile() {
            VelocityEngine velocityEngine =
                appContext.GetObject("propertiesFileBasedVelocityEngine") as VelocityEngine;
            Assert.IsNotNull(velocityEngine, "velocityEngine is null");
            string mergedTemplate = VelocityEngineUtils.MergeTemplateIntostring(velocityEngine,
                                                                                "Spring.Template.Velocity.Tests.Template.Velocity.SimpleTemplate.vm",
                                                                                Encoding.UTF8.WebName, model);
            Assert.AreEqual(string.Format("value={0}", "TEST_VALUE"), mergedTemplate);
        }

        /// <summary>
        /// Test using spring resource loader
        /// </summary>
        [Test]
        public void TestMergeUsingSpringResourceLoader() {
            VelocityEngine velocityEngine =
                appContext.GetObject("springResourceLoaderBasedVelocityEngine") as VelocityEngine;
            Assert.IsNotNull(velocityEngine, "velocityEngine is null");
            string mergedTemplate = VelocityEngineUtils.MergeTemplateIntostring(velocityEngine, "SimpleTemplate.vm",
                                                                                Encoding.UTF8.WebName, model);
            Assert.AreEqual(string.Format("value={0}", "TEST_VALUE"), mergedTemplate);
        }

        /// <summary>
        /// Test using invalid configuration
        /// </summary>
        [Test]
        public void TestInvalidConfiguration() {
            VelocityEngineFactory velocityEngineFactory = new VelocityEngineFactory();
            velocityEngineFactory.PreferFileSystemAccess = false;
            VelocityEngine velocityEngine = null;
            try {
                velocityEngineFactory.CreateVelocityEngine();
                throw new TestException(
                    "Should not be able to construct VelocityEngineFactory with SpringResourceLoader and no path");
            } catch (ArgumentException) {
                Assert.IsNull(velocityEngine, "velocityEngine should be null");
            }
        }
    }
}