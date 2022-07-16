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

using System.Collections;

using Spring.Data.Common;

namespace Spring.Data.Objects
{
    /// <summary>
    /// Encapsulate Command ExecuteScalar operations within a reusable class.
    /// </summary>
    /// <remarks>The default CommandType is CommandType.Text</remarks>
    /// <author>Mark Pollack (.NET)</author>
    public class AdoScalar : AdoOperation
    {
        #region Constructor (s)

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoScalar"/> class.
        /// </summary>
        public AdoScalar()
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoScalar"/> class.
        /// </summary>
        public AdoScalar(IDbProvider provider)
            : base(provider)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoScalar"/> class.
        /// </summary>
        public AdoScalar(IDbProvider dbProvider, string sql)
            : base(dbProvider, sql)
        {}

        #endregion

        #region Methods

        public IDictionary ExecuteScalar(params object[] inParameterValues)
        {
            ValidateParameters(inParameterValues);
            return AdoTemplate.ExecuteScalar(NewCommandCreatorWithParamValues(inParameterValues));
        }

        public IDictionary ExecuteScalarByNamedParam(IDictionary inParams)
        {
            ValidateNamedParameters(inParams);
            return AdoTemplate.ExecuteScalar(NewCommandCreator(inParams));
        }

        #endregion
    }
}
