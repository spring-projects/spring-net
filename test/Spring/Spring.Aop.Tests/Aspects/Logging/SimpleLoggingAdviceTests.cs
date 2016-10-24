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

#region Imports

using System;
using System.Reflection;

using AopAlliance.Intercept;
using Common.Logging;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Aop.Framework;

#endregion

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

        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
        }

        [Test]
        public void IntegrationTest()
        {
            ProxyFactory pf = new ProxyFactory(new TestTarget());

            ILog log = (ILog)mocks.CreateMock(typeof(ILog));
            SimpleLoggingAdvice loggingAdvice = new SimpleLoggingAdvice(log);
            pf.AddAdvice(loggingAdvice);

            Expect.Call(log.IsTraceEnabled).Return(true).Repeat.Any();
            log.Trace("Entering DoSomething");
            log.Trace("Exiting DoSomething");

            mocks.ReplayAll();

            object proxy = pf.GetProxy();
            ITestTarget ptt = (ITestTarget)proxy;
            ptt.DoSomething();

            mocks.VerifyAll();
        }

        [Test]
        public void SunnyDayLoggingCorrectly()
        {
            ILog log = (ILog)mocks.CreateMock(typeof(ILog));
            IMethodInvocation methodInvocation = (IMethodInvocation)mocks.CreateMock(typeof(IMethodInvocation));

            MethodInfo mi = typeof(string).GetMethod("ToString", Type.EmptyTypes);
            //two additional calls the method are to retrieve the method name on entry/exit...
            Expect.Call(methodInvocation.Method).Return(mi).Repeat.Any();

            Expect.Call(log.IsTraceEnabled).Return(true).Repeat.Any();
            log.Trace("Entering ToString");

            Expect.Call(methodInvocation.Proceed()).Return(null);

            log.Trace("Exiting ToString");

            mocks.ReplayAll();

            TestableSimpleLoggingAdvice loggingAdvice = new TestableSimpleLoggingAdvice(true);
            loggingAdvice.CallInvokeUnderLog(methodInvocation, log);

            mocks.VerifyAll();

        }

        [Test]
        public void SunnyDayLoggingCorrectlyDebugLevel()
        {
            ILog log = (ILog)mocks.CreateMock(typeof(ILog));
            IMethodInvocation methodInvocation = (IMethodInvocation)mocks.CreateMock(typeof(IMethodInvocation));

            MethodInfo mi = typeof(string).GetMethod("ToString", Type.EmptyTypes);
            //two additional calls the method are to retrieve the method name on entry/exit...
            Expect.Call(methodInvocation.Method).Return(mi).Repeat.Any();

            Expect.Call(log.IsTraceEnabled).Return(false).Repeat.Any();
            Expect.Call(log.IsDebugEnabled).Return(true).Repeat.Any();
            log.Debug("Entering ToString");

            Expect.Call(methodInvocation.Proceed()).Return(null);

            log.Debug("Exiting ToString");

            mocks.ReplayAll();

            TestableSimpleLoggingAdvice loggingAdvice = new TestableSimpleLoggingAdvice(true);
            loggingAdvice.LogLevel = LogLevel.Debug;
            Assert.IsTrue(loggingAdvice.CallIsInterceptorEnabled(methodInvocation, log));
            loggingAdvice.CallInvokeUnderLog(methodInvocation, log);

            mocks.VerifyAll();

        }


        [Test]
        public void ExceptionPathStillLogsCorrectly()
        {
            ILog log = (ILog)mocks.CreateMock(typeof(ILog));
            IMethodInvocation methodInvocation = (IMethodInvocation)mocks.CreateMock(typeof(IMethodInvocation));

            MethodInfo mi = typeof(string).GetMethod("ToString", Type.EmptyTypes);
            //two additional calls the method are to retrieve the method name on entry/exit...
            Expect.Call(methodInvocation.Method).Return(mi).Repeat.Any();

            Expect.Call(log.IsTraceEnabled).Return(true).Repeat.Any();
            log.Trace("Entering...");

            LastCall.On(log).IgnoreArguments();

            Exception e = new ArgumentException("bad value");
            Expect.Call(methodInvocation.Proceed()).Throw(e);

            log.Trace("Exception...", e);
            LastCall.On(log).IgnoreArguments();

            mocks.ReplayAll();

            TestableSimpleLoggingAdvice loggingAdvice = new TestableSimpleLoggingAdvice(true);
            try
            {
                loggingAdvice.CallInvokeUnderLog(methodInvocation, log);
                Assert.Fail("Must have propagated the IllegalArgumentException.");
            }
            catch (ArgumentException)
            {

            }

            mocks.VerifyAll();

        }


        [Test]
        public void SunnyDayLoggingAllOptionalInformationCorrectly()
        {
            ILog log = (ILog)mocks.CreateMock(typeof(ILog));
            IMethodInvocation methodInvocation = (IMethodInvocation)mocks.CreateMock(typeof(IMethodInvocation));

            MethodInfo mi = typeof(Dog).GetMethod("Bark");
            //two additional calls the method are to retrieve the method name on entry/exit...
            Expect.Call(methodInvocation.Method).Return(mi).Repeat.Any();
            int[] luckyNumbers = new int[] { 1, 2, 3 };
            object[] args = new object[] { "hello", luckyNumbers };


            Expect.Call(methodInvocation.Arguments).Return(args);

            Expect.Call(log.IsTraceEnabled).Return(true).Repeat.Any();
            log.Trace("Entering...");
            LastCall.IgnoreArguments();

            Expect.Call(methodInvocation.Proceed()).Return(4);

            log.Trace("Exiting...");
            LastCall.IgnoreArguments();

            mocks.ReplayAll();

            TestableSimpleLoggingAdvice loggingAdvice = new TestableSimpleLoggingAdvice(true);
            loggingAdvice.LogExecutionTime = true;
            loggingAdvice.LogMethodArguments = true;
            loggingAdvice.LogUniqueIdentifier = true;

            loggingAdvice.CallInvokeUnderLog(methodInvocation, log);

            mocks.VerifyAll();
        }
    }

    public class Dog
    {
        public int Bark(string message, int[] luckyNumbers)
        {
            return 4;
        }
    }
}