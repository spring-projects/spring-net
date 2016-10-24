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

using System.Collections;
using System.IO;
using System.Text;
using Commons.Collections;
using NUnit.Framework;
using NVelocity.App;
using NVelocity.Runtime;
using NVelocity.Runtime.Resource;
using NVelocity.Runtime.Resource.Loader;
using Spring.Core.IO;

#endregion

namespace Spring.Template.Velocity.Config {
    /// <summary>
    ///  This class contains tests for the template namespace configuration parser
    /// </summary>
    /// <author>Erez Mazor</author>
    [TestFixture]
    public class TemplateNamespaceParserTests : VelocityEngineTestBase {
        #region convinience properties aliases
        private const string PropertyModificationCheck =
            TemplateDefinitionConstants.PropertyResourceLoaderModificationCheckInterval;
        private const string PropertyResourceLoaderCachce =
            TemplateDefinitionConstants.PropertyResourceLoaderCaching;
        #endregion

        #region test constants
        private const int DEFAULT_CACHE_SIZE = 200;
        private const int DEFAULT_MOD_CHECK = 30;
        private const bool DEFAULT_CACHE_FLAG = true;
        #endregion

        /// <summary>
        /// Test a full file-base configuration
        /// </summary>
        [Test]
        public void TestFileBasedConfig() {
            VelocityEngine velocityEngine = appContext.GetObject("cnFileVelocityEngine") as VelocityEngine;
            Assert.IsNotNull(velocityEngine, "velocity engine is null");
            Assert.AreEqual(VelocityConstants.File, getSingleProperty(velocityEngine, RuntimeConstants.RESOURCE_LOADER), "incorrect resource loader");
            Assert.AreEqual(TemplateDefinitionConstants.FileResourceLoaderClass, getSingleProperty(velocityEngine,
                TemplateNamespaceParser.getResourceLoaderProperty(VelocityConstants.File, VelocityConstants.Class)), "incorrect resource loader type");
            Assert.AreEqual(new string[]{"Template/Velocity/", "Template/"}, velocityEngine.GetProperty(
                TemplateNamespaceParser.getResourceLoaderProperty(VelocityConstants.File, VelocityConstants.Path)), "incorrect resource loader path");
            Assert.AreEqual(DEFAULT_CACHE_SIZE, velocityEngine.GetProperty(RuntimeConstants.RESOURCE_MANAGER_DEFAULTCACHE_SIZE), "incorrect default cache size");
            Assert.AreEqual(DEFAULT_MOD_CHECK, velocityEngine.GetProperty(
                                    VelocityConstants.File + VelocityConstants.Separator + PropertyModificationCheck), "incorrect mod check interval");
            Assert.AreEqual(DEFAULT_CACHE_FLAG, velocityEngine.GetProperty(VelocityConstants.File + VelocityConstants.Separator + PropertyResourceLoaderCachce),
                            "incorrect caching flag");

            AssertMergedValue(velocityEngine, "SimpleTemplate.vm");
        }

        /// <summary>
        /// Test a full aaembly-based configuration
        /// </summary>
        [Test]
        public void TestAssemblyBasedConfig() {
            VelocityEngine velocityEngine = appContext.GetObject("cnAssemblyVelocityEngine") as VelocityEngine;
            Assert.IsNotNull(velocityEngine, "velocity engine is null");
            Assert.AreEqual(VelocityConstants.Assembly, getSingleProperty(velocityEngine, RuntimeConstants.RESOURCE_LOADER), "incorrect resource loader");
            Assert.AreEqual(TemplateDefinitionConstants.AssemblyResourceLoaderClass, getSingleProperty(velocityEngine,
                TemplateNamespaceParser.getResourceLoaderProperty(VelocityConstants.Assembly, VelocityConstants.Class)), "incorrect resource loader type");
            Assert.AreEqual("Spring.Template.Velocity.Tests", getSingleProperty(velocityEngine,
                TemplateNamespaceParser.getResourceLoaderProperty(VelocityConstants.Assembly,VelocityConstants.Assembly)), "incorrect resource loader path");
            Assert.AreEqual(DEFAULT_CACHE_SIZE, velocityEngine.GetProperty(RuntimeConstants.RESOURCE_MANAGER_DEFAULTCACHE_SIZE), "incorrect default cache size");
            Assert.AreEqual(DEFAULT_CACHE_FLAG, velocityEngine.GetProperty(VelocityConstants.Assembly + VelocityConstants.Separator + PropertyResourceLoaderCachce),
                            "incorrect caching flag");
            AssertMergedValue(velocityEngine, "Spring.Template.Velocity.SimpleTemplate.vm");
        }

        /// <summary>
        /// Test a full aaembly-based configuration
        /// </summary>
        [Test]
        public void TestSpringBasedConfig() {
            VelocityEngine velocityEngine = appContext.GetObject("cnSpringVelocityEngine") as VelocityEngine;
            Assert.IsNotNull(velocityEngine, "velocity engine is null");
            const string PropertySpring = TemplateDefinitionConstants.Spring;
            Assert.AreEqual(PropertySpring, getSingleProperty(velocityEngine, RuntimeConstants.RESOURCE_LOADER), "incorrect resource loader");
            Assert.AreEqual(TemplateDefinitionConstants.SpringResourceLoaderClass, getSingleProperty(velocityEngine,
                TemplateNamespaceParser.getResourceLoaderProperty(PropertySpring, VelocityConstants.Class)), "incorrect resource loader type");
            // no way to test the path property other than trying to actually perform a merge (it is set in velocity engine as an application attribute which is not exposed)
            Assert.AreEqual(DEFAULT_CACHE_SIZE, velocityEngine.GetProperty(RuntimeConstants.RESOURCE_MANAGER_DEFAULTCACHE_SIZE), "incorrect default cache size");
            Assert.AreEqual(DEFAULT_CACHE_FLAG, velocityEngine.GetProperty(PropertySpring + VelocityConstants.Separator + PropertyResourceLoaderCachce),
                            "incorrect caching flag");

            AssertMergedValue(velocityEngine, "SimpleTemplate.vm");
            AssertMergedValue(velocityEngine, "EmbeddedTemplate.vm");
        }

        /// <summary>
        /// Test using an external configuration properties definition
        /// </summary>
        [Test]
        public void TestGlobalConfig() {
            VelocityEngine velocityEngine = appContext.GetObject("cnVelocityEngineConfig") as VelocityEngine;
            Assert.IsNotNull(velocityEngine, "velocity engine is null");

            string classProp = TemplateNamespaceParser.getResourceLoaderProperty(VelocityConstants.Assembly, VelocityConstants.Class);
            string descProp = TemplateNamespaceParser.getResourceLoaderProperty(VelocityConstants.Assembly, VelocityConstants.Description);

            Assert.AreEqual(VelocityConstants.Assembly, getSingleProperty(velocityEngine, RuntimeConstants.RESOURCE_LOADER), "incorrect resource loader");
            Assert.AreEqual("NVelocity.Runtime.Resource.Loader.AssemblyResourceLoader", 
                getSingleProperty(velocityEngine,classProp), "incorrect resource loader type");
            Assert.AreEqual("TestDescription", getSingleProperty(velocityEngine, descProp), "incorrect description");
        }

        /// <summary>
        /// Test using local config.
        /// </summary>
        [Test]
        public void TestLocalConfig() {
            VelocityEngine velocityEngine = appContext.GetObject("cnVelocityEngineLocalConfig") as VelocityEngine;
            Assert.IsNotNull(velocityEngine, "velocity engine is null");

            Assert.AreEqual(Encoding.UTF8.WebName.ToUpper(), getSingleProperty(velocityEngine, RuntimeConstants.INPUT_ENCODING), "incorrect input encoding value");
            Assert.AreEqual(TEST_VALUE, getSingleProperty(velocityEngine, "myproperty.mysubproperty"), "incorrect custom property value");
        }

        /// <summary>
        /// Test a custom resource loader definition
        /// </summary>
        [Test]
        public void TestCustomResourceLoader() {
            VelocityEngine velocityEngine = appContext.GetObject("cnVelocityEngingCustomResourceLoader") as VelocityEngine;
            Assert.IsNotNull(velocityEngine, "velocity engine is null");
            const string PropertyMyResourceLoader = "myResourceLoader";
            string classProp = TemplateNamespaceParser.getResourceLoaderProperty(PropertyMyResourceLoader, VelocityConstants.Class);
            string descProp = TemplateNamespaceParser.getResourceLoaderProperty(PropertyMyResourceLoader, VelocityConstants.Description);

            Assert.AreEqual(PropertyMyResourceLoader, getSingleProperty(velocityEngine, RuntimeConstants.RESOURCE_LOADER), "incorrect resource loader");
            
            Assert.AreEqual("Spring.Template.Velocity.Config.TestCustomResourceLoader; Spring.Template.Velocity.Tests",
                getSingleProperty(velocityEngine, classProp), "incorrect resource loader type");
            Assert.AreEqual(PropertyMyResourceLoader, getSingleProperty(velocityEngine, RuntimeConstants.RESOURCE_LOADER), "incorrect resource loader");
            Assert.AreEqual("A custom resource loader",
                getSingleProperty(velocityEngine, descProp), "incorrect resource loader description");
            Assert.AreEqual("Template/Velocity/", getSingleProperty(velocityEngine,
                TemplateNamespaceParser.getResourceLoaderProperty(PropertyMyResourceLoader, VelocityConstants.Path)), "incorrect resource loader path");

            AssertMergedValue(velocityEngine, "Template/Velocity/SimpleTemplate.vm");
        }

        #region internal test methods
        /// <summary>
        /// Grab a single property from (if it's an array return the first value) from the velocityEngine
        /// </summary>
        private string getSingleProperty(VelocityEngine velocityEngine, string prop) {
            object property = velocityEngine.GetProperty(prop);
            if (property is ArrayList) {
                ArrayList list = (ArrayList)property;
                return (string)list[0];
            }
            return (string)property;
        }
        #endregion
    }

    #region test classes
    /// <summary>
    /// Test class for a custom resource loader class
    /// </summary>
    internal sealed class TestCustomResourceLoader : ResourceLoader {
        public override void Init(ExtendedProperties configuration) {
        }

        public override Stream GetResourceStream(string source) {
            return new ConfigurableResourceLoader().GetResource(source).InputStream;
        }

        public override bool IsSourceModified(Resource resource) {
            return false;
        }

        public override long GetLastModified(Resource resource) {
            return resource.LastModified;
        }
    }
    #endregion
}