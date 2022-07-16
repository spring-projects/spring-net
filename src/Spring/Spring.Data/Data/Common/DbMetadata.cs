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

using System.Reflection;
using Spring.Util;

namespace Spring.Data.Common
{
    /// <summary>
    /// Provides database metdata information.
    /// </summary>
    /// <author>Mark Pollack (.NET)</author>
    public class DbMetadata : IDbMetadata
    {
        #region Fields

        private bool supportsDeriveParametersMethod = true;

        private string productName;
        private string assemblyName;
        private Type connectionType;
        private Type commandType;
        private Type parameterType;
        private Type dataAdapterType;


        private Type parameterDbType;
        private PropertyInfo parameterDbTypeProperty;
        private PropertyInfo parameterIsNullableProperty;
        private string parameterNamePrefix;

        private Type exceptionType;
        private bool useParameterNamePrefixInParameterCollection;
        private bool useParameterPrefixInSql;
        private bool bindByName;




        private Type commandBuilderType;
        private MethodInfo commandBuilderDeriveParametersMethod;

        private string errorCodeExceptionExpression = string.Empty;

        private ErrorCodes errorCodes;

        #endregion

        #region Constructor (s)

        /// <summary>
        /// Initializes a new instance of the <see cref="DbMetadata"/> class.
        /// </summary>
        /// <param name="productName">Name of the product.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="connectionType">Type of the connection.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameterType">Type of the parameter.</param>
        /// <param name="dataAdapterType">Type of the data adapter.</param>
        /// <param name="commandBuilderType">Type of the command builder.</param>
        /// <param name="commandBuilderDeriveParametersMethod">The command builder derive parameters method.</param>
        /// <param name="parameterDbType">Type of the parameter db.</param>
        /// <param name="parameterDbTypeProperty">The parameter db type property.</param>
        /// <param name="parameterIsNullableProperty">The parameter is nullable property.</param>
        /// <param name="parameterNamePrefix">The parameter name prefix.</param>
        /// <param name="exceptionType">Type of the exception.</param>
        /// <param name="useParameterNamePrefixInParameterCollection">if set to <c>true</c> [use parameter name prefix in parameter collection].</param>
        /// <param name="useParameterPrefixInSql">if set to <c>true</c> [use parameter prefix in SQL].</param>
        /// <param name="bindByName">if set to <c>true</c> [bind by name].</param>
        /// <param name="errorCodeExceptionExpression">The error code exception expression.</param>
        public DbMetadata(string productName,
                          string assemblyName,
                          Type connectionType,
                          Type commandType,
                          Type parameterType,
                          Type dataAdapterType,
                          Type commandBuilderType,
                          string commandBuilderDeriveParametersMethod,
                          Type parameterDbType,
                          string parameterDbTypeProperty,
                          string parameterIsNullableProperty,
                          string parameterNamePrefix,
                          Type exceptionType,
                          bool useParameterNamePrefixInParameterCollection,
                          bool useParameterPrefixInSql,
                          bool bindByName,
                          string errorCodeExceptionExpression

            )
        {

            AssertUtils.ArgumentHasText(productName, "ProductName");
            this.productName = productName;
            AssertUtils.ArgumentHasText(assemblyName, "assemblyName", GetErrorMessage() );
            AssertUtils.ArgumentNotNull(connectionType, "connectionType", GetErrorMessage() );
            AssertUtils.ArgumentNotNull(commandType, "commandType", GetErrorMessage());
            AssertUtils.ArgumentNotNull(parameterType, "parameterType", GetErrorMessage());
            AssertUtils.ArgumentNotNull(dataAdapterType, "dataAdapterType", GetErrorMessage());
            AssertUtils.ArgumentNotNull(commandBuilderType, "commandBuilderType", GetErrorMessage());
            AssertUtils.ArgumentHasText(commandBuilderDeriveParametersMethod, "commandBuilderDeriveParametersMethod", GetErrorMessage());
            AssertUtils.ArgumentNotNull(parameterDbType, "parameterDbType", GetErrorMessage());
            AssertUtils.ArgumentHasText(parameterDbTypeProperty, "parameterDbTypeProperty", GetErrorMessage());
            AssertUtils.ArgumentHasText(parameterIsNullableProperty, "parameterIsNullableProperty", GetErrorMessage());
            AssertUtils.ArgumentHasText(parameterNamePrefix, "parameterNamePrefix", GetErrorMessage());
            AssertUtils.ArgumentNotNull(exceptionType, "exceptionType", GetErrorMessage());

            this.assemblyName = assemblyName;

            this.connectionType = connectionType;
            this.commandType = commandType;
            this.parameterType = parameterType;
            this.dataAdapterType = dataAdapterType;
            this.commandBuilderType = commandBuilderType;

            if (commandBuilderDeriveParametersMethod.ToLower().Trim().Equals("not supported"))
            {
                supportsDeriveParametersMethod = false;
            }

            if (supportsDeriveParametersMethod)
            {
                this.commandBuilderDeriveParametersMethod =
                    ReflectionUtils.GetMethod(this.commandBuilderType, commandBuilderDeriveParametersMethod,
                                              new Type[] {this.commandType});

                AssertUtils.ArgumentNotNull(this.commandBuilderDeriveParametersMethod,
                                            "commandBuilderDeriveParametersMethod", GetErrorMessage() +
                                                                                    ", could not resolve commandBuilderDeriveParametersMethod " +
                                                                                    commandBuilderDeriveParametersMethod +
                                                                                    " to MethodInfo.  Please check dbproviders.xml entry for correct metadata listing.");
            }

            this.commandType = commandType;

            this.parameterDbType = parameterDbType;

            this.parameterDbTypeProperty = this.parameterType.GetProperty(parameterDbTypeProperty,
                                                     BindingFlags.Instance | BindingFlags.Public);
            AssertUtils.ArgumentNotNull(this.parameterDbTypeProperty, "parameterDbTypeProperty", GetErrorMessage() +
                ", could not resolve parameterDbTypeProperty " +
                parameterDbTypeProperty +
                " to PropertyInfo.  Please check dbproviders.xml entry for correct metadata listing.");

            this.parameterIsNullableProperty = this.parameterType.GetProperty(parameterIsNullableProperty,
                                                     BindingFlags.Instance | BindingFlags.Public);

            AssertUtils.ArgumentNotNull(this.parameterIsNullableProperty, "parameterIsNullableProperty", GetErrorMessage() +
                ", could not resolve parameterIsNullableProperty " +
                parameterIsNullableProperty +
                " to PropertyInfo.  Please check dbproviders.xml entry for correct metadata listing.");

            this.parameterNamePrefix = parameterNamePrefix;
            this.exceptionType = exceptionType;

            this.useParameterNamePrefixInParameterCollection = useParameterNamePrefixInParameterCollection;
            this.useParameterPrefixInSql = useParameterPrefixInSql;
            this.bindByName = bindByName;

            this.errorCodeExceptionExpression = errorCodeExceptionExpression;


            errorCodes = new ErrorCodes();


        }

        private string GetErrorMessage()
        {
            return "DbProvider product name = " + productName;
        }


        #endregion

        #region IDbMetadata Members

        /// <summary>
        /// Gets a value indicating whether to use param name prefix in parameter collection.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if should use param name prefix in parameter collection; otherwise, <c>false</c>.
        /// </value>
        public bool UseParamNamePrefixInParameterCollection
        {
            get { return useParameterNamePrefixInParameterCollection; }
        }

        /// <summary>
        /// Gets the name of the assembly.
        /// </summary>
        /// <example>Example: System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</example>
        /// <value>The name of the assembly.</value>
        public string AssemblyName
        {
            get { return assemblyName; }
        }

        /// <summary>
        /// Gets a descriptive name of the product.
        /// </summary>
        /// <example>Example: Microsoft SQL Server, provider V2.0.0.0 in framework .NET V2.0</example>
        /// <value>The name of the product.</value>
        public string ProductName
        {
            get { return productName; }
        }

        /// <summary>
        /// Gets the type of the connection.  The fully qualified type name is given since some providers,
        /// notably for SqlServerCe, do not use the same namespace for all data access types.
        /// </summary>
        /// <example>Example: System.Data.SqlClient.SqlCommand, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</example>
        /// <value>The type of the connection.</value>
        public Type ConnectionType
        {
            get { return connectionType; }
        }

        /// <summary>
        /// Gets the type of the command.
        /// </summary>
        /// <example>Example: System.Data.SqlClient.SqlCommand, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</example>
        /// <value>The type of the command.</value>
        public Type CommandType
        {
            get { return commandType; }
        }

        /// <summary>
        /// Gets the type of the parameter.
        /// </summary>
        /// <example>Example: System.Data.SqlClient.SqlParameter, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</example>
        /// <value>The type of the parameter.</value>
        public Type ParameterType
        {
            get { return parameterType; }
        }

        /// <summary>
        /// Gets the type of the data adapter.
        /// </summary>
        /// <example>Example: System.Data.SqlClient.SqlDataAdapter, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</example>
        /// <value>The type of the data adapter.</value>
        public Type DataAdapterType
        {
            get { return dataAdapterType; }
        }

        /// <summary>
        /// Gets the type of the command builder.
        /// </summary>
        /// <example>Example: System.Data.SqlClient.SqlCommandBuilder, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</example>
        /// <value>The type of the command builder.</value>
        public Type CommandBuilderType
        {
            get { return commandBuilderType; }
        }

        /// <summary>
        /// Gets the command builder derive parameters method.
        /// </summary>
        /// <remarks>If the value 'not supported' is specified then this method will throw an ArgumentException</remarks>
        /// <example>Example: DeriveParameters</example>
        /// <value>The command builder derive parameters method.</value>
        public MethodInfo CommandBuilderDeriveParametersMethod
        {
            get
            {
                if (supportsDeriveParametersMethod)
                {
                    return commandBuilderDeriveParametersMethod;
                } else
                {
                    throw new ArgumentException("This provider does not support the DeriveParameters functionality");
                }
            }
        }

        /// <summary>
        /// Provide the prefix used to indentify named parameters in SQL text.
        /// </summary>
        /// <example>@ for Sql Server</example>
        public string ParameterNamePrefix
        {
            get { return parameterNamePrefix; }
        }

        /// <summary>
        /// Gets the type of the exception.
        /// </summary>
        /// <example>Example: System.Data.SqlClient.SqlException, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</example>
        /// <value>The type of the exception.</value>
        public Type ExceptionType
        {
            get { return exceptionType; }
        }

        /// <summary>
        /// Does the driver require the use of the parameter prefix when
        /// specifying the name of the parameter in the Command's
        /// Parameter collection.
        /// </summary>
        /// <value></value>
        /// <remarks>If true, then commamd.Parameters["@parameterName"] is
        /// used, otherwise, command.Parameters["parameterName"].
        /// </remarks>
        public bool UseParameterNamePrefixInParameterCollection
        {
            get { return useParameterNamePrefixInParameterCollection; }
        }


        /// <summary>
        /// Gets a value indicating whether the Provider requires
        /// the use of a named prefix in the SQL string.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if use parameter prefix in SQL; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// The OLE DB/ODBC .NET Provider does not support named parameters for
        /// passing parameters to an SQL Statement or a stored procedure called
        /// by an IDbCommand when CommandType is set to Text.
        /// </remarks>
        public bool UseParameterPrefixInSql
        {
            get { return useParameterPrefixInSql; }
        }

        /// <summary>
        /// For providers that allow you to choose between binding parameters
        /// to a command by name (true) or by position (false).
        /// </summary>
        /// <value></value>
        public bool BindByName
        {
            get { return bindByName; }
            set { bindByName = value; }
        }

        /// <summary>
        /// Gets the type of the parameter db.
        /// </summary>
        /// <example>Example: System.Data.SqlDbType, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</example>
        /// <value>The type of the parameter db.</value>
        public Type ParameterDbType
        {
            get { return parameterDbType; }
        }

        /// <summary>
        /// Gets the parameter db type property.
        /// </summary>
        /// <example>Example: SqlDbType for SqlServer</example>
        /// <value>The parameter db type property.</value>
        public PropertyInfo ParameterDbTypeProperty
        {
            get { return parameterDbTypeProperty; }
        }

        /// <summary>
        /// Gets the parameter is nullable property.
        /// </summary>
        /// <example>Example: IsNullable for Sql Server</example>
        /// <value>The parameter is nullable property.</value>
        public PropertyInfo ParameterIsNullableProperty
        {
            get { return parameterIsNullableProperty; }
        }

        /// <summary>
        /// Gets or sets the error codes.
        /// </summary>
        /// <remarks>The collection of error codes to map error code integer values to Spring's DAO exception hierarchy</remarks>
        /// <value>The error codes.</value>
        public ErrorCodes ErrorCodes
        {
            get { return errorCodes; }
            set { errorCodes = value;}
        }

        /// <summary>
        /// Gets the error code exception expression.
        /// </summary>
        /// <example>Example Errors[0].Number.ToString() for Sql Server </example>
        /// <value>The error code exception expression.</value>
        public string ErrorCodeExceptionExpression
        {
            get { return errorCodeExceptionExpression; }
        }

        #endregion


    }
}
