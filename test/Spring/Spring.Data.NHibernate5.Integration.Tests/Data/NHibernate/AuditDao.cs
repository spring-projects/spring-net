using System.Data;
using Microsoft.Extensions.Logging;
using Spring.Data.Core;

namespace Spring.Data.NHibernate
{
    public class AuditDao : AdoDaoSupport, IAuditDao
    {
        protected static readonly ILog logger =
            LogManager.GetLogger(typeof(AuditDao));
        public void AuditOperation(string operationIdenfitier)
        {
            logger.LogDebug("Executing AUDIT operation.");
            AdoTemplate.ExecuteNonQuery(CommandType.Text,
                                        "insert into AuditTable (AuditId) values (@AuditId)",
                                        "AuditId", DbType.String, 100, operationIdenfitier);
            logger.LogDebug("AUDIT operation done.");
        }
    }
}