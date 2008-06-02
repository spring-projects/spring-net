using System.Data;
using Spring.Transaction.Support;

namespace Spring.Data.Support
{
	/// <summary>
	/// Connection holder, wrapping a ADO.NET connection and transaction.
	/// </summary>
	/// <remarks>AdoTransactionManager binds instances of this class to the
	/// thread for a given DbProvider.</remarks>
	/// <author>Jurgen Hoeller</author>
	/// <author>Mark Pollack (.NET)</author>
	public class ConnectionHolder : ResourceHolderSupport
	{
        private IDbConnection currentConnection;

        private IDbTransaction currentTransaction;

	    private bool transactionActive = false;

        /// <summary>
        /// Create a new ConnectionHolder
        /// </summary>
        /// <param name="conn">The connection to hold</param>
        /// <param name="transaction">The transaction to hold</param>
		public ConnectionHolder(IDbConnection conn, IDbTransaction transaction)
		{
            //TODO assert conn is not null.
            currentConnection = conn;
            currentTransaction = transaction;
		}

        public IDbConnection Connection
        {
            get
            {
                return currentConnection;
            }
            set
            {
                currentConnection = value;
            }

        }

	    public IDbTransaction Transaction
	    {
	        get { return currentTransaction; }
	        set { currentTransaction = value; }
	    }

	    public bool HasConnection
        {
            get
            {
                return (currentConnection != null);
            }
        }

        public bool TransactionActive
        {
            get { return transactionActive; }
            set { transactionActive = value; }
        }

        public override void Clear()
        {
            base.Clear();
            transactionActive = false;
        }
	}
}
