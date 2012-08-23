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

using System;
using NUnit.Framework;
using Spring.Context.Support;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;

namespace Spring.Objects.Factory.Attributes
{
    [TestFixture]
    public class AutowireAttributeFieldTest
    {
        private GenericApplicationContext _applicationContext;

        [SetUp]
        public void Setup()
        {
            _applicationContext = new GenericApplicationContext();

            var objDef = new RootObjectDefinition(typeof(AutowiredAttributeObjectPostProcessor));
            objDef.Role = ObjectRole.ROLE_INFRASTRUCTURE;
            _applicationContext.RegisterObjectDefinition("AutowiredAttributeObjectPostProcessor", objDef);


            objDef = new RootObjectDefinition(typeof(FldFooImpl));
            _applicationContext.RegisterObjectDefinition("FldFoo", objDef);

            objDef = new RootObjectDefinition(typeof(FldHello));
            _applicationContext.RegisterObjectDefinition("FldHello", objDef);

            objDef = new RootObjectDefinition(typeof(FldCioa));
            _applicationContext.RegisterObjectDefinition("FldCioa", objDef);

            objDef = new RootObjectDefinition(typeof(FldHola));
            _applicationContext.RegisterObjectDefinition("FldHola", objDef);

            objDef = new RootObjectDefinition(typeof(FldTestObject1));
            objDef.Scope = "prototype";
            _applicationContext.RegisterObjectDefinition("FldTestObject1", objDef);

            objDef = new RootObjectDefinition(typeof(FldTestObject2));
            objDef.Scope = "prototype";
            _applicationContext.RegisterObjectDefinition("FldTestObject2", objDef);

            objDef = new RootObjectDefinition(typeof(FldTestObject3));
            objDef.Scope = "prototype";
            _applicationContext.RegisterObjectDefinition("FldTestObject3", objDef);

            objDef = new RootObjectDefinition(typeof(FldTestObject4));
            objDef.Scope = "prototype";
            _applicationContext.RegisterObjectDefinition("FldTestObject4", objDef);

            objDef = new RootObjectDefinition(typeof(FldTestObject5));
            objDef.Scope = "prototype";
            _applicationContext.RegisterObjectDefinition("FldTestObject5", objDef);

            objDef = new RootObjectDefinition(typeof(FldTestObject6));
            objDef.Scope = "prototype";
            _applicationContext.RegisterObjectDefinition("FldTestObject6", objDef);

            objDef = new RootObjectDefinition(typeof(FldTestObject7));
            objDef.Scope = "prototype";
            _applicationContext.RegisterObjectDefinition("FldTestObject7", objDef);

            _applicationContext.Refresh();
        }

        [Test]
        public void InjectPropertyBasedOnFieldType()
        {
            var testObj = (FldTestObject1)_applicationContext.GetObject("FldTestObject1");
            var objectDefinition = _applicationContext.ObjectFactory.GetObjectDefinition("FldTestObject1");

            Assert.That(testObj.Test(), Is.EqualTo("foo"));
            Assert.That(objectDefinition.DependsOn.Count, Is.EqualTo(1));
        }

        [Test]
        public void WithTwoTypesRegisteredAndNoNameShouldFail()
        {
            Assert.That(delegate { var testObj = (FldTestObject2)_applicationContext.GetObject("FldTestObject2"); }, Throws.Exception.TypeOf<ObjectCreationException>());
        }

        [Test]
        public void WithQualifierName()
        {
            var testObj = (FldTestObject3)_applicationContext.GetObject("FldTestObject3");

            Assert.That(testObj.Say(), Is.EqualTo("cioa"));            
        }

        [Test]
        public void WithTwoTypesAndNoQualifierUsePopertyName()
        {
            var testObj = (FldTestObject4)_applicationContext.GetObject("FldTestObject4");

            Assert.That(testObj.Say(), Is.EqualTo("cioa"));
        }

        [Test]
        public void FailIfTypeCantBeResolved()
        {
            Exception ex = null;
            try
            {
                var testObj = (FldTestObject5)_applicationContext.GetObject("FldTestObject5");
            }
            catch (Exception e) { ex = e; }

            Assert.That(ex, Is.Not.Null, "Should throw an exception");
            Assert.That(ex.Message, Is.StringContaining("Injection of autowired dependencies failed"));
        }

        [Test]
        public void InjectedObjectAssignedToTwoInterfaces()
        {
            var testObj = (FldTestObject6)_applicationContext.GetObject("FldTestObject6");

            Assert.That(testObj.Test(), Is.EqualTo("test"));
        }

        [Test]
        public void IsNotRequired()
        {
            var testObj = (FldTestObject7)_applicationContext.GetObject("FldTestObject7");

            Assert.That(testObj.IsNull(), Is.True);
        }

    }


    #region Test Objects

    public interface IFldNotAnObject
    {
        
    }

    public interface IFldAnotherOne
    {
        string Quite();
    }

    public interface IFldFoo
    {
        string Test();
    }

    public class FldFooImpl : IFldFoo
    {
        public string Test()
        {
            return "foo";
        }
    }

    public interface IFldSay
    {
        string Say();
    }

    public class FldHello : IFldSay
    {
        public string Say()
        {
            return "hello";
        }
    }

    public class FldCioa : IFldSay
    {
        public string Say()
        {
            return "cioa";
        }
    }

    public class FldHola : IFldFoo, IFldAnotherOne
    {
        public string Quite()
        {
            return "test";
        }

        public string Test()
        {
            return "test";
        }
    }

    public class FldTestObject1
    {
        [Autowired]
        private IFldFoo _fldFoo;

        public string Test()
        {
            return _fldFoo.Test();
        }
    }

    // object with 2 possibilities should fail
    public class FldTestObject2
    {
        [Autowired]
        private IFldSay _wrongName;

        public string Say()
        {
            return _wrongName.Say();
        }
    }

    // should not fail but inject Cioa object
    public class FldTestObject3
    {
        [Autowired]
        [Qualifier("FldCioa")]
        private IFldSay _fldCioa;

        public string Say()
        {
            return _fldCioa.Say();
        }
    }

    // should not fail but inject Hello via Propertyname
    public class FldTestObject4
    {
        [Autowired]
        private IFldSay _fldCioa;

        public string Say()
        {
            return _fldCioa.Say();
        }
    }

    // should not fail but inject Hello via Propertyname
    public class FldTestObject5
    {
        [Autowired]
        private IFldNotAnObject _ohhh;
    }

    public class FldTestObject6
    {
        [Autowired]
        private IFldAnotherOne _hola;

        public string Test()
        {
            return _hola.Quite();
        }
    }

    public class FldTestObject7
    {
        [Autowired(Required = false)]
        private IFldNotAnObject _nono;

        public bool IsNull()
        {
            return (_nono == null);
        }
    }

    #endregion

}
