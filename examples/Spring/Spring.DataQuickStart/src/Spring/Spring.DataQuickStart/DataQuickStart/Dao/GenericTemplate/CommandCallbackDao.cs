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

using System.Data.Common;
using Spring.Data.Generic;

namespace Spring.DataQuickStart.Dao.GenericTemplate
{
    /// <summary>
    /// A simple DAO that uses Generic.AdoTemplate CommandCallback functionality
    /// </summary>
    public class CommandCallbackDao : AdoDaoSupport
    {

        private string cmdText = "select count(*) from Customers where PostalCode = @PostalCode";

        /// <summary>
        /// Finds the number of customers with the given postal code.
        /// </summary>
        /// <param name="postalCode">The postal code.</param>
        /// <returns>Number of customers with the given postal code.</returns>
        public virtual int FindCountWithPostalCodeWithDelegate(string postalCode)
        {
            // Using anonymous delegates allows you to easily reference the
            // surrounding parameters for use with the DbCommand processing.

            return AdoTemplate.Execute<int>(delegate(DbCommand command)
                   {
                       // Do whatever you like with the DbCommand... downcast to get 
                       // provider specific funtionality if necesary.
                                                    
                       command.CommandText = cmdText;
                         
                       DbParameter p = command.CreateParameter();
                       p.ParameterName = "@PostalCode";
                       p.Value = postalCode;
                       command.Parameters.Add(p);

                       return (int)command.ExecuteScalar();

                   });

        }

        
       
        public virtual int FindCountWithPostalCode(string postalCode)
        {
            // Type inference allows you not to explicitly write .Execute<ResultObject>
            
            return AdoTemplate.Execute(new PostalCodeCommandCallback<ResultObject>(postalCode)).count;
        }
        
        // Note that you can not use System.Array, System.Delegate, System.Enum,
        // System.ValueType or object in the constraint.  for this example that returns just
        // an integer, a simple 'ResultObject' type is used.  
        // You code would generally have a return object more complex than an 'int'.
        
        /// <summary>
        /// A callback implementation that returns a 'ResultObject' containing a the count
        /// of customers with a specific zip code. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class PostalCodeCommandCallback<T> : ICommandCallback<T> where T : ResultObject, new()
        {
            private string postalCode;
            public PostalCodeCommandCallback(string postalCode)
            {
                this.postalCode = postalCode;
            }

            public T DoInCommand(DbCommand command)
            {
                T resultObject = new T();
                DbParameter p = command.CreateParameter();
                p.ParameterName = "@PostalCode";
                p.Value = postalCode;
                command.Parameters.Add(p);
                
                resultObject.count = (int)command.ExecuteScalar();
                return resultObject;
            }
        }

    }

    /// <summary>
    /// Helper class for returning simple integer value via generic class.
    /// </summary>
    public class ResultObject
    {
        public int count;
    }
}