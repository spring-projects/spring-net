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

using FakeItEasy;

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
            provider = A.Fake<IDbProvider>();
            IDbConnection connection = A.Fake<IDbConnection>();

            A.CallTo(() => provider.CreateConnection()).Returns(connection).Once();

            // Creating a query (setting DbProvider property)
            // will call new DbParameters(IDbProvider), which is a real pain to mock.
            // to store the declared parameters.

            command = A.Fake<IDbCommand>();
            //This IDbCommand is used as a container for the underlying parameter collection.	
            A.CallTo(() => provider.CreateCommand()).Returns(command).Once();

            //Create a real instance of IDbParameters to stored the declared parameters
            IDbProvider realDbProvider = DbProviderFactory.GetDbProvider("System.Data.SqlClient");
            IDbParameters dbParameters = new DbParameters(realDbProvider);

            //Pass real instance into mock instance.
            A.CallTo(() => command.Parameters).Returns(dbParameters.DataParameterCollection).Once();
            A.CallTo(() => provider.CreateCommand()).Returns(command).Once();

            // done with init of DbParameters mock/stubbing
        }
    }
}