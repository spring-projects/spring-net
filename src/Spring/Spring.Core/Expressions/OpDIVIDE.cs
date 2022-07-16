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
using Spring.Util;

namespace Spring.Expressions
{
    /// <summary>
    /// Represents arithmetic division operator.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class OpDIVIDE : BinaryOperator
    {
        /// <summary>
        /// Create a new instance
        /// </summary>
        public OpDIVIDE()
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected OpDIVIDE(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Returns a value for the arithmetic division operator node.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Node's value.</returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            object left = GetLeftValue(context, evalContext);
            object right = GetRightValue(context, evalContext);

            if (NumberUtils.IsNumber(left) && NumberUtils.IsNumber(right))
            {
                return NumberUtils.Divide(left, right);
            }
            else
            {
                throw new ArgumentException("Cannot divide instances of '"
                                            + left.GetType().FullName
                                            + "' and '"
                                            + right.GetType().FullName
                                            + "'.");
            }
        }
    }
}
