namespace Spring.Data.NHibernate.TxPromotion.Integration.Tests
{
    public interface IService2
    {
        void ServiceMethodWithNotSupported();

        void ServiceMethodWithRequiresNew();
    }
}
