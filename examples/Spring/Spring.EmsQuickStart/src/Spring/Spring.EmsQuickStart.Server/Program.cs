using System;
using System.Threading;
using Spring.Context.Support;
using Spring.EmsQuickStart.Server.Gateways;

namespace Spring.EmsQuickStart.Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Using Spring's IoC container
                ContextRegistry.GetContext(); // Force Spring to load configuration
                Console.Out.WriteLine("Server listening...");
                IMarketDataService marketDataService =
                    ContextRegistry.GetContext().GetObject("MarketDataGateway") as MarketDataServiceGateway;
                ThreadStart job = new ThreadStart(marketDataService.SendMarketData);
                Thread thread = new Thread(job);
                thread.Start();
                Console.Out.WriteLine("--- Press <return> to quit ---");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e);
                Console.Out.WriteLine("--- Press <return> to quit ---");
                Console.ReadLine();
            }
        }
    }
}
