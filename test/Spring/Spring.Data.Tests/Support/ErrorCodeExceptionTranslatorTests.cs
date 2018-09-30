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

using System;
using NUnit.Framework;
using Spring.Dao;
using Spring.Data;
using Spring.Data.Common;
using Spring.Data.Support;

namespace Spring.Support
{
	/// <summary>
	/// Test the database exception translation.
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	[TestFixture]
	public class ErrorCodeExceptionTranslatorTests 
	{
		private static ErrorCodes ERROR_CODES = new ErrorCodes();
	    
	    private IDbProvider dbProvider = new TestDbProvider();

		/// <summary>
		/// Initializes a new instance of the <see cref="ErrorCodeExceptionTranslatorTests"/> class.
        /// </summary>
		public 	ErrorCodeExceptionTranslatorTests()
		{

		}

		[SetUp]
        public void SetUp()
        {
            ERROR_CODES.BadSqlGrammarCodes = (new String[] { "1", "2" });
            ERROR_CODES.InvalidResultSetAccessCodes = (new String[] { "3", "4" });
            ERROR_CODES.DataAccessResourceFailureCodes = (new String[] { "5" });
            ERROR_CODES.DataIntegrityViolationCodes = (new String[] { "544", "2627", "8114", "8115" });
            ERROR_CODES.CannotAcquireLockCodes = (new String[] { "7" });
            ERROR_CODES.DeadlockLoserCodes = (new String[] { "8" });
            ERROR_CODES.CannotSerializeTransactionCodes = (new String[] { "9" });
        }
	    
        [Test]
	    public void ErrorCodeTransation()
	    {
	        IAdoExceptionTranslator exceptionTranslator = new ErrorCodeExceptionTranslator(dbProvider, ERROR_CODES);
	        TestSqlException badSqlEx = new TestSqlException("", "1");
	        
	        BadSqlGrammarException bsgex = (BadSqlGrammarException)exceptionTranslator.Translate("task","SQL", badSqlEx);
            Assert.AreEqual("SQL", bsgex.Sql);
            Assert.AreEqual(badSqlEx, bsgex.InnerException);

            TestSqlException invResEx = new TestSqlException("", "4");
            InvalidResultSetAccessException irsex = (InvalidResultSetAccessException) exceptionTranslator.Translate("task", "SQL", invResEx);
            Assert.AreEqual("SQL", irsex.Sql);
            Assert.AreEqual(invResEx, irsex.InnerException);
	        		
	        CheckTranslation(exceptionTranslator, "5", typeof(DataAccessResourceFailureException));
            CheckTranslation(exceptionTranslator, "544", typeof(DataIntegrityViolationException));
		    CheckTranslation(exceptionTranslator, "7", typeof(CannotAcquireLockException));
		    CheckTranslation(exceptionTranslator, "8", typeof(DeadlockLoserDataAccessException));
		    CheckTranslation(exceptionTranslator, "9", typeof(CannotSerializeTransactionException));
	    }

	    private void CheckTranslation(IAdoExceptionTranslator translator, string errorCode, Type exType)
	    {
            TestSqlException sex = new TestSqlException("", errorCode);
            DataAccessException ex = translator.Translate("", "", sex);
            Assert.IsTrue(exType.IsAssignableFrom(ex.GetType()));
            Assert.IsTrue(ex.InnerException == sex);
	    }
	}
}
