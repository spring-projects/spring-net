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
using System.Text.RegularExpressions;

namespace Spring.Expressions
{
    /// <summary>
    /// Represents logical MATCHES operator.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class OpMatches : BinaryOperator
    {
        private Regex regex;

        /// <summary>
        /// Create a new instance
        /// </summary>
        public OpMatches():base()
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected OpMatches(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Returns a value for the logical MATCHES operator node.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>
        /// true if the left operand matches the right operand, false otherwise.
        /// </returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            if (regex == null)
            {
                lock (this)
                {
                    if (regex == null)
                    {
                        string pattern = GetRightValue( context, evalContext ) as string;
                        regex = new Regex(pattern, RegexOptions.Compiled);
                    }
                }
            }

            string text = GetLeftValue( context, evalContext ) as string;
            return regex.IsMatch(text);
        }
    }
}
