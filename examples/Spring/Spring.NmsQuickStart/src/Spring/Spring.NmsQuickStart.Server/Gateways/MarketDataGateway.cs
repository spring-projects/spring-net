using System;
using System.Collections;
using System.Threading;
using Common.Logging;
using Spring.Messaging.Nms.Core;

namespace Spring.NmsQuickStart.Server.Gateways
{
    public class MarketDataGateway : NmsGatewaySupport
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (MarketDataGateway));

        private readonly Random random;
        private int sleepTimeInSeconds = 2;


        public MarketDataGateway()
        {
            random = new Random();
        }

        public int SleepTimeInSeconds
        {
            set { sleepTimeInSeconds = value; }
        }

        public void SendMarketData()
        {
            while (true)
            {
                IDictionary data = GenerateFakeMarketData();
                log.Info("Sending market data.");
                NmsTemplate.ConvertAndSend(data);
                log.Info("Sleeping " + sleepTimeInSeconds + " seconds before sending more market data.");
                Thread.Sleep(sleepTimeInSeconds*1000);
            }
        }

        private IDictionary GenerateFakeMarketData()
        {
            IDictionary md = new Hashtable();
            md.Add("TICKER", "CSCO");
            md.Add("PRICE", "22" + string.Format("{0:#.###}", Math.Abs(Gaussian())));
            return md;
        }

        private double Gaussian()
        {
            double x1, x2, w;

            do
            {
                x1 = 2.0*random.NextDouble() - 1.0;
                x2 = 2.0*random.NextDouble() - 1.0;
                w = x1*x1 + x2*x2;
            } while (w >= 1.0);

            w = Math.Sqrt(-2.0*Math.Log(w)/w);

            // two Gaussian random numbers are generated
            return x1*w;
            //y2 = x2 * w; 
        }
    }
}