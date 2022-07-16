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
using Spring.Core.TypeResolution;
using Spring.Expressions.Parser.antlr.collections;

namespace Spring.Expressions
{
    /// <summary>
    /// Represents parsed type node in the navigation expression.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class TypeNode : BaseNode
    {
        private Type type;

        /// <summary>
        /// Create a new instance
        /// </summary>
        public TypeNode()
            : base()
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
                lock(this)
                {
                    type = TypeResolutionUtils.ResolveType(getText());
                }
            }

            return type;
        }

        /// <summary>
        /// Overrides getText to allow easy way to get fully
        /// qualified typename.
        /// </summary>
        /// <returns>
        /// Fully qualified typename as a string.
        /// </returns>
        public override string getText()
        {
            string tmp = base.getText();
//            if (tmp != null && TypeRegistry.ContainsAlias(tmp))
//            {
//                Type type = TypeRegistry.ResolveType(tmp);
//                if (type != null)
//                {
//                    tmp = type.AssemblyQualifiedName;
//                }
//            }
            AST node = this.getFirstChild();
            while (node != null)
            {
                tmp += node.getText();
                node = node.getNextSibling();
            }
            return tmp;
        }
    }
}
