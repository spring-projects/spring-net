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
using Spring.Collections;
using Spring.Util;

namespace Spring.Expressions
{
    /// <summary>
    /// Represents arithmetic subtraction operator.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class OpSUBTRACT : BinaryOperator
    {
        /// <summary>
        /// Create a new instance
        /// </summary>
        public OpSUBTRACT():base()
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected OpSUBTRACT(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Returns a value for the arithmetic subtraction operator node.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Node's value.</returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            object left = GetLeftValue( context, evalContext );
            object right = GetRightValue( context, evalContext );

            if (NumberUtils.IsNumber(left) && NumberUtils.IsNumber(right))
            {
                return NumberUtils.Subtract(left, right);
            }
            else if (left is DateTime && (right is TimeSpan || right is string || NumberUtils.IsNumber(right)))
            {
                if (NumberUtils.IsNumber(right))
                {
                    right = TimeSpan.FromDays(Convert.ToDouble(right));
                }
                else if (right is string)
                {
                    right = TimeSpan.Parse((string) right);
                }
                return (DateTime) left - (TimeSpan) right;
            }
            else if (left is DateTime && right is DateTime)
            {
                return (DateTime) left - (DateTime) right;
            }
            else if (left is IList || left is ISet)
            {
                ISet leftset = new HybridSet(left as ICollection);
                ISet rightset;
                if(right is IList || right is ISet)
                {
                    rightset = new HybridSet(right as ICollection);
                }
                else if (right is IDictionary)
                {
                    rightset = new HybridSet(((IDictionary) right).Keys);
                }
                else
                {
                    throw new ArgumentException("Cannot subtract instances of '"
                    + left.GetType().FullName
                    + "' and '"
                    + right.GetType().FullName
                    + "'.");
                }
                return leftset.Minus(rightset);
            }
            else if (left is IDictionary)
            {
                ISet leftset = new HybridSet(((IDictionary) left).Keys);
                ISet rightset;
                if (right is IList || right is ISet)
                {
                    rightset = new HybridSet(right as ICollection);
                }
                else if (right is IDictionary)
                {
                    rightset = new HybridSet(((IDictionary) right).Keys);
                }
                else
                {
                    throw new ArgumentException("Cannot subtract instances of '"
                    + left.GetType().FullName
                    + "' and '"
                    + right.GetType().FullName
                    + "'.");
                }
                IDictionary result = new Hashtable(rightset.Count);
                foreach(object key in leftset.Minus(rightset))
                {
                    result.Add(key, ((IDictionary)left)[key]);
                }
                return result;
            }
            else
            {
                throw new ArgumentException("Cannot subtract instances of '"
                    + left.GetType().FullName
                    + "' and '"
                    + right.GetType().FullName
                    + "'.");
            }
        }
    }
}
