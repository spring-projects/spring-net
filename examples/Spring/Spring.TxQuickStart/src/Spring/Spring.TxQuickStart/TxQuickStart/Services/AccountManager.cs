
using System;
using System.Data;
using Spring.Transaction;
using Spring.Transaction.Interceptor;
using Spring.TxQuickStart.Dao;

namespace Spring.TxQuickStart.Services
{
    public class AccountManager : IAccountManager
    {
        
        private IAccountCreditDao accountCreditDao;
        private IAccountDebitDao accountDebitDao;

        private float maxTransferAmount = 1000000;

        public AccountManager(IAccountCreditDao accountCreditDao, IAccountDebitDao accountDebitDao)
        {
            this.accountCreditDao = accountCreditDao;
            this.accountDebitDao = accountDebitDao;
        }

        public float MaxTransferAmount
        {
            get { return maxTransferAmount; }
            set { maxTransferAmount = value; }
        }


        // The following rollback rule will result in commiting of only work done
        // by the CreateCreate DAO method.  The exception is still propagated out to the
        // calling code.

        // [Transaction(NoRollbackFor = new Type[] { typeof(ArithmeticException) })]

        [Transaction(TransactionPropagation.Required)]
        public void DoTransfer(float creditAmount, float debitAmount)
        {
            accountCreditDao.CreateCredit(creditAmount);

            if (creditAmount > maxTransferAmount || debitAmount > maxTransferAmount)
            {
                throw new ArithmeticException("see a teller big spender...");
            }
           
            accountDebitDao.DebitAccount(debitAmount);
        }
        
    }


}
