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

using System.Data;
using Spring.Data;
using Spring.Data.Core;

namespace Spring.DataQuickStart.Dao.Template
{
    /// <summary>
    /// A simple DAO that uses Generic.AdoTemplate CommandCallback functionality
    /// </summary>
    public class CommandCallbackDao : AdoDaoSupport
    {

       
        /// <summary>
        /// Finds the number of customers with the given postal code.
        /// </summary>
        /// <param name="postalCode">The postal code.</param>
        /// <returns>Number of customers with the given postal code.</returns>       
        public virtual int FindCountWithPostalCode(string postalCode)
        {
            return (int) AdoTemplate.Execute(new PostalCodeCommandCallback(postalCode));
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
        public class PostalCodeCommandCallback : ICommandCallback
        {
            private string cmdText = "select count(*) from Customers where PostalCode = @PostalCode";

            private string postalCode;
            
            public PostalCodeCommandCallback(string postalCode)
            {
                this.postalCode = postalCode;
            }

            public object DoInCommand(IDbCommand command)
            {
                command.CommandText = cmdText;
                
                IDbDataParameter p = command.CreateParameter();
                p.ParameterName = "@PostalCode";
                p.Value = postalCode;
                command.Parameters.Add(p);
                
                return command.ExecuteScalar();

            }


        }

    }

}