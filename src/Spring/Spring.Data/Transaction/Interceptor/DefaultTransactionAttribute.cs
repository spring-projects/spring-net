/*
 * Copyright 2002-2010 the original author or authors.
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

using Spring.Transaction.Support;

namespace Spring.Transaction.Interceptor
{
    /// <summary>
    /// Transaction attribute approach to rolling back on all exceptions, no other
    /// exceptions by default.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Griffin Caprio (.NET)</author>
    public class DefaultTransactionAttribute : DefaultTransactionDefinition, ITransactionAttribute
    {
        /// <summary>
        /// Prefix for rollback-on-exception rules in description strings.
        /// </summary>
        public static readonly string ROLLBACK_RULE_PREFIX = "-";

        /// <summary>
        /// Prefix for commit-on-exception rules in description strings.
        /// </summary>
        public static readonly string COMMIT_RULE_PREFIX = "+";

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Transaction.Interceptor.DefaultTransactionAttribute"/>
        /// class.
        /// </summary>
        public DefaultTransactionAttribute()
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Transaction.Interceptor.DefaultTransactionAttribute"/>
        /// class, setting the propagation behavior to the supplied value.
        /// </summary>
        /// <param name="propagationBehavior">
        /// The desired transaction propagation behaviour.
        /// </param>
        public DefaultTransactionAttribute(TransactionPropagation propagationBehavior)
            : base(propagationBehavior)
        {
        }

        /// <summary>
        /// Decides if rollback is required for the supplied <paramref name="exception"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The default behavior is to rollback on any exception.
        /// Consistent with <see cref="Spring.Transaction.Support.TransactionTemplate"/>'s behavior.
        /// </p>
        /// </remarks>
        /// <param name="exception">The <see cref="System.Exception"/> to evaluate.</param>
        /// <returns>True if the exception causes a rollback, false otherwise.</returns>
        public virtual bool RollbackOn(Exception exception)
        {
            return (true);
        }

        /// <summary>
        /// Return a description of this transaction attribute.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The format matches the one used by the
        /// <see cref="Spring.Transaction.Interceptor.TransactionAttributeEditor"/>,
        /// to be able to feed any result into a <see cref="Spring.Transaction.Interceptor.ITransactionAttribute"/>
        /// instance's properties.
        /// </p>
        /// </remarks>
        public override string ToString()
        {
            string result = DefinitionDescription;
            result += "," + ROLLBACK_RULE_PREFIX + "System.Exception";
            return result;
        }
    }
}
