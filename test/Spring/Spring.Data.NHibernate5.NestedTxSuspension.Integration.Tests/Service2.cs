using Spring.Data.NHibernate.Generic;
using Spring.Transaction;

namespace Spring.Data.NHibernate5.NestedTxSuspension.Integration.Tests
{
    [Transaction.Interceptor.Transaction(TransactionPropagation.Supports, ReadOnly = true)]
    public class Service2 : IService2
    {
        #region DI

        public HibernateTemplate HibernateTemplate { get; set; }

        #endregion

        [Transaction.Interceptor.Transaction(TransactionPropagation.NotSupported)]
        public virtual void ServiceMethodWithNotSupported()
        {
            ServiceMethodWithRequiresNew();
        }

        [Transaction.Interceptor.Transaction(TransactionPropagation.RequiresNew)]
        public virtual void ServiceMethodWithRequiresNew()
        {
            // do stuff
        }
    }
}
