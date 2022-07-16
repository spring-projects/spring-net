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
using Spring.Reflection.Dynamic;

namespace Spring.Expressions
{
    /// <summary>
    /// Represents parsed function node.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class FunctionNode : NodeWithArguments
    {
        /// <summary>
        /// Create a new instance
        /// </summary>
        public FunctionNode()
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected FunctionNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Evaluates function represented by this node.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Result of the function evaluation.</returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            string name = this.getText();

            object[] argValues = ResolveArguments(evalContext);

            object function = evalContext.Variables[name];

            // delegate?
            Delegate callback = function as Delegate;
            if (callback != null)
            {
                return InvokeDelegate(callback, argValues);
            }

            // lambda?
            LambdaExpressionNode lambda = function as LambdaExpressionNode;
            if (lambda != null)
            {
                try
                {
                    return GetValueWithArguments(lambda, context, evalContext, argValues);
                }
                catch (ArgumentMismatchException ame)
                {
                    throw new InvalidOperationException( "Failed executing function " + name + ": " + ame.Message );
                }
            }

            if (function == null)
            {
                throw new InvalidOperationException("Function '" + name + "' is not defined.");
            }
            throw new InvalidOperationException("Function '" + name + "' is defined but of unknown type.");
        }

        private object InvokeDelegate(Delegate callback, object[] arguments)
        {
            return new SafeMethod(callback.Method).Invoke(callback.Target, arguments);
        }
    }
}
