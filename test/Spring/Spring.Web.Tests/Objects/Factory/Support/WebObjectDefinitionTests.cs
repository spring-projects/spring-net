#region License

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
using Spring.Objects.Factory.Config;

namespace Spring.Objects.Factory.Support
{
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
            Assert.IsFalse(rwod.IsPage);
            Assert.IsTrue(rwod.IsSingleton);
            Assert.IsFalse(rwod.IsPrototype);
            Assert.AreEqual(ObjectScope.Singleton, ((IWebObjectDefinition)rwod).Scope);
            Assert.AreEqual(ObjectScope.Application, ((IWebObjectDefinition)rwod).Scope);
        }

        [Test]
        public void UnderstandsObjectScopePrototype()
        {
            RootWebObjectDefinition rwod = new RootWebObjectDefinition(typeof(object), new ConstructorArgumentValues(), new MutablePropertyValues());
            rwod.IsSingleton = false;
            Assert.IsFalse(rwod.IsPage);
            Assert.IsFalse(rwod.IsSingleton);
            Assert.IsTrue(rwod.IsPrototype);
            Assert.AreEqual(ObjectScope.Prototype, ((IWebObjectDefinition)rwod).Scope);
        }

//        [Test]
//        public void UnderstandsObjectScopeForPages()
//        {
//            RootWebObjectDefinition rwod = new RootWebObjectDefinition("pagename.aspx", new MutablePropertyValues());            
//            Assert.IsTrue(rwod.IsPage);
//            Assert.IsFalse(rwod.IsSingleton);
//            Assert.IsTrue(rwod.IsPrototype);
//            Assert.AreEqual(ObjectScope.Prototype, ((IWebObjectDefinition)rwod).Scope);
//        }
    }
}