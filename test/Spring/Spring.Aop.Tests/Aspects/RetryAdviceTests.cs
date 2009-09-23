#region License

/*
 * Copyright © 2002-2007 the original author or authors.
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
using System.Collections;
using NUnit.Framework;
using Spring.Aop.Framework;

#endregion

namespace Spring.Aspects
{
    /// <summary>
    /// This class contains tests for RetryAdvice
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class RetryAdviceTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestSunnyDay()
        {
            InvokeOncePassOnceFail(false, false);
            InvokeOncePassOnceFail(false, true);
            InvokeOncePassOnceFail(true, false);
            InvokeOncePassOnceFail(true, true);

        }

        [Test]
        public void TestUnexpectedException()
        {
            InvokeOnceFailWithUnexceptedException(false, false);
        }

        private static void InvokeOncePassOnceFail(bool useExceptionName, bool isDelay)
        {
            TestSleepHandler testSleepHandler = new TestSleepHandler();
            ITestRemoteService rs = GetRemoteService(2, useExceptionName, isDelay, testSleepHandler);

            rs.DoTransfer();
            AssertSleepsAndReset(testSleepHandler, 2, isDelay);

            rs = GetRemoteService(3, useExceptionName, isDelay, testSleepHandler);
            try
            {
                rs.DoTransfer();
                Assert.Fail("Should have failed.");
            } catch (ArithmeticException)
            {
                // they maximum retry count is 3, thus only 2 sleep calls
                AssertSleepsAndReset(testSleepHandler, 2, isDelay);
            }
        }

        private static void InvokeOnceFailWithUnexceptedException(bool useExceptionName, bool isDelay)
        {
            TestSleepHandler testSleepHandler = new TestSleepHandler();
            ITestRemoteService rs = GetRemoteService(3, useExceptionName, isDelay, testSleepHandler);
            try
            {
                rs.DoTransfer2();
                Assert.Fail("Should have failed.");
            }
            catch (ArgumentException)
            {

            }
        }

        private static void AssertSleepsAndReset(TestSleepHandler testSleepHandler, int numFailures, bool isDelay)
        {
            Assert.AreEqual(numFailures, testSleepHandler.CalledTimeSpans.Count);
            for(int i=0;i<numFailures;i++)
            {
                if (isDelay)
                {
                    Assert.AreEqual(TimeSpan.FromSeconds(1), testSleepHandler.CalledTimeSpans[i]);
                }
                else
                {
                    Assert.AreEqual(TimeSpan.FromSeconds(1 * (i + 1) + 0.5), testSleepHandler.CalledTimeSpans[i]);
                }
            }
            testSleepHandler.CalledTimeSpans.Clear();
        }

        private class TestSleepHandler
        {
            public readonly ArrayList CalledTimeSpans = new ArrayList();

            public void Sleep(TimeSpan duration)
            {
                CalledTimeSpans.Add(duration);
            }
        }

        private static ITestRemoteService GetRemoteService(int numFailures, bool usingExceptionName, bool isDelay, TestSleepHandler testSleepHandler)
        {
            TestRemoteService remoteService = new TestRemoteService();
            remoteService.NumFailures = numFailures;
            ProxyFactory factory = new ProxyFactory(remoteService);

            RetryAdvice retryAdvice = new RetryAdvice(new RetryAdvice.SleepHandler(testSleepHandler.Sleep));
            if (usingExceptionName)
            {
                if (isDelay)
                {
                    retryAdvice.RetryExpression = "on exception name ArithmeticException retry 3x delay 1s";
                }
                else
                {
                    retryAdvice.RetryExpression = "on exception name ArithmeticException retry 3x rate (1*#n + 0.5)";
                }
            }
            else
            {
                if (isDelay)
                {
                    retryAdvice.RetryExpression = "on exception (#e is T(System.ArithmeticException)) retry 3x delay 1s";
                }
                else
                {
                    retryAdvice.RetryExpression = "on exception (#e is T(System.ArithmeticException)) retry 3x rate (1*#n + 0.5)";
                }
            }
            retryAdvice.AfterPropertiesSet();
            factory.AddAdvice(retryAdvice);
            ITestRemoteService rs = factory.GetProxy() as ITestRemoteService;
            Assert.IsNotNull(rs);
            return rs;
        }
    }

    public interface ITestRemoteService
    {
        void DoTransfer();
        void DoTransfer2();
    }

    public class TestRemoteService : ITestRemoteService
    {
        private int numFailures;
        private int count = 0;
        private bool throwArithmeticException = false;


        public int NumFailures
        {
            get { return numFailures; }
            set { numFailures = value; }
        }

        public bool ThrowArithmeticException
        {
            get
            {

                if (count < NumFailures)
                {
                    count++;
                    return true;
                }
                else
                {
                    return throwArithmeticException;
                }
            }
            set { throwArithmeticException = value; }
        }

        public void DoTransfer()
        {
            if (ThrowArithmeticException)
            {
                throw new ArithmeticException("can't do the math");
            }
        }

        public void DoTransfer2()
        {
            throw new ArgumentException("bad argument");
        }
    }
}