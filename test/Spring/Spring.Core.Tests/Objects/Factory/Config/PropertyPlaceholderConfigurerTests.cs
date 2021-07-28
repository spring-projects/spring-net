#region License

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

#endregion

using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;

using FakeItEasy;

using NUnit.Framework;
using Spring.Collections;
using Spring.Context;
using Spring.Context.Support;
using Spring.Core.IO;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Unit tests for the PropertyPlaceholderConfigurer class.
    /// </summary>
    /// <author>Rick Evans</author>
	[TestFixture]
    public sealed class PropertyPlaceholderConfigurerTests
	{
#if !NETCOREAPP
	    private static string testConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0; Data Source=c:\Northwind.mdb;User ID=Admin;Password=;";
        private static string testConnectionStringTwo = @"Provider=Microsoft.Jet.OLEDB.4.0; Data Source=c:\Northwind.mdb;User ID=Admin;Password=Ernie;";
#endif

        [SetUp]
        public void SetUp()
        {
        }

		[Test]
		public void MismatchBetweenNumberOfConfigNamesAndNumberOfLocations()
		{
			PropertyPlaceholderConfigurer cfg = new PropertyPlaceholderConfigurer();
            cfg.Locations = new IResource[] { A.Fake<IResource>() }; // will never get to the point where we check the validity
			cfg.ConfigSections = new string[] { "", "" };
            Assert.Throws<ObjectInitializationException>(() => cfg.PostProcessObjectFactory(A.Fake<IConfigurableListableObjectFactory>()));
		}

		[Test]
		public void OneConfigNameIsOKForLotsOfLocations()
		{
			PropertyPlaceholderConfigurer cfg = new PropertyPlaceholderConfigurer();
            IResource mock = A.Fake<IResource>();
			A.CallTo(() => mock.Exists).Returns(true);
			A.CallTo(() => mock.InputStream).Throws(new FileNotFoundException());

			cfg.Locations = new IResource [] {mock};
			cfg.ConfigSections = new string[] { "" };
            Assert.Throws<ObjectsException>(() => cfg.PostProcessObjectFactory(A.Fake<IConfigurableListableObjectFactory>()));
        }

		[Test]
		public void ChokesOnBadResourceLocationIfIgnoreBadResourcesFlagNotSetToTrue()
		{
			PropertyPlaceholderConfigurer cfg = new PropertyPlaceholderConfigurer();
			cfg.IgnoreResourceNotFound = false;
			IResource mock = A.Fake<IResource>();
		    A.CallTo(() => mock.Exists).Returns(false);

			cfg.Locations = new IResource [] { mock};
			cfg.ConfigSections = new string[] { "" };
            Assert.Throws<ObjectInitializationException>(() => cfg.PostProcessObjectFactory(A.Fake<IConfigurableListableObjectFactory>()));
        }

		[Test]
		public void DoesNotChokeOnBadResourceLocationIfIgnoreBadResourcesFlagSetToTrue()
		{
			PropertyPlaceholderConfigurer cfg = new PropertyPlaceholderConfigurer();
			cfg.IgnoreResourceNotFound = true;
            IResource mockResource = A.Fake<IResource>();
		    A.CallTo(() => mockResource.Exists).Returns(false);
			cfg.Location = mockResource;
			cfg.ConfigSections = new string[] { "" };
            IConfigurableListableObjectFactory mockFactory = A.Fake<IConfigurableListableObjectFactory>();
			A.CallTo(() => mockFactory.GetObjectDefinitionNames(false)).Returns(new string[] {});

			cfg.PostProcessObjectFactory(mockFactory);
		}

		[Test]
		public void WithCircularReference()
		{
			StaticApplicationContext ac = new StaticApplicationContext();
			MutablePropertyValues pvs = new MutablePropertyValues();
			pvs.Add("age", "${age}");
			pvs.Add("name", "name${var}");
			pvs.Add("spouse", new RuntimeObjectReference("${ref}"));
			ac.RegisterSingleton("tb1", typeof (TestObject), pvs);
			pvs = new MutablePropertyValues();
			pvs.Add("age", "${age}");
			pvs.Add("name", "name${age}");
			ac.RegisterSingleton("tb2", typeof (TestObject), pvs);
			pvs = new MutablePropertyValues();
			pvs.Add("Properties", "<spring-config><add key=\"age\" value=\"99\"/><add key=\"var\" value=\"${m}var\"/><add key=\"ref\" value=\"tb2\"/><add key=\"m\" value=\"${var}\"/></spring-config>");
			ac.RegisterSingleton("configurer1", typeof (PropertyPlaceholderConfigurer), pvs);
			pvs = new MutablePropertyValues();
			pvs.Add("Properties", "<spring-config><add key=\"age\" value=\"98\"/></spring-config>");
			pvs.Add("order", "0");
			ac.RegisterSingleton("configurer2", typeof (PropertyPlaceholderConfigurer), pvs);
            Assert.Throws<ObjectDefinitionStoreException>(() => ac.Refresh());
		}

		[Test]
		public void WithDefaultProperties()
		{
			const string defName = "foo";
			const string placeholder = "${name}";
			MutablePropertyValues pvs = new MutablePropertyValues();

			const string theProperty = "name";
			pvs.Add(theProperty, placeholder);
			RootObjectDefinition def = new RootObjectDefinition(typeof(TestObject), pvs);

            IConfigurableListableObjectFactory mock = A.Fake<IConfigurableListableObjectFactory>();
			A.CallTo(() => mock.GetObjectDefinitionNames(false)).Returns(new string [] {defName});
			A.CallTo(() => mock.GetObjectDefinition(defName, false)).Returns(def);

			PropertyPlaceholderConfigurer cfg = new PropertyPlaceholderConfigurer();
			NameValueCollection defaultProperties = new NameValueCollection();
			const string expectedName = "Rick Evans";
			defaultProperties.Add(theProperty, expectedName);
			cfg.Properties = defaultProperties;
			cfg.PostProcessObjectFactory(mock);
			Assert.AreEqual(expectedName, def.PropertyValues.GetPropertyValue(theProperty).Value,
				"Property placeholder value was not replaced with the resolved value.");

		    A.CallTo(() => mock.AddEmbeddedValueResolver(A<IStringValueResolver>._)).MustHaveHappened();
		}

        [Test]
        public void IncludingAncestors()
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

            PropertyPlaceholderConfigurer cfg = new PropertyPlaceholderConfigurer();
            cfg.IncludeAncestors = true;

            NameValueCollection defaultProperties = new NameValueCollection();
            const string expectedName = "Rick Evans";
            defaultProperties.Add(theProperty, expectedName);
            cfg.Properties = defaultProperties;
            cfg.PostProcessObjectFactory(mock);
            Assert.AreEqual(expectedName, def.PropertyValues.GetPropertyValue(theProperty).Value,
                "Property placeholder value was not replaced with the resolved value.");

            A.CallTo(() => mock.AddEmbeddedValueResolver(A<IStringValueResolver>._)).MustHaveHappened();
        }

        /// <summary>
        /// Fallback is the default mode. Check if the PROCESSOR_ARCHITECTURE
        /// variable is replaced.
        /// </summary>
        [Test]
        [Platform("Win")]
		public void WithEnvironmentVariableFallback()
		{
			StaticApplicationContext ac = new StaticApplicationContext();
			MutablePropertyValues pvs = new MutablePropertyValues();
			pvs.Add("touchy", "${PROCESSOR_ARCHITECTURE}");
			ac.RegisterSingleton("to", typeof (TestObject), pvs);

			pvs = new MutablePropertyValues();
			ac.RegisterSingleton("configurer", typeof (PropertyPlaceholderConfigurer), pvs);
			ac.Refresh();

			TestObject to = (TestObject) ac["to"];
			Assert.AreEqual(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE"),
				to.Touchy);
		}

		/// <summary>
		/// Fallback is the default mode.  Explicity provide a value using the
		/// property (NameValueCollection Properties) of PropertyPlaceholder.
		/// Fallback mode will not change the value since it has been explicity set.
		/// </summary>
		[Test]
		public void WithEnvironmentPropertyNotUsed()
		{
			StaticApplicationContext ac = new StaticApplicationContext();
			MutablePropertyValues pvs = new MutablePropertyValues();
			pvs.Add("touchy", "${PROCESSOR_ARCHITECTURE}");
			ac.RegisterSingleton("to", typeof (TestObject), pvs);

			pvs = new MutablePropertyValues();
			NameValueCollection nvc = new NameValueCollection();
			nvc.Add("PROCESSOR_ARCHITECTURE", "G5");
			pvs.Add("properties", nvc);
			ac.RegisterSingleton("configurer", typeof (PropertyPlaceholderConfigurer), pvs);
			ac.Refresh();

			TestObject to = (TestObject) ac["to"];
			Assert.AreEqual("G5", to.Touchy, "Fallback mode is not respecting previously set values.");
		}

		/// <summary>
		/// Set the environment variable mode to override.  Now expect the environment
		/// variable setting to override the explicitly defined name value collection.
		/// </summary>
		[Test]
		[Platform("Win")]
		public void WithOverridingEnvironmentProperty()
		{
			StaticApplicationContext ac = new StaticApplicationContext();
			MutablePropertyValues pvs = new MutablePropertyValues();
			pvs.Add("touchy", "${PROCESSOR_ARCHITECTURE}");
			ac.RegisterSingleton("to", typeof (TestObject), pvs);

			pvs = new MutablePropertyValues();
			NameValueCollection nvc = new NameValueCollection();
			nvc.Add("PROCESSOR_ARCHITECTURE", "G5");
			pvs.Add("properties", nvc);
			pvs.Add("environmentVariableMode", EnvironmentVariableMode.Override);
			ac.RegisterSingleton("configurer", typeof (PropertyPlaceholderConfigurer), pvs);
			ac.Refresh();

			TestObject to = (TestObject) ac["to"];
			Assert.AreEqual(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE"),
				to.Touchy);
		}

		[Test]
		public void WithUnresolvableEnvironmentProperty()
		{
			StaticApplicationContext ac = new StaticApplicationContext();
			MutablePropertyValues pvs = new MutablePropertyValues();
			pvs.Add("touchy", "${PROCESSOR_ARCHITECTURE}");
			ac.RegisterSingleton("to", typeof (TestObject), pvs);

			pvs = new MutablePropertyValues();
			pvs.Add("environmentVariableMode", EnvironmentVariableMode.Never);
			ac.RegisterSingleton("configurer", typeof (PropertyPlaceholderConfigurer), pvs);
            Assert.Throws<ObjectDefinitionStoreException>(() => ac.Refresh(), "Error registering object with name 'to' defined in '' : Could not resolve placeholder 'PROCESSOR_ARCHITECTURE'.");
		}

		[Test]
		public void WithUnresolvablePlaceholder()
		{
			StaticApplicationContext ac = new StaticApplicationContext();
			MutablePropertyValues pvs = new MutablePropertyValues();
			pvs.Add("name", "${ref}");
			ac.RegisterSingleton("tb", typeof (TestObject), pvs);
			ac.RegisterSingleton("configurer", typeof (PropertyPlaceholderConfigurer), null);
			try
			{
				ac.Refresh();
				Assert.Fail("Should have thrown ObjectDefinitionStoreException");
			}
			catch (ObjectDefinitionStoreException ex)
			{
				// expected
				Assert.IsTrue(ex.Message.IndexOf("ref") != -1);
			}
		}

        [Test]
        public void WithExpressionProperty()
        {
            StaticApplicationContext ac = new StaticApplicationContext();
            MutablePropertyValues pvs = new MutablePropertyValues();
            pvs.Add("age", new ExpressionHolder("${age}"));
            ac.RegisterSingleton("to1", typeof(TestObject), pvs);

            pvs = new MutablePropertyValues();
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("age", "'0x7FFFFFFF'");
            pvs.Add("properties", nvc);
            ac.RegisterSingleton("configurer", typeof(PropertyPlaceholderConfigurer), pvs);
            ac.Refresh();


            TestObject to1 = (TestObject)ac.GetObject("to1");;
            Assert.AreEqual(2147483647, to1.Age);

        }

		[Test]
		public void SunnyDay()
		{
			StaticApplicationContext ac = new StaticApplicationContext();


            MutablePropertyValues pvs = new MutablePropertyValues();
            pvs.Add("age", "${age}");
            RootObjectDefinition def
                = new RootObjectDefinition("${fqn}", new ConstructorArgumentValues(), pvs);
            ac.RegisterObjectDefinition("tb3", def);



			pvs = new MutablePropertyValues();
			pvs.Add("age", "${age}");
			pvs.Add("name", "name${var}${");
			pvs.Add("spouse", new RuntimeObjectReference("${ref}"));
			ac.RegisterSingleton("tb1", typeof (TestObject), pvs);

			ConstructorArgumentValues cas = new ConstructorArgumentValues();
			cas.AddIndexedArgumentValue(1, "${age}");
			cas.AddGenericArgumentValue("${var}name${age}");

			pvs = new MutablePropertyValues();
			ArrayList friends = new ManagedList();
			friends.Add("na${age}me");
			friends.Add(new RuntimeObjectReference("${ref}"));
			pvs.Add("friends", friends);

			ISet someSet = new ManagedSet();
			someSet.Add("na${age}me");
			someSet.Add(new RuntimeObjectReference("${ref}"));
			pvs.Add("someSet", someSet);

			IDictionary someDictionary = new ManagedDictionary();
			someDictionary["key1"] = new RuntimeObjectReference("${ref}");
			someDictionary["key2"] = "${age}name";
			MutablePropertyValues innerPvs = new MutablePropertyValues();
			someDictionary["key3"] = new RootObjectDefinition(typeof (TestObject), innerPvs);
			someDictionary["key4"] = new ChildObjectDefinition("tb1", innerPvs);
			pvs.Add("someMap", someDictionary);

			RootObjectDefinition definition = new RootObjectDefinition(typeof (TestObject), cas, pvs);
			ac.DefaultListableObjectFactory.RegisterObjectDefinition("tb2", definition);

			pvs = new MutablePropertyValues();
            pvs.Add("Properties", "<spring-config><add key=\"age\" value=\"98\"/><add key=\"var\" value=\"${m}var\"/><add key=\"ref\" value=\"tb2\"/><add key=\"m\" value=\"my\"/><add key=\"fqn\" value=\"Spring.Objects.TestObject, Spring.Core.Tests\"/></spring-config>");
			ac.RegisterSingleton("configurer", typeof (PropertyPlaceholderConfigurer), pvs);
			ac.Refresh();

			TestObject tb1 = (TestObject) ac.GetObject("tb1");
			TestObject tb2 = (TestObject) ac.GetObject("tb2");
            TestObject tb3 = (TestObject) ac.GetObject("tb3");
			Assert.AreEqual(98, tb1.Age);
			Assert.AreEqual(98, tb2.Age);
            Assert.AreEqual(98, tb3.Age);
			Assert.AreEqual("namemyvar${", tb1.Name);
			Assert.AreEqual("myvarname98", tb2.Name);
			Assert.AreEqual(tb2, tb1.Spouse);
			Assert.AreEqual(2, tb2.Friends.Count);
			IEnumerator ie = tb2.Friends.GetEnumerator();
			ie.MoveNext();
			Assert.AreEqual("na98me", ie.Current);
			ie.MoveNext();
			Assert.AreEqual(tb2, ie.Current);
			Assert.AreEqual(2, tb2.SomeSet.Count);
			Assert.IsTrue(tb2.SomeSet.Contains("na98me"));
			Assert.IsTrue(tb2.SomeSet.Contains(tb2));
			Assert.AreEqual(4, tb2.SomeMap.Count);
			Assert.AreEqual(tb2, tb2.SomeMap["key1"]);
			Assert.AreEqual("98name", tb2.SomeMap["key2"]);
			TestObject inner1 = (TestObject) tb2.SomeMap["key3"];
			TestObject inner2 = (TestObject) tb2.SomeMap["key4"];
			Assert.AreEqual(0, inner1.Age);
			Assert.AreEqual(null, inner1.Name);
			Assert.AreEqual(98, inner2.Age);
			Assert.AreEqual("namemyvar${", inner2.Name);
		}

		/// <summary>
		/// Makes sure that an appropriate exception is raised when trying
		/// to resolve this placeholder... ${foo} with this value... foo=ba${foo}r
		/// </summary>
		[Test]
		public void ChokesOnCircularReferenceToPlaceHolder()
		{
			RootObjectDefinition def = new RootObjectDefinition();
			def.ObjectType = typeof (TestObject);
			ConstructorArgumentValues args = new ConstructorArgumentValues();
			args.AddNamedArgumentValue("name", "${foo}");
			def.ConstructorArgumentValues = args;

			NameValueCollection properties = new NameValueCollection();
			const string expectedName = "ba${foo}r";
			properties.Add("foo", expectedName);

			IConfigurableListableObjectFactory mock = A.Fake<IConfigurableListableObjectFactory>();
			A.CallTo(() => mock.GetObjectDefinitionNames(false)).Returns(new string[] {"foo"});
			A.CallTo(() => mock.GetObjectDefinition(null, false)).WithAnyArguments().Returns(def);

			PropertyPlaceholderConfigurer cfg = new PropertyPlaceholderConfigurer();
			cfg.Properties = properties;
			try
			{
				cfg.PostProcessObjectFactory(mock);
				Assert.Fail("Should have raised an ObjectDefinitionStoreException by this point.");
			}
			catch (ObjectDefinitionStoreException)
			{
			}
		}

		[Test]
		public void ReplacesNamedCtorArgument()
		{
			RootObjectDefinition def = new RootObjectDefinition();
			def.ObjectType = typeof (TestObject);
			ConstructorArgumentValues args = new ConstructorArgumentValues();
			args.AddNamedArgumentValue("name", "${hope.floats}");
			def.ConstructorArgumentValues = args;

			NameValueCollection properties = new NameValueCollection();
			const string expectedName = "Rick";
			properties.Add("hope.floats", expectedName);

			IConfigurableListableObjectFactory mock = A.Fake<IConfigurableListableObjectFactory>();
			A.CallTo(() => mock.GetObjectDefinitionNames(false)).Returns(new string[] {"foo"});
			A.CallTo(() => mock.GetObjectDefinition(null, false)).WithAnyArguments().Returns(def);

            PropertyPlaceholderConfigurer cfg = new PropertyPlaceholderConfigurer();
			cfg.Properties = properties;
			cfg.PostProcessObjectFactory(mock);

		    A.CallTo(() => mock.AddEmbeddedValueResolver(A<IStringValueResolver>._)).MustHaveHappened();

            Assert.AreEqual(expectedName,
				def.ConstructorArgumentValues.GetNamedArgumentValue("name").Value,
				"Named argument placeholder value was not replaced.");
		}

		[Test]
		public void UsingCustomMarkers()
		{
			RootObjectDefinition def = new RootObjectDefinition();
			def.ObjectType = typeof (TestObject);
			ConstructorArgumentValues args = new ConstructorArgumentValues();
			args.AddNamedArgumentValue("name", "#hope.floats#");
			def.ConstructorArgumentValues = args;

			NameValueCollection properties = new NameValueCollection();
			const string expectedName = "Rick";
			properties.Add("hope.floats", expectedName);

			IConfigurableListableObjectFactory mock = A.Fake<IConfigurableListableObjectFactory>();
			A.CallTo(() => mock.GetObjectDefinitionNames(false)).Returns(new string[] {"foo"});
			A.CallTo(() => mock.GetObjectDefinition(null, false)).WithAnyArguments().Returns(def);

			PropertyPlaceholderConfigurer cfg = new PropertyPlaceholderConfigurer();
			cfg.PlaceholderPrefix = cfg.PlaceholderSuffix = "#";
			cfg.Properties = properties;
			cfg.PostProcessObjectFactory(mock);

		    A.CallTo(() => mock.AddEmbeddedValueResolver(null)).WithAnyArguments().MustHaveHappened();

            Assert.AreEqual(expectedName,
				def.ConstructorArgumentValues.GetNamedArgumentValue("name").Value,
				"Named argument placeholder value was not replaced.");
		}

#if !NETCOREAPP
		/// <summary>
		/// Test that properties can be replaced from NameValueConfiguration sections
		/// from the main .NET application configuration file.
		/// </summary>
		[Test]
		public void WithAppConfigResolution()
		{
			IApplicationContext ctx = new XmlApplicationContext(
				"file://Spring/Objects/Factory/Config/PropertyPlaceholderConfigurerTests.xml");
			TestObjectDAO to = (TestObjectDAO) ctx["testObjectDao"];
			Assert.AreEqual(testConnectionString, to.DbConnection.ConnectionString);
			Assert.AreEqual(1000, to.MaxResults);
		}

		/// <summary>
		/// Test that properties can be replaced from NameValueConfiguration sections
		/// from the main .NET application configuration file and a seprate .xml file
		/// on the file system.
		/// </summary>
		[Test]
		public void WithTwoLocations()
		{
			IApplicationContext ctx = new XmlApplicationContext(
				"file://Spring/Objects/Factory/Config/PPCTwoLocationsTwoSectionsTests.xml");
			TestObjectDAO to = (TestObjectDAO) ctx["testObjectDao"];
            Assert.AreEqual(testConnectionStringTwo, to.DbConnection.ConnectionString);
			Assert.AreEqual(1000, to.MaxResults);
		}

		/// <summary>
		/// Test that properties can be merged from a NameValueConfiguration section
		/// in the main .NET application configuration file and a seprate .xml file
		/// on the file system.
		/// </summary>
		[Test]
		public void WithTwoLocationsOneSection()
		{
			IApplicationContext ctx = new XmlApplicationContext(
				"file://Spring/Objects/Factory/Config/PPCTwoLocationsOneSectionTests.xml");
			TestObjectDAO to = (TestObjectDAO) ctx["testObjectDao"];
            Assert.AreEqual(testConnectionString, to.DbConnection.ConnectionString);
			Assert.AreEqual(1000, to.MaxResults);
		}

		/// <summary>
		/// Test that if two locations configure the same properties they are appended
		/// or not depending on the <see cref="PropertyResourceConfigurer.LastLocationOverrides"/> property
		/// </summary>
		[Test(Description="SPRNET-55")]
		public void WithAppend()
		{
			string resourceName = "Spring/Objects/Factory/Config/PPC-SPRNET-55.xml";
			XmlObjectFactory ctx = new XmlObjectFactory(new FileSystemResource(resourceName));
			Assert.IsNotNull(ctx);
			IObjectFactoryPostProcessor processor = (IObjectFactoryPostProcessor) ctx["appendConfigurer"];
			processor.PostProcessObjectFactory(ctx);
			TestObjectDAO to = (TestObjectDAO) ctx["appendDao"];
			Assert.AreEqual(testConnectionString + "," + testConnectionStringTwo, to.DbConnection.ConnectionString);
			Assert.AreEqual(1000, to.MaxResults);

			ctx = new XmlObjectFactory(new FileSystemResource(resourceName));
			processor = (IObjectFactoryPostProcessor) ctx["noAppendConfigurer"];
			processor.PostProcessObjectFactory(ctx);

			to = (TestObjectDAO) ctx["noAppendDao"];
            Assert.AreEqual(testConnectionStringTwo, to.DbConnection.ConnectionString);
			Assert.AreEqual(1000, to.MaxResults);
		}
#endif

		[Test]
		public void WithIgnoreUnresolvablePlaceholder()
		{
			const string defName = "foo";
			const string placeholder = "${name}";
			TestObject foo = new TestObject(placeholder, 30);
			MutablePropertyValues pvs = new MutablePropertyValues();

			pvs.Add("name", placeholder);
			RootObjectDefinition def = new RootObjectDefinition(typeof(TestObject), pvs);

			IConfigurableListableObjectFactory mock = A.Fake<IConfigurableListableObjectFactory>();
			A.CallTo(() => mock.GetObjectDefinitionNames(false)).Returns(new string [] {defName});
			A.CallTo(() => mock.GetObjectDefinition(defName, false)).Returns(def);

			PropertyPlaceholderConfigurer cfg = new PropertyPlaceholderConfigurer();
			cfg.IgnoreUnresolvablePlaceholders = true;
			cfg.PostProcessObjectFactory(mock);
			Assert.AreEqual(placeholder, foo.Name);

		    A.CallTo(() => mock.AddEmbeddedValueResolver(null)).WithAnyArguments().MustHaveHappened();
		}

        [Test]
		public void ViaXML()
		{
			IResource resource = new ReadOnlyXmlTestResource("PropertyResourceConfigurerTests.xml", GetType());
			XmlObjectFactory xbf = new XmlObjectFactory(resource);
			PropertyPlaceholderConfigurer ppc = (PropertyPlaceholderConfigurer) xbf.GetObject("PlaceholderConfigurer");
			Assert.IsNotNull(ppc);
			ppc.PostProcessObjectFactory(xbf);
			TestObject to = (TestObject) xbf.GetObject("Test1");
			Assert.AreEqual("A DefName", to.Name);
        }

		[Test]
		public void ViaXMLAndConfigSection()
		{
			IResource resource = new ReadOnlyXmlTestResource("PropertyResourceConfigurerTests.xml", GetType());
			XmlObjectFactory xbf = new XmlObjectFactory(resource);
			PropertyPlaceholderConfigurer ppc = (PropertyPlaceholderConfigurer) xbf.GetObject("ConfigSectionPlaceholderConfigurer");
			Assert.IsNotNull(ppc);
			ppc.PostProcessObjectFactory(xbf);

            Assert.AreEqual("name from section", ((TestObject)xbf.GetObject("Test3")).Name);
            Assert.AreEqual("name from sectiongroup/section", ((TestObject)xbf.GetObject("Test4")).Name);
        }

        [Test]
        public void WithTypes()
        {
            IApplicationContext ctx = new XmlApplicationContext(
                "file://Spring/Objects/Factory/Config/PPCWithTypesTests.xml");

            object obj = ctx["testObject"];
            Assert.IsTrue(obj is TestObject);

            TestObject to = (TestObject)obj;

            Assert.AreEqual(2, to.Pets.Count);
            Assert.AreEqual(2, to.PeriodicTable.Count);
            Assert.AreEqual(2, to.Computers.Count);
            Assert.IsTrue(to.PeriodicTable.Contains("C"));
        }

        [Test]
        public void WithMultipleXml_MultiplePropertyPlaceholderConfigurersAndOrder_CanReplaceValueFromOtherXml()
        {
            var context =
                new XmlApplicationContext(
                    new[]
                        {
                            "file://Spring/Objects/Factory/Config/FirstPropertyPlaceholderConfigurer.xml",
                            "file://Spring/Objects/Factory/Config/SecondPropertyPlaceholderConfigurer.xml"
                        });

            var testObject = context.GetObject<TestObject>("testObject");

            Assert.AreEqual("correct_name", testObject.Name);
        }
    }
}
