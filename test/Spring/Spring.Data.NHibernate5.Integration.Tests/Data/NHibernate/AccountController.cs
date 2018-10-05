namespace Spring.Data.NHibernate
{
    public class AccountController : IAccountController
    {
        private IAccountManager accountManager;

        public IAccountManager AccountManager
        {
            get { return accountManager; }
            set { accountManager = value; }
        }

        #region Implementation of IAccountController

        public void DoWork()
        {
            accountManager.DoTransfer(30,30);
        }

        #endregion
    }
}