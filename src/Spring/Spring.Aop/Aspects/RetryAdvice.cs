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

using System.Text.RegularExpressions;
using AopAlliance.Intercept;

using Common.Logging;

using Spring.Core.TypeConversion;
using Spring.Expressions;

namespace Spring.Aspects
{
    /// <summary>
    /// AOP Advice to retry a method invocation on an exception.  The retry semantics are defined by a DSL of the
    /// form <code>on exception name [ExceptionName1,ExceptionName2,...] retry [number of times] [delay|rate] [delay time|rate expression]</code>.
    /// For example, <code>on exception name ArithmeticException retry 3x delay 1s</code>
    /// </summary>
    /// <remarks>
    ///
    /// </remarks>
    /// <author>Mark Pollack</author>
    [Serializable]
    public class RetryAdvice : AbstractExceptionHandlerAdvice
    {
        ///<summary>
        ///The type of the callback that is called for delaying retries.
        ///</summary>
        public delegate void SleepHandler(TimeSpan duration);

        private static readonly ILog log;
        private static readonly TimeSpanConverter timeSpanConverter;

        static RetryAdvice()
        {
            log = LogManager.GetLogger(typeof(RetryAdvice));
            timeSpanConverter = new TimeSpanConverter();
        }

        #region Fields

        private SleepHandler sleepHandler;

        [NonSerialized]
        private RetryExceptionHandler retryExceptionHandler;

        private string retryExpression;

        private string onExceptionNameRegex = @"^(on\s+exception\s+name)\s+(.*?)\s+(retry)\s*(.*?)$";

        private string onExceptionRegex = @"^(on\s+exception\s+)(\(.*?\))\s+(retry)\s*(.*?)$";

        //retry 3x delay 10s
        private string delayRegex = @"^(\d+)x\s+(delay)\s+(\d+\w+)?$";

        //retry 3x rate 10n+5
        private string rateRegex = @"^(\d+)x\s+(rate)\s+(\(.*?\))?$";
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the retry expression.
        /// </summary>
        /// <value>The retry expression.</value>
        public string RetryExpression
        {
            get { return retryExpression; }
            set { retryExpression = value; }
        }

        /// <summary>
        /// Gets or sets the Regex string used to parse advice expressions starting with 'on exception name' and exception handling actions.
        /// </summary>
        /// <value>The regex string to parse advice expressions starting with 'on exception name' and exception handling actions.</value>
        public override string OnExceptionNameRegex
        {
            get { return onExceptionNameRegex; }
            set { onExceptionNameRegex = value; }
        }

        /// <summary>
        /// Gets or sets the Regex string used to parse advice expressions starting with 'on exception (constraint)' and exception handling actions.
        /// </summary>
        /// <value>The regex string to parse advice expressions starting with 'on exception (constraint)' and exception handling actions.</value>
        public override string OnExceptionRegex
        {
            get { return onExceptionRegex; }
            set { onExceptionRegex = value; }
        }

        #endregion

        /// <summary>
        /// Creates a new RetryAdvice instance, using <see cref="Thread.Sleep(TimeSpan)"/> for delaying retries
        /// </summary>
        public RetryAdvice()
            :this(new SleepHandler(Thread.Sleep))
        {
        }

        /// <summary>
        /// Creates a new RetryAdvice instance, using any arbitrary callback for delaying retries
        /// </summary>
        public RetryAdvice(SleepHandler sleepHandler)
        {
            this.sleepHandler = sleepHandler;
        }

        #region IMethodInterceptor implementation

        /// <summary>
        /// Implement this method to perform extra treatments before and after
        /// the call to the supplied <paramref name="invocation"/>.
        /// </summary>
        /// <param name="invocation">The method invocation that is being intercepted.</param>
        /// <returns>
        /// The result of the call to the
        /// <see cref="AopAlliance.Intercept.IJoinpoint.Proceed"/> method of
        /// the supplied <paramref name="invocation"/>; this return value may
        /// well have been intercepted by the interceptor.
        /// </returns>
        /// <remarks>
        /// 	<p>
        /// Polite implementations would certainly like to invoke
        /// <see cref="AopAlliance.Intercept.IJoinpoint.Proceed"/>.
        /// </p>
        /// </remarks>
        /// <exception cref="System.Exception">
        /// If any of the interceptors in the chain or the target object itself
        /// throws an exception.
        /// </exception>
        public override object Invoke(IMethodInvocation invocation)
        {
            IDictionary<string, object> callContextDictionary = new Dictionary<string, object>();
            callContextDictionary.Add("method", invocation.Method);
            callContextDictionary.Add("args", invocation.Arguments);
            callContextDictionary.Add("target", invocation.Target);
            int numAttempts = 0;

            object returnVal = null;
            do
            {
                try
                {
                    returnVal = invocation.Proceed();
                    break;
                }
                catch (Exception ex)
                {
                    callContextDictionary["e"] = ex;
                    if (retryExceptionHandler.CanHandleException(ex, callContextDictionary))
                    {
                        numAttempts++;
                        if (numAttempts == retryExceptionHandler.MaximumRetryCount)
                        {
                            throw;
                        }
                        else
                        {
                            if (log.IsTraceEnabled)
                            {
                                log.Trace("Retrying " + invocation.Method.Name);
                            }
                            callContextDictionary["n"] = numAttempts;
                            Sleep(retryExceptionHandler, callContextDictionary, sleepHandler);
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            } while (numAttempts <= retryExceptionHandler.MaximumRetryCount);


            log.Debug("Invoked successfully after " + numAttempts + " attempt(s)");
            return returnVal;
        }

        private static void Sleep(RetryExceptionHandler handler, IDictionary<string, object> callContextDictionary, SleepHandler sleepHandler)
        {
            if (handler.IsDelayBased)
            {
                sleepHandler(handler.DelayTimeSpan);
            }
            else
            {
                try
                {
                    IExpression expression = Expression.Parse(handler.DelayRateExpression);
                    object result = expression.GetValue(null, callContextDictionary);
                    decimal d = decimal.Parse(result.ToString());
                    decimal rounded = decimal.Round(d*1000,0);
                    TimeSpan duration = TimeSpan.FromMilliseconds(decimal.ToDouble(rounded));
                    sleepHandler(duration);
                }
                catch (InvalidCastException e)
                {
                    log.Warn("Was not able to cast expression to decimal [" + handler.DelayRateExpression + "]. Sleeping for 1 second", e);
                    sleepHandler(new TimeSpan(0,0,1));
                }
                catch (Exception e)
                {
                    log.Warn("Was not able to evaluate rate expression [" + handler.DelayRateExpression + "]. Sleeping for 1 second", e);
                    sleepHandler(new TimeSpan(0,0,1));
                }
            }
        }

        #endregion

        #region IInitializingObject implementation

        /// <summary>
        /// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// after it has injected all of an object's dependencies.
        /// </summary>
        /// <remarks>
        /// 	<p>
        /// This method allows the object instance to perform the kind of
        /// initialization only possible when all of it's dependencies have
        /// been injected (set), and to throw an appropriate exception in the
        /// event of misconfiguration.
        /// </p>
        /// 	<p>
        /// Please do consult the class level documentation for the
        /// <see cref="Spring.Objects.Factory.IObjectFactory"/> interface for a
        /// description of exactly <i>when</i> this method is invoked. In
        /// particular, it is worth noting that the
        /// <see cref="Spring.Objects.Factory.IObjectFactoryAware"/>
        /// and <see cref="Spring.Context.IApplicationContextAware"/>
        /// callbacks will have been invoked <i>prior</i> to this method being
        /// called.
        /// </p>
        /// </remarks>
        /// <exception cref="System.Exception">
        /// In the event of misconfiguration (such as the failure to set a
        /// required property) or if initialization fails.
        /// </exception>
        public override void AfterPropertiesSet()
        {
            if (retryExpression == null)
            {
                throw new ArgumentException("Must specify retry expression.");
            }
            RetryExceptionHandler handler = Parse(retryExpression);
            if (handler == null)
            {
                throw new ArgumentException("Was not able to parse retry expression string [" + retryExpression + "]");
            }
            retryExceptionHandler = handler;
        }

        #endregion

        /// <summary>
        /// Parses the specified handler string.
        /// </summary>
        /// <param name="retryExpressionString">The handler string.</param>
        /// <returns></returns>
        protected virtual RetryExceptionHandler Parse(string retryExpressionString)
        {

            ParsedAdviceExpression parsedAdviceExpression = ParseAdviceExpression(retryExpressionString);

            if (!parsedAdviceExpression.Success)
            {
                log.Warn("Could not parse retry expression " + retryExpressionString);
                return null;
            }

            RetryExceptionHandler handler = new RetryExceptionHandler(parsedAdviceExpression.ExceptionNames);
            handler.ConstraintExpressionText = parsedAdviceExpression.ConstraintExpression;
            handler.ActionExpressionText = parsedAdviceExpression.AdviceExpression;

            Match match = GetMatchForActionExpression(parsedAdviceExpression.ActionExpressionText, delayRegex);

            if (match.Success)
            {
                handler.MaximumRetryCount = int.Parse(match.Groups[1].Value.Trim());
                handler.IsDelayBased = true;

                try
                {
                    string ts = match.Groups[3].Value.Trim();
                    handler.DelayTimeSpan = (TimeSpan) timeSpanConverter.ConvertFrom(null, null, ts);
                } catch (Exception)
                {
                    log.Warn("Could not parse timespan " + match.Groups[3].Value.Trim());
                    return null;
                }
                return handler;
            }
            else
            {
                match = GetMatchForActionExpression(parsedAdviceExpression.ActionExpressionText, rateRegex);
                if (match.Success)
                {
                    handler.MaximumRetryCount = int.Parse(match.Groups[1].Value.Trim());
                    handler.IsDelayBased = false;
                    handler.DelayRateExpression = match.Groups[3].Value.Trim();
                    return handler;
                }
                else
                {
                    return null;
                }
            }

        }

        /// <summary>
        /// Gets the match for action expression.
        /// </summary>
        /// <param name="actionExpressionString">The action expression string.</param>
        /// <param name="regexString">The regex string.</param>
        /// <returns>The Match object resulting from the regular expression match.</returns>
        protected virtual Match GetMatchForActionExpression(string actionExpressionString, string regexString)
        {
            RegexOptions options = ((RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline) | RegexOptions.IgnoreCase);
            Regex reg = new Regex(regexString, options);
            return reg.Match(actionExpressionString);
        }

        /// <summary>
        /// Override in case you need to initialized non-serialized fields on deserialization.
        /// </summary>
        protected override void OnDeserialization(object sender)
        {
            base.OnDeserialization(sender);
            if (retryExpression != null)
            {
                this.AfterPropertiesSet();
            }
        }
    }
}
