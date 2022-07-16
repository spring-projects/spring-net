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
using System.Text;
using System.Text.RegularExpressions;

namespace Spring.Expressions
{
    /// <summary>
    /// Represents VB-style logical LIKE operator.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class OpLike : BinaryOperator
    {
        /// <summary>
        /// Create a new instance
        /// </summary>
        public OpLike()
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected OpLike(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Returns a value for the logical LIKE operator node.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>
        /// true if the left operand matches the right operand, false otherwise.
        /// </returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            string text = GetLeftValue(context, evalContext) as string;
            string pattern = GetRightValue(context, evalContext) as string;

            return LikeString(text, pattern);
        }

        private static bool LikeString(string Source, string Pattern)
        {
            if (string.IsNullOrEmpty(Source) && string.IsNullOrEmpty(Pattern))
            {
                return true;
                // LAMESPEC : MSDN states "if either string or pattern is an empty string, the result is False."
                // but "" Like "[]" returns True
            }

            if ((string.IsNullOrEmpty(Source) || string.IsNullOrEmpty(Pattern)) && string.Compare(Pattern, "[]") != 0)
            {
                return false;
            }

            RegexOptions options = RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;

            string regexString = ConvertLikeExpression(Pattern);
            Regex regexpr = new Regex(regexString, options);

            //Console.WriteLine("{0} --> {1}", Pattern, regexString)

            return regexpr.IsMatch(Source);
        }

        private static string ConvertLikeExpression(string expression)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("^");

            for (int pos = 0; pos <= expression.Length - 1; pos++)
            {
                switch (expression[pos])
                {
                    case '?':
                        sb.Append('.');
                        break;
                    case '*':
                        sb.Append('.').Append('*');
                        break;
                    case '#':
                        sb.Append("\\d{1}");
                        break;
                    case '[':
                        StringBuilder gsb = ConvertGroupSubexpression(expression, ref pos);
                        // skip groups of form [], i.e. empty strings
                        if (gsb.Length > 2)
                        {
                            sb.Append(gsb);
                        }
                        break;
                    default:
                        sb.Append(Regex.Escape(expression[pos].ToString()));
                        break;
                }
            }

            sb.Append("$");

            return sb.ToString();
        }

        private static StringBuilder ConvertGroupSubexpression(string carr, ref int pos)
        {
            StringBuilder sb = new StringBuilder();
            bool negate = false;

            while (carr[pos] != ']')
            {
                if (negate)
                {
                    sb.Append('^');
                    negate = false;
                }
                if (carr[pos] == '!')
                {
                    sb.Remove(1, sb.Length - 1);
                    negate = true;
                }
                else
                {
                    sb.Append(carr[pos]);
                }
                pos = pos + 1;
            }
            sb.Append(']');

            return sb;
        }
    }
}
