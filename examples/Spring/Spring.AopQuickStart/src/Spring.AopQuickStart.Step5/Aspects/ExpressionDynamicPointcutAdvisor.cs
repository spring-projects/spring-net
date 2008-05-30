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

#region Imports

using System;
using System.Reflection;

using Spring.Expressions;
using Spring.Aop.Support;

#endregion

namespace Spring.AopQuickStart.Aspects
{
    /// <summary>
    /// Dynamic pointcut using Spring expressions.
    /// </summary>
    /// <author>Bruno Baia</author>
    /// <version>$Id: ExpressionDynamicPointcutAdvisor.cs,v 1.1 2007/07/26 00:50:11 bbaia Exp $</version>
    public class ExpressionDynamicPointcutAdvisor : DynamicMethodMatcherPointcutAdvisor
    {
        private string expression;
        public string Expression
        {
            get { return expression; }
            set { expression = value; }
        }

        /// <summary>
        /// Is there a runtime (dynamic) match for the supplied method ?
        /// </summary>
        public override bool Matches(MethodInfo method, Type targetType, object[] args)
        {
            IExpression expr = Spring.Expressions.Expression.Parse(Expression);
            MatchingInfoHolder infoHolder = new MatchingInfoHolder(method, targetType, args);

            try
            {
                // Evaluate the expression using the current matching info as current context
                return (bool)expr.GetValue(infoHolder);
            }
            catch (Exception e)
            {
                throw new ArgumentException(
                    "Expression cannot be evaluate or not return a boolean.", "Expression", e);
            }
        }

        #region MatchingInfoHolder inner class definition

        public class MatchingInfoHolder
        {
            #region Fields

            private MethodInfo _methodInfo;
            private Type _targetType;
            private object[] _args;

            #endregion

            #region Properties

            /// <summary>
            /// Get or sets the candidate method.
            /// </summary>
            public MethodInfo MethodInfo
            {
                get { return _methodInfo; }
                set { _methodInfo = value; }
            }

            /// <summary>
            /// Gets or sets the target type.
            /// </summary>
            public Type TargetType
            {
                get { return _targetType; }
                set { _targetType = value; }
            }

            /// <summary>
            /// Gets or sets the arguments to the method.
            /// </summary>
            public object[] Args
            {
                get { return _args; }
                set { _args = value; }
            }

            #endregion

            #region Constructor(s) / Destructor

            /// <summary>
            /// Creates a new instance of the
            /// <see cref="Spring.AopQuickStart.Aspects.ExpressionDynamicPoincutAdvisor+MatchingInfoHolder"/> class.
            /// </summary>
            public MatchingInfoHolder()
            {
            }

            /// <summary>
            /// Creates a new instance of the
            /// <see cref="Spring.AopQuickStart.Aspects.ExpressionDynamicPoincutAdvisor+MatchingInfoHolder"/> class.
            /// </summary>
            public MatchingInfoHolder(MethodInfo methodInfo, Type targetType, object[] args)
            {
                _methodInfo = methodInfo;
                _targetType = targetType;
                _args = args;
            }

            #endregion
        }

        #endregion
    }
}