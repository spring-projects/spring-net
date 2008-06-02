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


#if NET_2_0

using System.Collections.Generic;
using System.Data;
using Spring.Objects;

namespace Spring.Data.Generic
{
    /// <summary>
    /// This is 
    /// </summary>
    /// <remarks>
    ///
    /// </remarks>
    /// <author>Mark Pollack</author>
    public class TestObjectDao : AdoDaoSupport, ITestObjectDao
    {
        public IList<TestObject> FindAll()
        {
            return AdoTemplate.QueryWithRowMapper(CommandType.Text,
                "select TestObjectNo, Age, Name from TestObjects",
                new TestObjectRowMapper<TestObject>());
        }

        public TestObject FindOne()
        {
            return AdoTemplate.QueryForObject(CommandType.Text,
                                              "",
                                              new TestObjectRowMapper<TestObject>());
        }
    }
}

#endif