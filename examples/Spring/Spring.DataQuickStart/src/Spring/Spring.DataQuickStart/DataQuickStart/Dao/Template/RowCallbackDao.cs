using System.Collections;
using System.Data;
using Spring.Data;
using Spring.Data.Core;

namespace Spring.DataQuickStart.Dao.Template
{
    public class RowCallbackDao : AdoDaoSupport
    {
        private string cmdText = "select ContactName, PostalCode from Customers";

        public virtual IDictionary GetPostalCodeCustomerMapping()
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
        private IDictionary postalCodeMultimap = new Hashtable();

        public IDictionary PostalCodeMultimap
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
            IList contactNameList;
            if (postalCodeMultimap.Contains(postalCode))
            {
                contactNameList = postalCodeMultimap[postalCode] as IList;
            }
            else
            {
                postalCodeMultimap.Add(postalCode, contactNameList = new ArrayList());
            }
            contactNameList.Add(contactName);
        }
    }
}