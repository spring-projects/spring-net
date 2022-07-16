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

using System.Collections;
using System.Runtime.Serialization;

namespace Spring.Expressions
{
    /// <summary>
    /// Represents logical IN operator.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class OpIn : BinaryOperator
    {
        /// <summary>
        /// Create a new instance
        /// </summary>
        public OpIn():base()
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected OpIn(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Returns a value for the logical IN operator node.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>
        /// true if the left operand is contained within the right operand, false otherwise.
        /// </returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            object left = GetLeftValue( context, evalContext );
            object right = GetRightValue( context, evalContext );

            if (right == null)
            {
                return false;
            }
            else if (right is IList)
            {
                return ((IList) right).Contains(left);
            }
            else if (right is IDictionary)
            {
                return ((IDictionary) right).Contains(left);
            }
            else
            {
                throw new ArgumentException(
                    "Right hand parameter for 'in' operator has to be an instance of IList or IDictionary.");
            }
        }
    }
}
