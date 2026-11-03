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

using System.Collections;
using FakeItEasy;
using NUnit.Framework;
using Spring.Collections;
using Spring.Context.Support;
using Spring.Core.TypeResolution;
using Spring.Util;
using Spring.Context;

namespace Spring.Objects.Factory.Config;

/// <summary>
/// Unit tests for the TypeAliasConfigurer class
/// </summary>
/// <author>Mark Pollack</author>
[TestFixture]
public class TypeAliasConfigurerTests
{
    private IConfigurableListableObjectFactory factory;

    [SetUp]
    public void SetUp()
    {
        factory = A.Fake<IConfigurableListableObjectFactory>();
    }

    [Test]
    public void Serialization()
    {
        IDictionary typeAliases = new Hashtable();
        typeAliases.Add("LinkedList", typeof(LinkedList).AssemblyQualifiedName);

        TypeAliasConfigurer typeAliasConfigurer = new TypeAliasConfigurer();
        typeAliasConfigurer.TypeAliases = typeAliases;

        typeAliasConfigurer.Order = 1;

        SerializationTestUtils.SerializeAndDeserialize(typeAliasConfigurer);
    }

    [Test]
    public void UseInvalidTypeForDictionaryValue()
    {
        IDictionary typeAliases = new Hashtable();
        typeAliases.Add("LinkedList", new LinkedList());

        TypeAliasConfigurer typeAliasConfigurer = new TypeAliasConfigurer();
        typeAliasConfigurer.TypeAliases = typeAliases;

        Assert.Throws<ObjectInitializationException>(() => typeAliasConfigurer.PostProcessObjectFactory(factory));
    }

    [Test]
    public void UseNonResolvableTypeForDictionaryValue()
    {
        IDictionary typeAliases = new Hashtable();
        typeAliases.Add("LinkedList", "Spring.Collections.LinkkkedList");

        TypeAliasConfigurer typeAliasConfigurer = new TypeAliasConfigurer();
        typeAliasConfigurer.TypeAliases = typeAliases;

        Assert.Throws<ObjectInitializationException>(() => typeAliasConfigurer.PostProcessObjectFactory(factory));
    }

    [Test]
    public void SunnyDayScenarioUsingType()
    {
        IDictionary typeAliases = new Hashtable();
        typeAliases.Add("LinkedList", typeof(LinkedList));

        CreateConfigurerAndTestLinkedList(typeAliases);
    }

    [Test]
    public void SunnyDayScenarioUsingTypeString()
    {
        IDictionary typeAliases = new Hashtable();
        typeAliases.Add("LinkedList", "Spring.Collections.LinkedList, Spring.Core");
        CreateConfigurerAndTestLinkedList(typeAliases);
    }

    [Test]
    public void WithinApplicationContext()
    {
        IApplicationContext ctx = new XmlApplicationContext("file://Spring/Objects/Factory/Config/TypeAliases.xml");

        object obj1 = ctx.GetObject("testObject1");
        Assert.That(obj1, Is.Not.Null);
        Assert.That(obj1.GetType(), Is.EqualTo(typeof(TestObject)));

        object obj2 = ctx.GetObject("testObject2");
        Assert.That(obj2, Is.Not.Null);
        Assert.That(obj2.GetType(), Is.EqualTo(typeof(TestObject)));

        object obj3 = ctx.GetObject("testObject3");
        Assert.That(obj3, Is.Not.Null);
        Assert.That(obj3.GetType(), Is.EqualTo(typeof(TestObject)));
        Assert.That(((TestObject) obj3).Name, Is.EqualTo("Bruno"));
        Assert.That(((TestObject) obj3).Age, Is.EqualTo(26));

        // SPRNET-1119
        object obj4 = ctx.GetObject("testObject4");
        Assert.That(obj4, Is.Not.Null);
        Assert.That(obj4.GetType(), Is.EqualTo(typeof(TestObject)));
        Assert.That(((TestObject) obj4).Name, Is.EqualTo("Bruno"));
        Assert.That(((TestObject) obj4).Age, Is.EqualTo(30));

        object obj6 = ctx.GetObject("testObject6");
        Assert.That(obj6, Is.Not.Null);
        Assert.That(obj6.GetType(), Is.EqualTo(typeof(TestObject)));
        Assert.That(((TestObject) obj6).Name, Is.EqualTo("name from section"));
        Assert.That(((TestObject) obj6).Age, Is.EqualTo(27));

        object obj5 = ctx.GetObject("testObject5");
        Assert.That(obj5, Is.Not.Null);
        Assert.That(obj5.GetType(), Is.EqualTo(typeof(TestObject)));
        Assert.That(((TestObject) obj5).Name, Is.EqualTo("overide-name"));
        Assert.That(((TestObject) obj5).Age, Is.EqualTo(26));

        object vpc = ctx.GetObject("vpc");
        Assert.That(vpc, Is.Not.Null);
        Assert.That(vpc.GetType(), Is.EqualTo(typeof(VariablePlaceholderConfigurer)));
    }

    private void CreateConfigurerAndTestLinkedList(IDictionary typeAliases)
    {
        TypeAliasConfigurer typeAliasConfigurer = new TypeAliasConfigurer();
        typeAliasConfigurer.TypeAliases = typeAliases;

        typeAliasConfigurer.Order = 1;

        typeAliasConfigurer.PostProcessObjectFactory(factory);

        //todo investigate mocking the typeregistry, for now ask the actual one for information.
        Assert.That(TypeRegistry.ContainsAlias("LinkedList"),
            Is.True,
            "TypeAliasConfigurer did not register a type alias with the TypeRegistry");

        Type linkedListType = TypeRegistry.ResolveType("LinkedList");
        Assert.That(linkedListType.Equals(typeof(LinkedList)), Is.True, "Incorrect type resolved.");
        Assert.That(typeAliasConfigurer.Order, Is.EqualTo(1));
    }
}
