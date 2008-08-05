using System;
using System.Threading;
using Spring.Context.Support;
using Spring.NmsQuickStart.Server.Gateways;

namespace Spring.NmsQuickStart.Server
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
                MarketDataGateway marketDataGateway =
                    ContextRegistry.GetContext().GetObject("MarketDataGateway") as MarketDataGateway;
                ThreadStart job = new ThreadStart(marketDataGateway.SendMarketData);
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
