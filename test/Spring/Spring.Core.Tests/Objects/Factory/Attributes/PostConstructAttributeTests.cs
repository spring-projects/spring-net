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
    public class PostConstructAttributeTests
    {
        private GenericApplicationContext _applicationContext;
       

        [SetUp]
        public void Setup()
        {
            _applicationContext = new GenericApplicationContext();

            var objDef = new RootObjectDefinition(typeof (InitDestroyAttributeObjectPostProcessor));
            objDef.Role = ObjectRole.ROLE_INFRASTRUCTURE;
            _applicationContext.ObjectFactory.RegisterObjectDefinition("InitDestroyAttributeObjectPostProcessor", objDef);

            objDef = new RootObjectDefinition(typeof(PostContructTestObject1));
            _applicationContext.ObjectFactory.RegisterObjectDefinition("PostContructTestObject1", objDef);

            objDef = new RootObjectDefinition(typeof(PostContructTestObject2));
            _applicationContext.ObjectFactory.RegisterObjectDefinition("PostContructTestObject2", objDef);
            
            objDef = new RootObjectDefinition(typeof(PostContructTestObject3));
            _applicationContext.ObjectFactory.RegisterObjectDefinition("PostContructTestObject3", objDef);

            objDef = new RootObjectDefinition(typeof(PostContructTestObject4));
            objDef.Scope = "prototype";
            _applicationContext.ObjectFactory.RegisterObjectDefinition("PostContructTestObject4", objDef);

            objDef = new RootObjectDefinition(typeof(PostContructTestObject5));
            _applicationContext.ObjectFactory.RegisterObjectDefinition("PostContructTestObject5", objDef);

            objDef = new RootObjectDefinition(typeof(PostContructTestObject1));
            objDef.InitMethodName = "Init1";
            _applicationContext.ObjectFactory.RegisterObjectDefinition("PostContructTestObject6", objDef);

            _applicationContext.Refresh();
        }


        [Test]
        public void PostContructMethodExecuted()
        {
            var testObj = (PostContructTestObject1)_applicationContext.GetObject("PostContructTestObject1");

            Assert.That(testObj.InitCalled, Is.EqualTo(1));
        }

        [Test]
        public void ExecutedInCorrectOrder()
        {
            var testObj = (PostContructTestObject2)_applicationContext.GetObject("PostContructTestObject2");

            Assert.That(testObj.InitCalled, Is.EqualTo(2), "Two PostContruct methods defined, need to have two method calls.");
            Assert.That(testObj.CalledAfter, Is.True, "Order of PostConstruct not followed.");
        }

        [Test]
        public void NoExceptionIfMethodHasReturnValue()
        {
            var testObj = (PostContructTestObject3)_applicationContext.GetObject("PostContructTestObject3");

            Assert.That(testObj.InitCalled, Is.EqualTo(1), "A PostContruct method with return value should run the init method.");
        }

        [Test]
        public void WithArgumentMustThrowException()
        {
            Assert.That(() => { _applicationContext.GetObject("PostContructTestObject4"); }, Throws.Exception.TypeOf<ObjectCreationException>());
        }

        [Test]
        public void AttributeOnbaseTypeMethod()
        {
            var testObj = (PostContructTestObject5)_applicationContext.GetObject("PostContructTestObject5");

            Assert.That(testObj.InitCalled, Is.EqualTo(1));
        }

        [Test]
        public void SameMethodDefinedInXml()
        {
            var testObj = (PostContructTestObject1)_applicationContext.GetObject("PostContructTestObject6");

            Assert.That(testObj.InitCalled, Is.EqualTo(1));
        }
    }


    public class PostContructTestObject1
    {
        public int InitCalled { get; set; }

        [PostConstruct]
        public void Init1()
        {
            InitCalled++;
        }
    }

    public class PostContructTestObject2
    {
        public int InitCalled { get; set; }
        public bool Init1Called { get; set; }
        public bool CalledAfter { get; set; }

        [PostConstruct(Order = 1)]
        public void Init1()
        {
            InitCalled++;
            Init1Called = true;
        }

        [PostConstruct(Order = 2)]
        public void Init2()
        {
            InitCalled++;
            if (Init1Called)
                CalledAfter = true;
        }
    }

    public class PostContructTestObject3
    {
        public int InitCalled { get; set; }

        [PostConstruct]
        private bool Init1()
        {
            InitCalled++;
            return true;
        }
    }

    public class PostContructTestObject4
    {
        public int InitCalled { get; set; }

        [PostConstruct]
        private void Init1(bool enabled)
        {
            InitCalled++;
        }
    }

    public class PostContructTestObject5 : PostContructTestObject1
    {

    }


}
