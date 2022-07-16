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
using Spring.Core.TypeResolution;
using Spring.Expressions.Parser.antlr.collections;

namespace Spring.Expressions
{
    /// <summary>
    /// Represents parsed method node in the navigation expression.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class ArrayConstructorNode : NodeWithArguments
    {
        private Type arrayType;

        /// <summary>
        /// Create a new instance
        /// </summary>
        public ArrayConstructorNode()
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected ArrayConstructorNode(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// Creates new instance of the type defined by this node.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Node's value.</returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            if (arrayType == null)
            {
                lock (this)
                {
                    if (arrayType == null)
                    {
                        arrayType = TypeResolutionUtils.ResolveType(getText());
                    }
                }
            }

            AST rankRoot = getFirstChild();
            int dimensions = rankRoot.getNumberOfChildren();
            int[] ranks = new int[dimensions];
            if (dimensions > 0)
            {
                int i = 0;
                AST rankNode = rankRoot.getFirstChild();
                while (rankNode != null)
                {
                    ranks[i++] = (int)GetValue((BaseNode)rankNode, context, evalContext);
                    rankNode = rankNode.getNextSibling();
                }
                return Array.CreateInstance(arrayType, ranks);
            }
            else
            {
                AST valuesRoot = getFirstChild().getNextSibling();
                if (valuesRoot != null)
                {
                    ArrayList values = (ArrayList)GetValue(((BaseNode)valuesRoot), context, evalContext);
                    return values.ToArray(arrayType);
                }
            }

            throw new ArgumentException("You have to specify either rank or initializer for an array.");
        }
    }
}
