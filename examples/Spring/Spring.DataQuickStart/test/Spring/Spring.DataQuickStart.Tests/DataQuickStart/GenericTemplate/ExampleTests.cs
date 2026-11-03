using System.Collections.Generic;

using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Data.Config;
using Spring.Objects.Factory.Xml;

using Spring.DataQuickStart.Dao.GenericTemplate;
using Spring.DataQuickStart.Domain;

namespace Spring.DataQuickStart.GenericTemplate
{
    [TestFixture]
    public class ExampleTests
    {
        private IApplicationContext ctx;

        [SetUp]
        public void InitContext()
        {
            // Configure Spring programmatically
            NamespaceParserRegistry.RegisterParser(typeof(DatabaseNamespaceParser));
            ctx = new XmlApplicationContext(
                "assembly://Spring.DataQuickStart.Tests/Spring.DataQuickStart.GenericTemplate/ExampleTests.xml");
        }
        
        
        [Test]
        public void CallbackDaoTest()
        {
            CommandCallbackDao commandCallbackDao = ctx["commandCallbackDao"] as CommandCallbackDao;
            int count = commandCallbackDao.FindCountWithPostalCodeWithDelegate("1010");
            Assert.That(count, Is.EqualTo(3));
        }
        
        [Test]
        public void ResultSetExtractorDaoTest()
        {
            ResultSetExtractorDao dao = ctx["resultSetExtractorDao"] as ResultSetExtractorDao;
            IDictionary<string, IList<string>> postalCodeDictionary = dao.GetPostalCodeCustomerMapping();
            Assert.That(postalCodeDictionary, Is.Not.Null);
            Assert.That(postalCodeDictionary.Count, Is.EqualTo(87));

            IList<string> contactNameList = dao.GetCustomerNameByCountryAndCity("Brazil", "Sao Paulo");
            Assert.That(contactNameList.Count, Is.EqualTo(4));

            contactNameList = dao.GetCustomerNameByCountryAndCityWithParamsBuilder("Brazil", "Sao Paulo");
            Assert.That(contactNameList.Count, Is.EqualTo(4));

            contactNameList = dao.GetCustomerNameByCountryAndCityWithCommandSetterDelegate("Brazil", "Sao Paulo");
            Assert.That(contactNameList.Count, Is.EqualTo(4));

            contactNameList = dao.GetCustomerNameByCountryAndCityWithAllDelegates("Brazil", "Sao Paulo");
            Assert.That(contactNameList.Count, Is.EqualTo(4));

            contactNameList = dao.GetCustomerNameByCountryAndCityWithCommandSetter("Brazil", "Sao Paulo");
            Assert.That(contactNameList.Count, Is.EqualTo(4));

            contactNameList = dao.GetCustomerNameByCountry("Brazil");
            Assert.That(contactNameList.Count, Is.EqualTo(9));
        }
        
        [Test]
        public void RowMapperDaoTest()
        {
            RowMapperDao dao = ctx["rowMapperDao"] as RowMapperDao;
            
            IList<Customer> customerList = dao.GetCustomersWithDelegate();
            
            AssertsOnCustomerList(customerList);

            customerList = dao.GetCustomersWithDelegate();
            
            AssertsOnCustomerList(customerList);
                        
        }
        
        [Test]
        public void RowCallbackDaoTest()
        {
            RowCallbackDao dao = ctx["rowCallbackDao"] as RowCallbackDao;
            IDictionary<string, IList<string>> postalCodeDictionary = dao.GetPostalCodeCustomerMapping();
            Assert.That(postalCodeDictionary, Is.Not.Null);
            Assert.That(postalCodeDictionary.Count, Is.EqualTo(87));
        }

        [Test]
        public void QueryForObjectDaoTest()
        {
            QueryForObjectDao dao = ctx["queryForObjectDao"] as QueryForObjectDao;
            Customer customer = dao.GetCustomer("Hanna Moos");
            Assert.That(customer.ContactName, Is.EqualTo("Hanna Moos"));
        }

        [Test]
        public void QueryForDetailsUsingStoredProcObject()
        {
            CustOrdersDetailStoredProc storedProc = ctx["custOrdersDetailStoredProc"] as CustOrdersDetailStoredProc;
            List<OrderDetails> details = storedProc.GetOrderDetails(10278);
            Assert.That(details.Count, Is.EqualTo(4));
        }

        private static void AssertsOnCustomerList(IList<Customer> customerList)
        {
            Assert.That(customerList, Is.Not.Null);
            Assert.That(customerList.Count, Is.EqualTo(91));
        }
    }
}