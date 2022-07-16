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

namespace Spring.Transaction.Support
{
	/// <summary>
	/// Simple convenience class for TransactionCallback implementation.
	/// Allows for implementing a DoInTransaction version without result,
	/// i.e. without the need for a return statement.
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	public abstract class TransactionCallbackWithoutResult : ITransactionCallback
	{
		#region Methods

        /// <summary>
        /// Gets called by TransactionTemplate.execute within a transactional context
        /// when no return value is required.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <remarks>
        /// Does not need to care about transactions itself, although it can retrieve
        /// and influence the status of the current transaction via the given status
        /// object, e.g. setting rollback-only.
        /// A RuntimeException thrown by the callback is treated as application
        /// exception that enforces a rollback. An exception gets propagated to the
        /// caller of the template.
        /// </remarks>
        public abstract void DoInTransactionWithoutResult(ITransactionStatus status);

	    #endregion

        #region ITransactionCallback Members

        /// <summary>
        /// Gets called by TransactionTemplate.Execute within a
        /// transaction context.
        /// </summary>
        /// <param name="status">The associated transaction status.</param>
        /// <returns>a result object or <c>null</c></returns>
        public object DoInTransaction(ITransactionStatus status)
        {
            DoInTransactionWithoutResult(status);
            return null;
        }



	    #endregion
    }
}
