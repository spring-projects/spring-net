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
using System.Collections;
using System.Data;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Data.Common;
using Spring.Data.Core;

#endregion

namespace Spring.Data
{
	/// <summary>
	/// Integration tests for StoredProcedure support.
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	[TestFixture]
	public class StoredProcedureTests 
	{
	    
        #region Fields
        
        IAdoOperations adoOperations;
        IDbProvider dbProvider;
	    
        #endregion
	    
        [SetUp]
        public void CreateAdoTemplate()
        {
            IApplicationContext ctx =
                new XmlApplicationContext("assembly://Spring.Data.Integration.Tests/Spring.Data/adoTemplateTests.xml");
            Assert.IsNotNull(ctx);
            dbProvider = ctx["DbProvider"] as IDbProvider;
            Assert.IsNotNull(dbProvider);
            adoOperations = new AdoTemplate(dbProvider);
            
        }
	    
	    [Test]
	    public void ExecuteNonQueryTest()
	    {
	        CallCreateTestObject createTestObjectProcedure =
	            new CallCreateTestObject(dbProvider);
	        
	        createTestObjectProcedure.Create("Gabriel", 1);
	    }

        [Test]
        public void OutParamTest()
        {
            TestObjectStoredProcedure sproc = new TestObjectStoredProcedure(dbProvider);
            IDbParameters parameters = sproc.DeclaredParameters;
            Console.WriteLine("\n\n\n");
            for (int i = 0; i < parameters.Count; i++)
            {
                Console.WriteLine("- Declared Parameter " + parameters[i].ParameterName);
            }
            IDictionary results = sproc.GetResults("George");
            foreach (DictionaryEntry entry in results)
            {
                Console.WriteLine(entry.Key + 
                    ", " + entry.Value);
            }

            IDictionary inParams = new Hashtable();
            inParams.Add("@Name", "George");
            results = sproc.GetResultsUsingInDictionary(inParams);
            Console.WriteLine("\n\n\n");
            foreach (DictionaryEntry entry in results)
            {
                Console.WriteLine(entry.Key +
                    ", " + entry.Value);
            }

        }

        [Test]
        public void OutReturnValueTestWithAdoTemplate()
        {
            IDbParameters parameters = adoOperations.CreateDbParameters();
            parameters.AddOut("Count", DbType.Int32);
            parameters.AddReturn("RETURN_VALUE", DbType.Int32);
            parameters.AddWithValue("Name", "George");
            IList queryResults = adoOperations.QueryWithRowMapper(CommandType.StoredProcedure, "SelectByNameWithReturnAndOutValue",
                                                                  new SimpleRowMapper(),
                                                                  parameters);
            for (int i = 0; i < parameters.Count;i++ )
            {
                Console.WriteLine("parameter " + i 
                    + " name = " + parameters[i].ParameterName 
                    + " value = " + parameters[i].Value);
            }
            //    Console.WriteLine("out parameter 'count' = " + parameters["count"]);
            //Console.WriteLine("RETURN_VALE           = " + parameters["RETURN_VALUE"]);

        }
	}

    internal class SimpleRowMapper : IRowMapper
    {
        public object MapRow(IDataReader reader, int rowNum)
        {
            return "hello";
        }
    }
}
