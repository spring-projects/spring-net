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

using Spring.Aop.Framework;
using Spring.Aop.Interceptor;
using Spring.Objects;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Xml;
using Spring.Util;
using NUnit.Framework;

namespace Spring.Aop.Support;

/// <summary>
/// Unit tests for RegularExpressionMethodPointcutAdvisorTests.
/// </summary>
/// <author>Rod Johnson</author>
/// <author>Simon White (.NET)</author>
[TestFixture]
public class RegularExpressionMethodPointcutAdvisorTests
{
    [OneTimeSetUp]
    public void FixtureSetUp()
    {
        SystemUtils.RegisterLoadedAssemblyResolver();
    }

    /// <summary>
    /// Basic use case, a single pattern defined.
    /// </summary>
    [Test]
    public void SinglePattern()
    {
        IObjectFactory iof = new XmlObjectFactory(new ReadOnlyXmlTestResource("RegularExpressionSetterTests.xml", GetType()));
        IPerson advised = (IPerson) iof.GetObject("SettersAdvised");
        // Interceptor behind regexp advisor
        NopInterceptor nop = (NopInterceptor) iof.GetObject("NopInterceptor");
        Assert.That(nop.Count, Is.EqualTo(0));

        int newAge = 12;
        // Not advised
        advised.Exceptional(null);
        Assert.That(nop.Count, Is.EqualTo(0));
        advised.SetAge(newAge);
        Assert.That(advised.GetAge(), Is.EqualTo(newAge));
        // Only setter fired
        Assert.That(nop.Count, Is.EqualTo(1));
    }

    /// <summary>
    /// Multiple patterns defined within a single advisor.
    /// </summary>
    [Test]
    public void MultiplePatterns()
    {
        IObjectFactory iof = new XmlObjectFactory(new ReadOnlyXmlTestResource("RegularExpressionSetterTests.xml", GetType()));
        IPerson advised = (IPerson) iof.GetObject("SettersAndReturnsThisAdvised");

        // Interceptor behind regexp advisor
        NopInterceptor nop = (NopInterceptor) iof.GetObject("NopInterceptor");
        Assert.That(nop.Count, Is.EqualTo(0));

        int newAge = 12;
        // Not advised
        advised.Exceptional(null);
        Assert.That(nop.Count, Is.EqualTo(0));

        // This is proxied
        advised.ReturnsThis();
        Assert.That(nop.Count, Is.EqualTo(1));

        // Only setter is advised
        advised.SetAge(newAge);
        Assert.That(nop.Count, Is.EqualTo(2));

        Assert.That(advised.GetAge(), Is.EqualTo(newAge));
        Assert.That(nop.Count, Is.EqualTo(2));
    }

    [Test]
    [Platform("Win")]
    public void Serialization()
    {
        IObjectFactory iof = new XmlObjectFactory(new ReadOnlyXmlTestResource("RegularExpressionSetterTests.xml", GetType()));
        IPerson p = (IPerson) iof.GetObject("SerializableSettersAdvised");
        // Interceptor behind regexp advisor
        NopInterceptor nop = (NopInterceptor) iof.GetObject("NopInterceptor");
        Assert.That(nop.Count, Is.EqualTo(0));

        int newAge = 12;
        // Not advised
        Assert.That(p.GetAge(), Is.EqualTo(0));
        Assert.That(nop.Count, Is.EqualTo(0));

        // This is proxied
        p.SetAge(newAge);
        Assert.That(nop.Count, Is.EqualTo(1));
        p.SetAge(newAge);
        Assert.That(p.GetAge(), Is.EqualTo(newAge));
        // Only setter fired
        Assert.That(nop.Count, Is.EqualTo(2));

        // Serialize and continue...

#if !NETCOREAPP // deep chains for Type serialization problems, not worth the effort at the moment
        p = (IPerson) SerializationTestUtils.SerializeAndDeserialize(p);
        Assert.That(p.GetAge(), Is.EqualTo(newAge));
#endif
        // Remembers count, but we need to get a new reference to nop...
        nop = (SerializableNopInterceptor) ((IAdvised) p).Advisors[0].Advice;
        Assert.That(nop.Count, Is.EqualTo(2));
        Assert.That(p.GetName(), Is.EqualTo("SerializableSettersAdvised"));
        p.SetAge(newAge + 1);
        Assert.That(nop.Count, Is.EqualTo(3));
        Assert.That(p.GetAge(), Is.EqualTo(newAge + 1));
    }
}
