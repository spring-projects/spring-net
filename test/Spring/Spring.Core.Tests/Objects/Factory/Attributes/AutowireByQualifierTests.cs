﻿#region License

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

using NUnit.Framework;
using Spring.Context.Support;
using Spring.Objects.Factory.Attributes.ByType;
using AutowireTestConstructorNormal = Spring.Objects.Factory.Attributes.ByQualifier.AutowireTestConstructorNormal;
using AutowireTestFieldNormal = Spring.Objects.Factory.Attributes.ByQualifier.AutowireTestFieldNormal;
using AutowireTestMethodNormal = Spring.Objects.Factory.Attributes.ByQualifier.AutowireTestMethodNormal;
using AutowireTestPropertyNormal = Spring.Objects.Factory.Attributes.ByQualifier.AutowireTestPropertyNormal;

namespace Spring.Objects.Factory.Attributes;

[TestFixture]
public class AutowireByQualifierTests
{
    private XmlApplicationContext _applicationContext;

    [SetUp]
    public void Setup()
    {
        _applicationContext = new XmlApplicationContext(false,
            "assembly://Spring.Core.Tests/Spring.Objects.Factory.Attributes/ByQualifierObjects.xml");
    }

    [Test]
    public void InjectOnField()
    {
        var testObj = (AutowireTestFieldNormal) _applicationContext.GetObject("AutowireTestField");
        var objectDefinition = _applicationContext.ObjectFactory.GetObjectDefinition("AutowireTestField");

        Assert.That(testObj.ciao, Is.Not.Null);
        Assert.That(testObj.ciao.GetType(), Is.EqualTo(typeof(CiaoFoo)));
        Assert.That(objectDefinition.DependsOn.Count, Is.EqualTo(1));
    }

    [Test]
    public void InjectOnProperty()
    {
        var testObj = (AutowireTestPropertyNormal) _applicationContext.GetObject("AutowireTestProperty");
        var objectDefinition = _applicationContext.ObjectFactory.GetObjectDefinition("AutowireTestProperty");

        Assert.That(testObj.Ciao, Is.Not.Null);
        Assert.That(testObj.Ciao.GetType(), Is.EqualTo(typeof(CiaoFoo)));
        Assert.That(objectDefinition.DependsOn.Count, Is.EqualTo(1));
    }

    [Test]
    public void InjectOnMethod()
    {
        var testObj = (AutowireTestMethodNormal) _applicationContext.GetObject("AutowireTestMethod");
        var objectDefinition = _applicationContext.ObjectFactory.GetObjectDefinition("AutowireTestMethod");

        Assert.That(testObj.ciao, Is.Not.Null);
        Assert.That(testObj.ciao.GetType(), Is.EqualTo(typeof(CiaoFoo)));
        Assert.That(objectDefinition.DependsOn.Count, Is.EqualTo(1));
    }

    [Test]
    public void InjectOnConstructor()
    {
        var testObj = (AutowireTestConstructorNormal) _applicationContext.GetObject("AutowireTestConstructor");
        var objectDefinition = _applicationContext.ObjectFactory.GetObjectDefinition("AutowireTestConstructor");

        Assert.That(testObj.ciao, Is.Not.Null);
        Assert.That(testObj.ciao.GetType(), Is.EqualTo(typeof(CiaoFoo)));
        Assert.That(objectDefinition.DependsOn.Count, Is.EqualTo(0));
    }
}
