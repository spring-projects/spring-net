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

using System;
using System.Data;
using Spring.Data.Common;

#endregion

namespace Spring.Support
{
	/// <summary>
	/// A fake provider class used to test exception translation functionality
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	public class TestDbProvider : IDbProvider
	{
        private string connectionString;
	    private IDbConnection connection;

        protected IDbMetadata dbMetadata;
	    
	    public TestDbProvider()
	    {
            IDbProvider provider = DbProviderFactory.GetDbProvider("System.Data.SqlClient");
            dbMetadata = provider.DbMetadata;
	    }

        /// <summary>
        /// Return metadata information about the database.
        /// </summary>
        public IDbMetadata DbMetadata
        {
            get
            {
                return dbMetadata;
            }
        }
        /// <summary>
        /// Connection string used to create connections.
        /// </summary>
        public string ConnectionString
        {
            set { connectionString = value; }
            get { return connectionString; }
        }
	    public IDbCommand CreateCommand()
	    {
	        throw new NotImplementedException();
	    }

	    public IDbConnection CreateConnection()
	    {
	        return connection;
	    }

	    public IDbDataParameter CreateParameter()
	    {
	        throw new NotImplementedException();
	    }


	    public IDbDataAdapter CreateDataAdapter()
	    {
	        throw new NotImplementedException();
	    }

	    public object CreateCommandBuilder()
	    {
	        throw new NotImplementedException();
	    }

	    public string ExtractError(Exception e)
	    {
	        TestSqlException testSqlException = (TestSqlException)e;
	        return testSqlException.ErrorNumber;
	    }

        public bool IsDataAccessException(Exception e)
        {
            if (e is System.Data.Common.DbException)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

	    public bool IsDataAccessExceptionBCL11(Exception e)
	    {
	        return true;
	    }

        public string CreateParameterName(string name)
        {
            if (dbMetadata.BindByName)
            {
                if (DbMetadata.UseParameterPrefixInSql)
                {
                    return DbMetadata.ParameterNamePrefix + name;
                }
                else
                {
                    return name;
                }
            }
            else
            {
                return DbMetadata.ParameterNamePrefix;
            }
        }

	    public string CreateParameterNameForCollection(string name)
	    {
            if (dbMetadata.BindByName)
            {
                if (DbMetadata.UseParameterNamePrefixInParameterCollection)
                {
                    return DbMetadata.ParameterNamePrefix + name;
                }
                else
                {
                    return name;
                }
            }
            else
            {
                return DbMetadata.ParameterNamePrefix;
            }
	    }

	    #region Stub method to set behavior

	    public IDbConnection ConnectionToCreate
	    {
	        set { connection = value;}
        }
        #endregion
    }
}
