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
            Assert.AreEqual(3, count);
        }
        
        [Test]
        public void ResultSetExtractorDaoTest()
        {
            ResultSetExtractorDao dao = ctx["resultSetExtractorDao"] as ResultSetExtractorDao;
            IDictionary<string, IList<string>> postalCodeDictionary = dao.GetPostalCodeCustomerMapping();
            Assert.IsNotNull(postalCodeDictionary);
            Assert.AreEqual(87, postalCodeDictionary.Count);

            IList<string> contactNameList = dao.GetCustomerNameByCountryAndCity("Brazil", "Sao Paulo");
            Assert.AreEqual(4, contactNameList.Count);

            contactNameList = dao.GetCustomerNameByCountryAndCityWithParamsBuilder("Brazil", "Sao Paulo");
            Assert.AreEqual(4, contactNameList.Count);

            contactNameList = dao.GetCustomerNameByCountryAndCityWithCommandSetterDelegate("Brazil", "Sao Paulo");
            Assert.AreEqual(4, contactNameList.Count);

            contactNameList = dao.GetCustomerNameByCountryAndCityWithAllDelegates("Brazil", "Sao Paulo");
            Assert.AreEqual(4, contactNameList.Count);

            contactNameList = dao.GetCustomerNameByCountryAndCityWithCommandSetter("Brazil", "Sao Paulo");
            Assert.AreEqual(4, contactNameList.Count);

            contactNameList = dao.GetCustomerNameByCountry("Brazil");
            Assert.AreEqual(9, contactNameList.Count);
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
            Assert.IsNotNull(postalCodeDictionary);
            Assert.AreEqual(87, postalCodeDictionary.Count);
        }

        [Test]
        public void QueryForObjectDaoTest()
        {
            QueryForObjectDao dao = ctx["queryForObjectDao"] as QueryForObjectDao;
            Customer customer = dao.GetCustomer("Hanna Moos");
            Assert.AreEqual(customer.ContactName, "Hanna Moos");
        }

        [Test]
        public void QueryForDetailsUsingStoredProcObject()
        {
            CustOrdersDetailStoredProc storedProc = ctx["custOrdersDetailStoredProc"] as CustOrdersDetailStoredProc;
            List<OrderDetails> details = storedProc.GetOrderDetails(10278);
            Assert.AreEqual(4, details.Count);
        }

        private static void AssertsOnCustomerList(IList<Customer> customerList)
        {
            Assert.IsNotNull(customerList);
            Assert.AreEqual(91, customerList.Count);
        }
    }
}