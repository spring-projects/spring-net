using System.Data;
using Common.Logging;

namespace Spring.Data.Support
{
	/// <summary>
	/// Summary description for AdoUtils.
	/// </summary>
    public abstract class AdoUtils
    {
        #region Logging

        private static readonly ILog LOG = LogManager.GetLogger(typeof (AdoUtils));

        #endregion

        #region Constants

        /// <summary>
        /// Order value for TransactionSynchronization objects that clean up
        /// ADO.NET Connections.
        /// </summary>
        public static readonly int CONNECTION_SYNCHRONIZATION_ORDER = 1000;

        #endregion
        /// <summary>
        /// Property dispose of the command.  Useful in finally or catch blocks.
        /// </summary>
        /// <param name="command">command to dispose</param>
        public static void DisposeCommand(IDbCommand command)
        {
            if (command != null)
            {
                DoDisposeCommand(command);
            }
        }


	    public static void DisposeDataAdapterCommands(IDbDataAdapter adapter)
        {
            if (adapter.SelectCommand != null)
            {
                DoDisposeCommand(adapter.SelectCommand);
            }
	        if (adapter.InsertCommand != null)
	        {
	            DoDisposeCommand(adapter.InsertCommand);
	        }
            if (adapter.UpdateCommand != null)
            {
                DoDisposeCommand(adapter.UpdateCommand);
            }
	        if (adapter.DeleteCommand != null)
	        {
	            DoDisposeCommand(adapter.DeleteCommand);
	        }

        }
        public static void CloseReader(IDataReader reader)
        {
	        if (reader != null)
	        {
	            try
	            {
	                reader.Close();
	            }
                catch (Exception e)
                {
                    LOG.Warn("Could not close IDataRader", e);
                }
	        }
        }

        private static void DoDisposeCommand(IDbCommand command)
        {
            try
            {
                command.Dispose();
            }
            catch (Exception e)
            {
                LOG.Warn("Could not dispose of command", e);
            }
        }

    }

}
