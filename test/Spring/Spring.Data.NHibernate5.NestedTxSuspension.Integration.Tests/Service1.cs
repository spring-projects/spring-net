using Spring.Data.NHibernate.Generic;
using Spring.Transaction;

namespace Spring.Data.NHibernate5.NestedTxSuspension.Integration.Tests
{
    [Transaction.Interceptor.Transaction(TransactionPropagation.Supports, ReadOnly = true)]
    public class Service1 : IService1
    {
        #region DI

        public HibernateTemplate HibernateTemplate { get; set; }

        #endregion

        [Transaction.Interceptor.Transaction(TransactionPropagation.NotSupported)]
        public virtual void ServiceMethodWithNotSupported1()
        {
            ServiceMethodWithNotSupported2();
        }

        [Transaction.Interceptor.Transaction(TransactionPropagation.NotSupported)]
        public virtual void ServiceMethodWithNotSupported2()
        {
            // do some stuff
        }

        [Transaction.Interceptor.Transaction(TransactionPropagation.NotSupported)]
        public virtual void ServiceMethodWithNotSupported3()
        {
            ServiceMethodWithNotSupported4();
        }

        [Transaction.Interceptor.Transaction(TransactionPropagation.NotSupported)]
        public virtual void ServiceMethodWithNotSupported4()
        {
            ServiceMethodWithRequired();
        }

        [Transaction.Interceptor.Transaction(TransactionPropagation.Required)]
        public virtual void ServiceMethodWithRequired()
        {
            // do some stuff
        }
    }
}
