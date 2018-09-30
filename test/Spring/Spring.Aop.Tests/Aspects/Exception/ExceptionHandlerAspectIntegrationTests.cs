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
using System.Collections;

using Common.Logging;
using Common.Logging.Simple;

using NUnit.Framework;
using Spring.Aop.Framework;
using Spring.Objects;
using Spring.Util;

#endregion

namespace Spring.Aspects.Exceptions
{
    /// <summary>
    /// This class contains tests for ExceptionHandlerAdvice
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class ExceptionHandlerAspectIntegrationTests
    {
        private ExceptionHandlerAdvice exceptionHandlerAdvice;
        private CapturingLoggerFactoryAdapter loggerFactoryAdapter;
        private ILoggerFactoryAdapter originalAdapter;
        private static bool spelActionExecuted = false;

        [SetUp]
        public void Setup()
        {
            originalAdapter = LogManager.Adapter;
            loggerFactoryAdapter = new CapturingLoggerFactoryAdapter();
            LogManager.Adapter = loggerFactoryAdapter;
            exceptionHandlerAdvice = new ExceptionHandlerAdvice();
        }

        [TearDown]
        public void TearDown()
        {
            //            loggerFactoryAdapter.LogMessages.Clear();

            //reset so other tests can produce some output if needed.
            loggerFactoryAdapter.Clear();
            LogManager.Adapter = originalAdapter;
        }

        [Test]
        public void ExecuteSpelAction()
        {
            string executeHandlerText =
                "on exception name ArithmeticException execute Spring.Aspects.Exceptions.ExceptionHandlerAspectIntegrationTests.Executed(true)";
            ITestObject to = CreateTestObjectProxy(executeHandlerText);

            try
            {
                to.Exceptional(new ArithmeticException());
            }
            catch (ArithmeticException)
            {
                Assert.IsTrue(spelActionExecuted);
            }
        }

        public static void Executed(bool val)
        {
            spelActionExecuted = val;
        }

        [Test]
        public void LoggingTest()
        {
            LogExceptionHandler logHandler = new LogExceptionHandler();
            logHandler.LogName = "adviceHandler";
            string testText =
                @"'Hello World, exception message = ' + #e.Message + ', target method = ' + #method.Name";
            logHandler.SourceExceptionNames.Add("ArithmeticException");
            logHandler.ActionExpressionText = testText;

            exceptionHandlerAdvice.ExceptionHandlers.Add(logHandler);

            exceptionHandlerAdvice.AfterPropertiesSet();

            ProxyFactory pf = new ProxyFactory(new TestObject());
            pf.AddAdvice(exceptionHandlerAdvice);
            ITestObject to = (ITestObject)pf.GetProxy();

            try
            {
                to.Exceptional(new ArithmeticException());
                Assert.Fail("Should have thrown exception when only logging");
            }
            catch (ArithmeticException)
            {
                bool found = false;
                foreach (CapturingLoggerEvent loggerEvent in loggerFactoryAdapter.LoggerEvents)
                {
                    if (loggerEvent.RenderedMessage.IndexOf("Hello World") >= 0)
                    {
                        found = true;
                    }
                }
                Assert.IsTrue(found, "did not find logging output");
            }
        }

        [Test]
        public void LoggingTestWithString()
        {
            string logHandlerText = "on exception name ArithmeticException log 'My Message, Method Name ' + #method.Name";

            ExecuteLoggingHandler(logHandlerText, "My Message");
        }

        [Test]
        public void LoggingTestWithStringExplicitHandler()
        {
            string logHandlerText = "on exception name ArithmeticException log 'My Message, Method Name ' + #method.Name";

            ExecuteLoggingHandler(logHandlerText, "My Message");
        }

        [Test]
        public void LoggingTestWithConstraintExpression()
        {
            string logHandlerText = "on exception (#e is T(System.ArithmeticException)) log 'My Message, Method Name ' + #method.Name";

            ExecuteLoggingHandler(logHandlerText, "My Message");
        }

        [Test]
        public void LoggingTestWithConstraintExpressionWithExceptionHandlerInList()
        {
            LogExceptionHandler exHandler = new LogExceptionHandler();
            exHandler.ConstraintExpressionText = "#e is T(System.ArithmeticException)";
            exHandler.LogName = "adviceHandler";
            exHandler.ActionExpressionText = "#log.Fatal('Request Timeout occured', #e)";

            ExecuteLoggingHandlerInList(exHandler, "Request Timeout");
        }

        [Test]
        public void LoggingTestWithConstraintExpressionWithKeyedExceptionHandler()
        {
            LogExceptionHandler exHandler = new LogExceptionHandler();
            ExecuteLoggingHandlerWithKeyedLogHandler(exHandler,
               @"on exception (#e is T(System.ArithmeticException)) log 'Request Timeout occured'", "Request Timeout");
        }

        [Test]
        public void LoggingTestWithBadString()
        {
            string logHandlerText = "on foobar name ArithmeticException log 'My Message, Method Name ' + #method.Name";

            Assert.Throws<ArgumentException>(() => ExecuteLoggingHandler(logHandlerText, "My Message"));
        }

        [Test]
        public void LoggingTestWithInvalidConstraintExpression()
        {
            string logHandlerText = "on exception (#e is System.FooBar) log 'My Message, Method Name ' + #method.Name";

            ExecuteLoggingHandler(logHandlerText, "Was not able to evaluate constraint expression [#e is System.FooBar]");

        }

        [Test]
        public void LoggingTestWithNonBooleanConstraintExpression()
        {
            string logHandlerText = "on exception (1+1) log 'My Message, Method Name ' + #method.Name";

            ExecuteLoggingHandler(logHandlerText, "Was not able to unbox constraint expression to boolean [1+1]");

        }

        private void ExecuteLoggingHandler(string logHandlerText, string searchString)
        {
            ITestObject to = CreateTestObjectProxy(logHandlerText);

            try
            {
                to.Exceptional(new ArithmeticException());
            }
            catch (ArithmeticException)
            {
                AssertSearchString(searchString);
            }
        }

        private void ExecuteLoggingHandlerInList(IExceptionHandler handler, string searchString)
        {
            ITestObject to = CreateTestObjectProxyInList(handler);

            try
            {
                to.Exceptional(new ArithmeticException());
            }
            catch (ArithmeticException)
            {
                AssertSearchString(searchString);
            }
        }

        private void ExecuteLoggingHandlerWithKeyedLogHandler(IExceptionHandler handler, string handlerText, string searchString)
        {
            ITestObject to = CreateTestObjectProxyWithKeyedHandler(handler, handlerText);

            try
            {
                to.Exceptional(new ArithmeticException());
            }
            catch (ArithmeticException)
            {
                AssertSearchString(searchString);
            }
        }

        [Test]
        public void TranslationWithString()
        {
            string translationHandlerText =
                "on exception name ArithmeticException translate new System.InvalidOperationException('My Message, Method Name ' + #method.Name, #e)";

            ITestObject to = CreateTestObjectProxy(translationHandlerText);
            AssertTranslation(to);
        }


        [Test]
        public void TranslateWithExceptionHandlerInstance()
        {
            TranslationExceptionHandler exHandler = new TranslationExceptionHandler();
            IList exceptionNames = new ArrayList();
            exceptionNames.Add("ArithmeticException");
            exHandler.SourceExceptionNames = exceptionNames;
            exHandler.ActionExpressionText =
                "new System.InvalidOperationException('My Message, Method Name ' + #method.Name, #e)";
            ITestObject to = CreateTestObjectProxy(exHandler);
            AssertTranslation(to);
        }

        private static void AssertTranslation(ITestObject to)
        {
            try
            {
                to.Exceptional(new ArithmeticException("Bad Math"));
                Assert.Fail("Should have thrown exception");
            }
            catch (InvalidOperationException e)
            {
                Assert.That(e.InnerException, Is.InstanceOf(typeof(ArithmeticException)), "Inner exception.");
                Assert.AreEqual("My Message, Method Name Exceptional", e.Message);
            }
            catch (Exception e)
            {
                Assert.That(e, Is.InstanceOf(typeof (InvalidOperationException)), "wrong exception type thrown.");
            }
        }

        [Test]
        public void WrapWithString()
        {
            string translationHandlerText =
                "on exception name ArithmeticException wrap System.InvalidOperationException 'My Message'";

            ITestObject to = CreateTestObjectProxy(translationHandlerText);
            try
            {
                to.Exceptional(new ArithmeticException("Bad Math"));
                Assert.Fail("Should have thrown exception");
            }
            catch (InvalidOperationException e)
            {
                Assert.That(e.InnerException, Is.InstanceOf(typeof(ArithmeticException)));
                Assert.AreEqual("My Message", e.Message);
            }
            catch (Exception e)
            {
                Assert.That(e, Is.InstanceOf(typeof(InvalidOperationException)));
            }
        }

        [Test]
        public void WrapWithStringDefaultMessage()
        {
            string translationHandlerText =
                "on exception name ArithmeticException wrap System.InvalidOperationException";

            ITestObject to = CreateTestObjectProxy(translationHandlerText);
            try
            {
                to.Exceptional(new ArithmeticException("Bad Math"));
                Assert.Fail("Should have thrown exception");
            }
            catch (InvalidOperationException e)
            {
                Assert.AreEqual("Wrapped ArithmeticException", e.Message);
            }
            catch (Exception e)
            {
                Assert.That(e, Is.InstanceOf(typeof(InvalidOperationException)));
            }
        }


        [Test]
        public void ReplaceWithString()
        {
            string translationHandlerText =
                "on exception name ArithmeticException replace System.InvalidOperationException 'My Message'";

            ITestObject to = CreateTestObjectProxy(translationHandlerText);
            try
            {
                to.Exceptional(new ArithmeticException("Bad Math"));
                Assert.Fail("Should have thrown exception");
            }
            catch (InvalidOperationException e)
            {
                Assert.IsNull(e.InnerException);
                Assert.AreEqual("My Message", e.Message);
            }
            catch (Exception e)
            {
                Assert.That(e, Is.InstanceOf(typeof(InvalidOperationException)));
            }
        }

        [Test]
        public void ReplaceWithStringDefaultMessage()
        {
            string translationHandlerText =
                "on exception name ArithmeticException replace System.InvalidOperationException";

            ITestObject to = CreateTestObjectProxy(translationHandlerText);
            try
            {
                to.Exceptional(new ArithmeticException("Bad Math"));
                Assert.Fail("Should have thrown exception");
            }
            catch (InvalidOperationException e)
            {
                Assert.IsNull(e.InnerException);
                Assert.AreEqual("Replaced ArithmeticException", e.Message);
            }
            catch (Exception e)
            {
                Assert.That(e, Is.InstanceOf(typeof(InvalidOperationException)));
            }
        }

        [Test]
        public void SwallowWithString()
        {
            string returnHandlerText = "on exception name ArithmeticException swallow";
            ITestObject to = CreateTestObjectProxy(returnHandlerText);
            try
            {
                to.Exceptional(new ArithmeticException("Bad Math"));
            }
            catch (Exception e)
            {
                Assert.Fail("Should not have thrown exception" + e);
            }
        }

        [Test]
        public void SwallowReturnTypeIsValueType()
        {
            string returnHandlerText = "on exception name ArithmeticException swallow";
            ITestObject to = CreateTestObjectProxy(returnHandlerText);
            try
            {
                to.ExceptionalWithReturnValue(new ArithmeticException("Bad Math"));
            }
            catch (Exception e)
            {
                Assert.Fail("Should not have thrown exception. Exception type = " + e.GetType());
            }
        }


        [Test]
        public void ReturnWithString()
        {
            string returnHandlerText = "on exception name ArithmeticException return 12";
            ITestObject to = CreateTestObjectProxy(returnHandlerText);
            try
            {
                int retVal = to.ExceptionalWithReturnValue(new ArithmeticException("Bad Math"));
                Assert.AreEqual(12, retVal);
            }
            catch (Exception)
            {
                Assert.Fail("Should not have thrown exception");
            }
        }

        [Test]
        public void ChainLogAndWrap()
        {
            string logHandlerText = "on exception name ArithmeticException log 'My Message, Method Name ' + #method.Name";
            string translationHandlerText = "on exception name ArithmeticException wrap System.InvalidOperationException 'My Message'";
            exceptionHandlerAdvice.ExceptionHandlers.Add(logHandlerText);
            exceptionHandlerAdvice.ExceptionHandlers.Add(translationHandlerText);
            exceptionHandlerAdvice.AfterPropertiesSet();
            ProxyFactory pf = new ProxyFactory(new TestObject());
            pf.AddAdvice(exceptionHandlerAdvice);
            ITestObject to = (ITestObject)pf.GetProxy();
            try
            {
                to.Exceptional(new ArithmeticException("Bad Math"));
                Assert.Fail("Should have thrown exception");
            }
            catch (InvalidOperationException e)
            {
                Assert.IsNotNull(e.InnerException);
                Exception innerEx = e.InnerException;
                Assert.AreEqual("My Message", e.Message);
                Assert.AreEqual("Bad Math", innerEx.Message);
            }
            catch (Exception e)
            {
                Assert.That(e, Is.InstanceOf(typeof(InvalidOperationException)));
            }


        }

        private ITestObject CreateTestObjectProxy(string logHandlerText)
        {
            exceptionHandlerAdvice.ExceptionHandlers.Add(logHandlerText);
            return CreateProxy();
        }

        private ITestObject CreateTestObjectProxy(IExceptionHandler exceptionHander)
        {
            exceptionHandlerAdvice.ExceptionHandlers.Add(exceptionHander);
            return CreateProxy();
        }

        private ITestObject CreateTestObjectProxyInList(IExceptionHandler exceptionHander)
        {
            exceptionHandlerAdvice.ExceptionHandlers.Add(exceptionHander);
            return CreateProxy();
        }

        private ITestObject CreateTestObjectProxyWithKeyedHandler(IExceptionHandler exceptionHander, string handlerText)
        {
            exceptionHandlerAdvice.ExceptionHandlerDictionary.Add("log", exceptionHander);
            exceptionHandlerAdvice.ExceptionHandlers.Add(handlerText);
            return CreateProxy();
        }

        private ITestObject CreateProxy()
        {
            exceptionHandlerAdvice.AfterPropertiesSet();
            ProxyFactory pf = new ProxyFactory(new TestObject());
            pf.AddAdvice(exceptionHandlerAdvice);
            return (ITestObject)pf.GetProxy();
        }

        private void AssertSearchString(string searchString)
        {
            bool found = false;
            foreach (CapturingLoggerEvent loggerEvent in loggerFactoryAdapter.LoggerEvents)
            {
                if (loggerEvent.RenderedMessage.IndexOf(searchString) >= 0)
                {
                    found = true;
                }
            }
            Assert.IsTrue(found, "did not find logging output [" + searchString + "]  Logging values = "
                                 + StringUtils.CollectionToCommaDelimitedString(loggerFactoryAdapter.LoggerEvents));
        }
    }
}