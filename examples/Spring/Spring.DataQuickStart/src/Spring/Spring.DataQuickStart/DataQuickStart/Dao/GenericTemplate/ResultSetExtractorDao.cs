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

using System.Collections.Generic;
using System.Data;
using Spring.Data;
using Spring.Data.Common;
using Spring.Data.Generic;

namespace Spring.DataQuickStart.Dao.GenericTemplate
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
        public virtual IDictionary<string, IList<string>> GetPostalCodeCustomerMapping()
        {
            return AdoTemplate.QueryWithResultSetExtractor(CommandType.Text, postalCodeCustomerMappingCommandText,
                          new PostalDictionaryResultSetExtractor<Dictionary<string, IList<string>>>()
                );
        }

        
        public virtual IList<string> GetCustomerNameByCountry(string country)
        {
            return AdoTemplate.QueryWithResultSetExtractor(CommandType.Text,
                                                           customerByCountry,
                                                           new CustomerNameResultSetExtractor<List<string>>(),
                                                           "Country", DbType.String, 15, "Brazil");
                                                                       
        }
        
        public virtual IList<string> GetCustomerNameByCountryAndCity(string country, string city)
        {
            // note no need to use parameter prefix.
            
            // This allows the SQL to be changed via external configuration but the parameter setting code 
            // can remain the same if no provider specific DbType enumerations are used.
            
            IDbParameters parameters = CreateDbParameters();
            parameters.AddWithValue("Country", country).DbType = DbType.String;
            parameters.Add("City", DbType.String).Value = city;
            return AdoTemplate.QueryWithResultSetExtractor(CommandType.Text,
                                                           customerByCountryAndCityCommandText,
                                                           new CustomerNameResultSetExtractor<List<string>>(),
                                                           parameters);
        }

        public virtual IList<string> GetCustomerNameByCountryAndCityWithParamsBuilder(string country, string city)
        {
            // note no need to use parameter prefix.

            // This allows the SQL to be changed via external configuration but the parameter setting code 
            // can remain the same if no provider specific DbType enumerations are used.


            IDbParametersBuilder builder = CreateDbParametersBuilder();
            builder.Create().Name("Country").Type(DbType.String).Size(15).Value(country);
            builder.Create().Name("City").Type(DbType.String).Size(15).Value(city);
            return AdoTemplate.QueryWithResultSetExtractor(CommandType.Text,
                                                           customerByCountryAndCityCommandText,
                                                           new CustomerNameResultSetExtractor<List<string>>(),
                                                           builder.GetParameters());
        }
        
        public virtual IList<string> GetCustomerNameByCountryAndCityWithCommandSetterDelegate(string country, string city)
        {
            //using anonymous delegate allows for easy access to 'outer' calling parameter values.
            //but is not reusable across objects/methods as with the interface based ICommandSetter alternative.
            
            return AdoTemplate.QueryWithResultSetExtractor(CommandType.Text,
                                                           customerByCountryAndCityCommandText,
                                                           new CustomerNameResultSetExtractor<List<string>>(),
                                                           delegate(IDbCommand dbCommand)
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
                                                               });
        }

        public virtual IList<string> GetCustomerNameByCountryAndCityWithAllDelegates(string country, string city)
        {
            IDbParametersBuilder builder = CreateDbParametersBuilder();
            builder.Create().Name("Country").Type(DbType.String).Size(15).Value(country);
            builder.Create().Name("City").Type(DbType.String).Size(15).Value(city);

            ResultSetExtractorDelegate<IList<string>> extractorDelegate = delegate(IDataReader reader)
                                                                {
                                                                    IList<string> customerList = new List<string>();
                                                                    while (reader.Read())
                                                                    {
                                                                        string contactName = reader.GetString(0);
                                                                        customerList.Add(contactName);
                                                                    }
                                                                    return customerList;
                                                                };
            CommandSetterDelegate commandSetterDelegate = delegate(IDbCommand dbCommand)
                                                              {
                                                                  //Down cast to SqlCommand or other provider to access provider specific functionality....
                                                                  //In this case just use the general IDbCommand API.
                                                                  IDbDataParameter countryParameter =
                                                                      dbCommand.CreateParameter();
                                                                  countryParameter.ParameterName = "@Country";
                                                                  countryParameter.DbType = DbType.String;
                                                                  countryParameter.Value = country;

                                                                  IDbDataParameter cityParameter =
                                                                      dbCommand.CreateParameter();
                                                                  cityParameter.ParameterName = "@City";
                                                                  cityParameter.DbType = DbType.String;
                                                                  cityParameter.Value = city;

                                                                  dbCommand.Parameters.Add(countryParameter);
                                                                  dbCommand.Parameters.Add(cityParameter);
                                                              };
            
            
            //TODO compiler can't determine matching method if two anonymous methods are used within method parameters.
            //     how to 'cast' the anonymous delegate?
            return AdoTemplate.QueryWithResultSetExtractorDelegate(CommandType.Text,
                                               customerByCountryAndCityCommandText,
                                               extractorDelegate,
                                               commandSetterDelegate);
        }
        
        
        public virtual IList<string> GetCustomerNameByCountryAndCityWithCommandSetter(string country, string city)
        {
            return AdoTemplate.QueryWithResultSetExtractor(CommandType.Text,
                                                           customerByCountryAndCityCommandText,
                                                           new CustomerNameResultSetExtractor<List<string>>(),
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

    internal class CustomerNameResultSetExtractor<T> : IResultSetExtractor<T> where T : IList<string>, new()
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
        public T ExtractData(IDataReader reader)
        {
            T customerList = new T();
            while (reader.Read())
            {
                string contactName = reader.GetString(0);
                customerList.Add(contactName);
            }
            return customerList;
        }
    }

    internal class PostalDictionaryResultSetExtractor<T> : IResultSetExtractor<T> where T : IDictionary<string, IList<string>>, new()
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
        public T ExtractData(IDataReader reader)
        {
                T resultDictionary = new T();
                while(reader.Read())
                {   
                    string contactName = reader.GetString(0);
                    string postalCode = reader.GetString(1);
                    IList<string> contactNameList;
                    if (resultDictionary.ContainsKey(postalCode))
                    {
                        contactNameList = resultDictionary[postalCode];
                    }
                    else
                    {
                        resultDictionary.Add(postalCode, contactNameList = new List<string>());                    
                    }
                    contactNameList.Add(contactName);
                }	                
                return resultDictionary;
            
        }
    }
}