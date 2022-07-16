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

using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using AopAlliance.Intercept;
using Spring.Objects.Factory;
using Spring.Util;

namespace Spring.Aspects
{
    /// <summary>
    /// This is
    /// </summary>
    /// <remarks>
    ///
    /// </remarks>
    /// <author>Mark Pollack</author>
    [Serializable]
    public abstract class AbstractExceptionHandlerAdvice : IMethodInterceptor, IInitializingObject, IDeserializationCallback
    {
        /// <summary>
        /// Gets or sets the Regex string used to parse advice expressions starting with 'on exception name' and subclass specific actions.
        /// </summary>
        /// <value>The regex string to parse advice expressions starting with 'on exception name' and subclass specific actions.</value>
        public abstract string OnExceptionNameRegex
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the Regex string used to parse advice expressions starting with 'on exception (constraint)' and subclass specific actions.
        /// </summary>
        /// <value>The regex string to parse advice expressions starting with 'on exception (constraint)' and subclass specific actions.</value>
        public abstract string OnExceptionRegex
        {
            get; set;
        }

        /// <summary>
        /// Implement this method to perform extra treatments before and after
        /// the call to the supplied <paramref name="invocation"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Polite implementations would certainly like to invoke
        /// <see cref="AopAlliance.Intercept.IJoinpoint.Proceed"/>.
        /// </p>
        /// </remarks>
        /// <param name="invocation">
        /// The method invocation that is being intercepted.
        /// </param>
        /// <returns>
        /// The result of the call to the
        /// <see cref="AopAlliance.Intercept.IJoinpoint.Proceed"/> method of
        /// the supplied <paramref name="invocation"/>; this return value may
        /// well have been intercepted by the interceptor.
        /// </returns>
        /// <exception cref="System.Exception">
        /// If any of the interceptors in the chain or the target object itself
        /// throws an exception.
        /// </exception>
        public abstract object Invoke(IMethodInvocation invocation);

        /// <summary>
        /// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// after it has injected all of an object's dependencies.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method allows the object instance to perform the kind of
        /// initialization only possible when all of it's dependencies have
        /// been injected (set), and to throw an appropriate exception in the
        /// event of misconfiguration.
        /// </p>
        /// <p>
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
        public abstract void AfterPropertiesSet();

        /// <summary>
        /// Parses the advice expression.
        /// </summary>
        /// <param name="adviceExpression">The advice expression.</param>
        /// <returns>An instance of ParsedAdviceExpression</returns>
        protected virtual ParsedAdviceExpression ParseAdviceExpression(string adviceExpression)
        {
            ParsedAdviceExpression parsedAdviceExpression = new ParsedAdviceExpression(adviceExpression);

            Match match = GetMatch(adviceExpression, OnExceptionNameRegex);
            if (match.Success)
            {
                parsedAdviceExpression.Success = true;
                //using exception names for exception filter
                parsedAdviceExpression.ExceptionNames = StringUtils.CommaDelimitedListToStringArray(match.Groups[2].Value.Trim());
                parsedAdviceExpression.ActionText = match.Groups[3].Value.Trim();
                parsedAdviceExpression.ActionExpressionText = match.Groups[4].Value.Trim();
            }
            else
            {
                match = GetMatch(adviceExpression, OnExceptionRegex);
                if (match.Success)
                {
                    parsedAdviceExpression.Success = true;
                    //using constratin expression for exception filter
                    string constraintExpression = match.Groups[2].Value.Trim().Remove(0, 1);
                    parsedAdviceExpression.ConstraintExpression = constraintExpression.Substring(0, constraintExpression.Length - 1);
                    parsedAdviceExpression.ActionText = match.Groups[3].Value.Trim();
                    parsedAdviceExpression.ActionExpressionText = match.Groups[4].Value.Trim();
                }
            }
            return parsedAdviceExpression;
        }


        /// <summary>
        /// Gets the match using exception constraint expression.
        /// </summary>
        /// <param name="adviceExpressionString">The advice expression string.</param>
        /// <param name="regexString">The regex string.</param>
        /// <returns>The Match object resulting from the regular expression match.</returns>
        protected virtual Match GetMatch(string adviceExpressionString, string regexString)
        {
            RegexOptions options = ((RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline) | RegexOptions.IgnoreCase);
            Regex reg = new Regex(regexString, options);
            return reg.Match(adviceExpressionString);
        }

        #region Serialization

        void IDeserializationCallback.OnDeserialization(object sender)
        {
            OnDeserialization(sender);
        }

        /// <summary>
        /// Override in case you need to initialized non-serialized fields on deserialization.
        /// </summary>
        protected virtual void OnDeserialization(object sender)
        {
        }

        #endregion
    }
}
