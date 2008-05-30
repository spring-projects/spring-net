using System.Collections;
using System.Collections.Generic;
using Spring.Data.Common;
using Spring.Data.Objects.Generic;
using Spring.DataQuickStart.Domain;

namespace Spring.DataQuickStart.Dao.GenericTemplate
{
    public class CustOrdersDetailStoredProc : StoredProcedure
    {
        private static string procedureName = "CustOrdersDetail";

        public CustOrdersDetailStoredProc(IDbProvider dbProvider) : base(dbProvider, procedureName)
        {           
            DeriveParameters();
            AddRowMapper("orderDetailRowMapper", new OrderDetailRowMapper<OrderDetails>() );
            Compile();
        }
        
        public virtual List<OrderDetails> GetOrderDetails(int orderid)
        {
            
            IDictionary outParams = Query<OrderDetails>(orderid);
            return outParams["orderDetailRowMapper"] as List<OrderDetails>;
        }

    }
}
