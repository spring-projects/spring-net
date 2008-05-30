using System.Data;
using Spring.Data.Core;
using Spring.DataQuickStart.Domain;

namespace Spring.DataQuickStart.Dao.Template
{
    public class QueryForObjectDao : AdoDaoSupport
    {
        private string cmdText = @"select Address, City, CompanyName, ContactName, " +
                     "ContactTitle, Country, Fax, CustomerID, Phone, PostalCode, " +
                     "Region from Customers where ContactName = @ContactName";
        
        public Customer GetCustomer(string contactName)
        {
            return (Customer)AdoTemplate.QueryForObject(CommandType.Text, cmdText, new CustomerRowMapper(),
                                 "ContactName", DbType.String, 30, contactName);
        }
    }
}
