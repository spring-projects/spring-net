#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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
using Spring.Expressions;
using Spring.Objects.Factory;

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
        public ReferenceNode():base()
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected ReferenceNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        
        /// <summary>
        /// Returns a value for the integer literal node.
        /// </summary>
        /// <param name="context">Context to evaluate expressions against.</param>
        /// <param name="evalContext">Current expression evaluation context.</param>
        /// <returns>Node's value.</returns>
        protected override object Get(object context, EvaluationContext evalContext)
        {
            IApplicationContext ctx;
            string objectName;

            if (this.getNumberOfChildren() == 2)
            {
                string contextName = this.getFirstChild().getText();
                objectName = this.getFirstChild().getNextSibling().getText();
                ctx = ContextRegistry.GetContext(contextName);
                if (ctx == null)
                {
                    throw new ArgumentException(string.Format("Context '{0}' is not registered.", contextName));
                }
            }
            else
            {
                objectName = this.getFirstChild().getText();
                IObjectFactory currentObjectFactory = (evalContext.Variables != null)
                                                          ? (IObjectFactory)evalContext.Variables[Expression.ReservedVariableNames.CurrentObjectFactory]
                                                          : null;

                // this is a local reference within an object factory
                if (currentObjectFactory != null)
                {
                    return currentObjectFactory.GetObject(objectName);
                }

                // else lookup in default context
                ctx = ContextRegistry.GetContext();
                if (ctx == null)
                {
                    throw new ArgumentException("No context registered.");
                }
            }

            return ctx.GetObject(objectName);
        }
    }
}