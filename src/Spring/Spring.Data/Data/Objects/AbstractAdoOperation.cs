#region License

/*
 * Copyright 2002-2007 the original author or authors.
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
using Common.Logging;
using Spring.Dao;
using Spring.Data.Common;
using Spring.Objects.Factory;

namespace Spring.Data.Objects
{
    /// <summary>
    /// Abstract base class providing common functionality for generic and non
    /// generic implementations of "AdoOperation" subclasses.
    /// </summary>
    /// <author>Mark Pollack (.NET)</author>
    public abstract class AbstractAdoOperation : IInitializingObject
    {
        #region Logging Definition

        protected readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Fields

        private string sql;

        private CommandType commandType = CommandType.Text;

        private IDbParameters declaredParameters;


        /// <summary>
        /// Has this operation been compiled?  Compilation means at 
        /// least checking that a IDbProvider and sql have been provided,
        /// but subclasses may also implement their own custom validation.
        /// </summary>
        protected bool compiled;

        /// <summary>
        /// Object enabling us to create IDbCommands
        /// efficiently, based on this class's declared parameters.
        /// </summary>
        private IDbCommandCreatorFactory commandFactory;

        #endregion


        #region Properties


        /// <summary>
        /// Gets or sets the type of the command.
        /// </summary>
        /// <value>The type of the command.</value>
        public CommandType CommandType
        {
            get { return commandType; }
            set { commandType = value; }
        }

        /// <summary>
        /// Gets or sets the db provider.
        /// </summary>
        /// <value>The db provider.</value>
        public abstract IDbProvider DbProvider
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the SQL to execute
        /// </summary>
        /// <value>The SQL.</value>
        public string Sql
        {
            get
            {
                return sql;
            }
            set
            {
                sql = value;
            }
        }

        /// <summary>
        /// Gets or sets the declared parameters.
        /// </summary>
        /// <value>The declared parameters.</value>
        public IDbParameters DeclaredParameters
        {
            get
            {
                return declaredParameters;
            }
            set
            {
                if (Compiled)
                {
                    throw new InvalidDataAccessApiUsageException("Cannot add parameters once operation is compiled");
                }
                declaredParameters = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="AdoOperation"/> is compiled.
        /// </summary>
        /// <remarks>Compilation means that the operation is fully configured,
        ///  and ready to use.  The exact meaning of compilation will vary between subclasses.</remarks>
        /// <value><c>true</c> if compiled; otherwise, <c>false</c>.</value>
        public bool Compiled
        {
            get { return compiled; }
        }

        /// <summary>
        /// Sets the command timeout for IDbCommands that this AdoTemplate executes.
        /// </summary>
        /// <remarks>Default is 0, indicating to use the database provider's default.
        /// Any timeout specified here will be overridden by the remaining 
        /// transaction timeout when executing within a transaction that has a
        /// timeout specified at the transaction level. 
        /// </remarks>
        /// <value>The command timeout.</value>
        public abstract int CommandTimeout
        { 
            set;
        }


        #endregion

        #region Methods

        /// <summary>
        /// Ensures compilation if used in an IApplicationContext
        /// </summary>
        public void AfterPropertiesSet()
        {
            Compile();
        }

        /// <summary>
        /// Compiles this operation.  Ignores subsequent attempts to compile.
        /// </summary>
        public abstract void Compile();




        /// <summary>
        /// Check whether this operation has been compiled already;
        /// lazily compile it if not already compiled.
        /// </summary>
        /// <remarks>Automatically called by ValidateParameters and ValidateNamedParameters</remarks>
        protected void CheckCompiled()
        {
            if (!Compiled)
            {
                log.Debug("ADO operation not compiled before execution - invoking compile");
                Compile();
            }
        }


        protected virtual void ValidateParameters(params object[] inParamValues)
        {
            CheckCompiled();
            int declaredInParameters = 0;
            if (DeclaredParameters != null)
            {
                for (int i = 0; i < declaredParameters.Count; i++)
                {
                    IDataParameter declaredParameter = DeclaredParameters[i];
                    if (IsInputParameter(declaredParameter))
                    {
                        declaredInParameters++;
                    }
                }
            }

            if (inParamValues != null)
            {
                if (DeclaredParameters == null)
                {
                    throw new InvalidDataAccessApiUsageException(
                        "Didn't expect any parameters: none were declared");
                }
                if (inParamValues.Length < declaredInParameters)
                {
                    throw new InvalidDataAccessApiUsageException(
                        inParamValues.Length + " parameters were supplied, but " +
                        declaredInParameters + " in parameters were declared in class [" +
                        GetType().AssemblyQualifiedName + "]");
                }
                if (inParamValues.Length > declaredInParameters)
                {
                    throw new InvalidDataAccessApiUsageException(
                        inParamValues.Length + " parameters were supplied, but " +
                        DeclaredParameters.Count + " parameters were declared " +
                        "in class [" + GetType().AssemblyQualifiedName + "]");
                }
            }
            else
            {
                // No parameters were supplied
                if (DeclaredParameters != null && !(DeclaredParameters.Count == 0))
                {
                    throw new InvalidDataAccessApiUsageException(
                        DeclaredParameters.Count + " parameters must be supplied");
                }
            }
        }

        protected virtual bool IsInputParameter(IDataParameter parameter)
        {
            return (parameter.Direction == ParameterDirection.Input
                    || parameter.Direction == ParameterDirection.InputOutput);
        }

        /// <summary>
        /// Validates the named parameters passed to an AdoTemplate ExecuteXXX method based on
        /// declared parameters.
        /// </summary>
        /// <remarks>
        /// Subclasses should invoke this method very every ExecuteXXX method.
        /// </remarks>
        /// <param name="parameters">The parameter dictionary supplied.  May by null.</param>
        protected virtual void ValidateNamedParameters(IDictionary parameters)
        {
            CheckCompiled();
            IDictionary paramsToUse = (parameters != null ? parameters : new Hashtable());

            if (declaredParameters != null)
            {
                for (int i = 0; i < declaredParameters.Count; i++)
                {
                    IDataParameter declaredParameter = DeclaredParameters[i];
                    if (declaredParameter.ParameterName == null)
                    {
                        throw new InvalidDataAccessApiUsageException(
                            "All parameters must have name specified when using the methods " +
                            "dedicated to named parameter support");
                    }
                    if (IsInputParameter(declaredParameter) && !paramsToUse.Contains(declaredParameter.ParameterName))
                    {
                        throw new InvalidDataAccessApiUsageException(
                            "The parameter named '" + declaredParameter +
                            "' was not among the parameters supplied: " + paramsToUse.Keys);
                    }
                }
            }


            if (DeclaredParameters != null && DeclaredParameters.Count > 0)
            {
                if (DeclaredParameters == null)
                {
                    throw new InvalidDataAccessApiUsageException(
                        "Didn't expect any parameters: none were declared");
                }
            }
            else
            {
                // No parameters were supplied
                if (DeclaredParameters != null && !(DeclaredParameters.Count == 0))
                {
                    throw new InvalidDataAccessApiUsageException(
                        "Parameters must be supplied");
                }
            }

        }

        /// <summary>
        /// Subclasses must implement to perform their own compilation.
        /// Invoked after this class's compilation is complete.
        /// Subclasses can assume that SQL has been supplied and that
        /// a IDbProvider has been supplied.
        /// </summary>                                                                                                
        protected virtual void CompileInternal()
        {
            commandFactory = new IDbCommandCreatorFactory(DbProvider, CommandType, Sql, DeclaredParameters);
            OnCompileInternal();


        }
        /// <summary>
        /// Hook method that subclasses may override to react to compilation.
        /// This implementation does nothing.
        /// </summary>
        protected virtual void OnCompileInternal()
        {
        }

        protected virtual IDbCommandCreator NewCommandCreator(IDictionary inParams)
        {
            return commandFactory.NewDbCommandCreator(inParams);
        }

        protected virtual IDbCommandCreator NewCommandCreatorWithParamValues(params object[] inParams)
        {
            return commandFactory.NewDbCommandCreatorWithParamValues(inParams);
        }

        #endregion
    }
}