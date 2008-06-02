#region License

/*
 * Copyright © 2002-2006 the original author or authors.
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
using System.Collections;
using System.Runtime.Serialization;
using antlr.collections;

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
            if (args == null)
            {
                lock (this)
                {
                    if (args == null)
                    {
                        ArrayList argList = new ArrayList();
                        namedArgs = new Hashtable();

                        AST node = this.getFirstChild();

                        while (node != null)
                        {
                            if (node.getFirstChild() is LambdaExpressionNode)
                            {
                                argList.Add(node.getFirstChild());
                            }
                            else if (node is NamedArgumentNode)
                            {
                                namedArgs.Add(node.getText(), node);
                            }
                            else
                            {
                                argList.Add(node);
                            }
                            node = node.getNextSibling();
                        }

                        args = (BaseNode[])argList.ToArray(typeof(BaseNode));
                    }
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
            InitializeNode();
            object[] values = new object[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                values[i] = ResolveArgument(i, evalContext);
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
            InitializeNode();
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
            InitializeNode();
            if (args[position] is LambdaExpressionNode)
            {
                return args[position];
            }
            else
            {
                return ((BaseNode)args[position]).GetValueInternal(evalContext.ThisContext, evalContext);
            }
        }

        /// <summary>
        /// Resolves the named argument.
        /// </summary>
        /// <param name="name">Argument name.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Resolved named argument value.</returns>
        private object ResolveNamedArgument(string name, EvaluationContext evalContext)
        {
            return ((BaseNode)namedArgs[name]).GetValueInternal(evalContext.ThisContext, evalContext);
        }

    }
}