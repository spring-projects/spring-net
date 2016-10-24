#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

#region Imports

using System.Data;
using System.Data.SqlClient;

using NUnit.Framework;

using Rhino.Mocks;

using Spring.Data.Common;

#endregion

namespace Spring.Data.Objects
{
    /// <summary>
    /// Tests for StoredProcedure
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class StoredProcedureTests : AbstractAdoQueryTests
    {
        [SetUp]
        public void Setup()
        {
            SetUpMocks();
        }

        [Test]
        public void NullArg()
        {
            SqlParameter sqlParameter1 = new SqlParameter();
            command.Stub(x => x.CreateParameter()).Return(sqlParameter1);
            provider.Stub(x => x.CreateParameterNameForCollection("ptest")).Return("@ptest");

            //Create a real instance of IDbParameters to store the executable parameters
            //IDbProvider realDbProvider = DbProviderFactory.GetDbProvider("System.Data.SqlClient");
            //IDbParameters dbParameters = new DbParameters(realDbProvider);
            IDataParameterCollection dbParamCollection = new SqlCommand().Parameters;
            //provide the same instance to another call to extract output params
            command.Stub(x => x.Parameters).Return(dbParamCollection).Repeat.Twice();

            NullArg na = new NullArg(provider);
            na.Execute(null);
        }
    }

    internal class NullArg : StoredProcedure
    {
        public NullArg(IDbProvider provider)
            : base(provider, "takes_null")
        {
            DeclaredParameters.Add("ptest", DbType.String);
            Compile();
        }

        public void Execute(string s)
        {
            ExecuteNonQuery(s);
        }
    }
}