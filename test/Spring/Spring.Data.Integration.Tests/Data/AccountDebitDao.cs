using System;
using System.Data;
using Spring.Data.Core;
using Spring.Data.Support;

namespace Spring.Data
{
    public class AccountDebitDao : AdoDaoSupport, IAccountDebitDao
    {

        public AccountDebitDao()
        {
        }

        public void DebitAccount(float debitAmount)
        {
            AdoTemplate.ExecuteNonQuery(CommandType.Text, String.Format("insert into dbo.Debits (DebitAmount) VALUES ({0})",
                                                                        debitAmount));
        }


    }
}
