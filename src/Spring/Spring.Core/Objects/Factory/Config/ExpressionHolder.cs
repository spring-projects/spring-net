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

using Spring.Expressions;

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Immutable placeholder class used for the value of a
	/// <see cref="Spring.Objects.PropertyValue"/> object when it's a reference
	/// to a Spring <see cref="IExpression"/> that should be evaluated at runtime.
	/// </summary>
	/// <author>Aleksandar Seovic</author>
    [Serializable]
    public class ExpressionHolder
	{
		private IExpression expression;
        private string expressionString;
	    private MutablePropertyValues properties;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.Config.ExpressionHolder"/>
		/// class.
		/// </summary>
		/// <param name="expression">The expression to resolve.</param>
		public ExpressionHolder(string expression)
		{
            expressionString = expression;
            this.expression = Spring.Expressions.Expression.Parse(expression);
		}

		/// <summary>
        /// Gets or sets the expression string.  Setting the expression string will cause
        /// the expression to be parsed.
        /// </summary>
        /// <value>The expression string.</value>
	    public string ExpressionString
	    {
	        get { return expressionString; }
	    }

	    /// <summary>
		/// Return the expression.
		/// </summary>
		public IExpression Expression
		{
			get { return expression; }
		}

        /// <summary>
        /// Properties for this expression node.
        /// </summary>
	    public MutablePropertyValues Properties
	    {
	        get { return properties; }
	        set { properties = value; }
	    }

		/// <summary>
		/// Returns a string representation of this instance.
		/// </summary>
		/// <returns>A string representation of this instance.</returns>
		public override string ToString()
		{
			return $"<{expressionString}>";
		}
	}
}
