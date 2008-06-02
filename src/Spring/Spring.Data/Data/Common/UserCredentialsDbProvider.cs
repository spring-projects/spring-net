#region Licence

/*
 * Copyright © 2002-2007 the original author or authors.
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
using Spring.Threading;
using Spring.Util;

namespace Spring.Data.Common
{
    /// <summary>
    /// An adapter for a target IDbProvider, applying the specified user credentials
    /// to the connection string for every GetConnection call.
    /// </summary>
    public class UserCredentialsDbProvider : DelegatingDbProvider
    {
        private string username;

        private string password;

        private string separator = ";";

        private const string USERNAME = "UserCredentialsDbProvider.UserName";

        private const string PASSWORD = "UserCredentialsDbProvider.Password";

        /// <summary>
        /// Sets the username string that will be appended to the connection string.
        /// </summary>
        /// <value>The username.</value>
        public string Username
        {
            set { username = value; }
        }

        /// <summary>
        /// Sets the password string that will be appended to the connection string.
        /// </summary>
        /// <value>The password.</value>
        public string Password
        {
            set { password = value; }
        }


        /// <summary>
        /// Sets the separator used to separate elements of the connection string.  Default is ';'.
        /// </summary>
        /// 
        /// <value>The separator used to separate elements in the connection string.</value>
        public string Separator
        {
            set { separator = value; }
        }

        /// <summary>
        /// Sets the user credentials for current thread.  The given username and password
        /// strings will be added to the connection string for all subsequent GetConnection
        /// requests.  This will override any statically specified user credentials, that is,
        /// set by the properties Username nad Password.
        /// </summary>
        /// <param name="user">The username part of the connection string.</param>
        /// <param name="pass">The password part of the connection string.</param>
        public void SetCredentialsForCurrentThread(string user, string pass)
        {
            LogicalThreadContext.SetData(USERNAME, user);
            LogicalThreadContext.SetData(PASSWORD, pass);
        }

        /// <summary>
        /// Removes the user credentials from current thread.  Use statically specified 
        /// credentials afterwards.
        /// </summary>
        public void RemoveCredentialsFromCurrentThread()
        {
            LogicalThreadContext.FreeNamedDataSlot(USERNAME);
            LogicalThreadContext.FreeNamedDataSlot(PASSWORD);
        }


        /// <summary>
        /// Returns a new connection object to communicate with the database.
        /// Determine if there are currently thread-bound credentials, using them if
        /// available, falling back to the statically specified username and password
        /// (i.e. values of the properties 'Username' and 'Password') otherwise.  
        /// The username and password will be concatenated on the connection string
        /// using string in the Separator property
        /// </summary>
        /// <returns>A new <see cref="IDbConnection"/></returns>
        public override IDbConnection CreateConnection()
        {
            string user = LogicalThreadContext.GetData(USERNAME) as string;
            string pass = LogicalThreadContext.GetData(PASSWORD) as string;
            if (user != null && pass != null)
            {
                return DoCreateConnection(user, pass);
            }
            else
            {
                return DoCreateConnection(username, password);
            }
        }

        protected virtual IDbConnection DoCreateConnection(string user, string pass)
        {
            AssertUtils.ArgumentNotNull(TargetDbProvider,"TargetDbProvider");
            if (StringUtils.HasLength(user))
            {
                IDbConnection conn = TargetDbProvider.CreateConnection();
                string s= ConnectionString + separator + user + separator + pass;
                conn.ConnectionString = s;
                return conn;
            }
            else
            {
                return TargetDbProvider.CreateConnection();
            }
        }
    }

}