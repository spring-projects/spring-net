#region License

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

#endregion

using System.Collections;
using System.Data;
using Spring.Dao;
using Spring.Data.Common;

namespace Spring.Data
{
	/// <summary>
    /// Helper class that can efficiently create multiple IDbCommand
    /// objects with different parameters based on a SQL statement and a single
    /// set of parameter declarations.
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	public class IDbCommandCreatorFactory
	{
		#region Fields

	    private String sql;

	    private IDbParameters declaredParameters;

	    private IDbProvider dbProvider;

	    private CommandType commandType;

		#endregion


		#region Constructor (s)
		/// <summary>
		/// Initializes a new instance of the <see cref="IDbCommandCreatorFactory"/> class.
        /// </summary>
	    public IDbCommandCreatorFactory(IDbProvider dbProvider, CommandType commandType, string sql, IDbParameters declaredParameters)
	    {
            this.dbProvider = dbProvider;
            this.sql = sql;
		    this.commandType = commandType;
		    this.declaredParameters = declaredParameters;
	    }

	    #endregion

		#region Properties

	    public IDbParameters DeclaredParameters
	    {
	        get
	        {
	            return declaredParameters;
	        }
	    }

        public string Sql
        {
            get { return sql; }
        }

        public IDbProvider DbProvider
        {
            get { return dbProvider; }
        }

        public CommandType CommandType
        {
            get { return commandType; }
        }
		#endregion

		#region Methods

        public IDbCommandCreator NewDbCommandCreatorWithParamValues(params object[] inParamValues)
        {

            return new CommandCreatorWithParamValues(this, inParamValues != null ? ArrayList.Adapter(inParamValues) : new ArrayList());
        }


	    public IDbCommandCreator NewDbCommandCreator(IDictionary inParameters)
	    {

	        return new CommandCreatorImpl(this, inParameters != null ? inParameters : new Hashtable());
	    }

		#endregion

        private class CommandCreatorWithParamValues : IDbCommandCreator
        {
            private IDbCommandCreatorFactory factory;
            private IList inParamValues;

            public CommandCreatorWithParamValues(IDbCommandCreatorFactory factory, IList inParamValues)
            {
                this.factory = factory;
                this.inParamValues = inParamValues;
            }

            public IDbCommand CreateDbCommand(IDbProvider provider)
            {
                IDbCommand command = factory.dbProvider.CreateCommand();
                command.CommandText = factory.sql;
                command.CommandType = factory.commandType;
                if (factory.declaredParameters != null)
                {
                    int parameterIndex = 0;
                    foreach ( IDbDataParameter declaredParameter in factory.declaredParameters.DataParameterCollection)
                    {
                        IDbDataParameter clonedParameter = (IDbDataParameter)((ICloneable)declaredParameter).Clone();
                        if (!(clonedParameter.Direction == ParameterDirection.Output) &&
                            !(clonedParameter.Direction == ParameterDirection.ReturnValue))
                        {
                            if (clonedParameter.Direction == ParameterDirection.InputOutput)
                            {
                                //heuristic for case when have output parameter from stored procedure discovery classified
                                //as in-out.  Assume out parameters come last in the stored proc definition and ignore
                                //setting values if there are not enough input values as passed in by the user.
                                if (parameterIndex < inParamValues.Count)
                                {
                                    //The value may still be null
                                    Object inValue = inParamValues[parameterIndex];
                                    clonedParameter.Value = (inValue == null) ? DBNull.Value : inValue;
                                } else
                                {
                                    //Since it thinks it is an input-output value and we have no value from the user - pass in null
                                    //as a workaround.
                                    clonedParameter.Value = DBNull.Value;
                                }

                            }
                            else
                            {
                                //The value may still be null
                                Object inValue = inParamValues[parameterIndex];
                                clonedParameter.Value = (inValue == null) ? DBNull.Value : inValue;
                            }
                            // only increment index if we could assign a value from the user specified inputs.
                            parameterIndex++;
                        }

                        command.Parameters.Add(clonedParameter);

                    }
                }
                return command;
            }

        }
	    private class CommandCreatorImpl : IDbCommandCreator{

            private IDbCommandCreatorFactory factory;
	        private IDictionary inParams;


	        public CommandCreatorImpl(IDbCommandCreatorFactory factory, IDictionary inParameters)
	        {
	            this.factory = factory;
	            this.inParams = inParameters;
	        }

	        public IDbCommand CreateDbCommand(IDbProvider provider)
	        {
	            IDbCommand command = factory.dbProvider.CreateCommand();
	            command.CommandText = factory.sql;
	            command.CommandType = factory.commandType;
                if (factory.declaredParameters != null)
                {
                    foreach ( IDbDataParameter declaredParameter in factory.declaredParameters.DataParameterCollection)
                    {
                        IDbDataParameter clonedParameter = (IDbDataParameter)((ICloneable)declaredParameter).Clone();

                        if (IsInputParamter(clonedParameter) && !inParams.Contains(clonedParameter.ParameterName))
                        {
                            throw new InvalidDataAccessApiUsageException("Required input parameter '" +
                                                                         clonedParameter.ParameterName + "' is missing");
                        }
                        //The value may still be null
//                        Object inValue =
//                            inParams[factory.dbProvider.CreateParameterName(clonedParameter.ParameterName)];

                        // TODO: The above code doesn't work as it duplicates parameter prefix, thus failing
                        // to obtain parameter values. The whole parameter handling mechanism should be refactored
                        // not to require parameter prefix to be specified when populating named parameters collection
                        // within AdoOperations. Code below is just a temporary workaround until this issue is resolved.

                        Object inValue = inParams[clonedParameter.ParameterName];
                        if (!(clonedParameter.Direction == ParameterDirection.Output) &&
                            !(clonedParameter.Direction == ParameterDirection.ReturnValue))
                        {
                            clonedParameter.Value = (inValue == null) ? DBNull.Value : inValue;
                        }

                        command.Parameters.Add(clonedParameter);
                    }
                }
	            return command;
	        }

            private bool IsInputParamter(IDataParameter parameter)
            {
                //Due to quirk in DeriveParameters can only verify Input parameter as output parameters
                //are incorrectly listed as InputOutput
                return (parameter.Direction == ParameterDirection.Input);
            }

	    }
	}
}
