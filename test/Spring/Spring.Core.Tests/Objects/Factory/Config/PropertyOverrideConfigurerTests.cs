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

using System.Collections.Specialized;
using Common.Logging;
using Common.Logging.Simple;

using FakeItEasy;

using NUnit.Framework;
using Spring.Context.Support;
using Spring.Core.IO;
using Spring.Objects.Factory.Xml;

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Unit tests for the PropertyOverrideConfigurer class.
	/// </summary>
	/// <author>Rick Evans</author>
	[TestFixture]
	public sealed class PropertyOverrideConfigurerTests
	{
        [SetUp]
        public void Setup()
        {
        }

		/// <summary>
		/// The setup logic executed before the execution of this test fixture.
		/// </summary>
		[OneTimeSetUp]
		public void FixtureSetUp()
		{
			// enable (null appender) logging, just to ensure that the logging code is correct
            LogManager.Adapter = new NoOpLoggerFactoryAdapter();
		}

        [Test]
        public void AddPropertyValue()
        {
            StaticApplicationContext ac = new StaticApplicationContext();
            ac.RegisterSingleton("tb1", typeof(TestObject), new MutablePropertyValues());
            ac.RegisterSingleton("tb2", typeof(TestObject), new MutablePropertyValues());
            MutablePropertyValues pvs = new MutablePropertyValues();
            pvs.Add("Properties", "<spring-config><add key=\"tb1.Age\" value=\"99\"/><add key=\"tb2.Name\" value=\"test\"/></spring-config>");
            ac.RegisterSingleton("configurer1", typeof(PropertyOverrideConfigurer), pvs);
            pvs = new MutablePropertyValues();
            pvs.Add("Properties", "<spring-config><add key=\"tb2.Age\" value=\"99\"/><add key=\"tb2.Name\" value=\"test\"/></spring-config>");
            pvs.Add("order", "0");
            ac.RegisterSingleton("configurer2", typeof(PropertyOverrideConfigurer), pvs);
            ac.Refresh();
            TestObject tb1 = (TestObject)ac.GetObject("tb1");
            TestObject tb2 = (TestObject)ac.GetObject("tb2");
            Assert.AreEqual(99, tb1.Age);
            Assert.AreEqual(99, tb2.Age);
            Assert.AreEqual(null, tb1.Name);
            Assert.AreEqual("test", tb2.Name);
        }

		[Test]
        public void OverridePropertyValue()
		{
			StaticApplicationContext ac = new StaticApplicationContext();

            MutablePropertyValues pvs = new MutablePropertyValues();
            pvs.Add("Age", 27);
            pvs.Add("Name", "Bruno");
            ac.RegisterSingleton("tb1", typeof(TestObject), pvs);

            pvs = new MutablePropertyValues();
			pvs.Add("Properties", "<spring-config><add key=\"tb1.Age\" value=\"99\"/><add key=\"tb1.Name\" value=\"test\"/></spring-config>");
			ac.RegisterSingleton("configurer", typeof (PropertyOverrideConfigurer), pvs);

			ac.Refresh();
			TestObject tb1 = (TestObject) ac.GetObject("tb1");
			Assert.AreEqual(99, tb1.Age);
			Assert.AreEqual("test", tb1.Name);
		}

        [Test]
        public void OverridePropertyReference()
        {
            StaticApplicationContext ac = new StaticApplicationContext();

            MutablePropertyValues pvs = new MutablePropertyValues();
            pvs.Add("Spouse", new RuntimeObjectReference("spouse1"));
            ac.RegisterSingleton("tb1", typeof(TestObject), pvs);

            ac.RegisterSingleton("spouse1", typeof(TestObject), new MutablePropertyValues());
            ac.RegisterSingleton("spouse2", typeof(TestObject), new MutablePropertyValues());

            pvs = new MutablePropertyValues();
            pvs.Add("Properties", "<spring-config><add key=\"tb1.Spouse\" value=\"spouse2\"/></spring-config>");
            ac.RegisterSingleton("configurer", typeof(PropertyOverrideConfigurer), pvs);

            ac.Refresh();
            TestObject tb1 = (TestObject)ac.GetObject("tb1");
            TestObject spouse2 = (TestObject)ac.GetObject("spouse2");
            Assert.AreEqual(spouse2, tb1.Spouse);
        }

        [Test]
        public void OverridePropertyExpression()
        {
            StaticApplicationContext ac = new StaticApplicationContext();

            MutablePropertyValues pvs = new MutablePropertyValues();
            pvs.Add("Age", new ExpressionHolder("26+1"));
            ac.RegisterSingleton("tb1", typeof(TestObject), pvs);

            pvs = new MutablePropertyValues();
            pvs.Add("Properties", "<spring-config><add key=\"tb1.Age\" value=\"26-1\"/></spring-config>");
            ac.RegisterSingleton("configurer", typeof(PropertyOverrideConfigurer), pvs);

            ac.Refresh();
            TestObject tb1 = (TestObject)ac.GetObject("tb1");
            Assert.AreEqual(25, tb1.Age);
        }

		[Test]
		public void MalformedOverrideKey()
		{
			IConfigurableListableObjectFactory objectFactory = A.Fake<IConfigurableListableObjectFactory>();
		    IConfigurableListableObjectFactory fac = objectFactory;

			PropertyOverrideConfigurer cfg = new PropertyOverrideConfigurer();
			NameValueCollection defaultProperties = new NameValueCollection();
			defaultProperties.Add("malformedKey", "Rick Evans");
			cfg.Properties = defaultProperties;

            Assert.Throws<FatalObjectException>(() => cfg.PostProcessObjectFactory(fac));
		}

		[Test]
		public void MissingObjectDefinitionDoesntRaiseFatalException()
		{
			const string valueTo_NOT_BeOveridden = "Jenny Lewis";
			TestObject foo = new TestObject(valueTo_NOT_BeOveridden, 30);
            IConfigurableListableObjectFactory objectFactory = A.Fake<IConfigurableListableObjectFactory>();
		    A.CallTo(() => objectFactory.GetObjectDefinition("rubbish")).Returns(null);
		    IConfigurableListableObjectFactory fac = objectFactory;

			PropertyOverrideConfigurer cfg = new PropertyOverrideConfigurer();
			NameValueCollection defaultProperties = new NameValueCollection();
			defaultProperties.Add("rubbish.Name", "Rick Evans");
			cfg.Properties = defaultProperties;

			cfg.PostProcessObjectFactory(fac);
			Assert.AreEqual(valueTo_NOT_BeOveridden, foo.Name, "Property value was overridden, but a rubbish objectName root was supplied.");
		}

		[Test]
		public void ViaXML()
		{
			IResource resource = new ReadOnlyXmlTestResource("PropertyResourceConfigurerTests.xml", GetType());
			XmlObjectFactory xbf = new XmlObjectFactory(resource);
			PropertyOverrideConfigurer poc = (PropertyOverrideConfigurer) xbf.GetObject("OverrideConfigurer");
			Assert.IsNotNull(poc);
			poc.PostProcessObjectFactory(xbf);
			TestObject to = (TestObject) xbf.GetObject("Test2");
			Assert.AreEqual("Overriden Name", to.Name);
		}
	}
}