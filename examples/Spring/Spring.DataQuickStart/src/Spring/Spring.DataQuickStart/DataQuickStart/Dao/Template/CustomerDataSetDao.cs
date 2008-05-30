

using System;
using System.Data;
using Spring.Data.Common;
using Spring.Data.Core;

namespace Spring.DataQuickStart.Dao.Template
{
    public class CustomerDataSetDao : AdoDaoSupport
    {
        private string selectAll = @"select Address, City, CompanyName, ContactName, " +
             "ContactTitle, Country, Fax, CustomerID, Phone, PostalCode, " +
             "Region from Customers";

        private string selectLike = @"select Address, City, CompanyName, ContactName, " +
             "ContactTitle, Country, Fax, CustomerID, Phone, PostalCode, " +
             "Region from Customers where ContactName like @ContactName";

        public DataSet GetCustomers()
        {
            return AdoTemplate.DataSetCreate(CommandType.Text, selectAll);
        }

        public DataSet GetCustomers(string dataSetName)
        {
            return AdoTemplate.DataSetCreate(CommandType.Text, selectAll, new string[] { dataSetName });
        }

        public DataSet GetCustomersLike(string contactName)
        {

            IDbParameters dbParameters = CreateDbParameters();
            dbParameters.Add("ContactName", DbType.String).Value = contactName;
            return AdoTemplate.DataSetCreateWithParams(CommandType.Text, selectLike, dbParameters);
        }

        public void FillCustomers(DataSet dataSet)
        {
            AdoTemplate.DataSetFill(dataSet, CommandType.Text, selectAll, new string[] { "Customers"} );
        }

        public void UpdateWithCommandBuilder(DataSet dataSet)
        {
            AdoTemplate.DataSetUpdateWithCommandBuilder(dataSet, CommandType.Text, selectAll, null, "Customers");
        }


        public void Update(DataSet dataSet)
        {
            string insertSql = @"INSERT Customers (CustomerID, CompanyName) VALUES (@CustomerId, @CompanyName)";
            IDbParameters insertParams = CreateDbParameters();
            insertParams.Add("CustomerId", DbType.String, 0, "CustomerId");   //.Value = "NewID";
            insertParams.Add("CompanyName", DbType.String, 0, "CompanyName"); //.Value = "New Company Name";

            string updateSql = @"update Customers SET Phone=@Phone where CustomerId = @CustomerId";                
            IDbParameters updateParams = CreateDbParameters();
            updateParams.Add("Phone", DbType.String, 0, "Phone");//.Value = "030-0074322"; // simple change, last digit changed from 1 to 2.
            updateParams.Add("CustomerId", DbType.String, 0, "CustomerId");//.Value = "ALFKI";

            AdoTemplate.DataSetUpdate(dataSet, "Customers",
                                      CommandType.Text, insertSql, insertParams,
                                      CommandType.Text, updateSql, updateParams,
                                      CommandType.Text, null , null);

        }
    }
}