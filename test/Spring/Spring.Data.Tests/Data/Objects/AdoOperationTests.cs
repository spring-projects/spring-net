#region License

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
#region License

/*
 * Copyright © 2002-2008 the original author or authors.
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
using NUnit.Framework;
using Spring.Dao;
using Spring.Data.Common;

#endregion

namespace Spring.Data.Objects
{
    /// <summary>
    /// Tests for AdoOperation
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <version>$Id: AdoOperationTests.cs,v 1.1 2008/05/30 21:10:00 markpollack Exp $</version>
    [TestFixture]
    public class AdoOperationTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [ExpectedException(typeof(InvalidDataAccessApiUsageException))]
        public void EmptySql()
        {
            TestAdoOperation operation = new TestAdoOperation();
            operation.Compile();           
        }

        [Test]
        [ExpectedException(typeof(InvalidDataAccessApiUsageException))]
        public void DeclareParameterAfterCompile()
        {
            TestAdoOperation operation = new TestAdoOperation();
            operation.DbProvider = DbProviderFactory.GetDbProvider("System.Data.SqlClient");
            operation.Sql = "select * from table";
            operation.Compile();
            IDbParameters parameters = new DbParameters(operation.DbProvider);
            operation.DeclaredParameters = parameters;
        }

        [Test]
        [ExpectedException(typeof(InvalidDataAccessApiUsageException))]
        public void TooFewParameters()
        {
            TestAdoOperation operation = new TestAdoOperation();
            operation.DbProvider = DbProviderFactory.GetDbProvider("System.Data.SqlClient");
            operation.Sql = "select * from table";
            IDbParameters parameters = new DbParameters(operation.DbProvider);
            parameters.Add("name");
            operation.DeclaredParameters = parameters;
            operation.Compile();
            operation.ValidateParams(null);
        }
        
    }

    internal class TestAdoOperation : AdoOperation
    {
        protected override void CompileInternal()
        {
            
        }

        public void ValidateParams(params object[] inParamValues)
        {
            ValidateParameters(inParamValues);
        }
    }
}