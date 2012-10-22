#region Licence

/*
 * Copyright © 2002-2011 the original author or authors.
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

using System;
using System.Data;
using System.Data.SqlClient;

#endregion

namespace Spring.Data.Northwind
{
    /// <author>Mark Pollack (.NET)</author>
    public class NativeAdoShipperDao : IShipperDao
    {
        #region IShipperDao Members

        public Shipper Create(string name, string phone)
        {
            string connectionString = "Data Source=SPRINGQA;Initial Catalog=Northwind;Persist Security Info=True;User ID=springqa;Password=springqa";
            int id = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand insertCommand = connection.CreateCommand())
                {
                    insertCommand.CommandText =
                        "INSERT INTO Shippers (CompanyName, Phone) VALUES (@CompanyName, @Phone) SET @ShipperID = SCOPE_IDENTITY()";
                    SqlParameter companyNameParameter = new SqlParameter("@CompanyName", SqlDbType.NVarChar, 40);
                    companyNameParameter.Value = name;
                    insertCommand.Parameters.Add(companyNameParameter);
                    SqlParameter phoneParameter = new SqlParameter("@Phone", SqlDbType.NVarChar, 24);
                    if (phone.Length == 0)
                        phoneParameter.Value = DBNull.Value;
                    else
                        phoneParameter.Value = phone;
                    insertCommand.Parameters.Add(phoneParameter);
                    SqlParameter shipperIDParameter = new SqlParameter("@ShipperID", SqlDbType.Int);
                    shipperIDParameter.Direction = ParameterDirection.Output;
                    insertCommand.Parameters.Add(shipperIDParameter);
                    insertCommand.Connection.Open();
                    insertCommand.ExecuteNonQuery();
                    id = (int) shipperIDParameter.Value;
                }
            }
            return new Shipper(id, name, phone); //24
        }

        
        public Shipper CreateShorter(string name, string phone)
        {
            string connectionString = "Data Source=SPRINGQA;Initial Catalog=Northwind;Persist Security Info=True;User ID=springqa";
            int id = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand insertCommand = connection.CreateCommand())
                {
                    insertCommand.CommandText =
                        "INSERT INTO Shippers (CompanyName, Phone) VALUES (@CompanyName, @Phone) SET @ShipperID = SCOPE_IDENTITY()";
                    insertCommand.Parameters.Add("@CompanyName", SqlDbType.NVarChar, 40).Value = name;
                    if (phone.Length == 0)
                        insertCommand.Parameters.Add("Phone", SqlDbType.NVarChar, 24).Value = DBNull.Value;
                    else
                        insertCommand.Parameters.Add("Phone", SqlDbType.NVarChar, 24).Value = phone;                               
                    insertCommand.Parameters.Add("@ShipperID", SqlDbType.Int).Direction = ParameterDirection.Output;
                    insertCommand.Connection.Open();
                    insertCommand.ExecuteNonQuery();
                    id = (int) insertCommand.Parameters["@ShipperID"].Value;
                }
            }
            return new Shipper(id, name, phone); //17
        }

        #endregion
    }
}