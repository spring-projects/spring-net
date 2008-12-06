using System;
using System.Threading;
using Spring.Context;
using Spring.Context.Support;
using Spring.MsmqQuickStart.Server.Gateways;

namespace Spring.MsmqQuickStart.Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Using Spring's IoC container
                using (IApplicationContext appContext = ContextRegistry.GetContext())
                {
                    Console.Out.WriteLine("Server listening...");
                    IMarketDataService marketDataService = appContext.GetObject("marketDataGateway") as MarketDataServiceGateway;
                    ThreadStart job = new ThreadStart(marketDataService.SendMarketData);
                    Thread thread = new Thread(job);
                    thread.IsBackground = true;
                    thread.Start();
                    Console.Out.WriteLine("--- Press <return> to quit ---");
                    Console.ReadLine();
                }
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
