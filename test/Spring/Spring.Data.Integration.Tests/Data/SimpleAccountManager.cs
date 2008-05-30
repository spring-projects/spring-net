
using Spring.Transaction.Interceptor;

namespace Spring.Data
{
    
    public class SimpleAccountManager
    {
        private SimpleAccountCreditDao creditDao;
        private SimpleAccountDebitDao debitDao;

        public SimpleAccountManager()
        {
        }

        public SimpleAccountCreditDao AccountCreditDao
        {
            get { return creditDao; }
            set { creditDao = value; }
        }

        public SimpleAccountDebitDao AccountDebitDao
        {
            get { return debitDao; }
            set { debitDao = value; }
        }

        /// <summary>
        /// Transfer money, performing both credit and debit database operations
        /// within one transaction.
        /// </summary>
        /// <remarks>
        /// The method is marked as virtual so that a transactional AOP proxy
        /// can be created for it.
        /// </remarks>
        /// <param name="creditAmount">the credit amount.</param>
        /// <param name="debitAmount">the debit amount.</param>
        [Transaction()]
        public virtual void DoTransfer(float creditAmount, float debitAmount)
        {
            creditDao.CreateCredit(creditAmount);
            debitDao.DebitAccount(debitAmount);
        }

    }


}
