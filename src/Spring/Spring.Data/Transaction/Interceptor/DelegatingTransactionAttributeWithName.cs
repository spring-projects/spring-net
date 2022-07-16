#region License

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

#endregion

using System.Transactions;
using IsolationLevel=System.Data.IsolationLevel;

namespace Spring.Transaction.Interceptor
{
    /// <summary>
    /// ITransactionAttribute that delegates all calls to a give target attribute except for the
    /// name, which is specified in the constructor.
    /// </summary>
    public class DelegatingTransactionAttributeWithName : ITransactionAttribute
    {
        private readonly ITransactionAttribute targetAttribute;
        private readonly string joinpointIdentification;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegatingTransactionAttributeWithName"/> class.
        /// </summary>
        /// <param name="targetAttribute">The target attribute.</param>
        /// <param name="identification">The identification.</param>
        public DelegatingTransactionAttributeWithName(ITransactionAttribute targetAttribute, string identification)
        {
            this.targetAttribute = targetAttribute;
            joinpointIdentification = identification;
        }

        /// <summary>
        /// Decides if rollback is required for the supplied <paramref name="exception"/>.
        /// </summary>
        /// <param name="exception">The <see cref="System.Exception"/> to evaluate.</param>
        /// <returns>
        /// True if the exception causes a rollback, false otherwise.
        /// </returns>
        public bool RollbackOn(Exception exception)
        {
            return targetAttribute.RollbackOn(exception);
        }

        /// <summary>
        /// Return the propagation behavior of type
        /// <see cref="Spring.Transaction.TransactionPropagation"/>.
        /// </summary>
        /// <value></value>
        public TransactionPropagation PropagationBehavior => targetAttribute.PropagationBehavior;

        /// <summary>
        /// Return the isolation level of type <see cref="System.Data.IsolationLevel"/>.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// 	<p>
        /// Only makes sense in combination with
        /// <see cref="Spring.Transaction.TransactionPropagation.Required"/> and
        /// <see cref="Spring.Transaction.TransactionPropagation.RequiresNew"/>.
        /// </p>
        /// 	<p>
        /// Note that a transaction manager that does not support custom isolation levels
        /// will throw an exception when given any other level than
        /// <see cref="System.Data.IsolationLevel.Unspecified"/>.
        /// </p>
        /// </remarks>
        public IsolationLevel TransactionIsolationLevel => targetAttribute.TransactionIsolationLevel;

        /// <inheritdoc />
        public int TransactionTimeout => targetAttribute.TransactionTimeout;

        /// <inheritdoc />
        public bool ReadOnly => targetAttribute.ReadOnly;

        /// <inheritdoc />
        public string Name => joinpointIdentification;

        /// <inheritdoc />
        public TransactionScopeAsyncFlowOption AsyncFlowOption => targetAttribute.AsyncFlowOption;

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
            return targetAttribute.ToString();
        }
    }
}
