using NUnit.Framework;
using Spring.Objects;

/*
 * Copyright � 2002-2011 the original author or authors.
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

namespace Spring.Context.Support;

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
    {
    }

    /// <summary>
    /// Test using a custom parser to create our familiar "TestObject"
    /// </summary>
    [Test]
    public void UsingCustomParsers()
    {
        ContextRegistry.Clear();
        IApplicationContext ctx = ContextRegistry.GetContext();
        Assert.That(ctx, Is.Not.Null);

        IApplicationContext parentCtx = ContextRegistry.GetContext("Parent");
        Assert.That(parentCtx, Is.Not.Null, "Parent context not registered.");

        TestObject to = (TestObject) ctx.GetObject("Parent");
        Assert.That(to, Is.Not.Null);
        Assert.That(TestObjectConfigParser.ParseElementCalled, Is.True);

        TestObject to2 = (TestObject) ctx.GetObject("testObject");
        Assert.That(to2.Age, Is.EqualTo(12));
        Assert.That(to2.Name, Is.EqualTo("John"));

        Assert.That(ctx.ObjectDefinitionCount, Is.EqualTo(2));
    }
}
