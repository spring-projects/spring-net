#region Licence

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

using Spring.Data.Common;
using Spring.Data.Objects;

#endregion

namespace Spring.Data
{
	/// <summary>
	/// Simple stored procedure with only 'in' args to create a testobject record 
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	public class CallCreateTestObject : StoredProcedure
	{
        private static string procedureName = "CreateTestObject";
	    
        public CallCreateTestObject(IDbProvider dbProvider) : base(dbProvider, procedureName)
		{
            DeriveParameters();
		    Compile();
		}

        public void Create(string name, int age)
        {
            //if know the ordering of input parameters for the SP
            ExecuteNonQuery(name, age);
            
            //if want to use named params
            /*
            IDictionary inParams = new Hashtable();
            inParams["@name"] = name;
            inParams["@age"] = age;
            IDictionary outParams = ExecuteNonQuery(inParams);
            */
        }

	}
}
