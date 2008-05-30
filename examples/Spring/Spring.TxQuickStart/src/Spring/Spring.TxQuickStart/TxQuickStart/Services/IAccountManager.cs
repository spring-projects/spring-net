
namespace Spring.TxQuickStart.Services
{
    public interface IAccountManager
    {

        void DoTransfer(float creditAmount, float debitAmount);
    }
}
