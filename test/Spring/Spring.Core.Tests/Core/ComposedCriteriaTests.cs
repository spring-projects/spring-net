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

using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Spring.Core;

[TestFixture]
public class ComposedCriteriaTests : ComposedCriteria
{
    public ComposedCriteriaTests() : base() { }
    public ComposedCriteriaTests(ICriteria criteria) : base(criteria) { }

    [Test]
    public void IsSatisfiedWithNoCriteria()
    {
        ComposedCriteriaTests composedCriteria = new ComposedCriteriaTests();
        Assert.That(composedCriteria.IsSatisfied("foo"), Is.True);
    }

    [Test]
    public void SatifiesMyUpperCaseCriteria()
    {
        ComposedCriteriaTests composedCriteria = new ComposedCriteriaTests(new MyUpperCaseCriteria());
        Assert.That(composedCriteria.IsSatisfied("HELLO"), Is.True);
        Assert.That(composedCriteria.IsSatisfied("hello"), Is.False);
    }

    [Test]
    public void SatifiesTwoCriteria()
    {
        ComposedCriteriaTests composedCriteria = new ComposedCriteriaTests();
        composedCriteria.Add(new MyUpperCaseCriteria());
        composedCriteria.Add(new MyStringCriteria());
        Assert.That(composedCriteria.IsSatisfied("HELLO"), Is.True);
        Assert.That(composedCriteria.IsSatisfied("GOODBYE"), Is.False);
        Assert.That(composedCriteria.Criteria.Count == 2, Is.True);
    }

    internal class MyUpperCaseCriteria : ICriteria
    {
        public bool IsSatisfied(object datum)
        {
            return Regex.Match((string) datum, "[A-Z]").Success;
        }
    }

    internal class MyStringCriteria : ICriteria
    {
        public bool IsSatisfied(object datum)
        {
            return datum.ToString().ToLower() == "hello";
        }
    }
}
