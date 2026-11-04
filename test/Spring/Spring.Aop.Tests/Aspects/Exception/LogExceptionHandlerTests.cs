using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Spring.Aop.Framework;
using Spring.Core.TypeConversion;
using Spring.Objects;

namespace Spring.Aspects.Exceptions;

/// <summary>
/// Tests that <see cref="LogExceptionHandler"/> writes the entry it is configured for,
/// directly and through <see cref="ExceptionHandlerAdvice"/>.
/// </summary>
[TestFixture]
public class LogExceptionHandlerTests
{
    private sealed class LogEntry(string category, LogLevel level, string message, Exception exception)
    {
        public string Category { get; } = category;

        public LogLevel Level { get; } = level;

        public string Message { get; } = message;

        public Exception Exception { get; } = exception;
    }

    private sealed class RecordingLoggerFactory : ILoggerFactory
    {
        public List<LogEntry> Entries { get; } = [];

        public ILogger CreateLogger(string categoryName) => new RecordingLogger(categoryName, Entries);

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public void Dispose()
        {
        }

        private sealed class RecordingLogger(string category, List<LogEntry> entries) : ILogger
        {
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                entries.Add(new LogEntry(category, logLevel, formatter(state, exception), exception));
            }

            public bool IsEnabled(LogLevel logLevel) => true;

            public IDisposable BeginScope<TState>(TState state) => null;
        }
    }

    private ILoggerFactory originalFactory;
    private RecordingLoggerFactory loggerFactory;

    [SetUp]
    public void SetUp()
    {
        originalFactory = LogManager.LoggerFactory;
        loggerFactory = new RecordingLoggerFactory();
        LogManager.LoggerFactory = loggerFactory;
    }

    [TearDown]
    public void TearDown()
    {
        LogManager.LoggerFactory = originalFactory;
        loggerFactory.Dispose();
    }

    private static Dictionary<string, object> CreateCallContext(Exception exception)
    {
        return new Dictionary<string, object>
        {
            ["method"] = typeof(TestObject).GetMethod(nameof(TestObject.Exceptional)),
            ["args"] = new object[] { exception },
            ["target"] = new TestObject(),
            ["e"] = exception
        };
    }

    private static ITestObject CreateProxy(ExceptionHandlerAdvice advice)
    {
        advice.AfterPropertiesSet();
        ProxyFactory pf = new ProxyFactory(new TestObject());
        pf.AddAdvice(advice);
        return (ITestObject) pf.GetProxy();
    }

    [Test]
    public void LogsEvaluatedMessageAtConfiguredLevelAndCategoryWithException()
    {
        var thrown = new ArithmeticException("Bad Math");
        var handler = new LogExceptionHandler
        {
            LogName = "my.category",
            LogLevel = LogLevel.Error,
            ActionExpressionText = "'Failed in ' + #method.Name + ': ' + #e.Message"
        };

        object result = handler.HandleException(CreateCallContext(thrown));

        Assert.That(result, Is.EqualTo("logged"));
        Assert.That(loggerFactory.Entries, Has.Count.EqualTo(1));
        LogEntry entry = loggerFactory.Entries[0];
        Assert.That(entry.Category, Is.EqualTo("my.category"));
        Assert.That(entry.Level, Is.EqualTo(LogLevel.Error));
        Assert.That(entry.Message, Is.EqualTo("Failed in Exceptional: Bad Math"));
        Assert.That(entry.Exception, Is.SameAs(thrown));
    }

    [Test]
    public void DefaultsToTraceLevelAndLogExceptionHandlerCategory()
    {
        var handler = new LogExceptionHandler { ActionExpressionText = "'hello'" };

        handler.HandleException(CreateCallContext(new InvalidOperationException()));

        Assert.That(loggerFactory.Entries, Has.Count.EqualTo(1));
        Assert.That(loggerFactory.Entries[0].Category, Is.EqualTo("LogExceptionHandler"));
        Assert.That(loggerFactory.Entries[0].Level, Is.EqualTo(LogLevel.Trace));
    }

    [Test]
    public void LogMessageOnlyDoesNotAttachTheException()
    {
        var handler = new LogExceptionHandler
        {
            LogName = "my.category",
            LogLevel = LogLevel.Warning,
            LogMessageOnly = true,
            ActionExpressionText = "'only the message'"
        };

        handler.HandleException(CreateCallContext(new ArithmeticException("Bad Math")));

        Assert.That(loggerFactory.Entries, Has.Count.EqualTo(1));
        Assert.That(loggerFactory.Entries[0].Message, Is.EqualTo("only the message"));
        Assert.That(loggerFactory.Entries[0].Level, Is.EqualTo(LogLevel.Warning));
        Assert.That(loggerFactory.Entries[0].Exception, Is.Null);
    }

    [TestCase("Info", LogLevel.Information)]
    [TestCase("Warn", LogLevel.Warning)]
    [TestCase("Fatal", LogLevel.Critical)]
    [TestCase("Debug", LogLevel.Debug)]
    public void LegacyLevelNamesConvertedByLogLevelConverterAreHonoured(string levelName, LogLevel expected)
    {
        var level = (LogLevel) TypeConverterRegistry.GetConverter(typeof(LogLevel)).ConvertFromInvariantString(levelName);
        var handler = new LogExceptionHandler
        {
            LogName = "my.category",
            LogLevel = level,
            ActionExpressionText = "'legacy level'"
        };

        handler.HandleException(CreateCallContext(new ArithmeticException()));

        Assert.That(loggerFactory.Entries, Has.Count.EqualTo(1));
        Assert.That(loggerFactory.Entries[0].Level, Is.EqualTo(expected));
        Assert.That(loggerFactory.Entries[0].Message, Is.EqualTo("legacy level"));
    }

    [Test]
    public void PropertyOrderAndRepeatedAssignmentDoNotMatter()
    {
        var handler = new LogExceptionHandler();
        handler.ActionExpressionText = "'first'";
        handler.LogName = "my.category";
        handler.LogLevel = LogLevel.Critical;
        handler.ActionExpressionText = "'second'";

        Assert.That(handler.ActionExpressionText, Is.EqualTo("'second'"));
        handler.HandleException(CreateCallContext(new ArithmeticException()));

        Assert.That(loggerFactory.Entries, Has.Count.EqualTo(1));
        Assert.That(loggerFactory.Entries[0].Message, Is.EqualTo("second"));
        Assert.That(loggerFactory.Entries[0].Level, Is.EqualTo(LogLevel.Critical));
    }

    [Test]
    public void InvalidMessageExpressionLogsWarningAndDoesNotThrow()
    {
        var handler = new LogExceptionHandler
        {
            LogName = "my.category",
            LogLevel = LogLevel.Error,
            ActionExpressionText = "#e.NoSuchProperty.Foo"
        };

        object result = null;
        Assert.DoesNotThrow(() => result = handler.HandleException(CreateCallContext(new ArithmeticException())));

        Assert.That(result, Is.EqualTo("logged"));
        Assert.That(loggerFactory.Entries.Where(e => e.Category == "my.category"), Is.Empty);
        LogEntry warning = loggerFactory.Entries.Single();
        Assert.That(warning.Level, Is.EqualTo(LogLevel.Warning));
        Assert.That(warning.Category, Does.Contain(nameof(LogExceptionHandler)));
        Assert.That(warning.Message, Does.Contain("Was not able to evaluate action expression [#e.NoSuchProperty.Foo]"));
        Assert.That(warning.Exception, Is.Not.Null);
    }

    [Test]
    public void LegacyLogCallFormLogsSpecificWarningAndDoesNotThrow()
    {
        var handler = new LogExceptionHandler
        {
            LogName = "my.category",
            ActionExpressionText = "#log.Fatal('msg', #e)"
        };

        object result = null;
        Assert.DoesNotThrow(() => result = handler.HandleException(CreateCallContext(new ArithmeticException())));

        Assert.That(result, Is.EqualTo("logged"));
        LogEntry warning = loggerFactory.Entries.Single();
        Assert.That(warning.Level, Is.EqualTo(LogLevel.Warning));
        Assert.That(warning.Category, Does.Contain(nameof(LogExceptionHandler)));
        Assert.That(warning.Message, Does.Contain("The '#log.<Level>(...)' form is no longer supported"));
        Assert.That(warning.Message, Does.Contain("[#log.Fatal('msg', #e)]"));
        Assert.That(warning.Exception, Is.Not.Null);
    }

    [Test]
    public void MessageExpressionEvaluatingToNullLogsWithoutThrowing()
    {
        var thrown = new ArithmeticException();
        var handler = new LogExceptionHandler
        {
            LogName = "my.category",
            LogLevel = LogLevel.Error,
            ActionExpressionText = "null"
        };

        object result = null;
        Assert.DoesNotThrow(() => result = handler.HandleException(CreateCallContext(thrown)));

        Assert.That(result, Is.EqualTo("logged"));
        LogEntry entry = loggerFactory.Entries.Single();
        Assert.That(entry.Category, Is.EqualTo("my.category"));
        Assert.That(entry.Level, Is.EqualTo(LogLevel.Error));
        Assert.That(entry.Exception, Is.SameAs(thrown));
    }

    [Test]
    public void FailureOfTheLoggingProviderIsReportedAndDoesNotReplaceTheException()
    {
        // The handler's own logger comes from the recording factory; only the advice logger,
        // resolved per call, comes from the failing one.
        var handler = new LogExceptionHandler { LogName = "my.category", ActionExpressionText = "'msg'" };
        LogManager.LoggerFactory = new ThrowingLoggerFactory();

        object result = null;
        Assert.DoesNotThrow(() => result = handler.HandleException(CreateCallContext(new ArithmeticException())));

        Assert.That(result, Is.EqualTo("logged"));
        LogEntry warning = loggerFactory.Entries.Single();
        Assert.That(warning.Level, Is.EqualTo(LogLevel.Warning));
        Assert.That(warning.Message, Is.EqualTo("Was not able to write the log entry for action expression ['msg']"));
        Assert.That(warning.Exception, Is.TypeOf<InvalidOperationException>());
    }

    private sealed class ThrowingLoggerFactory : ILoggerFactory
    {
        public ILogger CreateLogger(string categoryName) => new ThrowingLogger();

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public void Dispose()
        {
        }

        private sealed class ThrowingLogger : ILogger
        {
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                throw new InvalidOperationException("provider failure");
            }

            public bool IsEnabled(LogLevel logLevel) => true;

            public IDisposable BeginScope<TState>(TState state) => null;
        }
    }

    [Test]
    public void SeveralLogHandlersCanShareACallContext()
    {
        var context = CreateCallContext(new ArithmeticException());
        var first = new LogExceptionHandler { LogName = "first", ActionExpressionText = "'one'" };
        var second = new LogExceptionHandler { LogName = "second", ActionExpressionText = "'two'" };

        first.HandleException(context);
        second.HandleException(context);

        Assert.That(loggerFactory.Entries.Select(e => e.Category + "=" + e.Message), Is.EqualTo(new[] { "first=one", "second=two" }));
    }

    [Test]
    public void DslLogActionThroughAdviceLogsMessageForProxiedMethodThatThrows()
    {
        var advice = new ExceptionHandlerAdvice();
        advice.ExceptionHandlers.Add("on exception name ArithmeticException log 'Hello ' + #method.Name");
        ITestObject proxy = CreateProxy(advice);
        var thrown = new ArithmeticException("Bad Math");

        var rethrown = Assert.Throws<ArithmeticException>(() => proxy.Exceptional(thrown));

        Assert.That(rethrown, Is.SameAs(thrown));

        Assert.That(loggerFactory.Entries, Has.Count.EqualTo(1));
        LogEntry entry = loggerFactory.Entries[0];
        Assert.That(entry.Category, Is.EqualTo("LogExceptionHandler"));
        Assert.That(entry.Level, Is.EqualTo(LogLevel.Trace));
        Assert.That(entry.Message, Is.EqualTo("Hello Exceptional"));
        Assert.That(entry.Exception, Is.SameAs(thrown));
    }

    [Test]
    public void DslLogActionWithConstraintExpressionThroughAdviceLogsMessage()
    {
        var advice = new ExceptionHandlerAdvice();
        advice.ExceptionHandlers.Add("on exception (#e is T(System.ArithmeticException)) log 'Constrained ' + #e.Message");
        ITestObject proxy = CreateProxy(advice);

        Assert.Throws<ArithmeticException>(() => proxy.Exceptional(new ArithmeticException("Bad Math")));

        Assert.That(loggerFactory.Entries.Single().Message, Is.EqualTo("Constrained Bad Math"));
    }

    [Test]
    public void ConfiguredHandlerTemplateKeyedAsLogSuppliesLevelCategoryAndMessageOnlyToDslAction()
    {
        var template = new LogExceptionHandler
        {
            LogName = "configured.category",
            LogLevel = LogLevel.Warning,
            LogMessageOnly = true
        };
        var advice = new ExceptionHandlerAdvice();
        advice.ExceptionHandlerDictionary.Add("log", template);
        advice.ExceptionHandlers.Add("on exception name ArithmeticException log 'Template ' + #method.Name");
        ITestObject proxy = CreateProxy(advice);

        Assert.Throws<ArithmeticException>(() => proxy.Exceptional(new ArithmeticException()));

        LogEntry entry = loggerFactory.Entries.Single();
        Assert.That(entry.Category, Is.EqualTo("configured.category"));
        Assert.That(entry.Level, Is.EqualTo(LogLevel.Warning));
        Assert.That(entry.Message, Is.EqualTo("Template Exceptional"));
        Assert.That(entry.Exception, Is.Null);
    }

    [Test]
    public void TwoDslLogStatementsEachKeepTheirOwnExceptionsAndMessageAndLeaveTheTemplateUnchanged()
    {
        var template = new LogExceptionHandler
        {
            LogName = "configured.category",
            LogLevel = LogLevel.Warning,
            LogMessageOnly = true
        };
        var advice = new ExceptionHandlerAdvice();
        advice.ExceptionHandlerDictionary.Add("log", template);
        advice.ExceptionHandlers.Add("on exception name ArithmeticException log 'A'");
        advice.ExceptionHandlers.Add("on exception name ArgumentException log 'B'");
        ITestObject proxy = CreateProxy(advice);

        Assert.Throws<ArithmeticException>(() => proxy.Exceptional(new ArithmeticException()));

        LogEntry arithmetic = loggerFactory.Entries.Single();
        Assert.That(arithmetic.Message, Is.EqualTo("A"));
        Assert.That(arithmetic.Category, Is.EqualTo("configured.category"));
        Assert.That(arithmetic.Level, Is.EqualTo(LogLevel.Warning));
        Assert.That(arithmetic.Exception, Is.Null);

        loggerFactory.Entries.Clear();
        Assert.Throws<ArgumentException>(() => proxy.Exceptional(new ArgumentException()));

        LogEntry argument = loggerFactory.Entries.Single();
        Assert.That(argument.Message, Is.EqualTo("B"));
        Assert.That(argument.Category, Is.EqualTo("configured.category"));
        Assert.That(argument.Level, Is.EqualTo(LogLevel.Warning));

        Assert.That(advice.ExceptionHandlers, Has.Count.EqualTo(2));
        Assert.That(advice.ExceptionHandlers[0], Is.Not.SameAs(advice.ExceptionHandlers[1]));
        Assert.That(advice.ExceptionHandlers[0], Is.Not.SameAs(template));
        Assert.That(template.ActionExpressionText, Is.Null);
        Assert.That(template.ConstraintExpressionText, Is.Null);
        Assert.That(template.SourceExceptionNames, Is.Empty);
    }

    [Test]
    public void CloneCopiesTheSettingsAndGivesTheCopyItsOwnExceptionLists()
    {
        var original = new LogExceptionHandler(["ArithmeticException"])
        {
            LogName = "configured.category",
            LogLevel = LogLevel.Error,
            LogMessageOnly = true,
            ActionExpressionText = "'A'"
        };
        original.SourceExceptionTypes.Add(typeof(ArithmeticException));

        var clone = (LogExceptionHandler) original.Clone();
        clone.SourceExceptionNames.Add("ArgumentException");
        clone.SourceExceptionTypes.Add(typeof(ArgumentException));

        Assert.That(clone, Is.Not.SameAs(original));
        Assert.That(clone.LogName, Is.EqualTo("configured.category"));
        Assert.That(clone.LogLevel, Is.EqualTo(LogLevel.Error));
        Assert.That(clone.LogMessageOnly, Is.True);
        Assert.That(clone.ActionExpressionText, Is.EqualTo("'A'"));
        Assert.That(original.SourceExceptionNames, Is.EqualTo(new[] { "ArithmeticException" }));
        Assert.That(original.SourceExceptionTypes, Is.EqualTo(new[] { typeof(ArithmeticException) }));
    }

    [Test]
    public void ExplicitHandlerInAdviceListLogsAndLetsLaterHandlersRun()
    {
        var handler = new LogExceptionHandler
        {
            LogName = "my.category",
            LogLevel = LogLevel.Error,
            ActionExpressionText = "'Logged ' + #e.Message"
        };
        handler.SourceExceptionNames.Add("ArithmeticException");
        var advice = new ExceptionHandlerAdvice();
        advice.ExceptionHandlers.Add(handler);
        advice.ExceptionHandlers.Add("on exception name ArithmeticException wrap System.InvalidOperationException 'Wrapped'");
        ITestObject proxy = CreateProxy(advice);

        var wrapped = Assert.Throws<InvalidOperationException>(() => proxy.Exceptional(new ArithmeticException("Bad Math")));

        Assert.That(wrapped.Message, Is.EqualTo("Wrapped"));
        LogEntry entry = loggerFactory.Entries.Single();
        Assert.That(entry.Category, Is.EqualTo("my.category"));
        Assert.That(entry.Level, Is.EqualTo(LogLevel.Error));
        Assert.That(entry.Message, Is.EqualTo("Logged Bad Math"));
    }
}
