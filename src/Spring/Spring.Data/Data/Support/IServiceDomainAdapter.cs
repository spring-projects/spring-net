#region License

/*
 * Copyright © 2002-2007 the original author or authors.
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

#if (!NET_1_0)

using System.EnterpriseServices;

namespace Spring.Data.Support
{
    /// <summary>
    /// Provides the necessary transactional state and operations for ServiceDomainTransactionManager to
    /// work with ServiceDomain and ContextUtil.
    /// </summary>
    /// <remarks>Introduced for purposes of unit testing.</remarks>
    /// <author>Mark Pollack (.NET)</author>
    public interface IServiceDomainAdapter
    {
        /// <summary>
        /// Creates the context specified by the ServiceConfig object and pushes it onto the context stack to 
        /// become the current context. 
        /// </summary>
        /// <param name="config">A ServiceConfig that contains the configuration information for the services to be used within the enclosed code.</param>
        void Enter(SimpleServiceConfig config);

        /// <summary>
        /// Leaves this ServiceDomain. The current context is then popped from the context stack, and the 
        /// context that was running when Enter was called becomes the current context. 
        /// </summary>
        /// <returns>One of the TransactionStatus values</returns>
        TransactionStatus Leave();

        /// <summary>
        /// Sets the consistent bit and the done bit to true in the COM+ context
        /// </summary>
        void SetComplete();

        /// <summary>
        /// Sets the consistent bit to false and the done bit to true in the COM+ context. 
        /// </summary>
        void SetAbort();

        /// <summary>
        /// Gets or sets the consistent bit in the COM+ context.
        /// </summary>
        /// <value>My transaction vote.</value>
        TransactionVote MyTransactionVote { get; set;}

        /// <summary>
        ///  Gets a value indicating whether the current context is transactional.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is existing transaction; otherwise, <c>false</c>.
        /// </value>
        bool IsInTransaction { get; }

    }
}

#endif