#region License
/*
* Copyright 2002-2010 the original author or authors.
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* 
*      http://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
#endregion

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

using NUnit.Framework;

using Spring.Threading;

namespace Spring.Services.WindowsService
{
    [TestFixture]	
    public class IntegrationTest
	{
        public class SyncedDeployer : IDeployer
        {
            private readonly ISync sync;
            IDeployer helper = new DefaultDeployer(new SeparateAppDomainHostManager());
            private Exception ex;

            public SyncedDeployer(ISync sync)
            {
                this.sync = sync;
            }

            public void Deploy(IApplication application)
            {
                try
                {
                    helper.Deploy(application);
                }
                catch (Exception e)
                {
                    ex = e;
                    sync.Release();
                }
                sync.Release();
            }

            public void UnDeploy(IApplication application)
            {
                helper.UnDeploy(application);
            }

            public void Dispose()
            {
                helper.Dispose();
            }

            public Exception Ex
            {
                get { return ex; }
            }
        }

        DeployManager deployManager;
        ISpringAssembliesDeployer springAssembliesDeployer;
        FileSystemDeployLocation location;
        SyncedDeployer defaultDeployer;
        string deployPath = String.Format("deploy-{0}", Guid.NewGuid());
        string baseOfConfigFiles = "Data/Spring/WindowsService";
        Latch sync;

        [SetUp]
        public void SetUp ()
        {
            springAssembliesDeployer = new SpringAssembliesDeployer(".");
            location = new FileSystemDeployLocation(deployPath);
            location.StartWatching();
            sync = new Latch();
            defaultDeployer = new SyncedDeployer(sync);
            deployManager = new DeployManager(springAssembliesDeployer, location, defaultDeployer);

            deployManager.Start();
        }

        [TearDown]
        public void TearDown ()
        {
            if (defaultDeployer.Ex != null)
            {
                Console.Out.WriteLine("defaultDeployer exception = {0}", defaultDeployer.Ex);
            }
            deployManager.Stop();
            location.Dispose();
            TestUtils.SafeDeleteDirectory(deployPath, 25);
        }

        [Test(Description="Any PropertyPlaceholderConfigurer in context will apply")]         
        public void ServiceContextCanSpecifyAPropertyPlaceholderConfigurer ()
        {
            Deploy("Echo");

            // test echo server just deployed
            using (TcpClient client = new TcpClient())
            {
                client.Connect(IPAddress.Loopback, 10);
                using (NetworkStream networkStream = client.GetStream())
                {
                    string guid = Guid.NewGuid().ToString();
                    Receive(networkStream);
                    Send (networkStream, guid);
                    String responseData = Receive (networkStream);
                    Assert.IsTrue(responseData.IndexOf(guid) != -1);
                }
            }            
        }

        [Test()]         
        public void DeployPathAndDeployNameAreAvailableToTheApplication ()
        {
            string where = Deploy ("Simple");

            using (TextReader reader = File.OpenText(Path.Combine(where, "simple.txt")))
            {
                string name = "simple";
                string fullPath = Path.GetFullPath(where).ToLower();
                Assert.AreEqual(name, reader.ReadLine());
                Assert.AreEqual(fullPath, reader.ReadLine().ToLower());
                Assert.AreEqual(String.Format("{0},{1}", name, fullPath), reader.ReadLine().ToLower(), "failed to configure in ctor");
            }
        }

        private string Deploy(string app)
        {
            string src = Path.Combine(baseOfConfigFiles, app);
            string dest = Path.Combine(deployPath, app);
            Directory.CreateDirectory(dest);
            string[] files = Directory.GetFiles(src);
            foreach(string file in files)
            {
                File.Copy(
                    file, 
                    Path.Combine(dest, Path.GetFileName(file)));
            }
            // create new application
            string fileName = Assembly.GetExecutingAssembly().Location;
            File.Copy(fileName, 
                      Path.Combine(dest, Path.GetFileName(fileName)));

            Assert.IsTrue(sync.Attempt(20 * 1000), "Timeout expired");

            return dest;
        }

        private string Receive(Stream stream)
        {
            byte [] data = new byte[1024];
            String responseData = String.Empty;
            Int32 bytes = stream.Read(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            Console.WriteLine("Received: [{0}]", responseData);
            return responseData;
        }

        private void Send(Stream stream, string message)
        {
            byte [] data = System.Text.Encoding.ASCII.GetBytes(message + Environment.NewLine);         
            stream.Write(data, 0, data.Length);
        }
	}
}
