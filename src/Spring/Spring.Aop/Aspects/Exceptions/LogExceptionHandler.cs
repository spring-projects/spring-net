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

using Microsoft.Extensions.Logging;
using Spring.Expressions;

namespace Spring.Aspects.Exceptions;

/// <summary>
/// Logs the exceptions that this handler is configured for.
/// Default log name is "LogExceptionHandler" and default log level is <see cref="Microsoft.Extensions.Logging.LogLevel.Trace"/>.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="ActionExpressionText"/> is the Spring expression (SpEL) that produces the log message. It is
/// evaluated against the call context, which provides <c>#method</c>, <c>#args</c>, <c>#target</c> and
/// <c>#e</c>, and the resulting text is written to the logger named <see cref="LogName"/> at <see cref="LogLevel"/>.
/// The exception is attached to the entry unless <see cref="LogMessageOnly"/> is set.
/// </para>
/// <para>
/// Before 3.0.3 an expression such as <c>#log.Fatal('message', #e)</c> also worked, by accident of nesting;
/// it has not since the switch to Microsoft.Extensions.Logging, whose
/// <see cref="Microsoft.Extensions.Logging.ILogger"/> has no such instance methods. Give the message expression
/// only, for example <c>'message'</c>. A failing expression that starts with <c>#log.</c> is reported with a
/// warning that names this form.
/// </para>
/// </remarks>
/// <author>Mark Pollack</author>
public class LogExceptionHandler : AbstractExceptionHandler
{
    private string logName = "LogExceptionHandler";

    private LogLevel logLevel = LogLevel.Trace;

    private bool logMessageOnly = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogExceptionHandler"/> class.
    /// </summary>
    public LogExceptionHandler()
    {
        ContinueProcessing = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogExceptionHandler"/> class.
    /// </summary>
    /// <param name="exceptionNames">The exception names.</param>
    public LogExceptionHandler(string[] exceptionNames) : base(exceptionNames)
    {
        ContinueProcessing = true;
    }

    /// <summary>
    /// Gets or sets the name of the log.
    /// </summary>
    /// <value>The name of the log.</value>
    public string LogName
    {
        get { return logName; }
        set { logName = value; }
    }

    /// <summary>
    /// Gets or sets the log level.
    /// </summary>
    /// <value>The log level.</value>
    public LogLevel LogLevel
    {
        get { return logLevel; }
        set { logLevel = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to log message only, and not pass in the
    /// exception to the logging API
    /// </summary>
    /// <value><c>true</c> if log message only; otherwise, <c>false</c>.</value>
    public bool LogMessageOnly
    {
        get { return logMessageOnly; }
        set { logMessageOnly = value; }
    }

    /// <summary>
    /// Gets or sets the Spring expression (SpEL) that produces the message to log.
    /// </summary>
    /// <remarks>
    /// The getter returns the expression as it was set. Earlier versions returned the expression wrapped in
    /// a call on <c>#log</c>, which depended on <see cref="LogLevel"/> and <see cref="LogMessageOnly"/> having been
    /// set first, and ignored every assignment after the first. The level and the message-only setting are now
    /// applied when the exception is handled, so the order of property assignments no longer matters.
    /// </remarks>
    /// <value>The message expression.</value>
    public override string ActionExpressionText
    {
        get { return base.ActionExpressionText; }
        set { base.ActionExpressionText = value; }
    }

    /// <summary>
    /// Handles the exception by logging the message that <see cref="ActionExpressionText"/> evaluates to.
    /// </summary>
    /// <param name="callContextDictionary">the calling context dictionary</param>
    /// <returns>
    /// The return value from handling the exception, if not rethrown or a new exception is thrown.
    /// </returns>
    public override object HandleException(IDictionary<string, object> callContextDictionary)
    {
        //TODO log name is targettype.
        ILogger adviceLogger = LogManager.GetLogger(logName);

        // Stays available to user expressions that reference #log; the handler itself does not call it.
        // Assigned rather than added, because several log handlers can run against the same call context.
        callContextDictionary["log"] = adviceLogger;
        object message;
        try
        {
            IExpression expression = Expression.Parse(ActionExpressionText);
            message = expression.GetValue(null, callContextDictionary);
        }
        catch (Exception e)
        {
            if (ActionExpressionText != null && ActionExpressionText.StartsWith("#log.", StringComparison.Ordinal))
            {
                log.LogWarning(
                    e,
                    "The '#log.<Level>(...)' form is no longer supported; set ActionExpressionText to the message expression and LogLevel to the level. Was not able to evaluate action expression [{ActionExpression}]",
                    ActionExpressionText);
            }
            else
            {
                log.LogWarning(e, "Was not able to evaluate action expression [{ActionExpression}]", ActionExpressionText);
            }

            return "logged";
        }

        Exception exception = null;
        if (!logMessageOnly && callContextDictionary.TryGetValue("e", out object thrown))
        {
            exception = thrown as Exception;
        }

        // A failing logging provider must not replace the exception the advised method threw.
        try
        {
            adviceLogger.Log(logLevel, exception, "{Message}", message?.ToString());
        }
        catch (Exception e)
        {
            log.LogWarning(e, "Was not able to write the log entry for action expression [{ActionExpression}]", ActionExpressionText);
        }

        return "logged";
    }
}
