using System.Collections;
using Spring.Data.Common;
using Spring.Data.Objects;

namespace Spring.DataQuickStart.Dao.Template
{
    public class CustOrdersDetailStoredProc : StoredProcedure
    {
        private static string procedureName = "CustOrdersDetail";

        public CustOrdersDetailStoredProc(IDbProvider dbProvider) : base(dbProvider, procedureName)
        {           
            DeriveParameters();
            AddRowMapper("orderDetailRowMapper", new OrderDetailRowMapper() );
            Compile();
        }
        
        public virtual IList GetOrderDetails(int orderid)
        {
            
            IDictionary outParams = Query(orderid);
            return outParams["orderDetailRowMapper"] as IList;
        }

    }
}
