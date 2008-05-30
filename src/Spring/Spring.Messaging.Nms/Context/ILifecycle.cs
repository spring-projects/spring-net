
namespace Spring.Context
{
    interface ILifecycle
    {
        void Start();

        void Stop();

        bool IsRunning
        {
            get;
        }
    }
}
