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
using System.Reflection;
using FakeItEasy;
using NUnit.Framework;
using Spring.Aop.Interceptor;
using Spring.Aop.Support;
using Spring.Collections;
using Spring.Objects;

namespace Spring.Aop.Framework.DynamicProxy;

/// <summary>
/// Unit tests for the AopUtils class.
/// </summary>
/// <author>Rod Johnson</author>
/// <author>Choy Rim (.NET)</author>
[TestFixture]
public sealed class AopUtilsTests
{
    [SetUp]
    public void SetUp()
    {
    }

    [Test]
    public void PointcutCanNeverApply()
    {
        IPointcut pointcut = new NeverMatchesPointcut();
        Assert.That(AopUtils.CanApply(pointcut, typeof(Object), null), Is.False);
    }

    [Test]
    public void PointcutAlwaysApplies()
    {
        Assert.That(AopUtils.CanApply(new DefaultPointcutAdvisor(new NopInterceptor()), typeof(Object), null), Is.True);
        Assert.That(AopUtils.CanApply(new DefaultPointcutAdvisor(new NopInterceptor()), typeof(TestObject), new Type[] { typeof(ITestObject) }), Is.True);
    }

    [Test]
    public void PointcutAppliesToOneMethodOnObject()
    {
        IPointcut pointcut = new OneMethodTestPointcut();
        Assert.That(AopUtils.CanApply(pointcut, typeof(object), null),
            Is.True,
            "Must return true if we're not proxying interfaces.");
        Assert.That(AopUtils.CanApply(pointcut, typeof(object),
                new Type[] { typeof(ITestObject) }),
            Is.False,
            "Must return false if we're proxying interfaces.");
    }

    [Test]
    public void PointcutAppliesToOneInterfaceOfSeveral()
    {
        IPointcut pointcut = new OneInterfaceTestPointcut();

        // Will return true if we're proxying interfaces including ITestObject
        Assert.That(AopUtils.CanApply(pointcut, typeof(TestObject), new Type[] { typeof(ITestObject), typeof(IComparable) }), Is.True);

        // Will return true if we're proxying interfaces including ITestObject
        Assert.That(AopUtils.CanApply(pointcut, typeof(TestObject), new Type[] { typeof(IComparable) }), Is.False);
    }

    [Test]
    public void CanApplyWithAdvisorYieldsTrueIfAdvisorIsNotKnownAdvisorType()
    {
        IAdvisor advisor = A.Fake<IAdvisor>();
        Assert.That(AopUtils.CanApply(advisor, typeof(TestObject), null), Is.True);
    }

    [Test]
    public void CanApplyWithAdvisorYieldsTrueIfAdvisorIsNull()
    {
        Assert.That(AopUtils.CanApply((IAdvisor) null, typeof(TestObject), null), Is.True);
    }

    /// <summary>
    /// Test preconditions for all tests related to GetAllInterfaces
    /// </summary>
    [Test]
    public void GetAllInterfacesTestsPreconditions()
    {
        DerivedTestObject testObject = new DerivedTestObject();
        IList interfaces = new ArrayList(testObject.GetType().GetInterfaces());
        Assert.That(interfaces.Count, Is.EqualTo(9), "Incorrect number of interfaces");
        Assert.That(interfaces.Contains(typeof(ITestObject)), Is.True, "Does not contain ITestObject");
        Assert.That(interfaces.Contains(typeof(IOther)), Is.True, "Does not contain IOther");
    }

    [Test]
    public void GetAllInterfacesWithNull()
    {
        Type[] interfaces = AopUtils.GetAllInterfaces(null);
        Assert.That(interfaces,
            Is.Not.Null,
            "Must never return null, even if the argument is null.");
        Assert.That(interfaces.Length, Is.EqualTo(0),
            "Must return an empty array is the argument is null.");
    }

    [Test]
    public void GetAllInterfacesFromTypeWithNull()
    {
        Assert.Throws<ArgumentNullException>(() => AopUtils.GetAllInterfacesFromType(null));
    }

    [Test]
    public void GetAllInterfacesWithObjectThatDoesntImpementAnything()
    {
        ImplementsNothing instance = new ImplementsNothing();
        Type[] interfaces = AopUtils.GetAllInterfaces(instance);
        Assert.That(interfaces,
            Is.Not.Null,
            "Must never return null, even if the argument doesn't implement any interfaces.");
        Assert.That(interfaces.Length, Is.EqualTo(0),
            "Must return an empty array is the argument doesn't implement any interfaces.");
    }

    [Test]
    public void GetAllInterfacesSunnyDay()
    {
        ImplementsTwoInterfaces instance = new ImplementsTwoInterfaces();
        Type[] interfaces = AopUtils.GetAllInterfaces(instance);
        Assert.That(interfaces, Is.Not.Null, "Must never return null.");
        Assert.That(interfaces.Length, Is.EqualTo(2),
            "Implements two interfaces.");
        ISet ifaces = new ListSet(interfaces);
        Assert.That(
            ifaces.ContainsAll(
                new Type[] { typeof(IDisposable), typeof(ICloneable) }),
            Is.True,
            "Did not find the correct interfaces.");
    }

    [Test]
    public void GetAllInterfacesWithObjectThatInheritsInterfaces()
    {
        InheritsOneInterface instance = new InheritsOneInterface();
        Type[] interfaces = AopUtils.GetAllInterfaces(instance);
        Assert.That(interfaces, Is.Not.Null, "Must never return null.");
        Assert.That(interfaces.Length, Is.EqualTo(1),
            "Inherited one interface from superclass.");
        Type iface = interfaces[0];
        Assert.That(iface, Is.Not.Null, "Returned interface cannot be null.");
        Assert.That(iface, Is.EqualTo(typeof(IDisposable)), "Wrong interface returned.");
    }

    [Test]
    public void CanApplyWithTrueIntroductionAdvisor()
    {
        IIntroductionAdvisor mockIntroAdvisor = A.Fake<IIntroductionAdvisor>();
        A.CallTo(() => mockIntroAdvisor.TypeFilter).Returns(TrueTypeFilter.True);

        Assert.That(AopUtils.CanApply(mockIntroAdvisor, typeof(TestObject), null), Is.True);
    }

    private sealed class OneMethodTestPointcut : StaticMethodMatcherPointcut
    {
        public override bool Matches(MethodInfo m, Type targetClass)
        {
            return m.Name.Equals("GetHashCode");
        }
    }

    private sealed class OneInterfaceTestPointcut : StaticMethodMatcherPointcut
    {
        public override bool Matches(MethodInfo m, Type targetClass)
        {
            return m.Name.Equals("ReturnsThis");
        }
    }

    private sealed class NeverMatchesPointcut : StaticMethodMatcherPointcut
    {
        public override bool Matches(MethodInfo m, Type targetClass)
        {
            return false;
        }
    }

    private sealed class ImplementsNothing
    {
    }

    private abstract class ImplementsOneInterface : IDisposable
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    private sealed class InheritsOneInterface : ImplementsOneInterface
    {
    }

    private sealed class ImplementsTwoInterfaces : IDisposable, ICloneable
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
