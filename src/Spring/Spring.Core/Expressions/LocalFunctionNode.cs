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

namespace Spring.Expressions
{
    /// <summary>
    /// Represents local function node.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class LocalFunctionNode : NodeWithArguments
    {
        /// <summary>
        /// Create a new instance
        /// </summary>
        public LocalFunctionNode()
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected LocalFunctionNode(SerializationInfo info, StreamingContext context)
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
            IDictionary locals = evalContext.LocalVariables;
            LambdaExpressionNode lambda = locals[name] as LambdaExpressionNode;

            if (lambda == null)
            {
                throw new InvalidOperationException("Function '" + name + "' is not defined.");
            }

            object[] argValues = ResolveArguments(evalContext);

            try
            {
                return GetValueWithArguments(lambda, context, evalContext, argValues);
            }
            catch (ArgumentMismatchException ame)
            {
                throw new InvalidOperationException("Failed executing function '" + name + "': " + ame.Message);
            }
        }
    }
}
