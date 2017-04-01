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

using System;
using System.Collections;
using System.Reflection;

using FakeItEasy;

using NUnit.Framework;
using Spring.Aop.Interceptor;
using Spring.Aop.Support;
using Spring.Collections;
using Spring.Objects;

namespace Spring.Aop.Framework.DynamicProxy
{
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
            Assert.IsFalse(AopUtils.CanApply(pointcut, typeof (Object), null));
        }

        [Test]
        public void PointcutAlwaysApplies()
        {
            Assert.IsTrue(AopUtils.CanApply(new DefaultPointcutAdvisor(new NopInterceptor()), typeof (Object), null));
            Assert.IsTrue(AopUtils.CanApply(new DefaultPointcutAdvisor(new NopInterceptor()), typeof (TestObject), new Type[] {typeof (ITestObject)}));
        }

        [Test]
        public void PointcutAppliesToOneMethodOnObject()
        {
            IPointcut pointcut = new OneMethodTestPointcut();
            Assert.IsTrue(AopUtils.CanApply(pointcut, typeof (object), null),
                          "Must return true if we're not proxying interfaces.");
            Assert.IsFalse(AopUtils.CanApply(pointcut, typeof (object),
                                             new Type[] {typeof (ITestObject)}),
                           "Must return false if we're proxying interfaces.");
        }

        [Test]
        public void PointcutAppliesToOneInterfaceOfSeveral()
        {
            IPointcut pointcut = new OneInterfaceTestPointcut();

            // Will return true if we're proxying interfaces including ITestObject
            Assert.IsTrue(AopUtils.CanApply(pointcut, typeof (TestObject), new Type[] {typeof (ITestObject), typeof (IComparable)}));

            // Will return true if we're proxying interfaces including ITestObject
            Assert.IsFalse(AopUtils.CanApply(pointcut, typeof (TestObject), new Type[] {typeof (IComparable)}));
        }

        [Test]
        public void CanApplyWithAdvisorYieldsTrueIfAdvisorIsNotKnownAdvisorType()
        {
            IAdvisor advisor = A.Fake<IAdvisor>();
            Assert.IsTrue(AopUtils.CanApply(advisor, typeof (TestObject), null));
        }

        [Test]
        public void CanApplyWithAdvisorYieldsTrueIfAdvisorIsNull()
        {
            Assert.IsTrue(AopUtils.CanApply((IAdvisor) null, typeof (TestObject), null));
        }

        /// <summary>
        /// Test preconditions for all tests related to GetAllInterfaces
        /// </summary>
        [Test]
        public void GetAllInterfacesTestsPreconditions()
        {
            DerivedTestObject testObject = new DerivedTestObject();
            IList interfaces = new ArrayList(testObject.GetType().GetInterfaces());
            Assert.AreEqual(9, interfaces.Count, "Incorrect number of interfaces");
            Assert.IsTrue(interfaces.Contains(typeof(ITestObject)), "Does not contain ITestObject");
            Assert.IsTrue(interfaces.Contains(typeof(IOther)), "Does not contain IOther");
        }

        [Test]
        public void GetAllInterfacesWithNull()
        {
            Type[] interfaces = AopUtils.GetAllInterfaces(null);
            Assert.IsNotNull(interfaces,
                             "Must never return null, even if the argument is null.");
            Assert.AreEqual(0, interfaces.Length,
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
            Assert.IsNotNull(interfaces,
                             "Must never return null, even if the argument doesn't implement any interfaces.");
            Assert.AreEqual(0, interfaces.Length,
                            "Must return an empty array is the argument doesn't implement any interfaces.");
        }

        [Test]
        public void GetAllInterfacesSunnyDay()
        {
            ImplementsTwoInterfaces instance = new ImplementsTwoInterfaces();
            Type[] interfaces = AopUtils.GetAllInterfaces(instance);
            Assert.IsNotNull(interfaces, "Must never return null.");
            Assert.AreEqual(2, interfaces.Length,
                            "Implements two interfaces.");
            ISet ifaces = new ListSet(interfaces);
            Assert.IsTrue(
                ifaces.ContainsAll(
                    new Type [] {typeof(IDisposable), typeof(ICloneable)}),
                "Did not find the correct interfaces.");
        }

        [Test]
        public void GetAllInterfacesWithObjectThatInheritsInterfaces()
        {
            InheritsOneInterface instance = new InheritsOneInterface();
            Type[] interfaces = AopUtils.GetAllInterfaces(instance);
            Assert.IsNotNull(interfaces, "Must never return null.");
            Assert.AreEqual(1, interfaces.Length,
                            "Inherited one interface from superclass.");
            Type iface = interfaces[0];
            Assert.IsNotNull(iface, "Returned interface cannot be null.");
            Assert.AreEqual(typeof(IDisposable), iface, "Wrong interface returned.");
        }

        [Test]
        public void CanApplyWithTrueIntroductionAdvisor()
        {
            IIntroductionAdvisor mockIntroAdvisor = A.Fake<IIntroductionAdvisor>();
            A.CallTo(() => mockIntroAdvisor.TypeFilter).Returns(TrueTypeFilter.True);

            Assert.IsTrue(AopUtils.CanApply(mockIntroAdvisor, typeof (TestObject), null));
        }

        #region Helper Classes

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

        #endregion
    }
}