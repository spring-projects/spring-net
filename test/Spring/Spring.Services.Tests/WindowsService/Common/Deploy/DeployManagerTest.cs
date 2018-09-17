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
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using DotNetMock.Dynamic;
using NUnit.Framework;

namespace Spring.Services.WindowsService.Common.Deploy
{
    internal class MyDeploy : IDeployLocation
    {
        public event DeployEventHandler DeployEvent;
        private ArrayList applications = new ArrayList ();
        internal bool wasDisposed = false;

        public void Raise (DeployEventArgs eventArgs)
        {
            if (DeployEvent != null)
                DeployEvent (this, eventArgs);
        }

        public IList Applications
        {
            get { return applications; }
        }

        public void Add (IApplication application)
        {
            Raise (new DeployEventArgs (application, DeployEventType.ApplicationAdded));
        }

        public void Remove (IApplication application)
        {
            Raise (new DeployEventArgs (application, DeployEventType.ApplicationRemoved));
        }

        public void Update (IApplication application)
        {
            Raise(new DeployEventArgs(application, DeployEventType.ApplicationUpdated));
        }

        public void Dispose ()
        {
            wasDisposed = true;
        }
    }

    [TestFixture]
    public class DeployManagerTest
    {
        MyDeploy deployLocation;
        IDeployer deployer;
        DeployManager deployManager;
        IDynamicMock deployerMock;
        bool needVerifyDeployer = true;

        class NullSpringDeployer : ISpringAssembliesDeployer
        {
            public void DeployForApplication (IApplication application)
            {
            }
        }

        [SetUp]
        public void SetUp ()
        {
            deployerMock = new DynamicMock(typeof(IDeployer));
            deployerMock.Strict = false;
            deployer = (IDeployer) deployerMock.Object;
            deployLocation = new MyDeploy ();
            deployManager = new DeployManager (new DirectExecutor(), new NullSpringDeployer(), deployLocation, deployer);
        }

        [TearDown]
        public void TearDown ()
        {
            if (needVerifyDeployer)
                deployerMock.Verify();            
        }

        [Test]
        public void ItIsOKToStopWhenNotStarted ()
        {
            needVerifyDeployer = false;
            deployManager.Stop();
        }

        [Test]
        public void WhenStartedDeploysExisitingApplications ()
        {
            foreach (IApplication application in deployManager.DeployLocation.Applications)
            {
                Assert.IsFalse (deployManager.IsDeployed (application), "application has not been deployed");
            }
            deployLocation.Applications.Add (new Application ("testApp"));
            deployManager.Start();
            foreach (IApplication application in deployLocation.Applications)
            {
                Assert.IsTrue (deployManager.IsDeployed (application), "application has not been deployed");
            }
            DeployInformation[] infos = deployManager.DeployInformations;
            Assert.AreEqual(deployLocation.Applications.Count, infos.Length);
            foreach (DeployInformation info in infos)
            {
                Assert.AreEqual(DeployStatus.Deployed, info.Status);
            }
            deployManager.Stop();
        }


        [Test]
        public void WhenAnApplicationGetsCreatedDeploysIt ()
        {
            NewAssertedDeployed ();
        }

        [Test]
        public void WhenAnApplicationGetsRemovedUndeployIt ()
        {
            IApplication application = NewAssertedDeployed ();
            deployerMock.Expect("UnDeploy");
            deployLocation.Remove (application);
            Assert.IsFalse (deployManager.IsDeployed (application), "application has not been un-deployed");
        }

        [Test]
        public void WhenAnApplicationGetsUpdatedUndeployAndRedeployIt ()
        {
            IApplication application = NewAssertedDeployed ();
            deployerMock.Expect("UnDeploy");
            deployerMock.Expect("Deploy");
            deployLocation.Update(application);
            Assert.IsTrue(deployManager.IsDeployed(application));
        }

        [Test]
        public void DisposeLocationAndDeployerOnDispose ()
        {
            deployerMock.Expect("Dispose");
            deployManager.Dispose();
            Assert.IsTrue(deployLocation.wasDisposed);
        }

        [Test]
        public void IfAnApplicationCannotBeDeployedTheOccurredExceptionWillBeAvailableToBeQueried ()
        {
            string noDeployReason = "";
            Exception exception = new Exception(noDeployReason);

            IApplication application = new Application("testApp");
            deployerMock.ExpectAndThrow("Deploy", exception, application);
            deployManager.Start();
            deployLocation.Add(application);
            Assert.IsFalse(deployManager.IsDeployed(application), "application considered deployed");
            deployManager.Stop();
            Assert.IsFalse(deployManager.IsDeployed(application), "application considered deployed");


            DeployInformation[] infos = deployManager.DeployInformations;
            Assert.AreEqual(1, infos.Length);
            Assert.AreEqual(DeployStatus.CannotDeploy, infos[0].Status);
            Assert.AreEqual(exception, infos[0].Error);
        }

        [Test]
        public void AfterAnApplicationHasBeenRemovedItsInfosCannotBeQueried ()
        {
            IApplication application = new Application("testApp");
            deployerMock.Expect("Deploy", application);
            deployManager.Start();
            deployLocation.Add(application);
            Assert.IsTrue(deployManager.IsDeployed(application), "application considered deployed");

            deployLocation.Remove(application);
            DeployInformation[] infos = deployManager.DeployInformations;
            Assert.AreEqual(0, infos.Length);
            deployManager.Stop();
        }

        [Test]
        public void KeepUndeployingApplicationsEvenIfThereAreExceptionsWhileUndeploying()
        {
            IApplication application = NewAssertedDeployed ();
            deployerMock.ExpectAndThrow("UnDeploy", new Exception());            
            deployManager.Start();
            deployManager.Stop();
            Assert.IsFalse(deployManager.IsDeployed(application), "failed to undeploy application still considered deployed");
        }

        private IApplication NewAssertedDeployed ()
        {
            IApplication application = new Application ("testApp");
            deployLocation.Add (application);
            Assert.IsTrue (deployManager.IsDeployed (application), "application has not been deployed");
            deployerMock.Expect("Deploy");
            return application;
        }

    }

    [TestFixture]
    public class DeployInformationTest
    {
        [Test]
        public void SerializationOfDeployInformation ()
        {
            string tempDir = Environment.GetEnvironmentVariable("TEMP");
            string tempFilename = Path.Combine(tempDir,"foo.dat");
            FileInfo file = new FileInfo (tempFilename);
            FileStream instream = null;
            try 
            {
                DeployInformation ex = new DeployInformation(new Application("testApp"), DeployStatus.CannotDeploy, new Exception("bar"));
                Stream outstream = file.OpenWrite ();
                new BinaryFormatter ().Serialize (outstream, ex);
                outstream.Flush ();
                outstream.Close ();
                instream = file.OpenRead ();
                DeployInformation inex = (DeployInformation) new BinaryFormatter ().Deserialize (instream);
                Assert.AreEqual(DeployStatus.CannotDeploy, inex.Status);
                Assert.AreEqual("testApp", inex.Application.Name);
                Assert.AreEqual("bar", inex.Error.Message);
            }
            finally 
            {
                try 
                {
                    instream.Close ();
                    file.Delete ();
                } 
                catch {}
            }
        }       
    }
}