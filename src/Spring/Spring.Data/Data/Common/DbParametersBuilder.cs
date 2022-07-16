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
using Common.Logging;

namespace Spring.Data.Common
{
    /// <summary>
    /// Assists in creating a collection of parameters by keeping track of all
    /// IDbParameters its creates.  After creating a number of parameters ask for the collection
    /// with the GetParameters methods.
    /// </summary>
    /// <remarks>This builder is stateful, you must create a new one for each collection
    /// of parameters you would like to create.</remarks>
    /// <author>Mark Pollack (.NET)</author>
    public class DbParametersBuilder : IDbParametersBuilder
    {
        #region Fields

        private IDbProvider dbProvider;
        private IList parameterList;

        #endregion

        #region Constants

        /// <summary>
        /// The shared log instance for this class (and derived classes).
        /// </summary>
        protected static readonly ILog log =
            LogManager.GetLogger(typeof (DbParametersBuilder));

        #endregion

        #region Constructor (s)

        /// <summary>
        /// Initializes a new instance of the <see cref="DbParametersBuilder"/> class.
        /// </summary>
        public DbParametersBuilder(IDbProvider dbProvider)
        {
            this.dbProvider = dbProvider;
            parameterList = new ArrayList();
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        public IDbParameter Create()
        {
            IDbParameter p = new DbParameter();
            parameterList.Add(p);
            return p;
        }

        public IDbParameters GetParameters()
        {
            IDbParameters dbParameters = CreateDbParameters();
            foreach (IDbParameter parameter in parameterList)
            {
                dbParameters.AddParameter(parameter.GetName(),
                                          parameter.GetDbType(),
                                          parameter.GetSize(),
                                          parameter.GetDirection(),
                                          parameter.GetIsNullable(),
                                          parameter.GetPrecision(),
                                          parameter.GetScale(),
                                          parameter.GetSourceColumn(),
                                          parameter.GetSourceVersion(),
                                          parameter.GetValue());
            }
            return dbParameters;
        }


        protected IDbParameters CreateDbParameters()
        {
            return new DbParameters(dbProvider);
        }

        #endregion
    }
}
