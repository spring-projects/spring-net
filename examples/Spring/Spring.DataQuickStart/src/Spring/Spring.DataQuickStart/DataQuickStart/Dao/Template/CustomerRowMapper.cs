#region Licence

/*
 * Copyright 2002-2006 the original author or authors.
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

using System.Data;
using Spring.Data;
using Spring.DataQuickStart.Domain;

namespace Spring.DataQuickStart.Dao.Template
{
    public class CustomerRowMapper : IRowMapper
    {
        /// <summary>
        /// Map a row of data to a Customer object.
        /// </summary>
        /// <remarks>This method should not call Next() on the 
        /// DataReader; it should only extract the values of the current row.
        /// </remarks>
        /// <param name="dataReader">The IDataReader to map</param>
        /// <param name="rowNum">the number of the current row.</param>
        /// <returns>A Customer object.</returns>
        public object MapRow(IDataReader dataReader, int rowNum)
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
        }
    }
}
