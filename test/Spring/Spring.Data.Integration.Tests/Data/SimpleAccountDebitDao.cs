using System;
using System.Data;
using Spring.Data.Core;

namespace Spring.Data
{
    public class SimpleAccountDebitDao : AdoDaoSupport
    {

        public SimpleAccountDebitDao()
        {
        }

        /// <summary>
        /// Insert into the debits table a debit amount
        /// </summary>
        /// <remarks>
        /// Note that this method is not declared as virtual since
        /// the transaction demarcation is done in the SimpleAccountManager
        /// class and does not need to have a transactional proxy created for
        /// it.  If one was not using a higher level class to 
        /// demarcate transactions and instead wanted to demarcate at the DAO
        /// directly, the method would be declared as virtual and the
        /// transaction attributes (or corresponding XML declarations) would
        /// be applied to this method.
        /// </remarks>
        /// <param name="debitAmount">the debit amount</param>
        public void DebitAccount(float debitAmount)
        {
            AdoTemplate.ExecuteNonQuery(CommandType.Text, String.Format("insert into dbo.Debits (DebitAmount) VALUES ({0})",
                                                                        debitAmount));
        }


    }
}
