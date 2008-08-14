using System;
using System.Collections;
using System.Threading;
using Common.Logging;
using Spring.Messaging.Core;


namespace Spring.MsmqQuickStart.Server.Gateways
{
    public class MarketDataServiceGateway : MessageQueueGatewaySupport, IMarketDataService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (MarketDataServiceGateway));

        private readonly Random random;
        private TimeSpan sleepTimeInSeconds = new TimeSpan(0,0,0,10,0);


        public MarketDataServiceGateway()
        {
            random = new Random();
        }

        public TimeSpan SleepTimeInSeconds
        {
            set { sleepTimeInSeconds = value; }
        }

        public void SendMarketData()
        {
            while (true)
            {
                string data = GenerateFakeMarketData();
                log.Info("Sending market data.");
                MessageQueueTemplate.ConvertAndSend(data);
                log.Info("Sleeping " + sleepTimeInSeconds + " seconds before sending more market data.");
                Thread.Sleep(sleepTimeInSeconds);
            }
        }

        private string GenerateFakeMarketData()
        {
            return "CSCO " + string.Format("{0:##.##}", 22 + Math.Abs(Gaussian()));            
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