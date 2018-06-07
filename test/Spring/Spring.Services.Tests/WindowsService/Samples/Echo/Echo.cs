using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using log4net;
          
namespace Spring.Services.WindowsService.Samples
{
    public class Echo
    {
        private int port = 10;
        private TcpListener tcpListener;
        ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Thread thread;

        public int Port
        {
            set { port = value; }
        }

        public void Stop ()
        {
            if (!tcpListener.Pending())
                tcpListener.Stop();
        }

        public void Start ()
        {
            log.Info("starting echo server");
            tcpListener = new TcpListener (IPAddress.Loopback, port);
            ThreadStart start = new ThreadStart(Run);
            thread = new Thread(start);
            thread.Start();
        }

        private void Run ()
        {
            log.Info("echo server running ");
            tcpListener.Start ();
    
            while (true)
            {
//                if (!tcpListener.Pending())
//                {
//                    Thread.SpinWait(1 / 2 * 1000);
//                    continue;                    
//                }
                Socket socketForClient;
                try
                {
                    socketForClient = tcpListener.AcceptSocket ();
                }
                catch
                {
                    break;
                }

                if (socketForClient.Connected)
                {
                    log.Info ("Client connected");
                    NetworkStream networkStream = new NetworkStream (socketForClient);
                    StreamWriter streamWriter =
                        new StreamWriter (networkStream);

                    StreamReader streamReader =
                        new StreamReader (networkStream);

                    string theString = "Welcome, type something";

                    streamWriter.WriteLine (theString);
                    streamWriter.Flush ();
                    theString = streamReader.ReadLine ();
                    streamWriter.WriteLine ("You typed: " + theString);
                    streamWriter.Flush ();

                    log.Info("From client: " + theString);
                    streamReader.Close ();
                    networkStream.Close ();
                    streamWriter.Close ();
                }
                socketForClient.Close ();
            }
        }
    }
}