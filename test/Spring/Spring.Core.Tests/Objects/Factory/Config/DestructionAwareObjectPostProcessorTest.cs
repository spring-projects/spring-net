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
using Spring.Context.Support;
using Spring.Objects.Factory.Support;

namespace Spring.Objects.Factory.Config
{
    [TestFixture]
    public class DestructionAwareObjectPostProcessorTest
    {
        private GenericApplicationContext _context;

        [SetUp]
        public void Setup()
        {
            _context = new GenericApplicationContext();

            var objectDefinition = new RootObjectDefinition(typeof(DestructionPostProcessor));
            objectDefinition.Role = ObjectRole.ROLE_INFRASTRUCTURE;
            _context.ObjectFactory.RegisterObjectDefinition("DestructionPostProcessor", objectDefinition);

            var objectDef = new RootObjectDefinition(typeof(DestroyTester));
            _context.ObjectFactory.RegisterObjectDefinition("DestroyTester", objectDef);

            _context.Refresh();
        }

        [Test]
        public void PostProcessBeforeDestructionIsCalled()
        {
            var testObj = _context.GetObject("DestroyTester");
            _context.Dispose();

            Assert.That(DestructionTester.MethodCalled, Is.EqualTo(1));
        }
    }

    public class DestructionPostProcessor : IDestructionAwareObjectPostProcessor
    {
        public void PostProcessBeforeDestruction(object instance, string name)
        {
            if (instance.GetType() == typeof(DestroyTester))
                DestructionTester.MethodCalled++;
        }

        public object PostProcessBeforeInitialization(object instance, string name)
        {
            return instance;
        }

        public object PostProcessAfterInitialization(object instance, string objectName)
        {
            return instance;
        }
    }

    public class DestroyTester
    {
    
    }

    public static class DestructionTester
    {
        public static int MethodCalled { get; set; }
    }
}
