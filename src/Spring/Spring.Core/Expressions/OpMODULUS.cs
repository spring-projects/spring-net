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
    /// Represents arithmetic modulus operator.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class OpMODULUS : BinaryOperator
    {
        /// <summary>
        /// Create a new instance
        /// </summary>
        public OpMODULUS():base()
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected OpMODULUS(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Returns a value for the arithmetic modulus operator node.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Node's value.</returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            object leftVal = GetLeftValue(context, evalContext );
            object rightVal = GetRightValue(context, evalContext );

            if (NumberUtils.IsNumber(leftVal) && NumberUtils.IsNumber(rightVal))
            {
                return NumberUtils.Modulus(leftVal, rightVal);
            }
            else
            {
                throw new ArgumentException("Cannot calculate modulus for instances of '"
                                            + leftVal.GetType().FullName
                                            + "' and '"
                                            + rightVal.GetType().FullName
                                            + "'.");
            }
        }
    }
}
