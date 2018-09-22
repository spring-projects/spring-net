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

using System.Data;
using System.Data.SqlClient;

using FakeItEasy;

using NUnit.Framework;

using Spring.Data.Common;

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
            A.CallTo(() => command.CreateParameter()).Returns(sqlParameter1);
            A.CallTo(() => provider.CreateParameterNameForCollection("ptest")).Returns("@ptest");

            //Create a real instance of IDbParameters to store the executable parameters
            //IDbProvider realDbProvider = DbProviderFactory.GetDbProvider("System.Data.SqlClient");
            //IDbParameters dbParameters = new DbParameters(realDbProvider);
            //provide the same instance to another call to extract output params
            A.CallTo(() => command.Parameters).ReturnsLazily(() => new SqlCommand().Parameters).Twice();

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