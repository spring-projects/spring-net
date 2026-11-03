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

#region Imports

using System.Data;
using NHibernate;
using NUnit.Framework;
using Spring.Data.NHibernate;
using Spring.Northwind.Dao;
using Spring.Northwind.Domain;

#endregion

namespace Spring.Northwind.IntegrationTests
{
    /// <summary>
    /// This class contains tests for the DAO implementations
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class NorthwindIntegrationTests : AbstractDaoIntegrationTests
    {
        private ICustomerDao customerDao;
        private IOrderDao orderDao;

        private ISessionFactory sessionFactory;


        // These properties will be injected based on type
        public ICustomerDao CustomerDao
        {
            set { customerDao = value; }
        }


        public IOrderDao OrderDao
        {
            set { orderDao = value; }
        }


        public ISessionFactory SessionFactory
        {
            set { sessionFactory = value; }
        }

        [Test]
        public void SanityCheck()
        {

            Assert.That(customerDao, Is.Not.Null, "customer dao is null");

        }

        [Test]
        public void CustomerDaoTests()
        {
            Assert.That(customerDao.GetAll().Count, Is.EqualTo(91));

            Customer c = new Customer(new DefaultCustomerClassificationCalculator());
            c.Id = "MPOLL";           
            c.CompanyName = "Interface21";
            customerDao.Save(c);
            c = customerDao.Get("MPOLL");
            Assert.That("MPOLL", Is.EqualTo(c.Id));
            Assert.That("Interface21", Is.EqualTo(c.CompanyName));

            //Without flushing, nothing changes in the database:
            int customerCount = (int)AdoTemplate.ExecuteScalar(CommandType.Text, "select count(*) from Customers");
            Assert.That(customerCount, Is.EqualTo(91));

            //Flush the session to execute sql in the db.
            SessionFactoryUtils.GetSession(sessionFactory, true).Flush();
           
            //Now changes are visible outside the session but within the same database transaction
            customerCount = (int)AdoTemplate.ExecuteScalar(CommandType.Text, "select count(*) from Customers");
            Assert.That(customerCount, Is.EqualTo(92));

            Assert.That(customerDao.GetAll().Count, Is.EqualTo(92));

            c.CompanyName = "SpringSource";

            customerDao.Update(c);

            c = customerDao.Get("MPOLL");
            Assert.That("MPOLL", Is.EqualTo(c.Id));
            Assert.That("SpringSource", Is.EqualTo(c.CompanyName));

            customerDao.Delete(c);


            SessionFactoryUtils.GetSession(sessionFactory, true).Flush();
            customerCount = (int)AdoTemplate.ExecuteScalar(CommandType.Text, "select count(*) from Customers");
            Assert.That(customerCount, Is.EqualTo(92));

            try
            {
                c = customerDao.Get("MPOLL");
                Assert.Fail("Should have thrown HibernateObjectRetrievalFailureException when finding customer with Id = MPOLL");
            }
            catch (HibernateObjectRetrievalFailureException e)
            {
                Assert.That(e.PersistentClassName, Is.EqualTo("Customer"));
            }

        }

        [Test]
        public void ProductDaoTests()
        {
            Assert.That(orderDao.GetAll().Count, Is.EqualTo(830));

            Order order = new Order();
            Customer customer = customerDao.Get("PICCO");
            order.Customer = customer;
            order.ShipCity = "New York";
            
            orderDao.Save(order);
            int orderId = order.Id;
            order = orderDao.Get(orderId);
            Assert.That(order.Customer.Id, Is.EqualTo("PICCO"));
            Assert.That(order.ShipCity, Is.EqualTo("New York"));

            //SessionFactoryUtils.GetSession(sessionFactory, true).Flush();
            // Required trip to db to get idendity column, so data is visible

            int ordersCount = (int)AdoTemplate.ExecuteScalar(CommandType.Text, "select count(*) from Orders");
            Assert.That(ordersCount, Is.EqualTo(831));

            Assert.That(orderDao.GetAll().Count, Is.EqualTo(831));

            order.ShipCity = "Sao Paulo";
            orderDao.Update(order);

            order = orderDao.Get(orderId);
            Assert.That(order.Customer.Id, Is.EqualTo("PICCO"));
            Assert.That(order.ShipCity, Is.EqualTo("Sao Paulo"));


            orderDao.Delete(order);

            //Without flushing, nothing changes in the database:
            SessionFactoryUtils.GetSession(sessionFactory, true).Flush();

            ordersCount = (int)AdoTemplate.ExecuteScalar(CommandType.Text, "select count(*) from Orders");
            Assert.That(ordersCount, Is.EqualTo(830));

            try
            {
                order = orderDao.Get(orderId);
                Assert.Fail("Should have thrown HibernateObjectRetrievalFailureException when finding order with Id = " + orderId);
            }
            catch (HibernateObjectRetrievalFailureException e)
            {
                Assert.That(e.PersistentClassName, Is.EqualTo("Order"));
            }

        }
    }
}