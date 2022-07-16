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

namespace Spring.Expressions
{
    /// <summary>
    /// Represents parsed boolean literal node.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class BooleanLiteralNode : BaseNode
    {
        private object nodeValue;

        /// <summary>
        /// Create a new instance
        /// </summary>
        public BooleanLiteralNode()
        {
        }

        /// <summary>
        /// Create a new instance
        /// </summary>
        public BooleanLiteralNode(string text)
        {
            this.Text = text;
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected BooleanLiteralNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Returns a value for the boolean literal node.
        /// </summary>
        /// <summary>
        /// This is the entrypoint into evaluating this expression.
        /// </summary>
        /// <returns>Node's value.</returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            if (nodeValue == null)
            {
                lock(this)
                {
                    if (nodeValue == null)
                    {
                        nodeValue = Boolean.Parse(this.getText());
                    }
                }
            }

            return nodeValue;
        }
    }
}
