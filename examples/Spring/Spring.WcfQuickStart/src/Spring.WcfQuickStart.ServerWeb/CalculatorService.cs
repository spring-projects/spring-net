using System.Threading;

namespace Spring.WcfQuickStart
{
    public class CalculatorService : AbstractCalculatorService
    {
        public override string GetName()
        {
            Thread.Sleep(SleepInSeconds * 1000);
            return "WebApp Calculator";
        }
    }
}