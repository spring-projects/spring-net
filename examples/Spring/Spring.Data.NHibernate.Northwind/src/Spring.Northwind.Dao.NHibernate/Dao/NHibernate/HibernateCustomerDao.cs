#region License

/*
 * Copyright 2002-2007 the original author or authors.
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


using System.Collections;

using Spring.Data.NHibernate.Support;
using Spring.Transaction.Interceptor;

using Spring.Northwind.Domain;

#endregion

namespace Spring.Northwind.Dao.NHibernate
{
    public class HibernateCustomerDao : HibernateDaoSupport, ICustomerDao
    {
        public Customer FindById(string customerId)
        {
            return HibernateTemplate.Load(typeof (Customer), customerId) as Customer;
        }

        public IList FindAll()
        {
            return HibernateTemplate.LoadAll(typeof (Customer));
        }

        // Note that the transaction demaraction is here only for the case when
        // the DAO object is being used directly, i.e. not as part of a service layer
        // call.  This would be commonly only when creating an application that contains
        // no business logic and is essentially a table maintenance application.  
        // These applications are affectionaly known as 'CRUD' applications, the acronym
        // refering to Create, Retrieve, Update, And Delete and the only operations
        // performed by the application.

        // If called from a transactional service layer, typically with the transaction
        // propagation setting set to REQUIRED, then any DAO operations will use the 
        // same settings as started from the transactional layer.

        [Transaction(ReadOnly = false)]
        public Customer Save(Customer customer)
        {
            HibernateTemplate.Save(customer);
            return customer;
        }

        [Transaction(ReadOnly = false)]
        public Customer SaveOrUpdate(Customer customer)
        {
            HibernateTemplate.SaveOrUpdate(customer);
            return customer;
        }

        [Transaction(ReadOnly = false)]
        public void Delete(Customer customer)
        {
            HibernateTemplate.Delete(customer);
        }
    }
}