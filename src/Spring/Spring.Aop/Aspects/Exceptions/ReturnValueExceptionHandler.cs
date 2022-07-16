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

using Spring.Expressions;

namespace Spring.Aspects.Exceptions
{
    /// <summary>
    /// Evaluates the expression for the return value of the method.
    /// </summary>
    /// <author>Mark Pollack</author>
    public class ReturnValueExceptionHandler : AbstractExceptionHandler
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ReturnValueExceptionHandler"/> class.
        /// </summary>
        public ReturnValueExceptionHandler()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReturnValueExceptionHandler"/> class.
        /// </summary>
        /// <param name="exceptionNames">The exception names.</param>
        public ReturnValueExceptionHandler(string[] exceptionNames) : base(exceptionNames)
        {
        }



        /// <summary>
        /// Returns the result of evaluating the translation expression.
        /// </summary>
        /// <returns>The return value from handling the exception, if not rethrown or a new exception is thrown.</returns>
        public override object HandleException(IDictionary<string, object> callContextDictionary)
        {
            object returnVal = null;
            try
            {
                IExpression expression = Expression.Parse(ActionExpressionText);
                returnVal = expression.GetValue(null, callContextDictionary);
            }
            catch (Exception e)
            {
                log.Warn("Was not able to evaluate action expression [" + ActionExpressionText + "]", e);
            }
            return returnVal;
        }
    }
}
