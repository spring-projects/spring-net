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

using System.Data;
using Spring.Data.Common;
using Spring.Data.Objects;

#endregion

namespace Spring.Data
{
	/// <summary>
	/// Simple insert non query object with 'in' args to create a testobject.
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	public class CreateTestObjectNonQuery : AdoNonQuery
	{
        private static string sql = "insert into TestObjects(Age,Name) values (@Age,@Name)";
	    
        public CreateTestObjectNonQuery(IDbProvider dbProvider) : base(dbProvider, sql)
		{
            DeclaredParameters.Add("Age", DbType.Int32);
            DeclaredParameters.Add("Name", SqlDbType.NVarChar, 16);
		    Compile();
		}

        public void Create(string name, int age)
        {
            ExecuteNonQuery(name, age);
        }

	}
}
