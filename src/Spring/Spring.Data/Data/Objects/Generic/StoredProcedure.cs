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

using System.Collections;
using System.Collections.Generic;
using System.Data;
using Spring.Dao;
using Spring.Data.Common;
using Spring.Data.Generic;
using Spring.Data.Support;

namespace Spring.Data.Objects.Generic
{
	/// <summary>
	/// A superclass for object based abstractions of RDBMS stored procedures. 
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	public abstract class StoredProcedure : AdoOperation
	{
	    //A collection of NamedResultSetProcessor
	     
        private List<object> resultProcessors = new List<object>();
	    private bool usingDerivedParameters = false;

	    /// <summary>
		/// Initializes a new instance of the <see cref="StoredProcedure"/> class.
        /// </summary>
		public StoredProcedure()
		{
            CommandType = CommandType.StoredProcedure;
		}
	    
        public StoredProcedure(IDbProvider dbProvider, string procedureName) : base(dbProvider, procedureName)
        {
    	    CommandType = CommandType.StoredProcedure;
        }

	    public void DeriveParameters()
	    {
	        DeriveParameters(false);
	    }
	    
	    
        public void DeriveParameters(bool includeReturnParameter)
        {
            //TODO does this account for offsets?
            IDataParameter[] derivedParameters = AdoTemplate.DeriveParameters(Sql, includeReturnParameter);
            for (int i = 0; i < derivedParameters.Length; i++)
            {
                IDataParameter parameter = derivedParameters[i];
                DeclaredParameters.AddParameter(parameter);
            } 
            usingDerivedParameters = true;
        }

	    public void AddResultSetExtractor(string name, IResultSetExtractor resultSetExtractor)
        {
            if (Compiled)
            {
                throw new InvalidDataAccessApiUsageException("Cannot add ResultSetExtractors once operation is compiled");
            }
            resultProcessors.Add(new NamedResultSetProcessor(name, resultSetExtractor));
        }

        public void AddRowCallback(string name, IRowCallback rowCallback)
        {
            if (Compiled)
            {
                throw new InvalidDataAccessApiUsageException("Cannot add RowCallbacks once operation is compiled");
            }
            resultProcessors.Add(new NamedResultSetProcessor(name, rowCallback));
        }

        public void AddRowMapper(string name, IRowMapper rowMapper)
        {
            if (Compiled)
            {
                throw new InvalidDataAccessApiUsageException("Cannot add RowMappers once operation is compiled");
            }
            resultProcessors.Add(new NamedResultSetProcessor(name, rowMapper));
        }

	    public void AddResultSetExtractor<T>(string name, IResultSetExtractor<T> resultSetExtractor)
        {
            if (Compiled)
            {
                throw new InvalidDataAccessApiUsageException("Cannot add ResultSetExtractors once operation is compiled");
            }
            resultProcessors.Add(new NamedResultSetProcessor<T>(name, resultSetExtractor));
        }
        

        public void AddRowMapper<T>(string name, IRowMapper<T> rowMapper)
        {
            if (Compiled)
            {
                throw new InvalidDataAccessApiUsageException("Cannot add RowMappers once operation is compiled");
            }
            resultProcessors.Add(new NamedResultSetProcessor<T>(name,rowMapper));
        }

	    protected virtual IDictionary ExecuteScalar(params object[] inParameterValues)
	    {
            ValidateParameters(inParameterValues);
            return AdoTemplate.ExecuteScalar(NewCommandCreatorWithParamValues(inParameterValues));		        
	    }
	    
        protected virtual IDictionary ExecuteNonQuery(params object[] inParameterValues)
        {
            ValidateParameters(inParameterValues);
            return AdoTemplate.ExecuteNonQuery(NewCommandCreatorWithParamValues(inParameterValues));
        }


        public IList<T> QueryWithRowMapper<T>(params object[] inParameterValues)
        {
            ValidateParameters(inParameterValues);
            if (resultProcessors.Count == 0)
            {
                throw new InvalidDataAccessApiUsageException("No row mapper is specified.");
            }
            
            NamedResultSetProcessor<T> resultSetProcessor = resultProcessors[0] as NamedResultSetProcessor<T>;
            if (resultSetProcessor == null)
            {
                throw new InvalidDataAccessApiUsageException("No row mapper is specified.");
            }

            if (resultSetProcessor.RowMapper == null)
            {
                throw new InvalidDataAccessApiUsageException("No row mapper is specified as first result set processor.");
            }
            IDictionary outParams = Query<T>(inParameterValues);
            return outParams[resultSetProcessor.Name] as IList<T>;
            
        }

        protected virtual IDictionary Query<T>(params object[] inParameterValues)
        {
            ValidateParameters(inParameterValues);
            return AdoTemplate.QueryWithCommandCreator<T>(NewCommandCreatorWithParamValues(inParameterValues), resultProcessors);

        }

        protected virtual IDictionary Query<T,U>(params object[] inParameterValues)
        {
            ValidateParameters(inParameterValues);
            return AdoTemplate.QueryWithCommandCreator<T,U>(NewCommandCreatorWithParamValues(inParameterValues), resultProcessors);

        }

	    /// <summary>
	    /// Execute the stored procedure using 'ExecuteScalar'
	    /// </summary>
	    /// <param name="inParams">Value of input parameters.</param>
        /// <returns>Dictionary with any named output parameters and the value of the
        /// scalar under the key "scalar".</returns>
	    protected virtual IDictionary ExecuteScalarByNamedParam(IDictionary inParams)
	    {
            ValidateNamedParameters(inParams);                        
            return AdoTemplate.ExecuteScalar(NewCommandCreator(inParams));	        
	    }
	    
        protected virtual IDictionary ExecuteNonQueryByNamedParam(IDictionary inParams)
        {
            ValidateNamedParameters(inParams);                        
            return AdoTemplate.ExecuteNonQuery(NewCommandCreator(inParams));
        }	    
	       
	    
        protected virtual IDictionary QueryByNamedParam<T>(IDictionary inParams)
        {
            ValidateNamedParameters(inParams);                        
            return AdoTemplate.QueryWithCommandCreator<T>(NewCommandCreator(inParams), resultProcessors);
        }

        protected virtual IDictionary QueryByNamedParam<T,U>(IDictionary inParams)
        {
            ValidateNamedParameters(inParams);
            return AdoTemplate.QueryWithCommandCreator<T,U>(NewCommandCreator(inParams), resultProcessors);
        }

	    protected override bool IsInputParameter(IDataParameter parameter)
        {
            if (usingDerivedParameters)
            {
                //Can only count Input, derived output parameters are incorrectly classified as input-output
                return (parameter.Direction == ParameterDirection.Input);
            }     
            else
            {
                return base.IsInputParameter(parameter);
            }
        }
	}
}
