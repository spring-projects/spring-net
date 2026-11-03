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

using System.Collections;
using NUnit.Framework;

namespace Spring.Objects.Support;

/// <summary>
///
/// </summary>
/// <author>Erich Eichinger</author>
[TestFixture]
public class AbstractSharedStateFactoryTests
{
    private class TestSharedStateFactory : AbstractSharedStateFactory
    {
        public static readonly object SPECIAL_OBJECT = new object();

        protected override object GetKey(object instance, string name)
        {
            if (instance == SPECIAL_OBJECT) return null;
            return name + "|" + instance.GetHashCode();
        }
    }

    [Test]
    public void DefaultsToCaseInsensitiveState()
    {
        TestSharedStateFactory p = new TestSharedStateFactory();
        Assert.That(p.CaseSensitiveState, Is.False);
        IDictionary state = p.GetSharedStateFor(new object(), "no name");
        state["foo"] = this;
        Assert.That(state["FOO"], Is.SameAs(this));
    }

    [Test]
    public void StateDictionaryBehavesAccordingToCaseSensitiveState()
    {
        TestSharedStateFactory p = new TestSharedStateFactory();

        // create case-insensitive dict
        Assert.That(p.CaseSensitiveState, Is.False);
        IDictionary state = p.GetSharedStateFor(new object(), "no name");
        state["foo"] = this;
        Assert.That(state["FOO"], Is.SameAs(this));

        // create case-sensitive dict
        p.CaseSensitiveState = true;
        state = p.GetSharedStateFor(new object(), "no name");
        state["foo"] = this;
        Assert.That(state.Contains("FOO"), Is.False);
    }

    [Test]
    public void ThrowsOnNullInstance()
    {
        TestSharedStateFactory p = new TestSharedStateFactory();
        // allow "null" for name
        IDictionary state = p.GetSharedStateFor(new object(), null);
        Assert.That(state, Is.Not.Null);
        // throws on null for instance
        Assert.Throws<ArgumentNullException>(() => p.GetSharedStateFor(null, "no name"));
    }

    [Test]
    public void SharedStateCacheIsCaseSensitive()
    {
        TestSharedStateFactory p = new TestSharedStateFactory();
        // allow "null" for name
        IDictionary state = p.GetSharedStateFor(this, "foo");
        IDictionary state2 = p.GetSharedStateFor(this, "FOO");
        Assert.That(state, Is.Not.Null);
        Assert.That(state2, Is.Not.Null);
        Assert.That(state2, Is.Not.SameAs(state));
    }

    [Test]
    public void ReturnsNullStateIfKeyIsNull()
    {
        TestSharedStateFactory p = new TestSharedStateFactory();
        // force provider to produce a null key
        IDictionary state = p.GetSharedStateFor(TestSharedStateFactory.SPECIAL_OBJECT, null);
        Assert.That(state, Is.Null);
    }
}
