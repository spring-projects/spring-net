using System.Data;
using Microsoft.Extensions.Logging;

namespace Spring.Data.Support;

/// <summary>
/// Summary description for AdoUtils.
/// </summary>
public abstract class AdoUtils
{
    private static readonly ILogger LOG = LogManager.GetLogger(typeof(AdoUtils));

    /// <summary>
    /// Order value for TransactionSynchronization objects that clean up
    /// ADO.NET Connections.
    /// </summary>
    public static readonly int CONNECTION_SYNCHRONIZATION_ORDER = 1000;

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
                LOG.LogWarning(e, "Could not close IDataRader");
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
            LOG.LogWarning(e, "Could not dispose of command");
        }
    }
}
