using System;
using System.Data;
using Spring.Data.Core;

namespace Spring.TxQuickStart.Dao.Ado
{
    public class AccountDebitDao : AdoDaoSupport, IAccountDebitDao
    {
        public void DebitAccount(float debitAmount)
        {
            AdoTemplate.ExecuteNonQuery(CommandType.Text,
                                       "insert into dbo.Debits (DebitAmount) VALUES (@amount)", "amount", DbType.Decimal, 0,
                                       debitAmount);
        }
    }
}
