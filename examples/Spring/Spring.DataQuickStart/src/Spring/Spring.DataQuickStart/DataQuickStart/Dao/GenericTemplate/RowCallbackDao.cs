using System.Collections.Generic;
using System.Data;
using Spring.Data;
using Spring.Data.Generic;

namespace Spring.DataQuickStart.Dao.GenericTemplate
{
    public class RowCallbackDao : AdoDaoSupport
    {
        private string cmdText = "select ContactName, PostalCode from Customers";

        public virtual IDictionary<string, IList<string>> GetPostalCodeCustomerMapping()
        {
            PostalCodeRowCallback statefullCallback = new PostalCodeRowCallback();
            AdoTemplate.QueryWithRowCallback(CommandType.Text, cmdText,
                                             statefullCallback);

            // Do something with results in stateful callback...
            return statefullCallback.PostalCodeMultimap;
        }
      
        
    }

    internal class PostalCodeRowCallback : IRowCallback
    {
        private IDictionary<string, IList<string>> postalCodeMultimap =
            new Dictionary<string, IList<string>>();

        public IDictionary<string, IList<string>> PostalCodeMultimap
        {
            get { return postalCodeMultimap; }
        }

        /// <summary>
        /// Implementations must implement this method to process each row of data
        /// in the data reader.
        /// </summary>
        /// <remarks>
        /// This method should not advance the cursor by calling Read()
        /// on IDataReader but only extract the current values.  The
        /// caller does not need to care about closing the reader, command, connection, or
        /// about handling transactions:  this will all be handled by 
        /// Spring's AdoTemplate
        /// </remarks>
        /// <param name="reader">An active IDataReader instance</param>
        /// <returns>The result object</returns>
        public void ProcessRow(IDataReader reader)
        {
            string contactName = reader.GetString(0);
            string postalCode = reader.GetString(1);
            IList<string> contactNameList;
            if (postalCodeMultimap.ContainsKey(postalCode))
            {
                contactNameList = postalCodeMultimap[postalCode];
            }
            else
            {
                postalCodeMultimap.Add(postalCode, contactNameList = new List<string>());
            }
            contactNameList.Add(contactName);
        }
    }
}