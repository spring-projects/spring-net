#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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

using System;
using System.Runtime.Serialization;

using antlr.collections;
using Spring.Core.TypeResolution;

namespace Spring.Expressions
{
    /// <summary>
    /// Represents parsed type node in the navigation expression.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    /// <version>$Id: TypeNode.cs,v 1.11 2007/09/07 03:01:26 markpollack Exp $</version>
    [Serializable]
    public class TypeNode : BaseNode
    {
        private Type type;

        /// <summary>
        /// Create a new instance
        /// </summary>
        public TypeNode():base()
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected TypeNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        
        /// <summary>
        /// Returns node's value for the given context.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Node's value.</returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            if (type == null)
            {
                lock (this)
                {
                    if (type == null)
                    {
                        string typeName = this.getText();
                        AST node = this.getFirstChild();
                        while (node != null)
                        {
                            typeName += node.getText();
                            node = node.getNextSibling();
                        }

                        type = TypeResolutionUtils.ResolveType(typeName);
                    }
                }
            }

            return type;
        }
    }
}