#region Licence

/*
 * Copyright © 2002-2005 the original author or authors.
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
using Spring.Data.Common;
using Spring.Data.Core;
using Spring.Data.Support;

#endregion

namespace Spring.Data.Northwind
{
	/// <summary>
	/// TODO: 
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	/// <version>$Id: AdoTemplateShipperDao.cs,v 1.3 2007/08/03 19:51:22 markpollack Exp $</version>
	public class AdoTemplateShipperDao : AdoDaoSupport, IShipperDao
    {
        #region IShipperDao Members

        public Shipper Create(string name, string phone)
        {
            string sql = "INSERT INTO Shippers (CompanyName, Phone) VALUES (@CompanyName, @Phone) SET @ShipperID = SCOPE_IDENTITY()";            
            IDbParameters dbParameters = AdoTemplate.CreateDbParameters();
            dbParameters.Add("CompanyName", SqlDbType.NVarChar, 40).Value = name;
            
            //Can we add automatic nullable support?
            if (phone.Length == 0)
                dbParameters.Add("Phone", SqlDbType.NVarChar, 24).Value = DBNull.Value;
            else
                dbParameters.Add("Phone", SqlDbType.NVarChar, 24).Value = phone;
            
            dbParameters.AddOut("ShipperID", SqlDbType.Int);
            
            int id = AdoTemplate.ExecuteNonQuery(CommandType.Text, sql, dbParameters);
            
            return new Shipper(id, name, phone); //10
        }

        #endregion
    }
}
