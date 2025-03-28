namespace Spring.Data;

public interface IAccountManager
{
    void DoTransfer(float creditAmount, float debitAmount);
}