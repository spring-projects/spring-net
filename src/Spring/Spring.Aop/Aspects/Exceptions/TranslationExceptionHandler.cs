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
    /// Translates from one exception to another based.  My wrap or replace exception depending on the expression.
    /// </summary>
    /// <author>Mark Pollack</author>
    public class TranslationExceptionHandler : AbstractExceptionHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationExceptionHandler"/> class.
        /// </summary>
        public TranslationExceptionHandler()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationExceptionHandler"/> class.
        /// </summary>
        /// <param name="exceptionNames">The exception names.</param>
        public TranslationExceptionHandler(string[] exceptionNames) : base(exceptionNames)
        {
        }



        /// <summary>
        /// Handles the exception.
        /// </summary>
        /// <returns>The return value from handling the exception, if not rethrown or a new exception is thrown.</returns>
        public override object HandleException(IDictionary<string, object> callContextDictionary)
        {
            object o = null;
            try {
                IExpression expression = Expression.Parse(ActionExpressionText);
                o = expression.GetValue(null, callContextDictionary);
            }
            catch (Exception e)
            {
                log.Warn("Was not able to evaluate action expression [" + ActionExpressionText + "]", e);
            }
            Exception translatedException = o as Exception;
            if (translatedException != null)
            {
                ThrowTranslatedException(translatedException);
            }
            return null;
        }

        private void ThrowTranslatedException(Exception exception)
        {
            throw exception;
        }
    }
}
