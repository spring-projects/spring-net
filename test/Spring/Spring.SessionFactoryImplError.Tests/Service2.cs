using Spring.Data.NHibernate.Generic;
using Spring.Transaction;

namespace Spring.SessionFactoryImplError.Tests
{
    [Transaction.Interceptor.Transaction(TransactionPropagation.Supports, ReadOnly = true)]
    public class Service2 : IService2
    {
        #region DI

        public HibernateTemplate HibernateTemplate { get; set; }

        #endregion

        [Transaction.Interceptor.Transaction(TransactionPropagation.NotSupported)]
        public virtual void ServiceMethod1()
        {
            ServiceMethod2();
        }

        [Transaction.Interceptor.Transaction(TransactionPropagation.RequiresNew)]
        public virtual void ServiceMethod2()
        {
            // do stuff
        }
    }
}
