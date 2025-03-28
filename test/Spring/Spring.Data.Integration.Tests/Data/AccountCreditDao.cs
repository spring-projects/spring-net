using System.Data;
using Spring.Data.Core;
using Spring.Transaction.Interceptor;

namespace Spring.Data;

public class AccountCreditDao : AdoDaoSupport, IAccountCreditDao
{
    public AccountCreditDao()
    {
    }

    [Transaction()]
    public void CreateCredit(float creditAmount)
    {
        AdoTemplate.ExecuteNonQuery(CommandType.Text,
            String.Format("insert into Credits (CreditAmount) VALUES ({0})",
                creditAmount));
    }
}
