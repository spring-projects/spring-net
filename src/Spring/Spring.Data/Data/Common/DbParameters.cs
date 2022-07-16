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

using System.Data;
using System.Reflection;
using Spring.Dao;

namespace Spring.Data.Common
{
    /// <summary>
    /// A more portable means to create a collection of ADO.NET
    /// parameters.
    /// </summary>
    /// <author>Mark Pollack (.NET)</author>
    public class DbParameters : IDbParameters
    {

        #region Fields

        private IDbProvider dbProvider;

        //Just used as a container for the underlying parameter collection.
        private IDbCommand dbCommand;
        private IDataParameterCollection dataParameterCollection;

        #endregion

        #region Constructor (s)

        /// <summary>
        /// Initializes a new instance of the <see cref="DbParameters"/> class.
        /// </summary>
        public DbParameters(IDbProvider dbProvider)
        {
            this.dbProvider = dbProvider;
            dbCommand = dbProvider.CreateCommand();
            dataParameterCollection = dbCommand.Parameters;
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        #endregion

        public IDataParameter this[string parameterName]
        {
            get { return (IDbDataParameter) dataParameterCollection[parameterName]; }
            set { dataParameterCollection[parameterName] = value; }
        }

        public IDataParameter this[int index]
        {
            get { return (IDbDataParameter) dataParameterCollection[index]; }
            set { dataParameterCollection[index] = value; }
        }

        public IDataParameterCollection DataParameterCollection
        {
            get { return dataParameterCollection; }
        }

        public int Count
        {
            get
            {
                return dataParameterCollection.Count;
            }
        }
        public bool Contains(string parameterName)
        {
            return dataParameterCollection.Contains(parameterName);
        }

        public void AddParameter(IDataParameter dbParameter)
        {
            dataParameterCollection.Add(dbParameter);
        }


        public IDbDataParameter AddParameter(string name,
                                 Enum parameterType,
                                 int size,
                                 ParameterDirection direction,
                                 bool isNullable,
                                 byte precision,
                                 byte scale,
                                 string sourceColumn,
                                 DataRowVersion sourceVersion,
                                 object parameterValue)
        {
            IDbDataParameter parameter = dbCommand.CreateParameter();

            parameter.ParameterName = dbProvider.CreateParameterNameForCollection(name);

            AssignParameterType(parameter, parameterType);

            if (size > 0)
            {
                parameter.Size = size;
            }
            parameter.Direction = direction;

            if (isNullable)
            {
                AssignIsNullable(isNullable, parameter);
            }

            parameter.Precision = precision;

            if (scale > 0)
            {
                parameter.Scale = scale;
            }

            if (sourceColumn != null && sourceColumn != string.Empty)
            {
                parameter.SourceColumn = sourceColumn;
            }

            parameter.SourceVersion = sourceVersion;

            parameter.Value = (parameterValue == null) ? DBNull.Value : parameterValue;

            dataParameterCollection.Add(parameter);

            return parameter;
        }

        public IDbDataParameter AddParameter(string name,
                                             Enum parameterType,
                                             ParameterDirection direction,
                                             bool isNullable,
                                             byte precision,
                                             byte scale,
                                             string sourceColumn,
                                             DataRowVersion sourceVersion,
                                             object parameterValue)
        {
            return AddParameter(name, parameterType, 0, direction, isNullable, precision, scale, sourceColumn, sourceVersion,
                         parameterValue);
        }


        public int Add(object parameterValue)
        {
            IDbDataParameter parameter = dbCommand.CreateParameter();
            parameter.Value = (parameterValue == null) ? DBNull.Value : parameterValue;
            dataParameterCollection.Add(parameter);
            return dataParameterCollection.Count - 1;
        }

        public void AddRange(Array values)
        {
            foreach (Array value in values)
            {
                Add(value);
            }
        }

        public IDbDataParameter AddWithValue(string name, object parameterValue)
        {
            return AddParameter(name, null, -1, ParameterDirection.Input, false,
                                0, 0, null, DataRowVersion.Default, parameterValue);
        }

        public IDbDataParameter Add(string name, Enum parameterType)
        {
            return AddParameter(name, parameterType, -1, ParameterDirection.Input,
                false, 0, 0, null, DataRowVersion.Default, null);
        }

        public IDbDataParameter Add(string name, Enum parameterType, int size)
        {
            return AddParameter(name, parameterType, size, ParameterDirection.Input,
                false, 0, 0, null, DataRowVersion.Default, null);
        }

        public IDbDataParameter Add(string name, Enum parameterType, int size, string sourceColumn)
        {
            return AddParameter(name, parameterType, size, ParameterDirection.Input,
                false, 0, 0, sourceColumn, DataRowVersion.Default, null);
        }

        public IDbDataParameter AddOut(string name, Enum parameterType)
        {
            return AddParameter(name, parameterType, -1, ParameterDirection.Output,
                false, 0, 0, null, DataRowVersion.Default, null);
        }

        public IDbDataParameter AddOut(string name, Enum parameterType, int size)
        {
            return AddParameter(name, parameterType, size, ParameterDirection.Output,
                false, 0, 0, null, DataRowVersion.Default, null);
        }

        public IDbDataParameter AddInOut(string name, Enum parameterType)
        {
            return AddParameter(name, parameterType, -1, ParameterDirection.InputOutput,
                false, 0, 0, null, DataRowVersion.Default, null);
        }

        public IDbDataParameter AddInOut(string name, Enum parameterType, int size)
        {
            return AddParameter(name, parameterType, size, ParameterDirection.InputOutput,
                false, 0, 0, null, DataRowVersion.Default, null);
        }

        public IDbDataParameter AddReturn(string name, Enum parameterType)
        {
            return AddParameter(name, parameterType, -1, ParameterDirection.ReturnValue,
                false, 0, 0, null, DataRowVersion.Default, null);
        }

        public IDbDataParameter AddReturn(string name, Enum parameterType, int size)
        {
            return AddParameter(name, parameterType, size, ParameterDirection.ReturnValue,
                false, 0, 0, null, DataRowVersion.Default, null);
        }


        public object GetValue(string name)
        {
            IDataParameter parameter = dataParameterCollection[dbProvider.CreateParameterNameForCollection(name)] as IDataParameter;
            if (parameter != null)
            {
                return parameter.Value;
            }
            throw new InvalidDataAccessApiUsageException(
                "object in IDataParameterCollection is not of the type IDataParameter, it is type [" +
                dataParameterCollection[dbProvider.CreateParameterNameForCollection(name)].GetType() + "].");
        }

        public void SetValue(string name, object parameterValue)
        {
            IDbDataParameter parameter = dataParameterCollection[dbProvider.CreateParameterNameForCollection(name)] as IDbDataParameter;
            if (parameter != null)
            {
                parameter.Value = (parameterValue == null) ? DBNull.Value : parameterValue;
            }
        }


        protected void AssignParameterType(IDbDataParameter parameter, Enum parameterType)
        {
            if (parameterType != null)
            {
                if (parameterType is DbType)
                {
                    parameter.DbType = (DbType) parameterType;
                }
                else
                {
                    if (parameterType.GetType() != dbProvider.DbMetadata.ParameterDbType)
                    {
                        throw new TypeMismatchDataAccessException("Invalid parameter type specified for parameter name ["
                                                                  + parameter.ParameterName + "].  ["
                                                                  + parameterType.GetType().AssemblyQualifiedName + "] is not of expected type ["
                                                                  + dbProvider.DbMetadata.ParameterDbType.AssemblyQualifiedName + "]");
                    }
                    dbProvider.DbMetadata.ParameterDbTypeProperty.SetValue(parameter, parameterType, null);
                }
            }
        }

        protected void AssignIsNullable(bool isNullable, IDbDataParameter parameter)
        {
            PropertyInfo propertyInfo = dbProvider.DbMetadata.ParameterIsNullableProperty;
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(parameter, isNullable, null);
            }
            if (parameter.Value == null)
            {
                parameter.Value = DBNull.Value;
            }
        }

    }
}
