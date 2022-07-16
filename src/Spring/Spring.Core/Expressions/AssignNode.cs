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

using System.Runtime.Serialization;
using Spring.Expressions.Parser.antlr.collections;

namespace Spring.Expressions
{
    /// <summary>
    /// Represents parsed assignment node in the navigation expression.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class AssignNode : BaseNode
    {
        /// <summary>
        /// Create a new instance
        /// </summary>
        public AssignNode()
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected AssignNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Assigns value of the right operand to the left one.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Node's value.</returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            AST left = getFirstChild();
            AST right = left.getNextSibling();

            object result;

            if (right.getFirstChild() is LambdaExpressionNode)
            {
                if (!(left.getFirstChild() is VariableNode))
                {
                    throw new ArgumentException("Lambda expression can only be assigned to a global variable.");
                }
                result = right.getFirstChild();
            }
            else
            {
                result = GetValue(((BaseNode)right), context, evalContext);
            }

            SetValue(((BaseNode)left), context, evalContext, result );

            return result;
        }
    }
}
