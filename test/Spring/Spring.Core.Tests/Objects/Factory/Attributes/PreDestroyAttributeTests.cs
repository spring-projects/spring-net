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

using NUnit.Framework;
using Spring.Context.Support;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;

namespace Spring.Objects.Factory.Attributes
{
    [TestFixture]
    public class PreDestroyAttributeTests
    {
        private GenericApplicationContext _applicationContext;


        [SetUp]
        public void Setup()
        {
            _applicationContext = new GenericApplicationContext();

            var objDef = new RootObjectDefinition(typeof(InitDestroyAttributeObjectPostProcessor));
            objDef.Role = ObjectRole.ROLE_INFRASTRUCTURE;
            _applicationContext.ObjectFactory.RegisterObjectDefinition("InitDestroyAttributeObjectPostProcessor", objDef);

            objDef = new RootObjectDefinition(typeof(PreDestroyTestObject1));
            _applicationContext.ObjectFactory.RegisterObjectDefinition("PreDestroyTestObject1", objDef);

            objDef = new RootObjectDefinition(typeof(PreDestroyTestObject2));
            _applicationContext.ObjectFactory.RegisterObjectDefinition("PreDestroyTestObject2", objDef);

            objDef = new RootObjectDefinition(typeof(PreDestroyTestObject3));
            objDef.DestroyMethodName = "Destroy";
            _applicationContext.ObjectFactory.RegisterObjectDefinition("PreDestroyTestObject3", objDef);

            objDef = new RootObjectDefinition(typeof(PreDestroyTestObject4));
            _applicationContext.ObjectFactory.RegisterObjectDefinition("PreDestroyTestObject4", objDef);

            objDef = new RootObjectDefinition(typeof(PreDestroyTestObject5));
            _applicationContext.ObjectFactory.RegisterObjectDefinition("PreDestroyTestObject5", objDef);

            _applicationContext.Refresh();            
        }

        [Test]
        public void PreDestroyMethodExecution()
        {
            DestroyTester.ExecutionCount1 = 0;
            var testObj = _applicationContext.GetObject("PreDestroyTestObject1");
            _applicationContext.Dispose();

            Assert.That(DestroyTester.ExecutionCount1, Is.EqualTo(1));
        }

        [Test]
        public void PreDestroyInBaseType()
        {
            DestroyTester.ExecutionCount2 = 0;
            var testObj = _applicationContext.GetObject("PreDestroyTestObject2");
            _applicationContext.Dispose();

            Assert.That(DestroyTester.ExecutionCount2, Is.EqualTo(1));
        }

        [Test]
        public void SameMethodDefinedInXml()
        {
            DestroyTester.ExecutionCount3 = 0;
            var testObj = _applicationContext.GetObject("PreDestroyTestObject3");
            _applicationContext.Dispose();

            Assert.That(DestroyTester.ExecutionCount3, Is.EqualTo(1));
        }

        [Test]
        public void InCorrectOrder()
        {
            DestroyTester.ExecutionCount4 = 0;
            var testObj = (PreDestroyTestObject4)_applicationContext.GetObject("PreDestroyTestObject4");
            _applicationContext.Dispose();

            Assert.That(DestroyTester.ExecutionCount4, Is.EqualTo(2));
            Assert.That(DestroyTester.CorrectOrder, Is.True);
        }

        [Test]
        public void WithArgumentMustThrowException()
        {
            Assert.That(() =>
            {
                DestroyTester.ExecutionCount5 = 0;
                var testObj = (PreDestroyTestObject5)_applicationContext.GetObject("PreDestroyTestObject5");
                _applicationContext.Dispose();
            },
                        Throws.Nothing);
        }
    }

    public class PreDestroyTestObject1
    {
        [PreDestroy]
        public void Destroy()
        {
            DestroyTester.ExecutionCount1++;
        }
    }

    public class PreDestroyBase
    {
        [PreDestroy]
        public void Destroy()
        {
            DestroyTester.ExecutionCount2++;
        }
    }

    public class PreDestroyTestObject2 : PreDestroyBase
    {
    }

    public class PreDestroyTestObject3
    {
        [PreDestroy]
        public void Destroy()
        {
            DestroyTester.ExecutionCount3++;
        }
    }

    public class PreDestroyTestObject4
    {
        public PreDestroyTestObject4()
        {
            DestroyTester.Destroy1Called = false;
            DestroyTester.CorrectOrder = false;
        }

        [PreDestroy(Order = 1)]
        public void Destroy1()
        {
            DestroyTester.ExecutionCount4++;
            DestroyTester.Destroy1Called = true;
        }

        [PreDestroy(Order = 2)]
        public void Destroy2()
        {
            DestroyTester.ExecutionCount4++;
            if (DestroyTester.Destroy1Called)
                DestroyTester.CorrectOrder = true;
        }
    }

    public class PreDestroyTestObject5
    {
        [PreDestroy]
        public void Destroy(bool arugment)
        {
            DestroyTester.ExecutionCount5++;
        }
    }


    public static class DestroyTester
    {
        public static int ExecutionCount1 { get; set; }
        public static int ExecutionCount2 { get; set; }
        public static int ExecutionCount3 { get; set; }
        public static int ExecutionCount4 { get; set; }
        public static int ExecutionCount5 { get; set; }
        public static bool Destroy1Called { get; set; }
        public static bool CorrectOrder { get; set; }
    }
}
