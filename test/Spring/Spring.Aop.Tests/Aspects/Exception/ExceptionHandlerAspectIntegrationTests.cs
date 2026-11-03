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
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Spring.Aop.Framework;
using Spring.Objects;
using Spring.Util;

namespace Spring.Aspects.Exceptions;

/// <summary>
/// This class contains tests for ExceptionHandlerAdvice
/// </summary>
/// <author>Mark Pollack</author>
[TestFixture]
public class ExceptionHandlerAspectIntegrationTests
{
    private ExceptionHandlerAdvice exceptionHandlerAdvice;
    private CapturingLoggerFactory loggerFactory;
    private ILoggerFactory originalFactory;
    private static bool spelActionExecuted = false;

    [SetUp]
    public void Setup()
    {
        originalFactory = LogManager.LoggerFactory;
        loggerFactory = new CapturingLoggerFactory();
        LogManager.LoggerFactory = loggerFactory;
        exceptionHandlerAdvice = new ExceptionHandlerAdvice();
    }

    public sealed class CapturedEvent(string category, LogLevel level, string message)
    {
        public string Category { get; } = category;

        public LogLevel Level { get; } = level;

        public string Message { get; } = message;

        public override string ToString() => $"{Level} {Category}: {Message}";
    }

    public class CapturingLoggerFactory : ILoggerFactory
    {
        public List<CapturedEvent> LoggerEvents { get; set; } = [];

        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new CapturingLogger(categoryName, LoggerEvents);
        }

        public class CapturingLogger(string category, List<CapturedEvent> loggedEvents) : ILogger
        {
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var message = formatter(state, exception);
                loggedEvents.Add(new CapturedEvent(category, logLevel, message));
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }
        }

        public void AddProvider(ILoggerProvider provider)
        {
        }
    }

    [TearDown]
    public void TearDown()
    {
        //            loggerFactoryAdapter.LogMessages.Clear();

        //reset so other tests can produce some output if needed.
        loggerFactory.LoggerEvents.Clear();
        LogManager.LoggerFactory = originalFactory;
        loggerFactory?.Dispose();
    }

    [Test]
    public void ExecuteSpelAction()
    {
        string executeHandlerText =
            "on exception name ArithmeticException execute T(Spring.Aspects.Exceptions.ExceptionHandlerAspectIntegrationTests, Spring.Aop.Tests).Executed(true)";
        ITestObject to = CreateTestObjectProxy(executeHandlerText);
        spelActionExecuted = false;

        // The execute action returns null, which the advice treats as handled: the exception is not rethrown.
        Assert.DoesNotThrow(() => to.Exceptional(new ArithmeticException()));

        Assert.That(spelActionExecuted, Is.True, "Logged: " + string.Join("; ", loggerFactory.LoggerEvents));
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
        ITestObject to = (ITestObject) pf.GetProxy();

        var thrown = new ArithmeticException("Bad Math");
        Assert.Throws<ArithmeticException>(() => to.Exceptional(thrown), "Should have thrown exception when only logging");

        AssertLogged("Hello World, exception message = Bad Math, target method = Exceptional", LogLevel.Trace, "adviceHandler");
    }

    [Test]
    public void LoggingTestWithString()
    {
        string logHandlerText = "on exception name ArithmeticException log 'My Message, Method Name ' + #method.Name";

        ExecuteLoggingHandler(logHandlerText, "My Message, Method Name Exceptional", LogLevel.Trace, "LogExceptionHandler");
    }

    [Test]
    public void LoggingTestWithStringExplicitHandler()
    {
        string logHandlerText = "on exception name ArithmeticException log 'My Message, Method Name ' + #method.Name";

        ExecuteLoggingHandler(logHandlerText, "My Message, Method Name Exceptional", LogLevel.Trace, "LogExceptionHandler");
    }

    [Test]
    public void LoggingTestWithConstraintExpression()
    {
        string logHandlerText = "on exception (#e is T(System.ArithmeticException)) log 'My Message, Method Name ' + #method.Name";

        ExecuteLoggingHandler(logHandlerText, "My Message, Method Name Exceptional", LogLevel.Trace, "LogExceptionHandler");
    }

    [Test]
    public void LoggingTestWithConstraintExpressionWithExceptionHandlerInList()
    {
        LogExceptionHandler exHandler = new LogExceptionHandler();
        exHandler.ConstraintExpressionText = "#e is T(System.ArithmeticException)";
        exHandler.LogName = "adviceHandler";
        exHandler.LogLevel = LogLevel.Critical;
        exHandler.ActionExpressionText = "'Request Timeout occured'";

        ITestObject to = CreateTestObjectProxyInList(exHandler);

        Assert.Throws<ArithmeticException>(() => to.Exceptional(new ArithmeticException()));

        AssertLogged("Request Timeout occured", LogLevel.Critical, "adviceHandler");
    }

    [Test]
    public void LoggingTestWithConstraintExpressionWithKeyedExceptionHandler()
    {
        LogExceptionHandler exHandler = new LogExceptionHandler();
        ITestObject to = CreateTestObjectProxyWithKeyedHandler(exHandler,
            @"on exception (#e is T(System.ArithmeticException)) log 'Request Timeout occured'");

        Assert.Throws<ArithmeticException>(() => to.Exceptional(new ArithmeticException()));

        AssertLogged("Request Timeout occured", LogLevel.Trace, "LogExceptionHandler");
    }

    [Test]
    public void LoggingTestWithBadString()
    {
        string logHandlerText = "on foobar name ArithmeticException log 'My Message, Method Name ' + #method.Name";

        Assert.Throws<ArgumentException>(() => CreateTestObjectProxy(logHandlerText));
    }

    [Test]
    public void LoggingTestWithInvalidConstraintExpression()
    {
        string logHandlerText = "on exception (#e is System.FooBar) log 'My Message, Method Name ' + #method.Name";

        ExecuteLoggingHandler(logHandlerText, "Was not able to evaluate constraint expression [#e is System.FooBar]", LogLevel.Warning, typeof(LogExceptionHandler).FullName, expectMessageLogged: false);
    }

    [Test]
    public void LoggingTestWithNonBooleanConstraintExpression()
    {
        string logHandlerText = "on exception (1+1) log 'My Message, Method Name ' + #method.Name";

        ExecuteLoggingHandler(logHandlerText, "Was not able to unbox constraint expression to boolean [1+1]", LogLevel.Warning, typeof(LogExceptionHandler).FullName, expectMessageLogged: false);
    }

    /// <summary>
    /// Runs a proxied method that throws and checks that exactly the expected entry was written.
    /// A handler whose constraint can't be evaluated reports a warning and logs no message,
    /// which <paramref name="expectMessageLogged"/> <c>false</c> checks for.
    /// </summary>
    private void ExecuteLoggingHandler(string logHandlerText, string expectedMessage, LogLevel expectedLevel, string expectedCategory, bool expectMessageLogged = true)
    {
        ITestObject to = CreateTestObjectProxy(logHandlerText);

        Assert.Throws<ArithmeticException>(() => to.Exceptional(new ArithmeticException()));

        AssertLogged(expectedMessage, expectedLevel, expectedCategory);
        if (!expectMessageLogged)
        {
            Assert.That(loggerFactory.LoggerEvents.Where(e => e.Message.StartsWith("My Message")), Is.Empty);
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
            Assert.That(e.Message, Is.EqualTo("My Message, Method Name Exceptional"));
        }
        catch (Exception e)
        {
            Assert.That(e, Is.InstanceOf(typeof(InvalidOperationException)), "wrong exception type thrown.");
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
            Assert.That(e.Message, Is.EqualTo("My Message"));
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
            Assert.That(e.Message, Is.EqualTo("Wrapped ArithmeticException"));
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
            Assert.That(e.InnerException, Is.Null);
            Assert.That(e.Message, Is.EqualTo("My Message"));
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
            Assert.That(e.InnerException, Is.Null);
            Assert.That(e.Message, Is.EqualTo("Replaced ArithmeticException"));
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
            Assert.That(retVal, Is.EqualTo(12));
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
        ITestObject to = (ITestObject) pf.GetProxy();
        try
        {
            to.Exceptional(new ArithmeticException("Bad Math"));
            Assert.Fail("Should have thrown exception");
        }
        catch (InvalidOperationException e)
        {
            Assert.That(e.InnerException, Is.Not.Null);
            Exception innerEx = e.InnerException;
            Assert.That(e.Message, Is.EqualTo("My Message"));
            Assert.That(innerEx.Message, Is.EqualTo("Bad Math"));
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
        return (ITestObject) pf.GetProxy();
    }

    /// <summary>
    /// Asserts that exactly one entry was written with this message, level and category.
    /// The message must match exactly, so the warning that a failed expression produces can't stand in for it.
    /// </summary>
    private void AssertLogged(string expectedMessage, LogLevel expectedLevel, string expectedCategory)
    {
        var matches = loggerFactory.LoggerEvents
            .Where(e => e.Message == expectedMessage && e.Level == expectedLevel && e.Category == expectedCategory)
            .ToList();

        Assert.That(matches, Has.Count.EqualTo(1), "did not find logging output [" + expectedMessage + "]  Logging values = "
                                                  + StringUtils.CollectionToCommaDelimitedString(loggerFactory.LoggerEvents));
    }
}
