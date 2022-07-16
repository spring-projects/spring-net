#region License

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

#endregion

using Spring.Expressions;
using Spring.Globalization;

namespace Spring.DataBinding
{
    /// <summary>
    /// Simple, expression-based implementation of <see cref="IBinding"/> that
    /// binds source to target one-to-one.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class SimpleExpressionBinding : AbstractSimpleBinding
    {
        #region Fields

        private IExpression sourceExpression;
        private IExpression targetExpression;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleExpressionBinding"/> class.
        /// </summary>
        /// <param name="sourceExpression">
        /// The source expression.
        /// </param>
        /// <param name="targetExpression">
        /// The target expression.
        /// </param>
        public SimpleExpressionBinding(string sourceExpression, string targetExpression)
        {
            this.sourceExpression = Expression.Parse(sourceExpression);
            this.targetExpression = Expression.Parse(targetExpression);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleExpressionBinding"/> class.
        /// </summary>
        /// <param name="sourceExpression">
        /// The source expression.
        /// </param>
        /// <param name="targetExpression">
        /// The target expression.
        /// </param>
        /// <param name="formatter">
        /// The formatter to use.
        /// </param>
        public SimpleExpressionBinding(string sourceExpression, string targetExpression, IFormatter formatter)
            :base(formatter)
        {
            this.sourceExpression = Expression.Parse(sourceExpression);
            this.targetExpression = Expression.Parse(targetExpression);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the source expression.
        /// </summary>
        /// <value>The source expression.</value>
        public IExpression SourceExpression
        {
            get { return sourceExpression; }
        }

        /// <summary>
        /// Gets the target expression.
        /// </summary>
        /// <value>The target expression.</value>
        public IExpression TargetExpression
        {
            get { return targetExpression; }
        }

        #endregion

        #region Abstract Methods Implementation

        /// <summary>
        /// Gets the source value for the binding.
        /// </summary>
        /// <param name="source">
        ///   Source object to extract value from.
        /// </param>
        /// <param name="variables">
        ///   Variables for expression evaluation.
        /// </param>
        /// <returns>
        /// The source value for the binding.
        /// </returns>
        protected override object GetSourceValue(object source, IDictionary<string, object> variables)
        {
            return this.SourceExpression.GetValue(source, variables);
        }

        /// <summary>
        /// Sets the source value for the binding.
        /// </summary>
        /// <param name="source">
        ///   The source object to set the value on.
        /// </param>
        /// <param name="value">
        ///   The value to set.
        /// </param>
        /// <param name="variables">
        ///   Variables for expression evaluation.
        /// </param>
        protected override void SetSourceValue(object source, object value, IDictionary<string, object> variables)
        {
            this.SourceExpression.SetValue(source, variables, value);
        }

        /// <summary>
        /// Gets the target value for the binding.
        /// </summary>
        /// <param name="target">
        ///   Source object to extract value from.
        /// </param>
        /// <param name="variables">
        ///   Variables for expression evaluation.
        /// </param>
        /// <returns>
        /// The target value for the binding.
        /// </returns>
        protected override object GetTargetValue(object target, IDictionary<string, object> variables)
        {
            return this.TargetExpression.GetValue(target, variables);
        }

        /// <summary>
        /// Sets the target value for the binding.
        /// </summary>
        /// <param name="target">
        ///   The target object to set the value on.
        /// </param>
        /// <param name="value">
        ///   The value to set.
        /// </param>
        /// <param name="variables">
        ///   Variables for expression evaluation.
        /// </param>
        protected override void SetTargetValue(object target, object value, IDictionary<string, object> variables)
        {
            this.TargetExpression.SetValue(target, variables, value);
        }

        #endregion

    }
}
