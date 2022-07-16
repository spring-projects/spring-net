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

namespace Spring.Expressions
{
    /// <summary>
    /// Utility class that enables easy expression evaluation.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This class allows users to get or set properties, execute methods, and evaluate
    /// logical and arithmetic expressions.
    /// </p>
    /// <p>
    /// Methods in this class parse expression on every invocation.
    /// If you plan to reuse the same expression many times, you should prepare
    /// the expression once using the static <see cref="Expression.Parse(string)"/> method,
    /// and then call <see cref="IExpression.GetValue()"/> to evaluate it.
    /// </p>
    /// <p>
    /// This can result in significant performance improvements as it avoids expression
    /// parsing and node resolution every time it is called.
    /// </p>
    /// <p>
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class ExpressionEvaluator
    {
        /// <summary>
        /// Parses and evaluates specified expression.
        /// </summary>
        /// <param name="root">Root object.</param>
        /// <param name="expression">Expression to evaluate.</param>
        /// <returns>Value of the last node in the expression.</returns>
        public static object GetValue(object root, string expression)
        {
            IExpression exp = Expression.Parse(expression);
            return exp.GetValue(root, null);
        }

        /// <summary>
        /// Parses and evaluates specified expression.
        /// </summary>
        /// <param name="root">Root object.</param>
        /// <param name="expression">Expression to evaluate.</param>
        /// <param name="variables">Expression variables map.</param>
        /// <returns>Value of the last node in the expression.</returns>
        public static object GetValue(object root, string expression, IDictionary<string, object> variables)
        {
            IExpression exp = Expression.Parse(expression);
            return exp.GetValue(root, variables);
        }

        /// <summary>
        /// Parses and specified expression and sets the value of the
        /// last node to the value of the <c>newValue</c> parameter.
        /// </summary>
        /// <param name="root">Root object.</param>
        /// <param name="expression">Expression to evaluate.</param>
        /// <param name="newValue">Value to set last node to.</param>
        public static void SetValue(object root, string expression, object newValue)
        {
            IExpression exp = Expression.Parse(expression);
            exp.SetValue(root, null, newValue);
        }

        /// <summary>
        /// Parses and specified expression and sets the value of the
        /// last node to the value of the <c>newValue</c> parameter.
        /// </summary>
        /// <param name="root">Root object.</param>
        /// <param name="expression">Expression to evaluate.</param>
        /// <param name="variables">Expression variables map.</param>
        /// <param name="newValue">Value to set last node to.</param>
        public static void SetValue(object root, string expression, IDictionary<string, object> variables, object newValue)
        {
            IExpression exp = Expression.Parse(expression);
            exp.SetValue(root, variables, newValue);
        }
    }
}
