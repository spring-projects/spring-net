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
using Spring.Expressions.Parser.antlr.collections;

namespace Spring.Expressions
{
    /// <summary>
    /// Base type for nodes that accept arguments.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public abstract class NodeWithArguments : BaseNode
    {
        private BaseNode[] args;
        private IDictionary namedArgs;

        /// <summary>
        /// Create a new instance
        /// </summary>
        public NodeWithArguments()
        {
        }

        /// <summary>
        /// Create a new instance
        /// </summary>
        public NodeWithArguments(string text)
        {
            this.setText(text);
        }

        /// <summary>
        /// Append an argument node to the list of child nodes
        /// </summary>
        /// <param name="argumentNode"></param>
        public void AddArgument(BaseNode argumentNode)
        {
            base.addChild(argumentNode);
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected NodeWithArguments(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes the node.
        /// </summary>
        private void InitializeNode()
        {
            lock (this)
            {
                if (args == null)
                {
                    List<BaseNode> argList = new List<BaseNode>();
                    namedArgs = new Hashtable();

                    AST node = this.getFirstChild();

                    while (node != null)
                    {
                        if (node.getFirstChild() is LambdaExpressionNode)
                        {
                            argList.Add((BaseNode) node.getFirstChild());
                        }
                        else if (node is NamedArgumentNode)
                        {
                            namedArgs.Add(node.getText(), node);
                        }
                        else
                        {
                            argList.Add((BaseNode) node);
                        }
                        node = node.getNextSibling();
                    }

                    args = argList.ToArray();
                }
            }
        }

        /// <summary>
        /// Asserts the argument count.
        /// </summary>
        /// <param name="requiredCount">The required count.</param>
        protected void AssertArgumentCount(int requiredCount)
        {
            InitializeNode();
            if (requiredCount != args.Length)
            {
                throw new ArgumentException("This expression node requires exactly " +
                                            requiredCount + " argument(s) and " +
                                            args.Length + " were specified.");
            }
        }

        /// <summary>
        /// Resolves the arguments.
        /// </summary>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>An array of argument values</returns>
        protected object[] ResolveArguments(EvaluationContext evalContext)
        {
            if (args == null)
            {
                InitializeNode();
            }

            int length = args.Length;
            object[] values = new object[length];
            for (int i = 0; i < length; i++)
            {
                values[i] = ResolveArgumentInternal(i, evalContext);
            }
            return values;
        }

        /// <summary>
        /// Resolves the named arguments.
        /// </summary>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>A dictionary of argument name to value mappings.</returns>
        protected IDictionary ResolveNamedArguments(EvaluationContext evalContext)
        {
            if (args == null)
            {
                InitializeNode();
            }

            if (namedArgs.Count == 0)
            {
                return null;
            }

            IDictionary namesAndValues = new Hashtable(namedArgs.Count);
            foreach (string name in namedArgs.Keys)
            {
                namesAndValues[name] = ResolveNamedArgument(name, evalContext);
            }
            return namesAndValues;
        }

        /// <summary>
        /// Resolves the argument.
        /// </summary>
        /// <param name="position">Argument position.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Resolved argument value.</returns>
        protected object ResolveArgument(int position, EvaluationContext evalContext)
        {
            if (args == null)
            {
                InitializeNode();
            }
            return ResolveArgumentInternal(position, evalContext);
        }

        /// <summary>
        /// Resolves the argument without ensuring <see cref="InitializeNode"/> was called.
        /// </summary>
        /// <param name="position">Argument position.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Resolved argument value.</returns>
        private object ResolveArgumentInternal(int position, EvaluationContext evalContext)
        {
            BaseNode arg = args[position];
            if (arg is LambdaExpressionNode)
            {
                return arg;
            }
            return GetValue(arg, evalContext.ThisContext, evalContext);
        }

        /// <summary>
        /// Resolves the named argument.
        /// </summary>
        /// <param name="name">Argument name.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Resolved named argument value.</returns>
        private object ResolveNamedArgument(string name, EvaluationContext evalContext)
        {
            return GetValue(((BaseNode)namedArgs[name]), evalContext.ThisContext, evalContext);
        }

    }
}
