namespace Spring.Data.NHibernate4.NestedTxSuspension.Integration.Tests
{
    public interface IService2
    {
        void ServiceMethodWithNotSupported();

        void ServiceMethodWithRequiresNew();
    }
}
