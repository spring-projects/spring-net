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

using System;
using NUnit.Framework;
using Spring.Context.Support;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;

namespace Spring.Objects.Factory.Attributes
{
    [TestFixture]
    public class AutowireAttributePropertyTest
    {
        private GenericApplicationContext _applicationContext;

        [SetUp]
        public void Setup()
        {
            _applicationContext = new GenericApplicationContext();

            var objDef = new RootObjectDefinition(typeof(AutowiredAttributeObjectPostProcessor));
            objDef.Role = ObjectRole.ROLE_INFRASTRUCTURE;
            _applicationContext.RegisterObjectDefinition("AutowiredAttributeObjectPostProcessor", objDef);


            objDef = new RootObjectDefinition(typeof(PropFooImpl));
            _applicationContext.RegisterObjectDefinition("PropFoo", objDef);

            objDef = new RootObjectDefinition(typeof(PropHello));
            _applicationContext.RegisterObjectDefinition("PropHello", objDef);

            objDef = new RootObjectDefinition(typeof(PropCioa));
            _applicationContext.RegisterObjectDefinition("PropCioa", objDef);

            objDef = new RootObjectDefinition(typeof(PropTestObject1));
            objDef.Scope = "prototype";
            _applicationContext.RegisterObjectDefinition("PropTestObject1", objDef);

            objDef = new RootObjectDefinition(typeof(PropTestObject2));
            objDef.Scope = "prototype";
            _applicationContext.RegisterObjectDefinition("PropTestObject2", objDef);

            objDef = new RootObjectDefinition(typeof(PropTestObject3));
            objDef.Scope = "prototype";
            _applicationContext.RegisterObjectDefinition("PropTestObject3", objDef);

            objDef = new RootObjectDefinition(typeof(PropTestObject4));
            objDef.Scope = "prototype";
            _applicationContext.RegisterObjectDefinition("PropTestObject4", objDef);

            objDef = new RootObjectDefinition(typeof(PropTestObject5));
            objDef.Scope = "prototype";
            _applicationContext.RegisterObjectDefinition("PropTestObject5", objDef);

            _applicationContext.Refresh();
        }

        [Test]
        public void InjectPropertyBasedOnPropertyType()
        {
            var testObj = (PropTestObject1)_applicationContext.GetObject("PropTestObject1");
            var objectDefinition = _applicationContext.ObjectFactory.GetObjectDefinition("PropTestObject1");

            Assert.That(testObj.Foo, Is.Not.Null);
            Assert.That(testObj.Test(), Is.EqualTo("foo"));
            Assert.That(objectDefinition.DependsOn.Count, Is.EqualTo(1), "Should have one Dependant Object");
        }

        [Test]
        public void WithTwoTypesRegisteredAndNoNameShouldFail()
        {
            Assert.That(delegate { var testObj = (PropTestObject2)_applicationContext.GetObject("PropTestObject2"); }, Throws.Exception.TypeOf<ObjectCreationException>());
        }

        [Test]
        public void WithQualifierName()
        {
            var testObj = (PropTestObject3)_applicationContext.GetObject("PropTestObject3");

            Assert.That(testObj.Cioa, Is.Not.Null);
            Assert.That(testObj.Say(), Is.EqualTo("cioa"));            
        }

        [Test]
        public void WithTwoTypesAndNoQualifierUsePopertyName()
        {
            var testObj = (PropTestObject4)_applicationContext.GetObject("PropTestObject4");

            Assert.That(testObj.PropCioa, Is.Not.Null);
            Assert.That(testObj.Say(), Is.EqualTo("cioa"));
        }

        [Test]
        public void FailIfTypeCantBeResolved()
        {
            Exception ex = null;
            try
            {
                var testObj = (FldTestObject5)_applicationContext.GetObject("PropTestObject5");
            }
            catch (Exception e) { ex = e; }

            Assert.That(ex, Is.Not.Null, "Should throw an exception");
            Assert.That(ex.Message, Is.StringContaining("Injection of autowired dependencies failed"));
        }

    }

    #region Test Objects

    public interface IPropNotAnObject
    {
        
    }

    public interface IPropFoo
    {
        string Test();
    }

    public class PropFooImpl : IPropFoo
    {
        public string Test()
        {
            return "foo";
        }
    }

    public interface IPropSay
    {
        string PropSay();
    }

    public class PropHello : IPropSay
    {
        public string PropSay()
        {
            return "hello";
        }
    }

    public class PropCioa : IPropSay
    {
        public string PropSay()
        {
            return "cioa";
        }
    }

    public class PropTestObject1
    {
        private IPropFoo _foo;

        [Autowired]
        public IPropFoo Foo
        {
            get { return _foo; }
            set { _foo = value; }
        }

        public string Test()
        {
            return _foo.Test();
        }
    }

    public class PropTestObject2
    {
        private IPropSay _hello;

        [Autowired]
        public IPropSay WrongName
        {
            get { return _hello; }
            set { _hello = value; }
        }

        public string Say()
        {
            return _hello.PropSay();
        }
    }

    // should not fail but inject Cioa object
    public class PropTestObject3
    {
        private IPropSay _cioa;

        [Autowired]
        [Qualifier("PropCioa")]
        public IPropSay Cioa
        {
            get { return _cioa; } 
            set { _cioa = value; }
        }

        public string Say()
        {
            return _cioa.PropSay();
        }
    }

    // should not fail but inject Hello via Propertyname
    public class PropTestObject4
    {
        private IPropSay _obj;

        [Autowired]
        public IPropSay PropCioa
        {
            get { return _obj; }
            set { _obj = value; }
        }

        public string Say()
        {
            return _obj.PropSay();
        }
    }

    // should not fail but inject Hello via Propertyname
    public class PropTestObject5
    {
        private IPropNotAnObject _obj;

        [Autowired]
        public IPropNotAnObject Ohhh
        {
            get { return _obj; }
            set { _obj = value; }
        }
    }

    #endregion

}
