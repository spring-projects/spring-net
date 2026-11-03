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

using NUnit.Framework;

namespace Spring.Objects.Factory.Config;

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
        Assert.That(vs.CanResolveVariable("key1"), Is.True); // case insensitive
        Assert.That(vs.ResolveVariable("key1"), Is.EqualTo("Value1")); // case insensitive
        Assert.That(vs.CanResolveVariable("Key2"), Is.True);
        Assert.That(vs.ResolveVariable("Key2"), Is.EqualTo("Value2"));

        // non-existant variable
        Assert.That(vs.CanResolveVariable("Key3"), Is.False);
        Assert.That(vs.ResolveVariable("Key3"), Is.Null);
    }
}
