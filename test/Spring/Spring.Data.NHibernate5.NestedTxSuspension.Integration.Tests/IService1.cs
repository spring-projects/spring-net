namespace Spring.Data.NHibernate5.NestedTxSuspension.Integration.Tests
{
    public interface IService1
    {
        void ServiceMethodWithNotSupported1();

        void ServiceMethodWithNotSupported2();

        void ServiceMethodWithNotSupported3();

        void ServiceMethodWithNotSupported4();

        void ServiceMethodWithRequired();
    }
}
