using System.Collections;
using System.Data;
using Spring.Data.Core;

namespace Spring.DataQuickStart.Dao.Template
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
        public virtual IList GetCustomers()
        {
            return AdoTemplate.QueryWithRowMapper(CommandType.Text, cmdText,
                                                  new CustomerRowMapper());
        }

    }
}