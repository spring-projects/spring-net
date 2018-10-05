
namespace Spring.Data.NHibernate
{
    public interface IAccountManager
    {
        void DoTransfer(float creditAmount, float debitAmount);
        
        /// <summary>
        /// For testing purposes...
        /// </summary>
        bool ThrowException
        { 
            get; 
            set;
        }
    }
}
