#region License

/*
 * Copyright © 2002-2007 the original author or authors.
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

#region Imports

using NUnit.Framework;
using Spring.Testing.NUnit;

#endregion

namespace Spring.Messaging.Core
{
    /// <summary>
    /// This class contains tests for MessageQueueTemplate
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class MessageQueueMetadataCacheTests : AbstractDependencyInjectionSpringContextTests
    {
        protected override string[] ConfigLocations
        {
            get { return new[] {"assembly://Spring.Messaging.Tests/Spring.Messaging.Core/MessageQueueTemplateTests.xml"}; }
        }

        [Test]
        public void InitialzeMessageQueueMetadata()
        {
            var cache = new MessageQueueMetadataCache(applicationContext);
            Assert.AreEqual(0, cache.Count);
            cache.Initialize();
            Assert.IsTrue(cache.Initalized);
            Assert.AreEqual(4, cache.Count);
            MessageQueueMetadata md = cache.Get(@".\Private$\testqueue");
            cache.Remove(@".\Private$\testqueue");
            Assert.AreEqual(3, cache.Count);
            Assert.IsNull(cache.Get(@".\Private$\testqueue"));

            var paths = new[]
                            {
                                @".\Private$\testtxqueue",
                                @"FormatName:Direct=TCP:192.168.1.105\Private$\testtxqueue",
                                @"FormatName:Direct=TCP:192.168.1.105\Private$\testqueue"
                            };

            Assert.That(paths, Is.EquivalentTo(cache.Paths));
            paths = new[] {@".\Private$\testtxqueue", @"FormatName:Direct=TCP:192.168.1.105\Private$\testtxqueue"};
            cache.RemoveAll(paths);
            Assert.AreEqual(1, cache.Count);
            cache.Clear();
            Assert.AreEqual(0, cache.Count);
        }
    }
}