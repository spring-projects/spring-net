using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using NUnit.Framework;
using Spring.Context.Support;

namespace Spring.Remoting
{
    public class BaseRemotingTestFixture
    {
        protected static TcpChannel channel;
        private static object lockObject = new object();

        [OneTimeSetUp]
        public virtual void FixtureSetUp()
        {
            lock (lockObject)
            {
                if (channel == null)
                {
                    try
                    {
                        foreach (IChannel registeredChannel in ChannelServices.RegisteredChannels)
                        {
                            if (registeredChannel is TcpChannel)
                            {
                                ((TcpChannel) registeredChannel).StopListening(null);
                            }
                            ChannelServices.UnregisterChannel(registeredChannel);
                        }

                        channel = new TcpChannel(8005);
                        ChannelServices.RegisterChannel(channel, false);
                    }
                    catch
                    {
                        // ignore duplicate registration exception if it occurs...
                    }
                }
            }
        }

        [TearDown]
        public virtual void TearDown()
        {
            ContextRegistry.Clear();
        }
    }
}