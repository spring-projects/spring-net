#region License

/*
 * Copyright © 2002-2006 the original author or authors.
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

using System.Collections;
using System.Data;
using Spring.Data;
using Spring.Data.Common;
using Spring.Data.Core;

namespace Spring.DataQuickStart.Dao.Template
{
    /// <summary>
    /// A simple DAO that uses Generic.AdoTemplate ResultSetExtractor functionality
    /// </summary>
    public class ResultSetExtractorDao : AdoDaoSupport
    {
        
        private string postalCodeCustomerMappingCommandText = "select ContactName, PostalCode from Customers";

        private string customerByCountry =
            @"select ContactName from Customers where Country = @Country";
        
        private string customerByCountryAndCityCommandText = 
                @"select ContactName from Customers where City = @City and Country = @Country";
        
        

        /// <summary>
        /// Gets a 'multi-map' of postal code, customer names.
        /// </summary>
        /// <returns>A multi-map of where the key is a postal code and the 
        /// value a list of customer names</returns>
        public virtual IDictionary GetPostalCodeCustomerMapping()
        {
            return (IDictionary)AdoTemplate.QueryWithResultSetExtractor(CommandType.Text, postalCodeCustomerMappingCommandText,
                          new PostalDictionaryResultSetExtractor()
                );
        }

        
        public virtual IList GetCustomerNameByCountry(string country)
        {
            return (IList)AdoTemplate.QueryWithResultSetExtractor(CommandType.Text,
                                                           customerByCountry,
                                                           new CustomerNameResultSetExtractor(),
                                                           "Country", DbType.String, 15, "Brazil");
                                                                       
        }
        
        public virtual IList GetCustomerNameByCountryAndCity(string country, string city)
        {
            // note no need to use parameter prefix.
            
            // This allows the SQL to be changed via external configuration but the parameter setting code 
            // can remain the same if no provider specific DbType enumerations are used.
            
            IDbParameters parameters = CreateDbParameters();
            parameters.AddWithValue("Country", country).DbType = DbType.String;
            parameters.Add("City", DbType.String).Value = city;
            return (IList)AdoTemplate.QueryWithResultSetExtractor(CommandType.Text,
                                                           customerByCountryAndCityCommandText,
                                                           new CustomerNameResultSetExtractor(),
                                                           parameters);
        }

        public virtual IList GetCustomerNameByCountryAndCityWithParamsBuilder(string country, string city)
        {
            // note no need to use parameter prefix.

            // This allows the SQL to be changed via external configuration but the parameter setting code 
            // can remain the same if no provider specific DbType enumerations are used.


            IDbParametersBuilder builder = CreateDbParametersBuilder();
            builder.Create().Name("Country").Type(DbType.String).Size(15).Value(country);
            builder.Create().Name("City").Type(DbType.String).Size(15).Value(city);
            return (IList)AdoTemplate.QueryWithResultSetExtractor(CommandType.Text,
                                                           customerByCountryAndCityCommandText,
                                                           new CustomerNameResultSetExtractor(),
                                                           builder.GetParameters());
        }
        
       

        public virtual IList GetCustomerNameByCountryAndCityWithCommandSetter(string country, string city)
        {
            return (IList)AdoTemplate.QueryWithResultSetExtractor(CommandType.Text,
                                                           customerByCountryAndCityCommandText,
                                                           new CustomerNameResultSetExtractor(),
                                                           new CustomerByCountryCityCommandSetter(country, city));
        }
        
        /// <summary>
        /// Gets or sets the command text used to get a multi-map of postal codes/customer names.
        /// </summary>
        /// <remarks>Can be set in configuration to use vendor specific SQL.</remarks>
        /// <value>The command text.</value>
        public string PostalCodeCustomerMappingCommandText
        {
            get { return postalCodeCustomerMappingCommandText; }
            set { postalCodeCustomerMappingCommandText = value; }
        }
    }

    internal class CustomerByCountryCityCommandSetter : ICommandSetter
    {
        private string country, city;
        
        public CustomerByCountryCityCommandSetter(string country, string city)
        {
            this.country = country;
            this.city = city;
        }

        public void SetValues(IDbCommand dbCommand)
        {
            
            //Down cast to SqlCommand or other provider to access provider specific functionality....
            //In this case just use the general IDbCommand API.
            IDbDataParameter countryParameter = dbCommand.CreateParameter();
            countryParameter.ParameterName = "@Country";
            countryParameter.DbType = DbType.String;
            countryParameter.Value = country;
            
            IDbDataParameter cityParameter = dbCommand.CreateParameter();
            cityParameter.ParameterName = "@City";
            cityParameter.DbType = DbType.String;
            cityParameter.Value = city;

            dbCommand.Parameters.Add(countryParameter);
            dbCommand.Parameters.Add(cityParameter);
        }
    }

    internal class CustomerNameResultSetExtractor : IResultSetExtractor
    {
        /// <summary>
        /// Implementations must implement this method to process all
        /// result set and rows in the IDataReader.
        /// </summary>
        /// <param name="reader">The IDataReader to extract data from.
        /// Implementations should not close this: it will be closed
        /// by the AdoTemplate.</param>
        /// <returns>An arbitrary result object or null if none.  The
        /// extractor will typically be stateful in the latter case.</returns>
        public object ExtractData(IDataReader reader)
        {
            ArrayList customerList = new ArrayList();
            while (reader.Read())
            {
                string contactName = reader.GetString(0);
                customerList.Add(contactName);
            }
            return customerList;
        }
    }

    internal class PostalDictionaryResultSetExtractor : IResultSetExtractor
    {
        
        /// <summary>
        /// Implementations must implement this method to process all
        /// result set and rows in the IDataReader.
        /// </summary>
        /// <param name="reader">The IDataReader to extract data from.
        /// Implementations should not close this: it will be closed
        /// by the AdoTemplate.</param>
        /// <returns>An arbitrary result object or null if none.  The
        /// xtractor will typically be stateful in the latter case.</returns>
        public object ExtractData(IDataReader reader)
        {
            IDictionary resultDictionary = new Hashtable();
                while(reader.Read())
                {   
                    string contactName = reader.GetString(0);
                    string postalCode = reader.GetString(1);
                    IList contactNameList;
                    if (resultDictionary.Contains(postalCode))
                    {
                        contactNameList = resultDictionary[postalCode] as IList;
                    }
                    else
                    {
                        resultDictionary.Add(postalCode, contactNameList = new ArrayList());                    
                    }
                    contactNameList.Add(contactName);
                }	                
                return resultDictionary;
            
        }
    }
}