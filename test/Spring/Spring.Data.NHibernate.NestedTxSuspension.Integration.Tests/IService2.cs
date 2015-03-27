namespace Spring.Data.NHibernate.NestedTxSuspension.Integration.Tests
{
    public interface IService2
    {
        void ServiceMethodWithNotSupported();

        void ServiceMethodWithRequiresNew();
    }
}
