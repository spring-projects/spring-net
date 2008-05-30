using System.Collections.Generic;
using System.Data;
using Spring.Data.Generic;
using Spring.DataQuickStart.Domain;
//TODO move 'classic' AdoDaoSupport under Spring.Data to avoid clashes...

namespace Spring.DataQuickStart.Dao.GenericTemplate
{
    /// <summary>
    /// A simple DAO that uses Generic.AdoTemplate RowMapper functionality
    /// </summary>
    public class RowMapperDao : AdoDaoSupport
    {
        private string cmdText = "select Address, City, CompanyName, ContactName, " +
                             "ContactTitle, Country, Fax, CustomerID, Phone, PostalCode, " +
                             "Region from Customers";


        /// <summary>
        /// Gets the customers.
        /// </summary>
        /// <returns>A list of customers</returns>


        /// <summary>
        /// Gets the customers using a row mapper delegate.
        /// </summary>
        /// <remarks>Note that a NullMappingDataReader is used to convert
        /// DbNull values to simple defaults (i.e. empty string)
        /// when getting data from the DataReader.  You can provide AdoTemplate
        /// with your own version or not use a dataReader 'wrapper' entirely.
        /// </remarks>
        /// <returns>A list of customers</returns>
        public virtual IList<Customer> GetCustomersWithDelegate()
        {
            return AdoTemplate.QueryWithRowMapperDelegate<Customer>(CommandType.Text, cmdText,
                        delegate(IDataReader dataReader, int rowNum)
                            {
                                Customer customer = new Customer();
                                customer.Address = dataReader.GetString(0);
                                customer.City = dataReader.GetString(1);
                                customer.CompanyName = dataReader.GetString(2);
                                customer.ContactName = dataReader.GetString(3);
                                customer.ContactTitle = dataReader.GetString(4);
                                customer.Country = dataReader.GetString(5);
                                customer.Fax = dataReader.GetString(6);
                                customer.Id = dataReader.GetString(7);
                                customer.Phone = dataReader.GetString(8);
                                customer.PostalCode = dataReader.GetString(9);
                                customer.Region = dataReader.GetString(10);
                                return customer;
                            });
        }
    }
}