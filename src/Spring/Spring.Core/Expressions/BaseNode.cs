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
    /// Base type for all expression nodes.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    //[Serializable]
    public abstract class BaseNode : SpringAST, IExpression
    {
        protected class ArgumentMismatchException : Exception
        {
            public ArgumentMismatchException(string message)
                : base(message)
            { }
        }

        #region EvaluationContext class

        /// <summary>
        /// Holds the state during evaluating an expression.
        /// </summary>
        protected class EvaluationContext
        {
            #region Holder classes

            private class ThisContextHolder : IDisposable
            {
                private readonly EvaluationContext owner;
                private readonly object savedThisContext;

                public ThisContextHolder(EvaluationContext owner)
                {
                    this.owner = owner;
                    this.savedThisContext = owner.ThisContext;
                }

                public void Dispose()
                {
                    owner.ThisContext = savedThisContext;
                }
            }

            private class LocalVariablesHolder : IDisposable
            {
                private readonly EvaluationContext owner;
                private readonly IDictionary savedLocalVariables;

                public LocalVariablesHolder(EvaluationContext owner, IDictionary newLocalVariables)
                {
                    this.owner = owner;
                    this.savedLocalVariables = owner.LocalVariables;
                    owner.LocalVariables = newLocalVariables;
                }

                public void Dispose()
                {
                    owner.LocalVariables = savedLocalVariables;
                }
            }

            #endregion

            /// <summary>
            /// Gets/Sets the root context of the current evaluation
            /// </summary>
            public object RootContext;
            /// <summary>
            /// Gets the type of the <see cref="RootContext"/>
            /// </summary>
            public Type RootContextType { get { return (RootContext == null) ? null : RootContext.GetType(); } }
            /// <summary>
            /// Gets/Sets the current context of the current evaluation
            /// </summary>
            public object ThisContext;
            /// <summary>
            /// Gets/Sets global variables of the current evaluation
            /// </summary>
            public IDictionary<string, object> Variables;
            /// <summary>
            /// Gets/Sets local variables of the current evaluation
            /// </summary>
            public IDictionary LocalVariables;

            /// <summary>
            /// Initializes a new EvaluationContext instance.
            /// </summary>
            /// <param name="rootContext">The root context for this evaluation</param>
            /// <param name="globalVariables">dictionary of global variables used during this evaluation</param>
            public EvaluationContext(object rootContext, IDictionary<string, object> globalVariables)
            {
                this.RootContext = rootContext;
                this.ThisContext = rootContext;
                this.Variables = globalVariables;
            }

            /// <summary>
            /// Switches current ThisContext.
            /// </summary>
            public IDisposable SwitchThisContext()
            {
                return new ThisContextHolder(this);
            }

            /// <summary>
            /// Switches current LocalVariables.
            /// </summary>
            public IDisposable SwitchLocalVariables(IDictionary newLocalVariables)
            {
                return new LocalVariablesHolder(this, newLocalVariables);
            }
        }

        #endregion

        /// <summary>
        /// Create a new instance
        /// </summary>
        public BaseNode()
        { }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected BaseNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Returns node's value.
        /// </summary>
        /// <returns>Node's value.</returns>
        public object GetValue()
        {
            return GetValue(null, null);
        }

        /// <summary>
        /// Returns node's value for the given context.
        /// </summary>
        /// <param name="context">Object to evaluate node against.</param>
        /// <returns>Node's value.</returns>
        public object GetValue(object context)
        {
            return GetValue(context, null);
        }

        /// <summary>
        /// Returns node's value for the given context.
        /// </summary>
        /// <param name="context">Object to evaluate node against.</param>
        /// <param name="variables">Expression variables map.</param>
        /// <returns>Node's value.</returns>
        public object GetValue(object context, IDictionary<string, object> variables)
        {
            EvaluationContext evalContext = new EvaluationContext(context, variables);
            return Get(context, evalContext);
        }

        /// <summary>
        /// Returns node's value for the given context.
        /// </summary>
        /// <returns>Node's value.</returns>
        protected abstract object Get(object context, EvaluationContext evalContext);

        /// <summary>
        /// Evaluates this node for the given context, switching local variables map to the ones specified in <paramref name="arguments"/>.
        /// </summary>
        protected virtual object Get(object context, EvaluationContext evalContext, object[] arguments)
        {
            throw new NotSupportedException("Node " + this.GetType() + " does not support evaluation with arguments");
        }

        /// <summary>
        /// Sets node's value for the given context.
        /// </summary>
        /// <param name="context">Object to evaluate node against.</param>
        /// <param name="newValue">New value for this node.</param>
        public void SetValue(object context, object newValue)
        {
            SetValue(context, null, newValue);
        }

        /// <summary>
        /// Sets node's value for the given context.
        /// </summary>
        /// <param name="context">Object to evaluate node against.</param>
        /// <param name="variables">Expression variables map.</param>
        /// <param name="newValue">New value for this node.</param>
        public void SetValue(object context, IDictionary<string, object> variables, object newValue)
        {
            EvaluationContext evalContext = new EvaluationContext(context, variables);
            Set(context, evalContext, newValue);
        }

        /// <summary>
        /// Sets node's value for the given context.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is a default implementation of <c>Set</c> method, which
        /// simply throws <see cref="NotSupportedException"/>.
        /// </p>
        /// <p>
        /// This was done in order to avoid redundant <c>Set</c> method implementations,
        /// because most of the node types do not support value setting.
        /// </p>
        /// </remarks>
        protected virtual void Set(object context, EvaluationContext evalContext, object newValue)
        {
            throw new NotSupportedException("You cannot set the value for the node of this type: [" + this.GetType().Name + "].");
        }

        /// <summary>
        /// Returns a string representation of this node instance.
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}[{1}]", this.GetType().Name, base.GetHashCode());
        }

        /// <summary>
        /// Evaluates this node, switching local variables map to the ones specified in <paramref name="arguments"/>.
        /// </summary>
        protected object GetValueWithArguments(BaseNode node, object context, EvaluationContext evalContext, object[] arguments)
        {
            return node.Get(context, evalContext, arguments);
        }

        protected object GetValue(BaseNode node, object context, EvaluationContext evalContext)
        {
            return node.Get(context, evalContext);
        }

        protected void SetValue(BaseNode node, object context, EvaluationContext evalContext, object newValue)
        {
            node.Set(context, evalContext, newValue);
        }
    }
}
