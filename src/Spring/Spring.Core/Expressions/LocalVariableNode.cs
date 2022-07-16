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
    /// Represents parsed variable node.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class LocalVariableNode : BaseNode
    {
        //internal const string LOCAL_VARIABLES = "__locals";

        /// <summary>
        /// Create a new instance
        /// </summary>
        public LocalVariableNode()
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected LocalVariableNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Returns value of the local variable represented by this node.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Node's value.</returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            string varName = this.getText();
            IDictionary locals = evalContext.LocalVariables;
            if (locals != null)
            {
                return locals[varName];
            }
            return null;
        }

        /// <summary>
        /// Sets value of the local variable represented by this node.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <param name="newValue">New value for this node.</param>
        protected override void Set(object context, EvaluationContext evalContext, object newValue)
        {
            string varName = this.getText();
            IDictionary locals = evalContext.LocalVariables;
            if (locals == null)
            {
                locals = new Hashtable();
                evalContext.LocalVariables = locals;
            }
            locals[varName] = newValue;
        }
    }
}
