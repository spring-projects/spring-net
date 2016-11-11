using Spring.Transaction.Interceptor;

namespace Spring.Data
{
    public class AccountManager : IAccountManager
    {
        private IAccountCreditDao creditDao;
        private IAccountDebitDao debitDao;

        public AccountManager()
        {
        }

        public IAccountCreditDao AccountCreditDao
        {
            get { return creditDao; }
            set { creditDao = value; }
        }

        public IAccountDebitDao AccountDebitDao
        {
            get { return debitDao; }
            set { debitDao = value; }
        }

        [Transaction]
        public void DoTransfer(float creditAmount, float debitAmount)
        {
            creditDao.CreateCredit(creditAmount);
            debitDao.DebitAccount(debitAmount);
 
        }

    }


}
