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
	/// Encapsulate Command.ExecuteNonQuery operations within a reusable class.
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	/// <remarks>The default CommandType is CommandType.Text</remarks>
	public class AdoNonQuery : AdoOperation
	{
        #region Constructor (s)

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoNonQuery"/> class.
        /// </summary>
        public AdoNonQuery()
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoNonQuery"/> class.
        /// </summary>
        public AdoNonQuery(IDbProvider provider)
            : base(provider)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="AdoNonQuery"/> class.
        /// </summary>
        public AdoNonQuery(IDbProvider dbProvider, string sql)
            : base(dbProvider, sql)
        {}

        #endregion

		#region Properties

		#endregion

		#region Methods

        public IDictionary ExecuteNonQuery(params object[] inParameterValues)
        {
            ValidateParameters(inParameterValues);
            return AdoTemplate.ExecuteNonQuery(NewCommandCreatorWithParamValues(inParameterValues));
        }


        public IDictionary ExecuteNonQueryByNamedParam(IDictionary inParams)
        {
            ValidateNamedParameters(inParams);
            return AdoTemplate.ExecuteNonQuery(NewCommandCreator(inParams));
        }

		#endregion
	}
}
