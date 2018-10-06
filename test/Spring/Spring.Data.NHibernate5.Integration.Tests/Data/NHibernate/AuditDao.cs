using System.Data;
using log4net;
using Spring.Data.Core;

namespace Spring.Data.NHibernate
{
    public class AuditDao : AdoDaoSupport, IAuditDao
    {
        protected static readonly ILog logger =
            LogManager.GetLogger(typeof(AuditDao));
        public void AuditOperation(string operationIdenfitier)
        {
            logger.Debug("Executing AUDIT operation.");
            AdoTemplate.ExecuteNonQuery(CommandType.Text,
                                        "insert into AuditTable (AuditId) values (@AuditId)",
                                        "AuditId", DbType.String, 100, operationIdenfitier);
            logger.Debug("AUDIT operation done.");
        }
    }
}