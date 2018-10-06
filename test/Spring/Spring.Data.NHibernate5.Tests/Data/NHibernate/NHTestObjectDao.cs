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

using System;
using System.Collections;

using NHibernate.Type;

using Spring.Data.NHibernate.Support;

#endregion

namespace Spring.Data.NHibernate
{
    /// <summary>
    /// NHibernate based DAO implementation of ITestObjectDAao: 
    /// </summary>
    /// <author>Mark Pollack (.NET)</author>
    public class TestObjectDao : HibernateDaoSupport, ITestObjectDao
    {
        #region Constructor (s)

        /// <summary>
        /// Initializes a new instance of the <see cref="TestObjectDao"/> class.
        /// </summary>
        public TestObjectDao()
        {
        }

        #endregion

        #region ITestObjectDao Members

        public void Create(TestObject to)
        {
            //HibernateTemplate.Save(to);
        }

        public void Update(TestObject to)
        {
            HibernateTemplate.Update(to);
        }

        public void Delete(TestObject to)
        {
            HibernateTemplate.Delete(to);
        }

        public TestObject FindByName(string name)
        {
            IList result = HibernateTemplate.Find(
                "select from TestObject as to where to.Name=?",
                name,
                TypeFactory.GetStringType(50)
                );

            if (result.Count > 0)
            {
                return (TestObject) result[0];
            }
            else
            {
                return null;
            }
        }

        public void CreateUpdateRollback(TestObject to)
        {
            HibernateTemplate.Save(to);
            to.Name = "Updated Name";
            HibernateTemplate.Update(to);
            throw new Exception("My expected exception for test purposes.");
        }

        #endregion
    }
}