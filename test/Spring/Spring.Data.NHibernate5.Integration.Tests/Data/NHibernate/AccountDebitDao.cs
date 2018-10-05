using Spring.Data.NHibernate.Support;

namespace Spring.Data.NHibernate
{
    public class AccountDebitDao : HibernateDaoSupport, IAccountDebitDao
    {
        public void DebitAccount(float debitAmount)
        {
            Debit d = new Debit();
            d.Amount = debitAmount;
            HibernateTemplate.Save(d);
        }
    }
}