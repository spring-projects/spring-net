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

using Common.Logging;
using Spring.Dao;
using Spring.Data.Common;

namespace Spring.Data.Support
{
	/// <summary>
	/// Implementation of IAdoExceptionTranslator that analyzes provider specific
	/// error codes and translates into the DAO exception hierarchy.
	/// </summary>
	/// <remarks>This class loads the obtains error codes from
	/// the IDbProvider metadata property ErrorCodes
	/// which defines error code mappings for various providers.
	/// </remarks>
	/// <author>Mark Pollack (.NET)</author>
	public class ErrorCodeExceptionTranslator : IAdoExceptionTranslator
	{
		#region Fields

	    private ErrorCodes errorCodes;

        private IAdoExceptionTranslator fallbackTranslator;

	    private IDbProvider dbProvider;

		#endregion

		#region Constants

		/// <summary>
		/// The shared log instance for this class (and derived classes).
		/// </summary>
		protected static readonly ILog log =
			LogManager.GetLogger(typeof (ErrorCodeExceptionTranslator));

		#endregion

		#region Constructor (s)

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorCodeExceptionTranslator"/> class.
        /// </summary>
        /// <param name="provider">The data provider.</param>
	    public ErrorCodeExceptionTranslator(IDbProvider provider)
	    {
	        DbProvider = provider;
	    }



        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorCodeExceptionTranslator"/> class.
        /// </summary>
        /// <param name="provider">The data provider.</param>
        /// <param name="ec">The error code collection.</param>
        public ErrorCodeExceptionTranslator(IDbProvider provider, ErrorCodes ec)
        {
            DbProvider = provider;
            errorCodes = ec;
        }



		#endregion

		#region Properties

        /// <summary>
        /// Sets the db provider.
        /// </summary>
        /// <value>The db provider.</value>
	    public IDbProvider DbProvider
	    {
	        set
	        {
	            dbProvider = value;
                errorCodes = dbProvider.DbMetadata.ErrorCodes;

	        }
	    }

        /// <summary>
        /// Gets the error codes for the provider
        /// </summary>
        /// <value>The error codes.</value>
	    public ErrorCodes ErrorCodes
	    {
	        get
	        {
	            return errorCodes;
	        }
	    }

		#endregion

        /// <summary>
        /// Gets or sets the fallback translator to use in case error code based translation
        /// fails.
        /// </summary>
        /// <value>The fallback translator.</value>
	    public IAdoExceptionTranslator FallbackTranslator
	    {
	        get
	        {
	            return fallbackTranslator;
	        }
	        set
	        {
	            fallbackTranslator = value;
	        }
	    }


		#region Methods

		#endregion


        /// <summary>
	    /// Translate the given <see cref="System.SystemException"/> into a generic data access exception.
	    /// </summary>
	    /// <param name="task">A readable string describing the task being attempted.</param>
	    /// <param name="sql">The SQL query or update that caused the problem. May be null.</param>
	    /// <param name="exception">
	    /// The <see cref="System.Exception"/> encountered by the ADO.NET implementation.
	    /// </param>
	    /// <returns>
	    /// A <see cref="Spring.Dao.DataAccessException"/> appropriate for the supplied
	    /// <paramref name="exception"/>.
	    /// </returns>
        public virtual DataAccessException Translate(string task, string sql, Exception exception)
        {
            if (task == null)
            {
                task = "";
            }
            if (sql == null)
            {
                sql = "";
            }
            string errorCode = ExtractErrorCode(exception);

            DataAccessException dex = DoTranslate(task, sql, errorCode, exception);
            if (dex != null)
            {
                // Specific exception match found.
                return dex;
            }
            // Looking for a fallback...
            if (log.IsDebugEnabled)
            {
                log.Debug("Unable to translate exception with errorCode '" + errorCode + "', will use the fallback translator");
            }
            IAdoExceptionTranslator fallback = FallbackTranslator;
            if (fallback != null)
            {
                return FallbackTranslator.Translate(task, sql, exception);
            }
            // We couldn't identify it more precisely.
            return new UncategorizedAdoException(task, sql, errorCode, exception);


        }

        /// <summary>
        /// Template method for actually translating the given exception.
        /// given <see cref="System.SystemException"/> into a generic data access exception.
        /// </summary>
        /// <param name="task">A readable string describing the task being attempted.</param>
        /// <param name="sql">The SQL query or update that caused the problem. May be null.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="exception">The <see cref="System.Exception"/> encountered by the ADO.NET implementation.</param>
        /// <returns>
        /// A <see cref="Spring.Dao.DataAccessException"/> appropriate for the supplied
        /// <paramref name="exception"/>.
        /// </returns>
        /// <remarks>
        /// The passed-in arguments will have been pre-checked.  furthermore, this method is allowed to
        /// return <code>null</code> to indicate that no exception match has been found and that
        /// fallback translation should kick in.
        /// </remarks>
	    protected virtual DataAccessException DoTranslate(string task, string sql, string errorCode, Exception exception)
	    {
            // First, try custom translation from overridden method.
            DataAccessException dex = TranslateException(task, sql, errorCode, exception);
            if (dex != null)
            {
                return dex;
            }


	        if (errorCodes != null)
	        {

	            if (errorCode != null)
	            {
                    if (Array.IndexOf(errorCodes.BadSqlGrammarCodes, errorCode) >= 0)
	                {
	                    LogTranslation(task, sql, errorCode, exception, false);
	                    return new BadSqlGrammarException(task, sql, exception);
	                }
                    else if (Array.IndexOf(errorCodes.InvalidResultSetAccessCodes, errorCode) >= 0)
                    {
                        LogTranslation(task, sql, errorCode, exception, false);
                        return new InvalidResultSetAccessException(task, sql, exception);
                    }
                    else if (Array.IndexOf(errorCodes.DuplicateKeyCodes, errorCode) >= 0)
                    {
                        LogTranslation(task, sql, errorCode, exception, false);
                        return new DuplicateKeyException(task, sql, exception);
                    }
                    else if (Array.IndexOf(errorCodes.DataAccessResourceFailureCodes, errorCode) >= 0)
                    {
                        LogTranslation(task, sql, errorCode, exception, false);
                        return new DataAccessResourceFailureException(BuildMessage(task, sql, exception), exception);
                    }
                    else if (Array.IndexOf(errorCodes.TransientAccessResourceFailureCodes, errorCode) >= 0)
                    {
                        LogTranslation(task, sql, errorCode, exception, false);
                        return new TransientDataAccessResourceException(BuildMessage(task, sql, exception), exception);
                    }
                    else if (Array.IndexOf(errorCodes.PermissionDeniedCodes, errorCode) >= 0)
                    {
                        LogTranslation(task, sql, errorCode, exception, false);
                        return new PermissionDeniedDataAccessException(BuildMessage(task, sql, exception), exception);
                    }
                    else if (Array.IndexOf(errorCodes.DataIntegrityViolationCodes, errorCode) >= 0)
                    {
                        LogTranslation(task, sql, errorCode, exception, false);
                        return new DataIntegrityViolationException(BuildMessage(task, sql, exception), exception);
                    }
                    else if (Array.IndexOf(errorCodes.CannotAcquireLockCodes, errorCode) >= 0)
                    {
                        LogTranslation(task, sql, errorCode, exception, false);
                        return new CannotAcquireLockException(BuildMessage(task, sql, exception), exception);
                    }
                    else if (Array.IndexOf(errorCodes.DeadlockLoserCodes, errorCode) >= 0)
                    {
                        LogTranslation(task, sql, errorCode, exception, false);
                        return new DeadlockLoserDataAccessException(BuildMessage(task, sql, exception), exception);
                    }
                    else if (Array.IndexOf(errorCodes.CannotSerializeTransactionCodes, errorCode) >= 0)
                    {
                        LogTranslation(task, sql, errorCode, exception, false);
                        return new CannotSerializeTransactionException(BuildMessage(task, sql, exception), exception);
                    }
	            }
	        }
	        return null;
	    }

        /// <summary>
        /// Subclasses can override this method to attempt a custom mapping from a data access Exception
        /// to DataAccessException.
        /// </summary>
        /// <param name="task">Readable text describing the task being attempted.</param>
        /// <param name="sql">SQL query or update that caused the problem. May be <code>null</code>.</param>
        /// <param name="errorCode">The error code extracted from the generic data access exception for
        /// a particular provider.</param>
        /// <param name="exception">The exception thrown from a data access operation.</param>
        /// <returns>
        /// null if no custom translation was possible, otherwise a DataAccessException
        /// resulting from custom translation.  This exception should include the exception parameter
        /// as a nested root cause. This implementation always returns null, meaning that
        /// the translator always falls back to the default error codes.
        /// </returns>
	    protected virtual DataAccessException TranslateException(string task, string sql, string errorCode, Exception exception)
	    {
	        return null;
	    }


	    /// <summary>
	    /// Builds the message.
	    /// </summary>
	    /// <param name="task">The task.</param>
	    /// <param name="sql">The SQL.</param>
	    /// <param name="exception">The exception.</param>
	    /// <returns></returns>
	    protected virtual string BuildMessage(string task, string sql, Exception exception)
	    {
	        return task + "; SQL [" + sql + "]; " + exception.Message;
	    }

	    /// <summary>
        /// Determines whether the specified exception is valid data access exception.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <returns>
        /// 	<c>true</c> if is valid data access exception; otherwise, <c>false</c>.
        /// </returns>
	    public bool IsValidDataAccessException(Exception e)
	    {
	       return dbProvider.IsDataAccessException(e);
        }

        /// <summary>
        /// Logs the translation.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="b">if set to <c>true</c> [b].</param>
	    private void LogTranslation(string task, string sql, string errorCode, Exception exception, bool b)
	    {
            if (log.IsDebugEnabled)
            {
                String intro = "Translating";
                log.Debug(intro + " ADO exception with error code '" + errorCode
                          + "', message [" + exception.Message +
                    "]; SQL was [" + sql + "] for task [" + task + "]");
            }
	    }

	    private string ExtractErrorCode(Exception exception)
	    {
            // Use provider specific reflection information to extract error code.
	        return dbProvider.ExtractError(exception);
	    }
	}
}
