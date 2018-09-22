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

using System;
using System.Collections;

using FakeItEasy;

using NUnit.Framework;

using Spring.Context.Support;
using Spring.Objects.Factory.Support;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// This class contains tests for
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class VariablePlaceholderConfigurerTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void ThrowsOnMissingVariableSources()
        {
            StaticApplicationContext ac = new StaticApplicationContext();
            VariablePlaceholderConfigurer vphc = new VariablePlaceholderConfigurer();

            try
            {
                vphc.PostProcessObjectFactory(ac.ObjectFactory);
                Assert.Fail("Expected ArgumentException not thrown.");
            }
            catch (ArgumentException)
            {
            }
        }


        [Test]
        public void ThrowsOnInvalidVariableSourcesElement()
        {
            StaticApplicationContext ac = new StaticApplicationContext();
            VariablePlaceholderConfigurer vphc = new VariablePlaceholderConfigurer();
            vphc.VariableSources = new ArrayList(new object[] { new object() });

            try
            {
                vphc.PostProcessObjectFactory(ac.ObjectFactory);
                Assert.Fail("Expected ArgumentException not thrown.");
            }
            catch (ArgumentException)
            {
            }
        }


        [Test]
        public void SunnyDay()
        {
            StaticApplicationContext ac = new StaticApplicationContext();

            MutablePropertyValues pvs = new MutablePropertyValues();
            pvs.Add("age", "${maxResults}");
            pvs.Add("name", "${name}");
            ac.RegisterSingleton("tb1", typeof(TestObject), pvs);

            IList variableSources = new ArrayList();
            CommandLineArgsVariableSource vs1 = new CommandLineArgsVariableSource(
                new string[] { "program.exe", "file.txt", "/name:Aleks Seovic", "/framework:Spring.NET" });
            variableSources.Add(vs1);

            ConfigSectionVariableSource vs2 = new ConfigSectionVariableSource();
            vs2.SectionName = "DaoConfiguration";
            variableSources.Add(vs2);


            pvs = new MutablePropertyValues();
            pvs.Add("VariableSources", variableSources);

            ac.RegisterSingleton("configurer", typeof(VariablePlaceholderConfigurer), pvs);
            ac.Refresh();

            TestObject tb1 = (TestObject)ac.GetObject("tb1");
            Assert.AreEqual(1000, tb1.Age);
            Assert.AreEqual("Aleks Seovic", tb1.Name);

        }

        [Test]
        public void UsesCustomVariablePrefixSuffix()
        {
            StaticApplicationContext ac = new StaticApplicationContext();

            MutablePropertyValues pvs = new MutablePropertyValues();
            pvs.Add("age", "%[maxResults]%");
            pvs.Add("name", "%[name]%");
            ac.RegisterSingleton("tb1", typeof(TestObject), pvs);

            IList variableSources = new ArrayList();
            variableSources.Add(new DictionaryVariableSource(new string[] { "maxResults", "35", "name", "Erich" }));


            pvs = new MutablePropertyValues();
            pvs.Add("VariableSources", variableSources);
            pvs.Add("PlaceholderPrefix", "%[");
            pvs.Add("PlaceholderSuffix", "]%");

            ac.RegisterSingleton("configurer", typeof(VariablePlaceholderConfigurer), pvs);
            ac.Refresh();

            TestObject tb1 = (TestObject)ac.GetObject("tb1");
            Assert.AreEqual(35, tb1.Age);
            Assert.AreEqual("Erich", tb1.Name);
        }

        [Test]
        public void MultiResolution()
        {
            DefaultListableObjectFactory of = new DefaultListableObjectFactory();
            MutablePropertyValues pvs = new MutablePropertyValues();
            pvs.Add("Greeting", "Hello ${firstname} ${lastname}!");
            of.RegisterObjectDefinition("tb1", new RootObjectDefinition("typename", null, pvs));

            IList variableSources = new ArrayList();
            variableSources.Add(new DictionaryVariableSource(new string[] { "firstname", "FirstName" }));
            variableSources.Add(new DictionaryVariableSource(new string[] { "lastname", "LastName" }));
            VariablePlaceholderConfigurer vphc = new VariablePlaceholderConfigurer(variableSources);
            vphc.PostProcessObjectFactory(of);

            RootObjectDefinition rod = (RootObjectDefinition)of.GetObjectDefinition("tb1");
            Assert.AreEqual("Hello FirstName LastName!", rod.PropertyValues.GetPropertyValue("Greeting").Value);
        }

        [Test]
        public void NestedResolution()
        {
            DefaultListableObjectFactory of = new DefaultListableObjectFactory();
            MutablePropertyValues pvs = new MutablePropertyValues();
            pvs.Add("NameProperty", "${name}");
            of.RegisterObjectDefinition("tb1", new RootObjectDefinition("typename", null, pvs));

            IList variableSources = new ArrayList();
            variableSources.Add(new DictionaryVariableSource(new string[] { "name", "${nickname}" }));
            variableSources.Add(new DictionaryVariableSource(new string[] { "nickname", "nickname-value" }));
            VariablePlaceholderConfigurer vphc = new VariablePlaceholderConfigurer(variableSources);
            vphc.PostProcessObjectFactory(of);

            RootObjectDefinition rod = (RootObjectDefinition)of.GetObjectDefinition("tb1");
            Assert.AreEqual("nickname-value", rod.PropertyValues.GetPropertyValue("NameProperty").Value);
        }

        [Test]
        public void ChainedResolution()
        {
            StaticApplicationContext ac = new StaticApplicationContext();

            MutablePropertyValues pvs = new MutablePropertyValues();
            pvs.Add("name", "${name}");
            pvs.Add("nickname", "${nickname}");
            ac.RegisterSingleton("tb1", typeof(TestObject), pvs);

            IList variableSources = new ArrayList();
            variableSources.Add(new DictionaryVariableSource(new string[] { "name", "name-value" }));
            variableSources.Add(new DictionaryVariableSource(new string[] { "nickname", "nickname-value" }));
            VariablePlaceholderConfigurer vphc = new VariablePlaceholderConfigurer(variableSources);
            ac.AddObjectFactoryPostProcessor(vphc);
            ac.Refresh();

            TestObject tb1 = (TestObject)ac.GetObject("tb1");
            Assert.AreEqual("name-value", tb1.Name);
            Assert.AreEqual("nickname-value", tb1.Nickname);
        }

        [Test]
        public void ChainedResolutionWithNullValues()
        {
            DefaultListableObjectFactory of = new DefaultListableObjectFactory();

            MutablePropertyValues pvs = new MutablePropertyValues();
            pvs.Add("NameProperty", "${name}");
            pvs.Add("NickNameProperty", "${nickname}");
            of.RegisterObjectDefinition("tb1", new RootObjectDefinition("typename", null, pvs));

            IList variableSources = new ArrayList();
            variableSources.Add(new DictionaryVariableSource(new string[] { "name", "name-value", "nickname", null }));
            variableSources.Add(new DictionaryVariableSource(new string[] { "nickname", "nickname-value" }));
            VariablePlaceholderConfigurer vphc = new VariablePlaceholderConfigurer(variableSources);

            vphc.PostProcessObjectFactory(of);
            RootObjectDefinition rod = (RootObjectDefinition)of.GetObjectDefinition("tb1");
            Assert.AreEqual("name-value", rod.PropertyValues.GetPropertyValue("NameProperty").Value);
            Assert.AreEqual(null, rod.PropertyValues.GetPropertyValue("NickNameProperty").Value);
        }

        [Test]
        public void WhitespaceHandling()
        {
            StaticApplicationContext ac = new StaticApplicationContext();

            MutablePropertyValues pvs = new MutablePropertyValues();
            pvs.Add("name", "${name}");
            pvs.Add("nickname", "${nickname}");
            ac.RegisterSingleton("tb1", typeof(TestObject), pvs);

            IList variableSources = new ArrayList();
            variableSources.Add(new DictionaryVariableSource(new string[] { "name", string.Empty, "nickname", null }));
            pvs = new MutablePropertyValues();
            pvs.Add("VariableSources", variableSources);
            ac.RegisterSingleton("configurer", typeof(VariablePlaceholderConfigurer), pvs);
            ac.Refresh();

            TestObject tb1 = (TestObject)ac.GetObject("tb1");
            Assert.AreEqual(string.Empty, tb1.Name);
            Assert.AreEqual(null, tb1.Nickname);
        }

        [Test]
        public void BailsOnUnresolvableVariable()
        {
            StaticApplicationContext ac = new StaticApplicationContext();

            MutablePropertyValues pvs = new MutablePropertyValues();
            pvs.Add("nickname", "${nickname}");
            ac.RegisterSingleton("tb1", typeof(TestObject), pvs);

            IList variableSources = new ArrayList();
            variableSources.Add(new DictionaryVariableSource(new string[] { }));
            pvs = new MutablePropertyValues();
            pvs.Add("VariableSources", variableSources);
            ac.RegisterSingleton("configurer", typeof(VariablePlaceholderConfigurer), pvs);

            try
            {
                ac.Refresh();
                Assert.Fail("something changed wrt VariablePlaceholder resolution");
            }
            catch (ObjectDefinitionStoreException ex)
            {
                Assert.IsTrue(ex.Message.IndexOf("nickname") > -1);
            }
        }

        [Test]
        public void IgnoresUnresolvableVariable()
        {
            StaticApplicationContext ac = new StaticApplicationContext();

            MutablePropertyValues pvs = new MutablePropertyValues();
            pvs.Add("name", "${name}");
            pvs.Add("nickname", "${nickname}");
            ac.RegisterSingleton("tb1", typeof(TestObject), pvs);

            VariablePlaceholderConfigurer vpc = new VariablePlaceholderConfigurer();
            vpc.IgnoreUnresolvablePlaceholders = true;
            vpc.VariableSource = new DictionaryVariableSource(new string[] { "name", "Erich" });
            ac.AddObjectFactoryPostProcessor(vpc);

            ac.Refresh();

            TestObject tb1 = (TestObject)ac.GetObject("tb1");
            Assert.AreEqual("Erich", tb1.Name);
            Assert.AreEqual("${nickname}", tb1.Nickname);
        }

        [Test]
        public void InlcludeAncestors()
        {
            const string defName = "foo";
            const string placeholder = "${name}";
            MutablePropertyValues pvs = new MutablePropertyValues();


            const string theProperty = "name";
            pvs.Add(theProperty, placeholder);
            RootObjectDefinition def = new RootObjectDefinition(typeof(TestObject), pvs);

            IConfigurableListableObjectFactory mock = A.Fake<IConfigurableListableObjectFactory>();
            A.CallTo(() => mock.GetObjectDefinitionNames(true)).Returns(new string[] { defName });
            A.CallTo(() => mock.GetObjectDefinition(defName, true)).Returns(def);

            VariablePlaceholderConfigurer vpc = new VariablePlaceholderConfigurer();
            vpc.IgnoreUnresolvablePlaceholders = true;
            vpc.VariableSource = new DictionaryVariableSource(new string[] { "name", "Erich" });
            vpc.IncludeAncestors = true;

            vpc.PostProcessObjectFactory(mock);
        }
    }
}