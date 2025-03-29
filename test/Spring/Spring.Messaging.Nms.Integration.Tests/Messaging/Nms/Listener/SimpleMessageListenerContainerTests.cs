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

using NUnit.Framework;
using Spring.Messaging.Nms.Core;
using Spring.Testing.NUnit;

namespace Spring.Messaging.Nms.Listener;

[TestFixture]
public class SimpleMessageListenerContainerTests : AbstractDependencyInjectionSpringContextTests
{
    protected SimpleGateway simpleGateway;

    protected SimpleMessageListener simpleMessageListener;

    /// <summary>
    /// Enable DI based on protected field names
    /// </summary>
    public SimpleMessageListenerContainerTests()
    {
        this.PopulateProtectedVariables = true;
    }

    [Test]
    public void SendAndRecieveAsync()
    {
        Assert.NotNull(simpleGateway);
        Assert.NotNull(simpleMessageListener);
        Assert.AreEqual(0, simpleMessageListener.MessageCount);
        simpleGateway.Publish("CSCO", 123.45);
        Thread.Sleep(1000);
        Assert.AreEqual(1, simpleMessageListener.MessageCount);
    }

    protected override string[] ConfigLocations
    {
        get { return new string[] { "assembly://Spring.Messaging.Nms.Integration.Tests/Spring.Messaging.Nms.Listener/SimpleMessageListenerContainerTests.xml" }; }
    }
}
