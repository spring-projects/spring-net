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

using NUnit.Framework;
using Spring.Objects.Factory.Config;

namespace Spring.Objects.Factory.Support;

/// <summary>
/// </summary>
/// <author>Erich Eichinger</author>
[TestFixture]
public class WebObjectDefinitionTests
{
    [Test]
    public void UnderstandsObjectScopeSingleton()
    {
        RootWebObjectDefinition rwod = new RootWebObjectDefinition(typeof(object), new ConstructorArgumentValues(), new MutablePropertyValues());
        rwod.IsSingleton = true;
        Assert.That(rwod.IsPage, Is.False);
        Assert.That(rwod.IsSingleton, Is.True);
        Assert.That(rwod.IsPrototype, Is.False);
        Assert.That(((IWebObjectDefinition) rwod).Scope, Is.EqualTo(ObjectScope.Singleton));
        Assert.That(((IWebObjectDefinition) rwod).Scope, Is.EqualTo(ObjectScope.Application));
    }

    [Test]
    public void UnderstandsObjectScopePrototype()
    {
        RootWebObjectDefinition rwod = new RootWebObjectDefinition(typeof(object), new ConstructorArgumentValues(), new MutablePropertyValues());
        rwod.IsSingleton = false;
        Assert.That(rwod.IsPage, Is.False);
        Assert.That(rwod.IsSingleton, Is.False);
        Assert.That(rwod.IsPrototype, Is.True);
        Assert.That(((IWebObjectDefinition) rwod).Scope, Is.EqualTo(ObjectScope.Prototype));
    }

//        [Test]
//        public void UnderstandsObjectScopeForPages()
//        {
//            RootWebObjectDefinition rwod = new RootWebObjectDefinition("pagename.aspx", new MutablePropertyValues());            
//            Assert.That(rwod.IsPage, Is.True);
//            Assert.That(rwod.IsSingleton, Is.False);
//            Assert.That(rwod.IsPrototype, Is.True);
//            Assert.That(((IWebObjectDefinition)rwod).Scope, Is.EqualTo(ObjectScope.Prototype));
//        }
}
