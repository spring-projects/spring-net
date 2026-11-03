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

using System.Collections;
using NUnit.Framework;
using Spring.Context.Support;
using Spring.Objects.Factory.Xml;

namespace Spring.Objects.Factory.Support;

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
        Assert.That(rod, Is.Not.Null);
        Assert.That(rod.HasObjectType, Is.False);
        Assert.That(rod.ObjectTypeName, Is.EqualTo(typeof(TestObject).FullName));
    }

    [Test]
    public void ObjectNameWithFactoryMethod()
    {
        ObjectDefinitionBuilder odb =
            ObjectDefinitionBuilder.RootObjectDefinition(odf, typeof(TestObject).FullName, "Create");
        RootObjectDefinition rod = odb.ObjectDefinition as RootObjectDefinition;
        Assert.That(rod, Is.Not.Null);
        Assert.That(rod.HasObjectType, Is.False);
        Assert.That(rod.ObjectTypeName, Is.EqualTo(typeof(TestObject).FullName));
        Assert.That(rod.FactoryMethodName, Is.EqualTo("Create"));
    }

    [Test]
    public void ObjectNameChild()
    {
        ObjectDefinitionBuilder odb = ObjectDefinitionBuilder.ChildObjectDefinition(odf, typeof(TestObject).FullName);
        ChildObjectDefinition rod = odb.ObjectDefinition as ChildObjectDefinition;
        Assert.That(rod, Is.Not.Null);
        Assert.That(rod.HasObjectType, Is.False);
        Assert.That(rod.ParentName, Is.EqualTo(typeof(TestObject).FullName));
    }

    [Test]
    public void ObjectType()
    {
        ObjectDefinitionBuilder odb = ObjectDefinitionBuilder.RootObjectDefinition(odf, typeof(TestObject));
        RootObjectDefinition rod = odb.ObjectDefinition as RootObjectDefinition;
        Assert.That(rod, Is.Not.Null);
        Assert.That(rod.HasObjectType, Is.True);
        Assert.That(rod.ObjectType, Is.EqualTo(typeof(TestObject)));
    }

    [Test]
    public void ObjectTypeWithFactoryMethod()
    {
        ObjectDefinitionBuilder odb =
            ObjectDefinitionBuilder.RootObjectDefinition(odf, typeof(TestObject), "Create");
        RootObjectDefinition rod = odb.ObjectDefinition as RootObjectDefinition;
        Assert.That(rod, Is.Not.Null);
        Assert.That(rod.HasObjectType, Is.True);
        Assert.That(rod.ObjectType, Is.EqualTo(typeof(TestObject)));
        Assert.That(rod.FactoryMethodName, Is.EqualTo("Create"));
    }

    [Test]
    public void ObjectTypeWithSimpleProperty()
    {
        string[] dependsOn = new string[] { "A", "B", "C" };
        ObjectDefinitionBuilder odb = ObjectDefinitionBuilder.RootObjectDefinition(odf, typeof(TestObject));

        odb.SetSingleton(false).AddPropertyReference("Age", "15");
        foreach (string dep in dependsOn)
        {
            odb.AddDependsOn(dep);
        }

        RootObjectDefinition rod = odb.ObjectDefinition as RootObjectDefinition;
        Assert.That(rod, Is.Not.Null);
        Assert.That(rod.IsSingleton, Is.False);
        Assert.That(rod.ObjectType, Is.EqualTo(typeof(TestObject)));
        Assert.That(rod.DependsOn, Is.EqualTo(dependsOn), "DependsOn not as expected");
        Assert.That(rod.PropertyValues.Contains("Age"), Is.True);
    }

    [Test]
    public void Simple()
    {
        GenericApplicationContext ctx = new GenericApplicationContext();
        IObjectDefinitionFactory objectDefinitionFactory = new DefaultObjectDefinitionFactory();

        ObjectDefinitionBuilder builder = ObjectDefinitionBuilder.RootObjectDefinition(objectDefinitionFactory,
            typeof(TestObject));

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

        Assert.That(to, Is.Not.Null);

        Assert.That(to.Name, Is.EqualTo("Joe"));
        Assert.That(to.Age, Is.EqualTo(22));
        Assert.That(to.Spouse.Friends.Count, Is.EqualTo(2));
        Assert.That(to.Spouse.Age, Is.EqualTo(23));

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
