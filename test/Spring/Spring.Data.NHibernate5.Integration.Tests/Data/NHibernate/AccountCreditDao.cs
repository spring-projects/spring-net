using Spring.Data.NHibernate.Support;

namespace Spring.Data.NHibernate
{
    public class AccountCreditDao : HibernateDaoSupport, IAccountCreditDao
    {
        public void CreateCredit(float creditAmount)
        {
            Credit c = new Credit();
            c.Amount = creditAmount;
            HibernateTemplate.Save(c);
        }
    }
}