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
using System.Collections.Generic;
using Spring.Data.NHibernate.Support;

using Spring.Northwind.Domain;
using Spring.Stereotype;
using Spring.Transaction.Interceptor;

#endregion

namespace Spring.Northwind.Dao.NHibernate
{
    [Repository]
    public class HibernateProductDao : HibernateDao, IProductDao
    {
        [Transaction(ReadOnly = true)]
        public Product Get(int productId)
        {
            return CurrentSession.Get<Product>(productId);
        }

        [Transaction(ReadOnly = true)]
        public IList<Product> GetAll()
        {
            return GetAll<Product>();
        }

        [Transaction]
        public int Save(Product product)
        {
            return (int) CurrentSession.Save(product);
        }

        [Transaction]
        public void Update(Product product)
        {
            CurrentSession.SaveOrUpdate(product);
        }

        [Transaction]
        public void Delete(Product product)
        {
            CurrentSession.Delete(product);
        }
    }
}
