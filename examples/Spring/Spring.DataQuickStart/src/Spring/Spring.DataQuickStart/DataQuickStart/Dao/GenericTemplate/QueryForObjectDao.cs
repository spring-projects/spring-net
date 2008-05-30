using System.Data;
using Spring.Data.Generic;
using Spring.DataQuickStart.Domain;

namespace Spring.DataQuickStart.Dao.GenericTemplate
{
    public class QueryForObjectDao : AdoDaoSupport
    {
        private string cmdText = "select Address, City, CompanyName, ContactName, " +
                     "ContactTitle, Country, Fax, CustomerID, Phone, PostalCode, " +
                     "Region from Customers where ContactName = @ContactName";
        
        public Customer GetCustomer(string contactName)
        {
            return AdoTemplate.QueryForObject(CommandType.Text, cmdText, 
                                              new CustomerRowMapper<Customer>(),
                                              "ContactName", DbType.String, 30, contactName);
        }
    }
}
