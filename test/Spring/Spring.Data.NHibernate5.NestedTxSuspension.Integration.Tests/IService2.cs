namespace Spring.Data.NHibernate5.NestedTxSuspension.Integration.Tests
{
    public interface IService2
    {
        void ServiceMethodWithNotSupported();

        void ServiceMethodWithRequiresNew();
    }
}
