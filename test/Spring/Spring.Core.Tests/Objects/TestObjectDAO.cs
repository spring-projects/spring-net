#region License

/*
 * Copyright 2002-2010 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System.Data;

namespace Spring.Objects
{
    /// <summary>
    /// Test class to help test PropertyPlaceholderConfigurer
    /// </summary>
    public class TestObjectDAO
    {
        /// <summary>
        /// Create an instance of TestObjectDAO
        /// </summary>
        public TestObjectDAO()
        {
        }


        private int maxResults = 100;

        private IDbConnection dbConnection;

        /// <summary>
        /// The database connection property
        /// </summary>
        public IDbConnection DbConnection
        {
            get
            {
                return dbConnection;
            }
            set
            {
                dbConnection = value;
            }
        }


        /// <summary>
        /// Maximum number of results to return in a query page.
        /// </summary>
        public int MaxResults
        {
            get
            {
                return maxResults;
            }
            set
            {
                maxResults = value;
            }
        }

    }
}