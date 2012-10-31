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

using System;
using System.Data;
using Spring.Data.Common;
using Spring.Data.Objects;
using Spring.Objects;

#endregion

namespace Spring.Data
{
	/// <summary>
	/// Simple MappingAdoQuery implementation 
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
    public class TestObjectQuery : MappingAdoQuery
    {
        private static string sql = "select TestObjectNo, Age, Name from TestObjects where Name = @Name";
            
        public TestObjectQuery(IDbProvider dbProvider) 
            : base(dbProvider, sql)
        {
            //DeclaredParameters = new DbParameters(dbProvider);
            try
            {
                DeclaredParameters.Add("Name", SqlDbType.VarChar, 50);
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            CommandType = CommandType.Text;
            Compile();
        }
       
	    protected override object MapRow(IDataReader reader, int num)
	    {
            TestObject to = new TestObject();
            to.ObjectNumber = reader.GetInt32(0);
            to.Age = reader.GetInt32(1);
            to.Name = reader.GetString(2);
	        return to;
	    }
    }
}
