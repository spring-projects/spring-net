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

using AopAlliance.Intercept;
using FakeItEasy;
using NUnit.Framework;

using Spring.Util;

namespace Spring.Aop.Framework.Adapter
{
    /// <summary>
    /// Unit tests for the AfterReturningAdviceInterceptor class.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Simon White (.NET)</author>
    [TestFixture]
    public sealed class AfterReturningAdviceInterceptorTests
    {
        [Test]
        public void PassNullAdviceToCtor()
        {
            Assert.Throws<ArgumentNullException>(() => new AfterReturningAdviceInterceptor(null));
        }

        [Test]
        public void IsNotInvokedIfServiceObjectThrowsException()
        {
            IMethodInvocation mockInvocation = A.Fake<IMethodInvocation>();
            IAfterReturningAdvice mockAdvice = A.Fake<IAfterReturningAdvice>();

            A.CallTo(() => mockAdvice.AfterReturning(null, null, null, null)).WithAnyArguments().Throws<FormatException>();
            A.CallTo(() => mockInvocation.Method).Returns(ReflectionUtils.GetMethod(typeof(object), "HashCode", new Type[] { }));
            A.CallTo(() => mockInvocation.Arguments).Returns(null);
            A.CallTo(() => mockInvocation.This).Returns(new object());
            A.CallTo(() => mockInvocation.Proceed()).Returns(null);

            try
            {
                AfterReturningAdviceInterceptor interceptor = new AfterReturningAdviceInterceptor(mockAdvice);
                interceptor.Invoke(mockInvocation);
                Assert.Fail("Must have thrown a FormatException by this point.");
            }
            catch (FormatException)
            {
            }
        }

        [Test]
        public void JustPassesAfterReturningAdviceExceptionUpWithoutAnyWrapping()
        {
            IMethodInvocation mockInvocation = A.Fake<IMethodInvocation>();
            IAfterReturningAdvice mockAdvice = A.Fake<IAfterReturningAdvice>();
            A.CallTo(() => mockAdvice.AfterReturning(null, null, null, null)).WithAnyArguments().Throws<FormatException>();

            A.CallTo(() => mockInvocation.Method).Returns(ReflectionUtils.GetMethod(typeof(object), "HashCode", new Type[] { }));
            A.CallTo(() => mockInvocation.Arguments).Returns(null);
            A.CallTo(() => mockInvocation.This).Returns(new object());
            A.CallTo(() => mockInvocation.Proceed()).Returns(null);
            try
            {
                AfterReturningAdviceInterceptor interceptor = new AfterReturningAdviceInterceptor(mockAdvice);
                interceptor.Invoke(mockInvocation);
                Assert.Fail("Must have thrown a FormatException by this point.");
            }
            catch (FormatException)
            {
            }
        }
    }
}