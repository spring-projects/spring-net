#region License

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

#endregion

using System;
using AopAlliance.Aop;
using NUnit.Framework;

namespace Spring.Aop.Support
{
    /// <summary>
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class DefaultIntroductionAdvisorTests
    {
        public interface IBaseInterface { }
        public interface IDerivedInterface : IBaseInterface { }

        private class TestIntroductionAdvice : IDerivedInterface, IAdvice
        {
            private object equalsToObject;

            public void SetEqualsToObject(object other)
            {
                equalsToObject = other;
            }

            public override bool Equals(object obj)
            {
                return object.Equals(equalsToObject, obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        [Test]
        public void EqualsOnAdviceEqualAndInterfacesEqual()
        {
            TestIntroductionAdvice to1 = new TestIntroductionAdvice();
            TestIntroductionAdvice to2 = new TestIntroductionAdvice();
            to1.SetEqualsToObject(to2);
            to2.SetEqualsToObject(to1);

            DefaultIntroductionAdvisor a1 = new DefaultIntroductionAdvisor(to1);
            DefaultIntroductionAdvisor a2 = new DefaultIntroductionAdvisor(to2);
            bool result = a1.Equals(a2);

            Assert.IsTrue(result);
        }

        [Test]
        public void BailsIfInterfaceTypeIsNotAnInterface()
        {
            Assert.Throws<ArgumentException>(() => new DefaultIntroductionAdvisor(new TestIntroductionAdvice(), GetType()), "Type [Spring.Aop.Support.DefaultIntroductionAdvisorTests] is not an interface; cannot be used in an introduction.");
        }

        [Test]
        public void IntroductionMustImplementIntroducedInterfaces()
        {
            DefaultIntroductionAdvisor advisor = new DefaultIntroductionAdvisor(new TestIntroductionAdvice(), typeof(ICloneable));
            Assert.Throws<ArgumentException>(() => advisor.ValidateInterfaces(), "Introduction [Spring.Aop.Support.DefaultIntroductionAdvisorTests+TestIntroductionAdvice] does not implement interface 'System.ICloneable' specified in introduction advice.");
        }

        [Test]
        public void BaseInterfacesAreValid()
        {
            DefaultIntroductionAdvisor advisor = new DefaultIntroductionAdvisor(new TestIntroductionAdvice(), typeof(IBaseInterface));
            advisor.ValidateInterfaces();
        }
    }
}