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
    /// Represents arithmetic addition operator.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class OpADD : BinaryOperator
    {
        /// <summary>
        /// Create a new instance
        /// </summary>
        public OpADD()
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected OpADD(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Returns a value for the arithmetic addition operator node.
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
                return NumberUtils.Add(left, right);
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

                return (DateTime) left + (TimeSpan) right;
            }
            else if (left is String || right is String)
            {
                return string.Concat(left, right);
            }
            else if ((left is IList || left is ISet) && (right is IList || right is ISet))
            {
                ISet leftset = new HybridSet(left as ICollection);
                ISet rightset = new HybridSet(right as ICollection);
                return leftset.Union(rightset);
            }
            else if (left is IDictionary && right is IDictionary)
            {
                ISet leftset = new HybridSet(((IDictionary) left).Keys);
                ISet rightset = new HybridSet(((IDictionary) right).Keys);
                ISet unionset = leftset.Union(rightset);

                IDictionary result = new Hashtable(unionset.Count);
                foreach(object key in unionset)
                {
                    if(leftset.Contains(key))
                    {
                        result.Add(key, ((IDictionary)left)[key]);
                    }
                    else
                    {
                        result.Add(key, ((IDictionary)right)[key]);
                    }
                }
                return result;
            }
            else
            {
                throw new ArgumentException("Cannot add instances of '"
                                            + left.GetType().FullName
                                            + "' and '"
                                            + right.GetType().FullName
                                            + "'.");
            }
        }
    }
}
