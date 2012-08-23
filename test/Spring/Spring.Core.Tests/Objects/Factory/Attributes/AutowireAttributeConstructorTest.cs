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
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spring.Context.Support;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;

namespace Spring.Objects.Factory.Attributes
{
    [TestFixture]
    public class AutowireAttributeConstructorTest
    {
        private GenericApplicationContext _applicationContext;

        [SetUp]
        public void Setup()
        {
            _applicationContext = new GenericApplicationContext();

            var objDef = new RootObjectDefinition(typeof(AutowiredAttributeObjectPostProcessor));
            objDef.Role = ObjectRole.ROLE_INFRASTRUCTURE;
            _applicationContext.RegisterObjectDefinition("AutowiredAttributeObjectPostProcessor", objDef);

            objDef = new RootObjectDefinition(typeof(ConsSimple));
            _applicationContext.ObjectFactory.RegisterObjectDefinition("ConsSimple", objDef);

            objDef = new RootObjectDefinition(typeof(ConsAdvanced));
            _applicationContext.ObjectFactory.RegisterObjectDefinition("ConsAdvanced", objDef);

            objDef = new RootObjectDefinition(typeof(ConsHello));
            _applicationContext.ObjectFactory.RegisterObjectDefinition("ConsHello", objDef);

            objDef = new RootObjectDefinition(typeof(ConsCiao));
            _applicationContext.ObjectFactory.RegisterObjectDefinition("ConsCiao", objDef);

            objDef = new RootObjectDefinition(typeof(ConsTestObject1));
            objDef.Scope = "prototype";
            _applicationContext.ObjectFactory.RegisterObjectDefinition("ConsTestObject1", objDef);

            objDef = new RootObjectDefinition(typeof(ConsTestObject2));
            objDef.Scope = "prototype";
            _applicationContext.ObjectFactory.RegisterObjectDefinition("ConsTestObject2", objDef);

            objDef = new RootObjectDefinition(typeof(ConsTestObject3));
            objDef.Scope = "prototype";
            _applicationContext.ObjectFactory.RegisterObjectDefinition("ConsTestObject3", objDef);

            objDef = new RootObjectDefinition(typeof(ConsTestObject4));
            objDef.Scope = "prototype";
            _applicationContext.ObjectFactory.RegisterObjectDefinition("ConsTestObject4", objDef);

            objDef = new RootObjectDefinition(typeof(ConsTestObject5));
            objDef.Scope = "prototype";
            _applicationContext.ObjectFactory.RegisterObjectDefinition("ConsTestObject5", objDef);

            objDef = new RootObjectDefinition(typeof(ConsTestObject6));
            objDef.Scope = "prototype";
            _applicationContext.ObjectFactory.RegisterObjectDefinition("ConsTestObject6", objDef);

            objDef = new RootObjectDefinition(typeof(ConsTestObject7));
            objDef.Scope = "prototype";
            _applicationContext.ObjectFactory.RegisterObjectDefinition("ConsTestObject7", objDef);

            _applicationContext.Refresh();
        }

        [Test]
        public void ConstructorWithInjectParameter()
        {
            var testObj = (ConsTestObject1)_applicationContext.GetObject("ConsTestObject1");

            Assert.That(testObj.IsSet(), Is.True);
        }

        [Test]
        public void SelectInjectConstructorOverDefault()
        {
            var testObj = (ConsTestObject2)_applicationContext.GetObject("ConsTestObject2");

            Assert.That(testObj.IsSet(), Is.True);
        }

        [Test]
        public void SelectConstructorWithMostParamters()
        {
            var testObj = (ConsTestObject3)_applicationContext.GetObject("ConsTestObject3");

            Assert.That(testObj.IsSet(), Is.True);
        }

        [Test]
        public void FailIfParameterAreRequired()
        {
            Exception ex = null;
            try
            {
                var testObj = (ConsTestObject4)_applicationContext.GetObject("ConsTestObject4");
            }
            catch (Exception e)  { ex = e; }

            Assert.That(ex, Is.Not.Null, "Exception should be thrown");
            Assert.That(ex.Message, Is.StringContaining("Unsatisfied dependency expressed"));
        }

        [Test]
        public void InjectCollection()
        {
            var testObj = (ConsTestObject5)_applicationContext.GetObject("ConsTestObject5");

            Assert.That(testObj.IsSet(), Is.True);
            Assert.That(testObj.ObjectCount(), Is.EqualTo(2));
        }


        [Test]
        public void SetParameterByParameterName()
        {
            var testObj = (ConsTestObject6)_applicationContext.GetObject("ConsTestObject6");

            Assert.That(testObj.IsSet(), Is.True);
        }

        [Test]
        public void SetParameterByQualifier()
        {
            var testObj = (ConsTestObject7)_applicationContext.GetObject("ConsTestObject7");

            Assert.That(testObj.IsSet(), Is.True);
            Assert.That(testObj.CorrectObject(), Is.True);
        }
    }

    #region Test Objects

    public interface IConsSimple
    {
        string Message();
    }

    public class ConsSimple : IConsSimple
    {
        public string Message()
        {
            return "ok";
        }
    }

    public interface IConsAdvanced
    {
        string Message();
    }

    public class ConsAdvanced : IConsAdvanced
    {
        public string Message()
        {
            return "ok";
        }
    }

    public interface INotSet
    {
    }

    public interface IConsCol
    {
        string Message();
    }

    public class ConsHello : IConsCol
    {
        public string Message()
        {
            return "hello";
        }
    }

    public class ConsCiao : IConsCol
    {
        public string Message()
        {
            return "ciao";
        }
    }

    public class ConsTestObject1
    {
        private IConsSimple _consSimple;

        [Autowired]
        public ConsTestObject1(IConsSimple consSimple)
        {
            _consSimple = consSimple;
        }

        public bool IsSet()
        {
            return _consSimple != null;
        }
    }

    public class ConsTestObject2
    {
        private IConsSimple _consSimple;

        public ConsTestObject2()
        {
        }

        [Autowired]
        public ConsTestObject2(IConsSimple consSimple)
        {
            _consSimple = consSimple;
        }

        public bool IsSet()
        {
            return _consSimple != null;
        }
    }

    public class ConsTestObject3
    {
        private IConsSimple _consSimple;
        private IConsAdvanced _consAdvanced;

        [Autowired(Required = false)]
        public ConsTestObject3(IConsSimple consSimple)
        {
            _consSimple = consSimple;
        }

        [Autowired(Required = false)]
        public ConsTestObject3(IConsSimple consSimple, IConsAdvanced consAdvanced)
        {
            _consSimple = consSimple;
            _consAdvanced = consAdvanced;
        }

        public bool IsSet()
        {
            return _consSimple != null && _consAdvanced != null;
        }
    }

    public class ConsTestObject4
    {
        private INotSet _notSet;

        [Autowired]
        public ConsTestObject4(INotSet notSet)
        {
            _notSet = notSet;
        }

        public bool IsSet()
        {
            return _notSet != null;
        }
    }

    public class ConsTestObject5
    {
        private IDictionary<string,IConsCol> _consCol;

        [Autowired]
        public ConsTestObject5(IDictionary<string, IConsCol> consCol)
        {
            _consCol = consCol;
        }

        public bool IsSet()
        {
            return _consCol != null;
        }

        public int ObjectCount()
        {
            return _consCol.Count;
        }
    }

    public class ConsTestObject6
    {
        private IConsCol _consHello;

        [Autowired]
        public ConsTestObject6(IConsCol consHello)
        {
            _consHello = consHello;
        }

        public bool IsSet()
        {
            return _consHello != null;
        }
    }

    public class ConsTestObject7
    {
        private IConsCol _consHello;

        [Autowired]
        public ConsTestObject7([Qualifier("ConsCiao")] IConsCol consHello)
        {
            _consHello = consHello;
        }

        public bool IsSet()
        {
            return _consHello != null;
        }

        public bool CorrectObject()
        {
            return _consHello.Message() == "ciao";
        }
    }

    #endregion
}
