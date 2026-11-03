/*
 * Copyright 2004 the original author or authors.
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

using NUnit.Framework;
using Spring.Objects.Factory.Config;

namespace Spring.Objects.Factory.Support;

/// <summary>
/// Unit tests for the SimpleInstantiationStrategy class.
/// </summary>
/// <author>Rick Evans</author>
[TestFixture]
public sealed class SimpleInstantiationStrategyTests
{
    /// <summary>
    /// The setup logic executed before the execution of each individual test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _singletonDefinition = new RootObjectDefinition(typeof(TestObject), AutoWiringMode.No);
        _singletonDefinitionWithFactory = new RootObjectDefinition(_singletonDefinition);
        _singletonDefinitionWithFactory.FactoryMethodName = "GetObject";
        _singletonDefinitionWithFactory.FactoryObjectName = "TestObjectFactoryDefinition";
        _testObjectFactory = new RootObjectDefinition(typeof(TestObjectFactory), AutoWiringMode.No);
        DefaultListableObjectFactory myFactory = new DefaultListableObjectFactory();
        myFactory.RegisterObjectDefinition("SingletonObjectDefinition", SingletonDefinition);
        myFactory.RegisterObjectDefinition("SingletonDefinitionWithFactory", SingletonDefinitionWithFactory);
        myFactory.RegisterObjectDefinition("TestObjectFactoryDefinition", TestObjectFactoryDefinition);
        _factory = myFactory;
    }

    [Test]
    public void InstantiateWithNulls()
    {
        SimpleInstantiationStrategy strategy = new SimpleInstantiationStrategy();
        Assert.Throws<ArgumentNullException>(() => strategy.Instantiate(null, null, null));
    }

    [Test]
    public void InstantiateWithNullObjectName()
    {
        SimpleInstantiationStrategy strategy = new SimpleInstantiationStrategy();
        object obj = strategy.Instantiate(SingletonDefinition, null, Factory);
        Assert.That(obj, Is.Not.Null);
        Assert.That(obj is ITestObject, Is.True);
        ITestObject actual = (ITestObject) obj;
        Assert.That(actual.Name, Is.Null);
        Assert.That(actual.Age, Is.EqualTo(0));
    }

    [Test]
    public void InstantiateWithExplicitCtor()
    {
        SimpleInstantiationStrategy strategy = new SimpleInstantiationStrategy();
        object obj = strategy.Instantiate(
            SingletonDefinition, null, Factory,
            SingletonDefinition.ObjectType.GetConstructor(
                new Type[] { typeof(string), typeof(int) }),
            new object[] { "Rick", 19 });
        Assert.That(obj, Is.Not.Null);
        Assert.That(obj is ITestObject, Is.True);
        ITestObject actual = (ITestObject) obj;
        Assert.That(actual.Name, Is.EqualTo("Rick"));
        Assert.That(actual.Age, Is.EqualTo(19));
    }

    [Test]
    public void InstantiateWithFactoryMethod()
    {
        SimpleInstantiationStrategy strategy = new SimpleInstantiationStrategy();
        object obj = strategy.Instantiate(SingletonDefinitionWithFactory,
            string.Empty, Factory, typeof(TestObjectFactory).GetMethod("GetObject"), null);
        Assert.That(obj, Is.Not.Null);
        Assert.That(obj is ITestObject, Is.True);
        ITestObject actual = (ITestObject) obj;
        Assert.That(actual.Name, Is.EqualTo(TestObjectFactory.TheName));
        Assert.That(actual.Age, Is.EqualTo(TestObjectFactory.TheAge));
    }

    [Test]
    public void InstantiateWithDefinitionThatDoesNotHaveAResolvedObjectClass()
    {
        RootObjectDefinition def = new RootObjectDefinition();
        def.ObjectTypeName = typeof(TestObject).FullName;

        SimpleInstantiationStrategy strategy = new SimpleInstantiationStrategy();
        object foo = strategy.Instantiate(def, "foo", Factory);
        Assert.That(foo, Is.Not.Null);
        Assert.That(foo.GetType(), Is.EqualTo(typeof(TestObject)));
    }

    private RootObjectDefinition SingletonDefinition
    {
        get { return _singletonDefinition; }
    }

    private RootObjectDefinition SingletonDefinitionWithFactory
    {
        get { return _singletonDefinitionWithFactory; }
    }

    private RootObjectDefinition TestObjectFactoryDefinition
    {
        get { return _testObjectFactory; }
    }

    private IObjectFactory Factory
    {
        get { return _factory; }
    }

    private RootObjectDefinition _singletonDefinition;
    private RootObjectDefinition _testObjectFactory;
    private RootObjectDefinition _singletonDefinitionWithFactory;
    private IObjectFactory _factory;
}

public class TestObjectFactory
{
    public const string TheName = "Old Goriot";
    public const int TheAge = 78;

    public virtual object GetObject()
    {
        return new TestObject(TheName, TheAge);
    }
}
