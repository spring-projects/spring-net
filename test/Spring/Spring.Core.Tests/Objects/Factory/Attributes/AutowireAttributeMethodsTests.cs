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
using System.Collections.Generic;
using NUnit.Framework;
using Spring.Context.Support;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;

namespace Spring.Objects.Factory.Attributes
{
    [TestFixture]
    public class AutowireAttributeMethodsTests
    {
        private GenericApplicationContext _applicationContext;

        [SetUp]
        public void Setup()
        {
            _applicationContext = new GenericApplicationContext();

            var objDef = new RootObjectDefinition(typeof(AutowiredAttributeObjectPostProcessor));
            objDef.Role = ObjectRole.ROLE_INFRASTRUCTURE;
            _applicationContext.RegisterObjectDefinition("AutowiredAttributeObjectPostProcessor", objDef);

            objDef = new RootObjectDefinition(typeof(Simple));
            _applicationContext.ObjectFactory.RegisterObjectDefinition("Simple", objDef);

            objDef = new RootObjectDefinition(typeof(MethodHello));
            _applicationContext.ObjectFactory.RegisterObjectDefinition("MethodHello", objDef);

            objDef = new RootObjectDefinition(typeof(MethodCiao));
            _applicationContext.ObjectFactory.RegisterObjectDefinition("MethodCiao", objDef);

            objDef = new RootObjectDefinition(typeof(MethodTestObject1));
            objDef.Scope = "prototype";
            _applicationContext.ObjectFactory.RegisterObjectDefinition("MethodTestObject1", objDef);

            objDef = new RootObjectDefinition(typeof(MethodTestObject2));
            objDef.Scope = "prototype";
            _applicationContext.ObjectFactory.RegisterObjectDefinition("MethodTestObject2", objDef);

            objDef = new RootObjectDefinition(typeof(MethodTestObject3));
            objDef.Scope = "prototype";
            _applicationContext.ObjectFactory.RegisterObjectDefinition("MethodTestObject3", objDef);

            objDef = new RootObjectDefinition(typeof(MethodTestObject4));
            objDef.Scope = "prototype";
            _applicationContext.ObjectFactory.RegisterObjectDefinition("MethodTestObject4", objDef);

            objDef = new RootObjectDefinition(typeof(MethodTestObject5));
            objDef.Scope = "prototype";
            _applicationContext.ObjectFactory.RegisterObjectDefinition("MethodTestObject5", objDef);

            objDef = new RootObjectDefinition(typeof(MethodTestObject6));
            objDef.Scope = "prototype";
            _applicationContext.ObjectFactory.RegisterObjectDefinition("MethodTestObject6", objDef);

            objDef = new RootObjectDefinition(typeof(MethodTestObject7));
            objDef.Scope = "prototype";
            _applicationContext.ObjectFactory.RegisterObjectDefinition("MethodTestObject7", objDef);

            objDef = new RootObjectDefinition(typeof(MethodTestObject8));
            objDef.Scope = "prototype";
            _applicationContext.ObjectFactory.RegisterObjectDefinition("MethodTestObject8", objDef);

            objDef = new RootObjectDefinition(typeof(MethodTestObject9));
            objDef.Scope = "prototype";
            _applicationContext.ObjectFactory.RegisterObjectDefinition("MethodTestObject9", objDef);

            _applicationContext.Refresh();
        }

        [Test]
        public void InObjectByType()
        {
            var testObj = (MethodTestObject1)_applicationContext.GetObject("MethodTestObject1");
            var objectDefinition = _applicationContext.ObjectFactory.GetObjectDefinition("MethodTestObject1");

            Assert.That(testObj.GetObject(), Is.Not.Null);
            Assert.That(objectDefinition.DependsOn.Count, Is.EqualTo(1));
        }

        [Test]
        public void InjectByParamterName()
        {
            var testObj = (MethodTestObject2)_applicationContext.GetObject("MethodTestObject2");

            Assert.That(testObj.GetObject(), Is.Not.Null);
        }

        [Test]
        public void InjectByQualifier()
        {
            var testObj = (MethodTestObject3)_applicationContext.GetObject("MethodTestObject3");

            Assert.That(testObj.GetObject(), Is.Not.Null);
        }

        [Test]
        public void FailIfTypeCantBeResolved()
        {
            Exception ex = null;
            try
            {
                var testObj = (FldTestObject5)_applicationContext.GetObject("MethodTestObject4");
            }
            catch (Exception e) { ex = e; }

            Assert.That(ex, Is.Not.Null, "Should throw an exception");
            Assert.That(ex.Message, Is.StringContaining("Injection of autowired dependencies failed"));
        }

        [Test]
        public void InjectListOfObjects()
        {
            var testObj = (MethodTestObject5)_applicationContext.GetObject("MethodTestObject5");

            Assert.That(testObj.GetObject(), Is.Not.Null);
            Assert.That(testObj.GetObject().Count, Is.EqualTo(2));
        }

        [Test]
        public void InjectSeveralParameters()
        {
            var testObj = (MethodTestObject6)_applicationContext.GetObject("MethodTestObject6");
            var objectDefinition = _applicationContext.ObjectFactory.GetObjectDefinition("MethodTestObject6");

            Assert.That(testObj.GetObject(), Is.Not.Null);
            Assert.That(testObj.GetSimple(), Is.Not.Null);
            Assert.That(objectDefinition.DependsOn.Count, Is.EqualTo(2));
        }

        [Test]
        public void FailIfObjecNotAvailable()
        {
            Assert.That(delegate { var testObj = (MethodTestObject7)_applicationContext.GetObject("MethodTestObject7"); },
                Throws.Exception.TypeOf<ObjectCreationException>());
        }

        [Test]
        public void PassIfObjectNotavailableButNotRequired()
        {
            var testObj1 = (MethodTestObject8)_applicationContext.GetObject("MethodTestObject8");
            var objectDefinition = _applicationContext.ObjectFactory.GetObjectDefinition("MethodTestObject8");

            Assert.That(testObj1.GetSimple(), Is.Null);
            Assert.That(objectDefinition.DependsOn.Count, Is.EqualTo(0));

            var testObj2 = (MethodTestObject9)_applicationContext.GetObject("MethodTestObject9");
            objectDefinition = _applicationContext.ObjectFactory.GetObjectDefinition("MethodTestObject9");

            Assert.That(testObj2.GetrAdvanced(), Is.Null);
            Assert.That(objectDefinition.DependsOn.Count, Is.EqualTo(0));
        }

    }


    #region Test Objects

    public interface ISimple
    {
        string Foo();
    }

    public class Simple : ISimple
    {
        public string Foo()
        {
            return "simple";
        }
    }

    public interface IAdvanced
    {
        
    }

    public interface IMethodFoo
    {
        string Foo();
    }

    public class MethodHello : IMethodFoo
    {
        public string Foo()
        {
            return "hello";
        }
    }

    public class MethodCiao : IMethodFoo
    {
        public string Foo()
        {
            return "ciao";
        }
    }

    public class MethodTestObject1
    {
        private ISimple _impl;

        [Autowired]
        public void Prepare(ISimple impl)
        {
            _impl = impl;
        }

        public object GetObject()
        {
            return _impl;
        }
    }

    public class MethodTestObject2
    {
        private IMethodFoo _methodCiao;

        [Autowired]
        public void Prepare(IMethodFoo methodCiao)
        {
            _methodCiao = methodCiao;
        }

        public object GetObject()
        {
            return _methodCiao;
        }
    }

    public class MethodTestObject3
    {
        private IMethodFoo _impl;

        [Autowired]
        public void Prepare([Qualifier("MethodHello")] IMethodFoo impl)
        {
            _impl = impl;
        }

        public object GetObject()
        {
            return _impl;
        }
    }

    public class MethodTestObject4
    {
        private IMethodFoo _impl;

        [Autowired]
        public void Prepare([Qualifier("MethodHello")] IMethodFoo impl, string test)
        {
            _impl = impl;
        }

        public object GetObject()
        {
            return _impl;
        }
    }

    public class MethodTestObject5
    {
        private IList<IMethodFoo> _impl;

        [Autowired]
        public void Prepare(IList<IMethodFoo> impl)
        {
            _impl = impl;
        }

        public IList<IMethodFoo> GetObject()
        {
            return _impl;
        }
    }

    public class MethodTestObject6
    {
        private IMethodFoo _impl;
        private ISimple _simple;

        [Autowired]
        public void Prepare(IMethodFoo methodHello, ISimple simple)
        {
            _impl = methodHello;
            _simple = simple;
        }

        public object GetObject()
        {
            return _impl;
        }

        public object GetSimple()
        {
            return _simple;
        }
    }

    public class MethodTestObject7
    {
        private ISimple _simple;

        [Autowired]
        public void Prepare([Qualifier("NotAvailable")] ISimple simple)
        {
            _simple = simple;
        }

        public object GetSimple()
        {
            return _simple;
        }
    }

    public class MethodTestObject8
    {
        private ISimple _simple;

        [Autowired(Required = false)]
        public void Prepare([Qualifier("NotAvailable")] ISimple simple)
        {
            _simple = simple;
        }

        public object GetSimple()
        {
            return _simple;
        }
    }

    public class MethodTestObject9
    {
        private IAdvanced _advanced;

        [Autowired(Required = false)]
        public void Prepare(IAdvanced advanced)
        {
            _advanced = advanced;
        }

        public object GetrAdvanced()
        {
            return _advanced;
        }
    }

    #endregion
}
