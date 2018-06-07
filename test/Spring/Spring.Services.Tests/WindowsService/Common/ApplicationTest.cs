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
using System.Collections.Specialized;
using System.IO;
using NUnit.Framework;

namespace Spring.Services.WindowsService.Common
{
    [TestFixture]
    public class ApplicationTest
	{
        Application application;

        [SetUp]
        public void SetUp ()
        {
            application = new Application("testApp");
        }

        [Test]
        public void ApplicationFullPathEqualsApplicationBase ()
        {
            Assert.AreEqual(application.FullPath,
                application.DomainSetup.ApplicationBase,
                "unexpected application base");
        }

        [Test]
		public void DefaultPrivateBinPathIsBin()
		{
            Assert.AreEqual(
                "bin",
                application.DomainSetup.PrivateBinPath,
                "bin subdirectory not in private path");
        }

        [Test]
		public void ConfigurationFileNameIsHardCodedAsServiceDotConfig()
		{
            string expected = 
                Path.Combine(application.FullPath, Application.ServiceConfig);
            Assert.AreEqual(expected, application.DomainSetup.ConfigurationFile,
                    "unexpected configuration file name");

        }

        [Test]
        public void AFullPathAsNameIsReducedToLastPart ()
        {
            string fullpath = @"c:\spring\deploy\sample";
            application = new Application(fullpath);
            Assert.AreEqual("sample", application.Name);
            Assert.AreEqual(
                Path.Combine (fullpath, Application.ServiceConfig), 
                application.DomainSetup.ConfigurationFile);
        }

        [Test]
        public void BaseDirectoryCanBeSpecifiedWithFullPath ()
        {
            application = new Application(@"c:\spring\deploy\sample");
            Assert.AreEqual(@"c:\spring\deploy\sample", application.ApplicationBase);
            Assert.AreEqual("sample", application.Name);
        }

        [Test]
        public void DomainSetupIsConfiguredToShadowCopyFiles ()
        {
            Assert.AreEqual("true", application.DomainSetup.ShadowCopyFiles);
        }

        [Test]
        public void HashCodeIsTheSameOfTheFullPath ()
        {
            Assert.AreEqual(application.FullPath.GetHashCode(), application.GetHashCode());
        }

        [Test]
        public void ShouldHonoursEqualsContract()
        {
            Assert.IsFalse(application.Equals(new object()));
            Assert.IsFalse(application.Equals(null));
            Assert.IsTrue(application.Equals(application));
            Assert.IsTrue(application.Equals(new Application(application.FullPath)));
        }

        [Test]
        public void DefaultConfigurerAllowToReferenceTheApplicationFullPathAndName ()
        {
            NameValueCollection collection = Application.DefaultConfigurer(application).Properties;
            Assert.AreEqual(application.FullPath, collection[Application.ApplicationFullPathPlaceholder]);
            Assert.AreEqual(application.Name, collection[Application.ApplicationNamePlaceholder]);
            Assert.AreEqual(2, collection.Count);
        }
	}
}
