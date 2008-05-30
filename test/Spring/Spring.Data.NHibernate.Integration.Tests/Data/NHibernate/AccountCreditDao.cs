using System;
using Spring.Data;
using Spring.Data.NHibernate.Support;

namespace Spring.Data.NHibernate
{
    public class AccountCreditDao : HibernateDaoSupport, IAccountCreditDao
    {
       
        public AccountCreditDao()
        {
        }

        public void CreateCredit(float creditAmount)
        {
            Credit c = new Credit();
            c.Amount = creditAmount;
            HibernateTemplate.SaveOrUpdate(c);
        }

    }
}
