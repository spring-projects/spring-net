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

namespace Spring.Data.Common
{
    /// <summary>
    /// Provides minimal database metadata information to support the
    /// functionality in Spring.NET ADO.NET Framework.
    /// </summary>
    ///
    /// <author>Mark Pollack (.NET)</author>
    public interface IDbMetadata
    {
        /// <summary>
        /// Gets a descriptive name of the product.
        /// </summary>
        /// <example>Example: Microsoft SQL Server, provider V2.0.0.0 in framework .NET V2.0</example>
        /// <value>The name of the product.</value>
        string ProductName { get; }

        /// <summary>
        /// Gets the type of the connection.  The fully qualified type name is given since some providers,
        /// notably for SqlServerCe, do not use the same namespace for all data access types.
        /// </summary>
        /// <example>Example: System.Data.SqlClient.SqlCommand, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</example>
        /// <value>The type of the connection.</value>
        Type ConnectionType { get; }

        /// <summary>
        /// Gets the type of the command.
        /// </summary>
        /// <example>Example: System.Data.SqlClient.SqlCommand, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</example>
        /// <value>The type of the command.</value>
        Type CommandType { get; }

        /// <summary>
        /// Gets the type of the parameter.
        /// </summary>
        /// <example>Example: System.Data.SqlClient.SqlParameter, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</example>
        /// <value>The type of the parameter.</value>
        Type ParameterType { get; }

        /// <summary>
        /// Gets the type of the data adapter.
        /// </summary>
        /// <example>Example: System.Data.SqlClient.SqlDataAdapter, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</example>
        /// <value>The type of the data adapter.</value>
        Type DataAdapterType { get; }

        /// <summary>
        /// Gets the type of the command builder.
        /// </summary>
        /// <example>Example: System.Data.SqlClient.SqlCommandBuilder, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</example>
        /// <value>The type of the command builder.</value>
        Type CommandBuilderType { get; }

        /// <summary>
        /// Gets the type of the exception.
        /// </summary>
        /// <example>Example: System.Data.SqlClient.SqlException, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</example>
        /// <value>The type of the exception.</value>
        Type ExceptionType { get; }

        /// <summary>
        /// Gets the error code exception expression.
        /// </summary>
        /// <example>Example Errors[0].Number.ToString() for Sql Server </example>
        /// <value>The error code exception expression.</value>
        string ErrorCodeExceptionExpression { get; }

        /// <summary>
        /// Gets the command builder derive parameters method.
        /// </summary>
        /// <remarks>If the value 'not supported' is specified then this method will throw an ArgumentException</remarks>
        /// <example>Example: DeriveParameters</example>
        /// <value>The command builder derive parameters method.</value>
        MethodInfo CommandBuilderDeriveParametersMethod { get; }

        /// <summary>
        /// Provide the prefix used to indentify named parameters in SQL text.
        /// </summary>
        /// <example>@ for Sql Server</example>
        string ParameterNamePrefix { get; }

        /// <summary>
        /// Does the driver require the use of the parameter prefix when
        /// specifying the name of the parameter in the Command's
        /// Parameter collection.
        /// </summary>
        /// <remarks>If true, then commamd.Parameters["@parameterName"] is
        /// used, otherwise, command.Parameters["parameterName"].
        /// </remarks>
        bool UseParameterNamePrefixInParameterCollection { get; }


        /// <summary>
        /// Gets a value indicating whether the Provider requires
        /// the use of a named prefix in the SQL string.
        /// </summary>
        /// <remarks>
        /// The OLE DB/ODBC .NET Provider does not support named parameters for
        /// passing parameters to an SQL Statement or a stored procedure called
        /// by an IDbCommand when CommandType is set to Text.
        /// </remarks>
        /// <value>
        /// 	<c>true</c> if use parameter prefix in SQL; otherwise, <c>false</c>.
        /// </value>
        bool UseParameterPrefixInSql { get; }

        /// <summary>
        /// For providers that allow you to choose between binding parameters
        /// to a command by name (true) or by position (false).
        /// </summary>
        bool BindByName {
            get;
            //TODO will go away when make all of this fully configuration driven.
            set;
        }

        /// <summary>
        /// Gets the type of the parameter db.
        /// </summary>
        /// <example>Example: System.Data.SqlDbType, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</example>
        /// <value>The type of the parameter db.</value>
        Type ParameterDbType { get; }

        /// <summary>
        /// Gets the parameter db type property.
        /// </summary>
        /// <example>Example: SqlDbType for SqlServer</example>
        /// <value>The parameter db type property.</value>
        PropertyInfo ParameterDbTypeProperty { get; }

        /// <summary>
        /// Gets the parameter is nullable property.
        /// </summary>
        /// <example>Example: IsNullable for Sql Server</example>
        /// <value>The parameter is nullable property.</value>
        PropertyInfo ParameterIsNullableProperty { get; }

        /// <summary>
        /// Gets or sets the error codes.
        /// </summary>
        /// <remarks>The collection of error codes to map error code integer values to Spring's DAO exception hierarchy</remarks>
        /// <value>The error codes.</value>
        ErrorCodes ErrorCodes { get; }
    }
}
