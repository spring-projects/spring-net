using System;
using System.Data;
using Spring.Data.Core;

namespace Spring.TxQuickStart.Dao.Ado
{

    public class AccountCreditDao : AdoDaoSupport, IAccountCreditDao
    {
        public void CreateCredit(float creditAmount)
        {
            AdoTemplate.ExecuteNonQuery(CommandType.Text,
                                        "insert into Credits (CreditAmount) VALUES (@amount)", "amount", DbType.Decimal, 0,
                                        creditAmount);
        }
    }
}
