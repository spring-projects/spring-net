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

using Spring.Northwind.Domain;

#endregion

namespace Spring.Northwind.Dao.NHibernate
{
    public class HibernateProductDao : HibernateDaoSupport, IProductDao
    {
        public Product FindById(int productId)
        {
            return HibernateTemplate.Load(typeof(Product), productId) as Product;
     
        }

        public IList FindAll()
        {
            return HibernateTemplate.LoadAll(typeof(Product));
        }

        public Product Save(Product product)
        {
            HibernateTemplate.Save(product);
            return product;
        }

        public Product SaveOrUpdate(Product product)
        {
            HibernateTemplate.SaveOrUpdate(product);
            return product;
        }

        public void Delete(Product product)
        {
            HibernateTemplate.Delete(product);
        }
    }
}
