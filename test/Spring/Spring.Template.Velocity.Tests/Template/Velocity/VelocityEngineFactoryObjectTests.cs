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

#region Imports

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NVelocity.App;
using NVelocity.Exception;

#endregion

namespace Spring.Template.Velocity {

    /// <summary>
    /// This class contains tests for VelocityEngineFactoryObject
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <author>Erez Mazor</author>
    [TestFixture]
    public class VelocityEngineFactoryObjectTests : VelocityEngineTestBase {
        /// <summary>
        /// Test the assemblyBasedVelocityEngine bean configuration from VelocityEngineFactoryObjectTests.xml
        /// </summary>
        [Test]
        public void TestMergeUsingAssembly() {
            VelocityEngine velocityEngine = appContext.GetObject("assemblyBasedVelocityEngine") as VelocityEngine;
            Assert.IsNotNull(velocityEngine, "velocityEngine is null");
            AssertMergedValue(velocityEngine, "Spring.Template.Velocity.SimpleTemplate.vm");
         }

        /// <summary>
        /// Test the fileBasedVelocityEngine bean configuration from VelocityEngineFactoryObjectTests.xml
        /// </summary>
        [Test]
        public void TestMergeUsingFile() {
            VelocityEngine velocityEngine = appContext.GetObject("fileBasedVelocityEngine") as VelocityEngine;
            Assert.IsNotNull(velocityEngine, "velocityEngine is null");
            AssertMergedValue(velocityEngine, "Template/Velocity/SimpleTemplate.vm");

            try {
                VelocityEngineUtils.MergeTemplateIntoString(velocityEngine, "NoneExistingFile", Encoding.UTF8.WebName, model);
                throw new TestException(
                    "Merge using non existing file should throw exception");
            } catch (Exception ex) {
                Assert.IsTrue(ex is VelocityException, "Illegal merge should throw VelocityException");
            }
        }

        /// <summary>
        /// Test the fileBasedVelocityEngine bean configuration from VelocityEngineFactoryObjectTests.xml
        /// </summary>
        [Test]
        public void TestMergeUsingCustomNamespaceDefinition() {
            VelocityEngine velocityEngine =
                appContext.GetObject("cnFileVelocityEngine") as VelocityEngine;
            Assert.IsNotNull(velocityEngine, "velocityEngine is null");
            AssertMergedValue(velocityEngine, "SimpleTemplate.vm");
        }

        /// <summary>
        /// Test using definition of ResourceLoaderPath (file-based configuration) referencing just the template name
        /// </summary>
        [Test]
        public void TestMergeUsingResourceLoaderPath() {
            VelocityEngine velocityEngine =
                appContext.GetObject("pathBasedVelocityEngine") as VelocityEngine;
            Assert.IsNotNull(velocityEngine, "velocityEngine is null");
            AssertMergedValue(velocityEngine, "SimpleTemplate.vm");
        }  
        
        /// <summary>
        /// Test using definition of ResourceLoaderPath (file-based configuration) falling back from velocity
        /// file-base to spring-based (prefer-file-system-access one but resource path is string compliant)
        /// </summary>
        [Test]
        public void TestMergeUsingResourceLoaderPathFallback() {
            VelocityEngine velocityEngine =
                appContext.GetObject("springFallbackVelocityEngine") as VelocityEngine;
            Assert.IsNotNull(velocityEngine, "velocityEngine is null");
            AssertMergedValue(velocityEngine, "SimpleTemplate.vm");
        }

        /// <summary>
        /// Test using a custom properties file (assembly-based configuration)
        /// </summary>
        [Test]
        public void TestMergeUsingConfigPropertiesFile() {
            VelocityEngine velocityEngine =
                appContext.GetObject("propertiesFileBasedVelocityEngine") as VelocityEngine;
            Assert.IsNotNull(velocityEngine, "velocityEngine is null");
            AssertMergedValue(velocityEngine, "Spring.Template.Velocity.SimpleTemplate.vm");
        }

        /// <summary>
        /// Test using spring resource loader
        /// </summary>
        [Test]
        public void TestMergeUsingSpringResourceLoader() {
            VelocityEngine velocityEngine =
                appContext.GetObject("springResourceLoaderBasedVelocityEngine") as VelocityEngine;
            Assert.IsNotNull(velocityEngine, "velocityEngine is null");
            AssertMergedValue(velocityEngine, "SimpleTemplate.vm");
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

            // no resource loader with spring
            velocityEngineFactory = new VelocityEngineFactory();
            velocityEngineFactory.ResourceLoader = null;
            velocityEngineFactory.PreferFileSystemAccess = false;
            try {
                velocityEngineFactory.CreateVelocityEngine();
                throw new TestException(
                    "Should not be able to construct VelocityEngineFactory with null ResourceLoader");
            } catch (ArgumentException) {
                Assert.IsNull(velocityEngine, "velocityEngine should be null");
            }

            // no resource loader path with spring
            velocityEngineFactory = new VelocityEngineFactory();
            velocityEngineFactory.ResourceLoaderPaths = new List<string>();
            velocityEngineFactory.PreferFileSystemAccess = false;
            try {
                velocityEngineFactory.CreateVelocityEngine();
                throw new TestException(
                    "Should not be able to construct VelocityEngineFactory with empty resource loader path list");
            } catch (ArgumentException) {
                Assert.IsNull(velocityEngine, "velocityEngine should be null");
            }
        }

        /// <summary>
        /// Test engine's logging capabilities.
        /// </summary>
        [Test]
        public void TestLogging(){
            VelocityEngine velocityEngine = new VelocityEngineFactoryObject().CreateVelocityEngine();
            velocityEngine.Info("test");
            velocityEngine.Debug("test");
            velocityEngine.Warn("test");
            velocityEngine.Error("test");
        }


    }
}