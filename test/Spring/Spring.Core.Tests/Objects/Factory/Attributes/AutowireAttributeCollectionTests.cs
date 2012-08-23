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

using System.Collections.Generic;
using NUnit.Framework;
using Spring.Context.Support;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;

namespace Spring.Objects.Factory.Attributes
{
    [TestFixture]
    public class AutowireAttributeCollectionTests
    {
        private GenericApplicationContext _applicationContext;

        [SetUp]
        public void Setup()
        {
            _applicationContext = new GenericApplicationContext();

            var objDef = new RootObjectDefinition(typeof(AutowiredAttributeObjectPostProcessor));
            objDef.Role = ObjectRole.ROLE_INFRASTRUCTURE;
            _applicationContext.RegisterObjectDefinition("AutowiredAttributeObjectPostProcessor", objDef);

            objDef = new RootObjectDefinition(typeof(ColFoo1));
            _applicationContext.RegisterObjectDefinition("Foo1", objDef);

            objDef = new RootObjectDefinition(typeof(ColFoo2));
            _applicationContext.RegisterObjectDefinition("Foo2", objDef);

            objDef = new RootObjectDefinition(typeof(ColTestObject1));
            objDef.Scope = "prototype";
            _applicationContext.RegisterObjectDefinition("ColTestObject1", objDef);

            objDef = new RootObjectDefinition(typeof(ColTestObject2));
            objDef.Scope = "prototype";
            _applicationContext.RegisterObjectDefinition("ColTestObject2", objDef);

            objDef = new RootObjectDefinition(typeof(ColTestObject3));
            objDef.Scope = "prototype";
            _applicationContext.RegisterObjectDefinition("ColTestObject3", objDef);

            objDef = new RootObjectDefinition(typeof(ColTestObject4));
            objDef.Scope = "prototype";
            _applicationContext.RegisterObjectDefinition("ColTestObject4", objDef);

            _applicationContext.Refresh();
        }

        [Test]
        public void InjectAListOfObjects()
        {
            var testObj = (ColTestObject1)_applicationContext.GetObject("ColTestObject1");
            var objDef = _applicationContext.ObjectFactory.GetObjectDefinition("ColTestObject1");

            Assert.That(testObj.Count, Is.EqualTo(2));
            Assert.That(objDef.DependsOn.Count, Is.EqualTo(2));
        }

        [Test]
        public void InjectASetOfObjects()
        {
            var testObj = (ColTestObject2)_applicationContext.GetObject("ColTestObject2");

            Assert.That(testObj.Count, Is.EqualTo(2));
        }

        [Test]
        public void InjectADictionaryOfObjects()
        {
            var testObj = (ColTestObject3)_applicationContext.GetObject("ColTestObject3");

            Assert.That(testObj.Count, Is.EqualTo(2));
        }

        [Test]
        public void InjectAnArrayOfObjects()
        {
            var testObj = (ColTestObject4)_applicationContext.GetObject("ColTestObject4");

            Assert.That(testObj.Count, Is.EqualTo(2));
        }
    }

    #region Test Objects

    public interface IColFoo
    {
        string Name();
    }

    public class ColFoo1 : IColFoo
    {
        public string Name()
        {
            return "Foo1";
        }
    }

    public class ColFoo2 : IColFoo
    {
        public string Name()
        {
            return "Foo2";
        }
    }

    public class ColTestObject1
    {
        [Autowired]
        private IList<IColFoo> _col;

        public int Count { get { return _col.Count; } }
    }

    public class ColTestObject2
    {
        [Autowired]
        private Spring.Collections.Generic.ISet<IColFoo> _col;

        public int Count { get { return _col.Count; } }
    }

    public class ColTestObject3
    {
        [Autowired]
        private IDictionary<string, IColFoo> _col;

        public int Count { get { return _col.Count; } }
    }

    public class ColTestObject4
    {
        [Autowired]
        private IColFoo[] _col;

        public int Count { get { return _col.Length; } }
    }

    #endregion

}