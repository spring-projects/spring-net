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
using System.Collections.Specialized;
using Common.Logging;
using Common.Logging.Simple;
using NUnit.Framework;
using Spring.Aop.Framework;
using Spring.Aspects.Exceptions;
using Spring.Expressions;
using Spring.Objects;

#endregion

namespace Spring.Aspects.Exceptions
{
    /// <summary>
    /// This class contains tests for ExceptionHandlerAdvice
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <version>$Id: ExceptionHandlerAspectIntegrationTests.cs,v 1.6 2008/02/26 00:03:43 markpollack Exp $</version>
    [TestFixture]
    public class ExceptionHandlerAspectIntegrationTests
    {
        private ExceptionHandlerAdvice exceptionHandlerAdvice;

        [SetUp]
        public void Setup()
        {
            LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter(new NameValueCollection());
            exceptionHandlerAdvice = new ExceptionHandlerAdvice();
        }

        [Test]
        public void LoggingTest()
        {


            LogExceptionHandler logHandler = new LogExceptionHandler();
            string testText = @"#log.Debug('Hello World, exception message = ' + #e.Message + ', target method = ' + #method.Name)";
            logHandler.SourceExceptionNames.Add("ArithmeticException");
            logHandler.ActionExpressionText = testText;

            exceptionHandlerAdvice.ExceptionHandlers.Add(logHandler);
            
            ProxyFactory pf = new ProxyFactory(new TestObject());
            pf.AddAdvice(exceptionHandlerAdvice);
            ITestObject to = (ITestObject) pf.GetProxy();

            try
            {
                to.Exceptional(new ArithmeticException());
                Assert.Fail("Should have thrown exception when only logging");
            } catch (ArithmeticException)
            {
                //TODO need to create adapter implementation to replay logged text.
            }

        }

        [Test]
        public void LoggingTestWithString()
        {
            string logHandlerText = "on exception name ArithmeticException log 'My Message, Method Name ' + #method.Name";

            ExecuteLoggingHandler(logHandlerText);
        }


        [Test]
        public void LoggingTestWithConstraintExpression()
        {
            string logHandlerText = "on exception (#e is T(System.ArithmeticException)) log 'My Message, Method Name ' + #method.Name";

            ExecuteLoggingHandler(logHandlerText);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void LoggingTestWithBadString()
        {
            string logHandlerText = "on foobar name ArithmeticException log 'My Message, Method Name ' + #method.Name";

            ExecuteLoggingHandler(logHandlerText);
        }

        [Test]
        public void LoggingTestWithInvalidConstraintExpression()
        {
            string logHandlerText = "on exception (#e is System.FooBar) log 'My Message, Method Name ' + #method.Name";

            ExecuteLoggingHandler(logHandlerText);
            
            //No exception is expected.  

            //TODO need to make sure log statement was executed.
        }

        [Test]
        public void LoggingTestWithNonBooleanConstraintExpression()
        {
            string logHandlerText = "on exception (1+1) log 'My Message, Method Name ' + #method.Name";

            ExecuteLoggingHandler(logHandlerText);

            //No exception is expected.  

            //TODO need to make sure log statement was executed.
        }

        private void ExecuteLoggingHandler(string logHandlerText)
        {
            ITestObject to = CreateTestObjectProxy(logHandlerText);

            try
            {
                to.Exceptional(new ArithmeticException());
            }
            catch (ArithmeticException)
            {
                //TODO assert logging occured.
            }
        }



        [Test]
        public void TranslationWithString()
        {
            string translationHandlerText =
                "on exception name ArithmeticException translate new System.InvalidOperationException('My Message, Method Name ' + #method.Name, #e)";

            ITestObject to = CreateTestObjectProxy(translationHandlerText);
            try
            {
                to.Exceptional(new ArithmeticException("Bad Math"));
                Assert.Fail("Should have thrown exception");
            }
            catch (InvalidOperationException e)
            {
                Assert.IsInstanceOfType(typeof(ArithmeticException), e.InnerException, "Inner exception.");
                Assert.AreEqual("My Message, Method Name Exceptional", e.Message);
            } catch (Exception e)
            {
                Assert.IsInstanceOfType(typeof(InvalidOperationException), e, "wrong exception type thrown.");
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
                Assert.IsInstanceOfType(typeof(ArithmeticException), e.InnerException);
                Assert.AreEqual("My Message", e.Message);
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(typeof(InvalidOperationException), e);
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
                Assert.IsInstanceOfType(typeof(ArithmeticException), e.InnerException);
                Assert.AreEqual("Wrapped ArithmeticException", e.Message);
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(typeof(InvalidOperationException), e);
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
                Assert.IsInstanceOfType(typeof(InvalidOperationException), e);
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
                Assert.IsInstanceOfType(typeof(InvalidOperationException), e);
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
            } catch (Exception)
            {
                Assert.Fail("Should not have thrown exception");   
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
                Assert.IsInstanceOfType(typeof(InvalidOperationException), e);
            }


        }

        private ITestObject CreateTestObjectProxy(string logHandlerText)
        {
            exceptionHandlerAdvice.ExceptionHandlers.Add(logHandlerText);
            exceptionHandlerAdvice.AfterPropertiesSet();

            ProxyFactory pf = new ProxyFactory(new TestObject());
            pf.AddAdvice(exceptionHandlerAdvice);
            return (ITestObject)pf.GetProxy();
        }
        
    }
}