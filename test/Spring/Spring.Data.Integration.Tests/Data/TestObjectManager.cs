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

#region Imports

using System.Collections;
using Common.Logging;
using Spring.Objects;
using Spring.Transaction.Interceptor;
using Spring.Transaction.Support;

#endregion

namespace Spring.Data
{
	/// <summary>
	/// Group together multiple ITestObjectDao operations.
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	public class TestObjectManager : ITestObjectManager
	{
		#region Fields
        ITestObjectDao testObjectDao;
        private static readonly ILog LOG = LogManager.GetLogger(typeof(TestObjectManager));
		#endregion

		#region Constructor (s)
		/// <summary>
		/// Initializes a new instance of the <see cref="TestObjectManager"/> class.
        /// </summary>
		public 	TestObjectManager()
		{

		}

		#endregion

		#region Properties

	    public ITestObjectDao TestObjectDao
	    {
	        get { return testObjectDao; }
	        set { testObjectDao = value; }
	    }

	    #endregion

		#region Methods

        [Transaction]
        public void SaveTwoTestObjects(TestObject to1, TestObject to2)
        {
            LOG.Debug("TransactionActive = " + TransactionSynchronizationManager.ActualTransactionActive);
            //Console.WriteLine("TransactionSynchronizationManager.CurrentTransactionIsolationLevel = " +
            //                  TransactionSynchronizationManager.CurrentTransactionIsolationLevel);
            //Console.WriteLine("System.Transactions.Transaction.Current.IsolationLevel = " + System.Transactions.Transaction.Current.IsolationLevel);
            testObjectDao.Create(to1.Name, to1.Age);
            testObjectDao.Create(to2.Name, to2.Age);
        }

        [Transaction()]
	    public void DeleteTwoTestObjects(string name1, string name2)
        {
            testObjectDao.Delete(name1);
            testObjectDao.Delete(name2);
        }

	    public void DeleteAllTestObjects()
	    {
	        IList objects = testObjectDao.FindAll();

	        foreach (object testObject in objects)
	        {
	            if (testObject is ITestObject)
	            {
	                testObjectDao.Delete(((ITestObject)testObject).Name);
	            }
	        }
	    }

	    #endregion
	}


}
