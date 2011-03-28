using NUnit.Framework;

using Spring.Objects;

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

namespace Spring.Context.Support
{
    /// <summary>
    /// Test configuration of the application context with custom parsers,
    /// resource handlers, and type aliases.
    /// </summary>
    [TestFixture]
    public class ApplicationContextExtensionTests
    {
        /// <summary>
        /// Create an instance of the test.
        /// </summary>
        public ApplicationContextExtensionTests()
        {}

        /// <summary>
        /// Test using a custom parser to create our familiar "TestObject"
        /// </summary>
        [Test]
        public void UsingCustomParsers()
        {
            ContextRegistry.Clear();
            IApplicationContext ctx = ContextRegistry.GetContext();
            Assert.IsNotNull(ctx);

            IApplicationContext parentCtx = ContextRegistry.GetContext("Parent");
            Assert.IsNotNull(parentCtx, "Parent context not registered.");

            TestObject to = (TestObject) ctx.GetObject("Parent");
            Assert.IsNotNull(to);
            Assert.IsTrue(TestObjectConfigParser.ParseElementCalled);

            TestObject to2 = (TestObject) ctx.GetObject("testObject");
            Assert.AreEqual(12, to2.Age);
            Assert.AreEqual("John", to2.Name);

            Assert.AreEqual(2, ctx.ObjectDefinitionCount);

        }

    }
}