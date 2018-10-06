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

using NHibernate;

#endregion

namespace Spring.Data.NHibernate
{
	public class NativeNHTestObjectDao  : ITestObjectDao
	{
	    public ISessionFactory SessionFactory
	    {
	        get { return sessionFactory; }
	        set { sessionFactory = value; }
	    }

	    private ISessionFactory sessionFactory;

		#region Constructor (s)
		/// <summary>
		/// Initializes a new instance of the <see cref="NativeNHTestObjectDao"/> class.
                /// </summary>
		public 	NativeNHTestObjectDao()
		{

		}

		#endregion


		#region Methods

		#endregion

        #region ITestObjectDao Members

        public void Create(TestObject to)
        {
            ISession session = null;
            ITransaction transaction = null;
	            
            try
            {
                session = SessionFactory.OpenSession();
                
                transaction = session.BeginTransaction();
	
                session.Save(to);

                transaction.Commit();
            }
            catch
            {
                if(transaction != null)
                    transaction.Rollback(); 
                throw; 
            }
            finally
            {
                if(session != null)
                    session.Close();
            }

        }

        public void Update(TestObject to)
        {
            // TODO:  Add NativeNHTestObjectDao.Update implementation
        }

        public void Delete(TestObject to)
        {
            // TODO:  Add NativeNHTestObjectDao.Delete implementation
        }

        public TestObject FindByName(string name)
        {
            // TODO:  Add NativeNHTestObjectDao.FindByName implementation
            return null;
        }

        public void CreateUpdateRollback(TestObject to)
        {
            // TODO:  Add NativeNHTestObjectDao.CreateUpdateRollback implementation
        }

        #endregion
    }
}
