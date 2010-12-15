#region License

/*
 * Copyright © 2002-2010 the original author or authors.
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
using Common.Logging;
using Spring.Objects;
using Spring.Transaction;
using Spring.Transaction.Interceptor;
using Spring.Transaction.Support;
using IsolationLevel=System.Data.IsolationLevel;

#endregion

namespace Spring.Data
{
	/// <summary>
	/// Group together multiple ITestObjectDao operations.
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	public class TestObjectMgr : ITestObjectMgr
	{
		#region Fields

        private static readonly ILog LOG = LogManager.GetLogger(typeof(TestObjectMgr));
		#endregion

		#region Constructor (s)
		/// <summary>
		/// Initializes a new instance of the <see cref="TestObjectMgr"/> class.
        /// </summary>
		public 	TestObjectMgr()
		{

		}

		#endregion



    	#region Methods

        [Transaction()]
        public void SaveTwoTestObjects(TestObject to1, TestObject to2)
        {
            LOG.Debug("TransactionActive = " + TransactionSynchronizationManager.ActualTransactionActive);
        }
#if !NET_1_0 && !NET_1_1
        [Transaction(TransactionPropagation.Required, IsolationLevel.Unspecified, Timeout = 50,
           RollbackFor = new Type[]{typeof(ArgumentNullException)},
           ReadOnly = false,
           EnterpriseServicesInteropOption = System.Transactions.EnterpriseServicesInteropOption.Automatic,
           NoRollbackFor = new Type[] { typeof(ArithmeticException), typeof(NotSupportedException) })]
#else
        [Transaction(TransactionPropagation.Required, IsolationLevel.Unspecified, Timeout = 50,
           RollbackFor = new Type[]{typeof(ArgumentNullException)},
           ReadOnly = false,
           NoRollbackFor = new Type[] { typeof(ArithmeticException), typeof(NotSupportedException) })]
#endif
	    public void DeleteTwoTestObjects(string name1, string name2)
        {
        }

		#endregion
	}
}
