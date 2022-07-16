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

using Spring.Expressions;
using Spring.Objects.Factory;
using System.Runtime.Serialization;

namespace Spring.Context.Support
{
    /// <summary>
    /// Represents a reference to a Spring-managed object.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class ReferenceNode : BaseNode
    {
        /// <summary>
        /// Create a new instance
        /// </summary>
        public ReferenceNode() { }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected ReferenceNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns a value for the integer literal node.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Node's value.</returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            var objectName = ResolveNames(out var contextName);

            var sourceContext = SelectSourceContext(evalContext, contextName);

            return sourceContext.GetObject(objectName);
        }

        private string ResolveNames(out string contextName)
        {
            var hasContextDefined = getNumberOfChildren() == 2;

            if (hasContextDefined)
            {
                contextName = getFirstChild().getText();
                return getFirstChild().getNextSibling().getText();
            }

            contextName = null;
            return getFirstChild().getText();
        }

        private static IObjectFactory SelectSourceContext(EvaluationContext evalContext, string contextName)
        {
            if (contextName != null)
                return ContextRegistry.GetContext(contextName) ?? throw new ArgumentException($"Context '{contextName}' is not registered.");

            if (TryGetFromCurrentContext(evalContext, out var currentObjectFactory))
                return (IObjectFactory) currentObjectFactory;

            return ContextRegistry.GetContext() ?? throw new ArgumentException("No context registered.");
        }

        private static bool TryGetFromCurrentContext(EvaluationContext evalContext, out object currentObjectFactory)
        {
            currentObjectFactory = null;

            if (evalContext.Variables is null)
                return false;

            return evalContext.Variables.TryGetValue(Expression.ReservedVariableNames.CurrentObjectFactory, out currentObjectFactory);
        }
    }
}
