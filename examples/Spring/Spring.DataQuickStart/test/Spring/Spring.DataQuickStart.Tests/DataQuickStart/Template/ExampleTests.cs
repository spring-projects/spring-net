using System.Collections;
using System.Data;
using System.Globalization;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Data.Config;
using Spring.Data.Core;
using Spring.Objects.Factory.Xml;

using Spring.DataQuickStart.Dao.Template;
using Spring.DataQuickStart.Domain;

namespace Spring.DataQuickStart.Template
{
    [TestFixture]
    public class ExampleTests
    {
        private IApplicationContext ctx;
        private AdoTemplate adoTemplate;

        [SetUp]
        public void InitContext()
        {
            // Configure Spring programmatically
            NamespaceParserRegistry.RegisterParser(typeof(DatabaseNamespaceParser));
            ctx = new XmlApplicationContext(
                "assembly://Spring.DataQuickStart.Tests/Spring.DataQuickStart.Template/ExampleTests.xml");
            adoTemplate = ctx["adoTemplate"] as AdoTemplate;
        }
        
        
        [Test]
        public void CallbackDaoTest()
        {
            CommandCallbackDao commandCallbackDao = ctx["commandCallbackDao"] as CommandCallbackDao;
            int count = commandCallbackDao.FindCountWithPostalCode("1010");
            Assert.AreEqual(3, count);
        }
        
        [Test]
        public void ResultSetExtractorDaoTest()
        {
            ResultSetExtractorDao dao = ctx["resultSetExtractorDao"] as ResultSetExtractorDao;
            IDictionary postalCodeDictionary = dao.GetPostalCodeCustomerMapping();
            Assert.IsNotNull(postalCodeDictionary);
            Assert.AreEqual(87, postalCodeDictionary.Count);

            IList contactNameList = dao.GetCustomerNameByCountryAndCity("Brazil", "Sao Paulo");
            Assert.AreEqual(4, contactNameList.Count);

            contactNameList = dao.GetCustomerNameByCountryAndCityWithParamsBuilder("Brazil", "Sao Paulo");
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
            
            IList customerList = dao.GetCustomers();
            
            AssertsOnCustomerList(customerList);


        }
        
        [Test]
        public void RowCallbackDaoTest()
        {
            RowCallbackDao dao = ctx["rowCallbackDao"] as RowCallbackDao;
            IDictionary postalCodeDictionary = dao.GetPostalCodeCustomerMapping();
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
            IList details = storedProc.GetOrderDetails(10278);
            Assert.AreEqual(4, details.Count);
        }

        [Test]
        public void DataSetDemo()
        {
            CustomerDataSetDao customerDataSetDao = ctx["customerDataSetDao"] as CustomerDataSetDao;
            DataSet dataSet = customerDataSetDao.GetCustomers();
            Assert.AreEqual(91, dataSet.Tables["Table"].Rows.Count);

            dataSet = customerDataSetDao.GetCustomers("Customers");
            Assert.AreEqual(91, dataSet.Tables["Customers"].Rows.Count);

            dataSet = customerDataSetDao.GetCustomersLike("M%");
            Assert.AreEqual(12, dataSet.Tables["Table"].Rows.Count);

            dataSet = new DataSet();
            dataSet.Locale = CultureInfo.InvariantCulture;


            customerDataSetDao.FillCustomers(dataSet);
            Assert.AreEqual(91, dataSet.Tables["Customers"].Rows.Count);


            AddAndEditRow(dataSet);


            try
            {
                customerDataSetDao.UpdateWithCommandBuilder(dataSet);

                int count =
                    (int)adoTemplate.ExecuteScalar(CommandType.Text, "select count(*) from Customers where CustomerId = 'NewID'");
                Assert.AreEqual(1, count);

                count = (int)adoTemplate.ExecuteScalar(CommandType.Text, "select count(*) from Customers where CustomerId = 'ALFKI' and Phone = '030-0074322'");
                Assert.AreEqual(1, count);

            }
            finally
            {
                //revert changes
                adoTemplate.ExecuteNonQuery(CommandType.Text, "delete from Customers where CustomerId = 'NewID'");
                adoTemplate.ExecuteNonQuery(CommandType.Text,
                                            "update Customers SET Phone='030-0074321' where CustomerId = 'ALFKI'");
            }


        }

        private static void AddAndEditRow(DataSet dataSet)
        {
            DataRow dataRow = dataSet.Tables["Customers"].NewRow();
            dataRow["CustomerId"] = "NewID";
            dataRow["CompanyName"] = "New Company Name";
            dataRow["ContactName"] = "New Name";
            dataRow["ContactTitle"] = "New Contact Title";
            dataRow["Address"] = "New Address";
            dataRow["City"] = "New City";
            dataRow["Region"] = "NR";
            dataRow["PostalCode"] = "New Code";
            dataRow["Country"] = "New Country";
            dataRow["Phone"] = "New Phone";
            dataRow["Fax"] = "New Fax";
            dataSet.Tables["Customers"].Rows.Add(dataRow);

            DataRow alfkiDataRow = dataSet.Tables["Customers"].Rows[0];
            alfkiDataRow["Phone"] = "030-0074322"; // simple change, last digit changed from 1 to 2.
        }

        [Test]
        public void UpdateWithoutCommandBuilder()
        {
            CustomerDataSetDao customerDataSetDao = ctx["customerDataSetDao"] as CustomerDataSetDao;
            try
            {
                DataSet dataSet = new DataSet();
                dataSet.Locale = CultureInfo.InvariantCulture;
                customerDataSetDao.FillCustomers(dataSet);
                Assert.AreEqual(91, dataSet.Tables["Customers"].Rows.Count);

                AddAndEditRow(dataSet);

                customerDataSetDao.Update(dataSet);

                int count = (int)adoTemplate.ExecuteScalar(CommandType.Text, "select count(*) from Customers where CustomerId = 'NewID'");
                Assert.AreEqual(1, count);

                count = (int)adoTemplate.ExecuteScalar(CommandType.Text, "select count(*) from Customers where CustomerId = 'ALFKI' and Phone = '030-0074322'");
                Assert.AreEqual(1, count);

            } finally
            {
                //revert changes
                adoTemplate.ExecuteNonQuery(CommandType.Text, "delete from Customers where CustomerId = 'NewID'");
                adoTemplate.ExecuteNonQuery(CommandType.Text,
                                            "update Customers SET Phone='030-0074321' where CustomerId = 'ALFKI'");
            }
        }

        private static void AssertsOnCustomerList(IList customerList)
        {
            Assert.IsNotNull(customerList);
            Assert.AreEqual(91, customerList.Count);
        }
    }
}