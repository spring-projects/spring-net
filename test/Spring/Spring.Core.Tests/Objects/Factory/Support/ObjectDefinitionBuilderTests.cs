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

using System.Collections;
using NUnit.Framework;
using Spring.Context.Support;
using Spring.Objects.Factory.Xml;

#endregion

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// This class contains tests for ObjectDefinitionBuilder
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class ObjectDefinitionBuilderTests
    {
        private IObjectDefinitionFactory odf;

        [SetUp]
        public void CreateObjectDefinitionBuilder()
        {
            odf = new DefaultObjectDefinitionFactory();
        }

        
        [Test]
        public void ObjectName()
        {
            ObjectDefinitionBuilder odb =
                ObjectDefinitionBuilder.RootObjectDefinition(odf, typeof(TestObject).FullName);
            RootObjectDefinition rod = odb.ObjectDefinition as RootObjectDefinition;
            Assert.IsNotNull(rod);
            Assert.IsFalse(rod.HasObjectType);
            Assert.AreEqual(typeof(TestObject).FullName, rod.ObjectTypeName);
        }

        [Test]
        public void ObjectNameWithFactoryMethod()
        {
            ObjectDefinitionBuilder odb =
                ObjectDefinitionBuilder.RootObjectDefinition(odf, typeof(TestObject).FullName, "Create");
            RootObjectDefinition rod = odb.ObjectDefinition as RootObjectDefinition;
            Assert.IsNotNull(rod);
            Assert.IsFalse(rod.HasObjectType);
            Assert.AreEqual(typeof(TestObject).FullName, rod.ObjectTypeName);
            Assert.AreEqual("Create", rod.FactoryMethodName);
        }

        [Test]
        public void ObjectNameChild()
        {
            ObjectDefinitionBuilder odb = ObjectDefinitionBuilder.ChildObjectDefinition(odf, typeof(TestObject).FullName);
            ChildObjectDefinition rod = odb.ObjectDefinition as ChildObjectDefinition;
            Assert.IsNotNull(rod);
            Assert.IsFalse(rod.HasObjectType);
            Assert.AreEqual(typeof(TestObject).FullName, rod.ParentName);
        }
        

        [Test]
        public void ObjectType()
        {
            ObjectDefinitionBuilder odb = ObjectDefinitionBuilder.RootObjectDefinition(odf, typeof (TestObject));
            RootObjectDefinition rod = odb.ObjectDefinition as RootObjectDefinition;
            Assert.IsNotNull(rod);
            Assert.IsTrue(rod.HasObjectType);
            Assert.AreEqual(typeof (TestObject), rod.ObjectType);
        }

        [Test]
        public void ObjectTypeWithFactoryMethod()
        {
            ObjectDefinitionBuilder odb =
                ObjectDefinitionBuilder.RootObjectDefinition(odf, typeof (TestObject), "Create");
            RootObjectDefinition rod = odb.ObjectDefinition as RootObjectDefinition;
            Assert.IsNotNull(rod);
            Assert.IsTrue(rod.HasObjectType);
            Assert.AreEqual(typeof (TestObject), rod.ObjectType);
            Assert.AreEqual("Create", rod.FactoryMethodName);
        }

        [Test]
        public void ObjectTypeWithSimpleProperty()
        {
            string[] dependsOn = new string[] {"A", "B", "C"};
            ObjectDefinitionBuilder odb = ObjectDefinitionBuilder.RootObjectDefinition(odf, typeof (TestObject));

            odb.SetSingleton(false).AddPropertyReference("Age", "15");
            foreach (string dep in dependsOn)
            {
                odb.AddDependsOn(dep);
            }

            RootObjectDefinition rod = odb.ObjectDefinition as RootObjectDefinition;
            Assert.IsNotNull(rod);
            Assert.IsFalse(rod.IsSingleton);
            Assert.AreEqual(typeof (TestObject), rod.ObjectType);
            Assert.AreEqual(dependsOn, rod.DependsOn, "DependsOn not as expected");
            Assert.IsTrue(rod.PropertyValues.Contains("Age"));
        }

        [Test]
        public void Simple()
        {
            GenericApplicationContext ctx = new GenericApplicationContext();
            IObjectDefinitionFactory objectDefinitionFactory = new DefaultObjectDefinitionFactory();

            ObjectDefinitionBuilder builder = ObjectDefinitionBuilder.RootObjectDefinition(objectDefinitionFactory,
                                                                                           typeof (TestObject));
            
            builder.AddPropertyValue("Age", 22)
                .AddPropertyValue("Name", "Joe")
                .AddPropertyReference("Spouse", "Spouse")
                .SetSingleton(false);
            
            ctx.RegisterObjectDefinition("TestObject", builder.ObjectDefinition);

            builder = ObjectDefinitionBuilder.RootObjectDefinition(objectDefinitionFactory, typeof(TestObject));

            IList friends = new ArrayList();
            friends.Add(new TestObject("Dan", 34));
            friends.Add(new TestObject("Mary", 33));
            builder.AddPropertyValue("Friends", friends)
                .AddConstructorArg("Susan")
                .AddConstructorArg(23)
                .SetSingleton(false);

           ctx.RegisterObjectDefinition("Spouse", builder.ObjectDefinition);


            

            TestObject to = ctx.GetObject("TestObject") as TestObject;

            Assert.IsNotNull(to);

            Assert.AreEqual("Joe", to.Name);
            Assert.AreEqual(22, to.Age);
            Assert.AreEqual(2,to.Spouse.Friends.Count);
            Assert.AreEqual(23, to.Spouse.Age);

            /*
            AbstractApplicationContext ctx = ContextRegistry.GetContext() as AbstractApplicationContext;



            //XmlObjectFactory objectFactory = ctx.ObjectFactory as XmlObjectFactory;

            IObjectFactory objectFactory = ctx.ObjectFactory;

            //DefaultListableObjectFactory
            if (objectFactory != null)
            {
                Console.WriteLine("hi");
            }
            //objectFactory.RegisterObjectDefinition("TestObject", builder.ObjectDefinition);
             */
                



        }

        public void SimpleReader()
        {
            GenericApplicationContext applicationContext = new GenericApplicationContext();


            IObjectDefinitionReader objectDefinitionReader = new XmlObjectDefinitionReader(applicationContext);

            objectDefinitionReader.LoadObjectDefinitions("assembly://foo");

            applicationContext.Refresh();
            

        }
    }
}