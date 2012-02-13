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
using Rhino.Mocks;
using Spring.Data.Common;

namespace Spring.Data.Objects
{
    /// <summary>
    /// Contains shared mocking calls for performing tests on AdoQuery and subclasses.
    /// </summary>
    public abstract class AbstractAdoQueryTests
    {
        protected IDbProvider provider;
        protected IDbCommand command;

        public void SetUpMocks()
        {
            provider = MockRepository.GenerateMock<IDbProvider>();
            IDbConnection connection = MockRepository.GenerateMock<IDbConnection>();

            provider.Stub(x => x.CreateConnection()).Return(connection).Repeat.Once();

            // Creating a query (setting DbProvider property)
            // will call new DbParameters(IDbProvider), which is a real pain to mock.
            // to store the declared parameters.

            command = MockRepository.GenerateMock<IDbCommand>();
            //This IDbCommand is used as a container for the underlying parameter collection.	
            provider.Stub(x => x.CreateCommand()).Return(command).Repeat.Once();

            //Create a real instance of IDbParameters to stored the declared parameters
            IDbProvider realDbProvider = DbProviderFactory.GetDbProvider("System.Data.SqlClient");
            IDbParameters dbParameters = new DbParameters(realDbProvider);

            //Pass real instance into mock instance.
            command.Stub(x => x.Parameters).Return(dbParameters.DataParameterCollection).Repeat.Once();
            provider.Stub(x => x.CreateCommand()).Return(command).Repeat.Once();

            // done with init of DbParameters mock/stubbing
        }
    }
}