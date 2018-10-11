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
using Spring.Data.Support;

namespace Spring.Data.Objects
{
	/// <summary>
	/// A superclass for object based abstractions of RDBMS stored procedures. 
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	public abstract class StoredProcedure : AdoOperation
	{
		//A collection of NamedResultSetProcessor
        private readonly List<NamedResultSetProcessor> resultProcessors = new List<NamedResultSetProcessor>();
	    private bool usingDerivedParameters = false;

		/// <summary>
		/// Initializes a new instance of the <see cref="StoredProcedure"/> class.
        /// </summary>
		public StoredProcedure()
		{
            CommandType = CommandType.StoredProcedure;
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="StoredProcedure"/> class.
        /// </summary>
        /// <param name="dbProvider">The db provider.</param>
        /// <param name="procedureName">Name of the stored procedure.</param>
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
            resultProcessors.Add(new NamedResultSetProcessor(name,rowCallback));
        }
        
        public void AddRowMapper(string name, IRowMapper rowMapper)
        {
            if (Compiled)
            {
                throw new InvalidDataAccessApiUsageException("Cannot add RowMappers once operation is compiled");
            }
            resultProcessors.Add(new NamedResultSetProcessor(name,rowMapper));
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

        protected virtual IDictionary Query(params object[] inParameterValues)
        {
            ValidateParameters(inParameterValues);
            return AdoTemplate.QueryWithCommandCreator(NewCommandCreatorWithParamValues(inParameterValues), resultProcessors);		        
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
	       
	    
        protected virtual IDictionary QueryByNamedParam(IDictionary inParams)
        {
            ValidateNamedParameters(inParams);                        
            return AdoTemplate.QueryWithCommandCreator(NewCommandCreator(inParams), resultProcessors);
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
