#region Licence

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

namespace Spring.Data.Common
{
    /// <summary>
    /// Holds ADO.NET error codes for a particular provider.
    /// </summary>
    /// <remarks>
    /// Used by ErrorCodeExceptionTranslator.  The embedded resource
    /// "dbProviders.xml" in Spring.Data.Common contains default
    /// ErrorCodes instances for various providers.
    /// </remarks>
    /// <author>Mark Pollack (.NET)</author>
    public class ErrorCodes
    {
        #region Fields
        private string[] databaseProductNames;

        private string[] badSqlGrammarCodes = new String[0];

        private string[] invalidResultSetAccessCodes = new String[0];

        private string[] duplicateKeyCodes = new string[0];

        private string[] dataIntegrityViolationCodes = new String[0];

        private string[] permissionDeniedCodes = new String[0];

        private string[] dataAccessResourceFailureCodes = new String[0];

        private string[] transientAccessResourceFailureCodes = new String[0];

        private string[] cannotAcquireLockCodes = new String[0];

        private string[] deadlockLoserCodes = new String[0];

        private string[] cannotSerializeTransactionCodes = new String[0];

        // CustomErrorCodesTranslation[] customTranslations;
        #endregion

        #region Constants

        /// <summary>
        /// The shared log instance for this class (and derived classes).
        /// </summary>
        protected static readonly ILog log =
            LogManager.GetLogger(typeof (ErrorCodes));

        #endregion

        #region Constructor (s)
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorCodes"/> class.
        /// </summary>
        public 	ErrorCodes()
        {

        }

        #endregion

        #region Properties
        public string DatabaseProductName
        {
            get
            {
                return (databaseProductNames != null && databaseProductNames.Length > 0 ?
                        databaseProductNames[0] : null);
            }
        }

        public string[] DatabaseProductNames
        {
            get { return databaseProductNames; }
            set { databaseProductNames = value; }
        }

        public string[] BadSqlGrammarCodes
        {
            get { return badSqlGrammarCodes; }
            set { badSqlGrammarCodes = value; }
        }

        public string[] InvalidResultSetAccessCodes
        {
            get { return invalidResultSetAccessCodes; }
            set { invalidResultSetAccessCodes = value; }
        }

        public string[] DataAccessResourceFailureCodes
        {
            get { return dataAccessResourceFailureCodes; }
            set { dataAccessResourceFailureCodes = value; }
        }

        public string[] PermissionDeniedCodes
        {
            get { return permissionDeniedCodes; }
            set { permissionDeniedCodes = value; }
        }

        public string[] DataIntegrityViolationCodes
        {
            get { return dataIntegrityViolationCodes; }
            set { dataIntegrityViolationCodes = value; }
        }

        public string[] CannotAcquireLockCodes
        {
            get { return cannotAcquireLockCodes; }
            set { cannotAcquireLockCodes = value; }
        }

        public string[] TransientAccessResourceFailureCodes
        {
            get { return transientAccessResourceFailureCodes; }
            set { transientAccessResourceFailureCodes = value; }
        }

        public string[] DeadlockLoserCodes
        {
            get { return deadlockLoserCodes; }
            set { deadlockLoserCodes = value; }
        }

        public string[] CannotSerializeTransactionCodes
        {
            get { return cannotSerializeTransactionCodes; }
            set { cannotSerializeTransactionCodes = value; }
        }

        public string[] DuplicateKeyCodes
        {
            get { return duplicateKeyCodes; }
            set { duplicateKeyCodes = value; }
        }

        #endregion

    }
}
