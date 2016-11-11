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

using System.Configuration;
using System.Reflection;
using System.Web;
using Common.Logging;
using NUnit.Framework;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;

#endregion

namespace Spring.Context.Support
{
    /// <summary>
    ///
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class HttpApplicationConfigurerTests
    {
        [OneTimeSetUp]
        public void SetUpFixture()
        {
            LogManager.Adapter = new Common.Logging.Simple.TraceLoggerFactoryAdapter();
        }

        [SetUp]
        public void SetUp()
        {
            HttpApplicationConfigurer h1 = new HttpApplicationConfigurer();
            h1.ApplicationTemplate = null;
            h1.ModuleTemplates.Clear();
        }

        /// <summary>
        /// All <see cref="HttpApplicationConfigurer"/> instances share the same underlying ApplicationTemplate
        /// </summary>
        [Test]
        public void ShareApplicationTemplate()
        {
            RootObjectDefinition rod = new RootObjectDefinition();

            HttpApplicationConfigurer h1 = new HttpApplicationConfigurer();
            h1.ApplicationTemplate = rod;
            Assert.AreEqual(rod, h1.ApplicationTemplate);

            // h2 returns same template as h1
            HttpApplicationConfigurer h2 = new HttpApplicationConfigurer();
            Assert.AreEqual(rod, h2.ApplicationTemplate);
            Assert.AreEqual(h2.ApplicationTemplate, h1.ApplicationTemplate);

            // allows overriding
            rod = new RootObjectDefinition();
            h2.ApplicationTemplate = rod;
            Assert.AreEqual(rod, h1.ApplicationTemplate);
        }

        /// <summary>
        /// All <see cref="HttpApplicationConfigurer"/> instances share the same underlying ModuleTemplates table
        /// </summary>
        [Test]
        public void ShareModuleTemplates()
        {
            // is never null
            HttpApplicationConfigurer h1;
            HttpApplicationConfigurer h2;
            RootObjectDefinition rod;

            // is shared
            h1 = new HttpApplicationConfigurer();
            Assert.IsNotNull(h1.ModuleTemplates);
            h2 = new HttpApplicationConfigurer();
            Assert.AreEqual(h1.ModuleTemplates, h2.ModuleTemplates);

            // allows overriding
            rod = new RootObjectDefinition();
            h1.ModuleTemplates.Add("test", rod);
            Assert.AreEqual(rod, h1.ModuleTemplates["test"]);
            h2.ModuleTemplates.Add("test", rod);
            Assert.AreEqual(1, h2.ModuleTemplates.Count);
            Assert.AreEqual(rod, h2.ModuleTemplates["test"]);
        }

        [Test]
        public void ConfiguresApplicationAndModulesFromTemplate()
        {
            StaticApplicationContext appContext = CreateTestContext();

            HttpApplicationConfigurer h;
            RootObjectDefinition rod;

            h = new HttpApplicationConfigurer();
            rod = new RootObjectDefinition();
            rod.PropertyValues.Add("TestObject", new RuntimeObjectReference("testObject"));
            h.ApplicationTemplate = rod;
            rod = new RootObjectDefinition();
            rod.PropertyValues.Add("TestObject", new RuntimeObjectReference("testObject1"));
            h.ModuleTemplates.Add("TestModule1", rod);
            rod = new RootObjectDefinition();
            rod.PropertyValues.Add("TestObject", new RuntimeObjectReference("testObject2"));
            h.ModuleTemplates.Add("TestModule2", rod);

            TestModule m1 = new TestModule();
            TestModule m2 = new TestModule();

            TestApplication appObject = new TestApplication(new ModuleEntry[]
                                                                {
                                                                    new ModuleEntry("TestModule1", m1)
                                                                    , new ModuleEntry("TestModule2", m2),
                                                                });
            HttpApplicationConfigurer.Configure(appContext, appObject);
            // app configured
            Assert.AreEqual(appContext.GetObject("testObject"), appObject.TestObject);
            // modules configured
            Assert.AreEqual(appContext.GetObject("testObject1"), m1.TestObject);
            Assert.AreEqual(appContext.GetObject("testObject2"), m2.TestObject);
        }

        [Test]
        public void ThrowsOnUnapplicableModuleTemplate()
        {
            StaticApplicationContext appContext = CreateTestContext();

            HttpApplicationConfigurer h;
            RootObjectDefinition rod;

            h = new HttpApplicationConfigurer();
            rod = new RootObjectDefinition();
            rod.PropertyValues.Add("TestObject", new RuntimeObjectReference("testObject1"));
            h.ModuleTemplates.Add("TestModule1", rod);

            TestApplication appObject = new TestApplication(null);
            Assert.Throws<ConfigurationErrorsException>(() => HttpApplicationConfigurer.Configure(appContext, appObject));
        }

        ////////////////////////////////////////////////////

        public class GrandParentHttpModule : IHttpModule
        {

            private string[] _grandParentProperty;
            public string[] GrandParentProperty
            {
                get { return _grandParentProperty; }
                set
                {
                    _grandParentProperty = value;
                }
            }


            public void Dispose()
            {

            }

            public void Init(HttpApplication context)
            {

            }
        }

        public class ParentHttpModule : GrandParentHttpModule
        {
            private string[] _parentProperty;
            public string[] ParentProperty
            {
                get { return _parentProperty; }
                set
                {
                    _parentProperty = value;
                }
            }
            
        }
   
        [Test]
        public void ConfigureUsingXmlApplicationContext_CanMergePropertyValues()
        {
            XmlApplicationContext appContext = new XmlApplicationContext(false, ReadOnlyXmlTestResource.GetFilePath("HttpApplicationConfigurerMergablePropertiesTests.xml", typeof(HttpApplicationConfigurerTests)));

            ParentHttpModule module = new ParentHttpModule();

            TestApplication appObject = new TestApplication(new ModuleEntry[]
                                                                {
                                                                    new ModuleEntry("DirectoryServicesAuthentication", module)
                                                                });
            HttpApplicationConfigurer.Configure(appContext, appObject);
            
            //base class property has carried through successfully
            Assert.Contains("GrandParentValue1", module.GrandParentProperty);

            //parent property values remain in the final instance
            Assert.Contains("ParentValue1", module.ParentProperty);
            Assert.Contains("ParentValue2", module.ParentProperty);
            Assert.Contains("ParentValue3", module.ParentProperty);
            Assert.Contains("ParentValue4", module.ParentProperty);

            //the new additional value has been merged into the property
            Assert.Contains("MergedValueToFind", module.ParentProperty);
        }

        [Test]
        public void ConfigureUsingXmlApplicationContext()
        {
            XmlApplicationContext appContext = new XmlApplicationContext(false, ReadOnlyXmlTestResource.GetFilePath("HttpApplicationConfigurerTests.xml", typeof(HttpApplicationConfigurerTests)));

            TestModule m1 = new TestModule();
            TestModule m2 = new TestModule();

            TestApplication appObject = new TestApplication(new ModuleEntry[]
                                                                {
                                                                    new ModuleEntry("TestModule1", m1)
                                                                    , new ModuleEntry("TestModule2", m2),
                                                                });
            HttpApplicationConfigurer.Configure(appContext, appObject);
            // app configured
            Assert.AreEqual(appContext.GetObject("testObject"), appObject.TestObject);
            // modules configured
            Assert.AreEqual(appContext.GetObject("testObject1"), m1.TestObject);
            Assert.AreEqual(null, m2.TestObject);
        }

        private static StaticApplicationContext CreateTestContext()
        {
            object testObject;
            StaticApplicationContext appContext = new StaticApplicationContext();
            appContext.RegisterSingleton("testObject", typeof(object), null);
            appContext.RegisterSingleton("testObject1", typeof(object), null);
            appContext.RegisterSingleton("testObject2", typeof(object), null);
            appContext.Refresh();
            testObject = appContext.GetObject("testObject");
            Assert.IsNotNull(testObject);
            return appContext;
        }

        #region Test Classes

        public class ModuleEntry
        {
            // Fields
            public readonly string Name;
            public readonly IHttpModule Module;

            public ModuleEntry(string name, IHttpModule module)
            {
                Name = name;
                Module = module;
            }
        }

        public class TestApplication : HttpApplication
        {
            private static readonly MethodInfo miAddModule =
                typeof(HttpModuleCollection).GetMethod("AddModule", BindingFlags.Instance | BindingFlags.NonPublic);

            private object testObject;

            public TestApplication(ModuleEntry[] testModule)
            {
                HttpModuleCollection modules = this.Modules;
                if (testModule != null)
                {
                    ModuleEntry moduleEntry = null;
                    for (int i = 0; i < testModule.Length; i++)
                    {
                        moduleEntry = testModule[i];
                        miAddModule.Invoke(modules, new object[] { moduleEntry.Name, moduleEntry.Module });
                    }
                }
            }

            public object TestObject
            {
                get { return this.testObject; }
                set { this.testObject = value; }
            }
        }

        public class TestModule : IHttpModule
        {
            private object testObject;

            public object TestObject
            {
                get { return this.testObject; }
                set { this.testObject = value; }
            }

            public void Init(HttpApplication context)
            {
            }

            public void Dispose()
            {
            }
        }

        #endregion
    }
}