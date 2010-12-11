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

namespace Spring.Aspects
{
    /// <summary>
    /// This class contains the results of parsing an advice expresion of the form
    /// on exception name [ExceptionName1,ExceptionName2,...] [action] [action expression]
    /// or
    /// on exception [constraint expression] [action] [action expression]
    /// </summary>
    /// <remarks>
    ///
    /// </remarks>
    /// <author>Mark Pollack</author>
    public class ParsedAdviceExpression
    {
        private string adviceExpression;

        private string[] exceptionNames = new string[0];
        private string constraintExpression = null;
        private string actionExpressionText = null;
        private string actionText = null;
        private bool success;


        /// <summary>
        /// Initializes a new instance of the <see cref="ParsedAdviceExpression"/> class.
        /// </summary>
        /// <param name="adviceExpression">The advice expression.</param>
        public ParsedAdviceExpression(string adviceExpression)
        {
            this.adviceExpression = adviceExpression;
        }


        /// <summary>
        /// Gets or sets the advice expression.
        /// </summary>
        /// <value>The advice expression.</value>
        public string AdviceExpression
        {
            get { return adviceExpression; }
            set { adviceExpression = value; }
        }

        /// <summary>
        /// Gets or sets the exception names.
        /// </summary>
        /// <value>The exception names.</value>
        public string[] ExceptionNames
        {
            get { return exceptionNames; }
            set { exceptionNames = value; }
        }

        /// <summary>
        /// Gets or sets the constraint expression.
        /// </summary>
        /// <value>The constraint expression.</value>
        public string ConstraintExpression
        {
            get { return constraintExpression; }
            set { constraintExpression = value; }
        }

        /// <summary>
        /// Gets or sets the action expression text.
        /// </summary>
        /// <value>The action expression text.</value>
        public string ActionExpressionText
        {
            get { return actionExpressionText; }
            set { actionExpressionText = value; }
        }

        /// <summary>
        /// Gets or sets the action text.
        /// </summary>
        /// <value>The action text.</value>
        public string ActionText
        {
            get { return actionText; }
            set { actionText = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ParsedAdviceExpression"/> is success.
        /// </summary>
        /// <value><c>true</c> if success; otherwise, <c>false</c>.</value>
        public bool Success
        {
            get { return success; }
            set { success = value; }
        }
    }
}