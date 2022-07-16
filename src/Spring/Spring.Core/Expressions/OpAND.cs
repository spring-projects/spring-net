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
    /// Represents AND operator (both, bitwise and logical).
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class OpAND : BinaryOperator
    {
        /// <summary>
        /// Create a new instance
        /// </summary>
        public OpAND()
        {
        }

        /// <summary>
        /// Create a new instance
        /// </summary>
        public OpAND(BaseNode left, BaseNode right)
            :base(left, right)
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected OpAND(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        
        /// <summary>
        /// Returns a value for the logical AND operator node.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Node's value.</returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            object l = GetLeftValue(context, evalContext);

            if (NumberUtils.IsInteger(l))
            {
                object r = GetRightValue(context, evalContext);
                if (NumberUtils.IsInteger(r))
                {
                    return NumberUtils.BitwiseAnd(l, r);
                }
            }
            else if (l is Enum)
            {
                object r = GetRightValue(context, evalContext);
                if (l.GetType() == r.GetType())
                {
                    Type enumType = l.GetType();
                    Type integralType = Enum.GetUnderlyingType(enumType);
                    l = Convert.ChangeType(l, integralType);
                    r = Convert.ChangeType(r, integralType);
                    object result = NumberUtils.BitwiseAnd(l, r);
                    return Enum.ToObject(enumType, result);
                }
            }

            return Convert.ToBoolean(l) &&
                Convert.ToBoolean(GetRightValue(context, evalContext));
        }
    }
}
