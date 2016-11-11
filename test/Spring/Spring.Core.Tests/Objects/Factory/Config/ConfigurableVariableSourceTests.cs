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

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Unit tests for the ConfigurableVariableSource class.
    /// </summary>
    /// <author>Bruno Baia</author>
    [TestFixture]
    public sealed class ConfigurableVariableSourceTests
    {
        [Test]
        public void Basic()
        {
            ConfigurableVariableSource vs = new ConfigurableVariableSource();
            vs.Variables.Add("Key1", "Value1");
            vs.Variables.Add("Key2", "Value2");

            // existing vars
            Assert.IsTrue(vs.CanResolveVariable("key1")); // case insensitive
            Assert.AreEqual("Value1", vs.ResolveVariable("key1")); // case insensitive
            Assert.IsTrue(vs.CanResolveVariable("Key2"));
            Assert.AreEqual("Value2", vs.ResolveVariable("Key2"));

            // non-existant variable
            Assert.IsFalse(vs.CanResolveVariable("Key3"));
            Assert.IsNull(vs.ResolveVariable("Key3"));
        }
    }
}