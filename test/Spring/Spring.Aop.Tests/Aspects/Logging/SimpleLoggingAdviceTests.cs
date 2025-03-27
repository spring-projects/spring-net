#region License

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

#endregion

using System;
using System.Reflection;

using AopAlliance.Intercept;

using FakeItEasy;
using FakeItEasy.Configuration;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Spring.Aop.Framework;

namespace Spring.Aspects.Logging
{
    /// <summary>
    /// This class contains tests for SimpleLoggingAdvice
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class SimpleLoggingAdviceTests
    {
        public interface ITestTarget
        {
            void DoSomething();
        }

        private class TestTarget : ITestTarget
        {
            public void DoSomething()
            { }
        }

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void IntegrationTest()
        {
            ProxyFactory pf = new ProxyFactory(new TestTarget());

            ILog log = A.Fake<ILog>();
            SimpleLoggingAdvice loggingAdvice = new SimpleLoggingAdvice(log);
            pf.AddAdvice(loggingAdvice);

            A.CallTo(() => log.IsEnabled(LogLevel.Trace)).Returns(true);

            object proxy = pf.GetProxy();
            ITestTarget ptt = (ITestTarget)proxy;
            ptt.DoSomething();

            log.VerifyLogMustHaveHappened(LogLevel.Trace, "Entering DoSomething");
            log.VerifyLogMustHaveHappened(LogLevel.Trace, "Exiting DoSomething");
        }

        [Test]
        public void SunnyDayLoggingCorrectly()
        {
            ILog log = A.Fake<ILog>();
            IMethodInvocation methodInvocation = A.Fake<IMethodInvocation>();

            MethodInfo mi = typeof(string).GetMethod("ToString", Type.EmptyTypes);
            //two additional calls the method are to retrieve the method name on entry/exit...
            A.CallTo(() => methodInvocation.Method).Returns(mi);
            A.CallTo(() => log.IsEnabled(LogLevel.Trace)).Returns(true);
            A.CallTo(() => methodInvocation.Proceed()).Returns(null);

            TestableSimpleLoggingAdvice loggingAdvice = new TestableSimpleLoggingAdvice(true);
            loggingAdvice.CallInvokeUnderLog(methodInvocation, log);

            log.VerifyLogMustHaveHappened(LogLevel.Trace, "Entering ToString");
            log.VerifyLogMustHaveHappened(LogLevel.Trace, "Exiting ToString");
        }

        [Test]
        public void SunnyDayLoggingCorrectlyDebugLevel()
        {
            ILog log = A.Fake<ILog>();
            IMethodInvocation methodInvocation = A.Fake<IMethodInvocation>();

            MethodInfo mi = typeof(string).GetMethod("ToString", Type.EmptyTypes);
            //two additional calls the method are to retrieve the method name on entry/exit...
            A.CallTo(() => methodInvocation.Method).Returns(mi);

            A.CallTo(() => log.IsEnabled(LogLevel.Trace)).Returns(false);
            A.CallTo(() => log.IsEnabled(LogLevel.Debug)).Returns(true);

            A.CallTo(() => methodInvocation.Proceed()).Returns(null);

            TestableSimpleLoggingAdvice loggingAdvice = new TestableSimpleLoggingAdvice(true);
            loggingAdvice.LogLevel = LogLevel.Debug;
            Assert.IsTrue(loggingAdvice.CallIsInterceptorEnabled(methodInvocation, log));
            loggingAdvice.CallInvokeUnderLog(methodInvocation, log);

            log.VerifyLogMustHaveHappened(LogLevel.Debug, "Entering ToString");
            log.VerifyLogMustHaveHappened(LogLevel.Debug, "Exiting ToString");
        }

        [Test]
        public void ExceptionPathStillLogsCorrectly()
        {
            ILog log = A.Fake<ILog>();
            IMethodInvocation methodInvocation = A.Fake<IMethodInvocation>();

            MethodInfo mi = typeof(string).GetMethod("ToString", Type.EmptyTypes);
            //two additional calls the method are to retrieve the method name on entry/exit...
            A.CallTo(() => methodInvocation.Method).Returns(mi);
            A.CallTo(() => log.IsEnabled(LogLevel.Trace)).Returns(true);

            Exception e = new ArgumentException("bad value");
            A.CallTo(() => methodInvocation.Proceed()).Throws(e);

            TestableSimpleLoggingAdvice loggingAdvice = new TestableSimpleLoggingAdvice(true);
            try
            {
                loggingAdvice.CallInvokeUnderLog(methodInvocation, log);
                Assert.Fail("Must have propagated the IllegalArgumentException.");
            }
            catch (ArgumentException)
            {
            }

            log.VerifyLogMustHaveHappened(LogLevel.Trace, "Entering ToString");
            log.VerifyLogMustHaveHappened(LogLevel.Trace, "Exception thrown in ToString, ToString");
        }

        [Test]
        public void SunnyDayLoggingAllOptionalInformationCorrectly()
        {
            ILog log = A.Fake<ILog>();
            IMethodInvocation methodInvocation = A.Fake<IMethodInvocation>();

            MethodInfo mi = typeof(Dog).GetMethod("Bark");
            //two additional calls the method are to retrieve the method name on entry/exit...
            A.CallTo(() => methodInvocation.Method).Returns(mi);
            int[] luckyNumbers = new int[] { 1, 2, 3 };
            object[] args = new object[] { "hello", luckyNumbers };

            A.CallTo(() => methodInvocation.Arguments).Returns(args);
            A.CallTo(() => log.IsEnabled(LogLevel.Trace)).Returns(true);
            A.CallTo(() => methodInvocation.Proceed()).Returns(4);

            TestableSimpleLoggingAdvice loggingAdvice = new TestableSimpleLoggingAdvice(true);
            loggingAdvice.LogExecutionTime = true;
            loggingAdvice.LogMethodArguments = true;
            loggingAdvice.LogUniqueIdentifier = true;

            loggingAdvice.CallInvokeUnderLog(methodInvocation, log);

            log.VerifyLogMustHaveHappened(LogLevel.Trace, "Entering Bark");
            log.VerifyLogMustHaveHappened(LogLevel.Trace, "Exiting Bark");
        }
    }

    public class Dog
    {
        public int Bark(string message, int[] luckyNumbers)
        {
            return 4;
        }
    }

    public static class LoggerExtensions
    {
        public static void VerifyLogMustHaveHappened(this ILogger logger, LogLevel level, string message)
        {
            try
            {
                logger.VerifyLog(level, message)
                    .MustHaveHappened();
            }
            catch (Exception e)
            {
                throw new ExpectationException($"while verifying a call to log with message: \"{message}\"", e);
            }
        }

        public static void VerifyLogMustNotHaveHappened(this ILogger logger, LogLevel level, string message)
        {
            try
            {
                logger.VerifyLog(level, message)
                    .MustNotHaveHappened();
            }
            catch (Exception e)
            {
                throw new ExpectationException($"while verifying a call to log with message: \"{message}\"", e);
            }
        }

        public static IVoidArgumentValidationConfiguration VerifyLog(this ILogger logger, LogLevel level,
            string message)
        {
            return A.CallTo(() => logger.Log(
                level,
                A<EventId>._,
                A<object>.That.Matches(e => e.ToString().Contains(message)),
                A<Exception>._,
                A<Func<object, Exception, string>>._)
            );
        }
    }
}
