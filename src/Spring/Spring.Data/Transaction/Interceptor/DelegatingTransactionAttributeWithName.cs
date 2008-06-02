#region License

/*
 * Copyright 2002-2004 the original author or authors.
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
#if NET_2_0
using System.Transactions;
#endif
using IsolationLevel=System.Data.IsolationLevel;

namespace Spring.Transaction.Interceptor
{
    /// <summary>
    /// ITransactionAttribute that delegates all calls to a give target attribute except for the
    /// name, which is specified in the constructor.
    /// </summary>
    public class DelegatingTransactionAttributeWithName : ITransactionAttribute
    {
        private ITransactionAttribute targetAttribute;
        private string joinpointIdentification;

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
        public TransactionPropagation PropagationBehavior
        {
            get { return targetAttribute.PropagationBehavior; }
        }

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
        public IsolationLevel TransactionIsolationLevel
        {
            get { return targetAttribute.TransactionIsolationLevel; }
        }

        /// <summary>
        /// Return the transaction timeout.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// 	<p>
        /// Must return a number of seconds, or -1.
        /// Only makes sense in combination with
        /// <see cref="Spring.Transaction.TransactionPropagation.Required"/> and
        /// <see cref="Spring.Transaction.TransactionPropagation.RequiresNew"/>.
        /// Note that a transaction manager that does not support timeouts will
        /// throw an exception when given any other timeout than -1.
        /// </p>
        /// </remarks>
        public int TransactionTimeout
        {
            get { return targetAttribute.TransactionTimeout; }
        }

        /// <summary>
        /// Get whether to optimize as read-only transaction.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// 	<p>
        /// This just serves as hint for the actual transaction subsystem,
        /// it will <i>not necessarily</i> cause failure of write accesses.
        /// </p>
        /// 	<p>
        /// Only makes sense in combination with
        /// <see cref="Spring.Transaction.TransactionPropagation.Required"/> and
        /// <see cref="Spring.Transaction.TransactionPropagation.RequiresNew"/>.
        /// </p>
        /// 	<p>
        /// A transaction manager that cannot interpret the read-only hint
        /// will <i>not</i> throw an exception when given <c>ReadOnly=true</c>.
        /// </p>
        /// </remarks>
        public bool ReadOnly
        {
            get { return targetAttribute.ReadOnly; }
        }

        /// <summary>
        /// Return the name of this transaction.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// The exposed name will be the fully
        /// qualified type name + "." method name + assembly (by default).
        /// </remarks>
        public string Name
        {
            get { return joinpointIdentification; }
        }

#if NET_2_0

        /// <summary>
        /// Gets the enterprise services interop option.
        /// </summary>
        /// <value>The enterprise services interop option.</value>
        public EnterpriseServicesInteropOption EnterpriseServicesInteropOption
        {
            get { return targetAttribute.EnterpriseServicesInteropOption;  }
        }
#endif

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