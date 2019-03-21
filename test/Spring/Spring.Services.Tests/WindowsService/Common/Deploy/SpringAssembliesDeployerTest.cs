#region License
/*
* Copyright 2002-2010 the original author or authors.
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* 
*      https://www.apache.org/licenses/LICENSE-2.0
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
using NUnit.Framework;

namespace Spring.Services.WindowsService.Common.Deploy
{
    [TestFixture]
	public class SpringAssembliesDeployerTest
	{
        [Test]
		public void RepeatedConfiguresDontAugmentPrivateBinPathList()
		{
            IApplication app = new Application("app");
            SpringAssembliesDeployer deployer = new SpringAssembliesDeployer(String.Empty);
            deployer.ConfigurePrivateBinForApplication(app);
            deployer.ConfigurePrivateBinForApplication(app);
            Assert.AreEqual(2, app.DomainSetup.PrivateBinPath.Split(';').Length);
		}

        [Test]
        public void FilterMatchSpringAssembliesSubDir ()
        {
            string directory = Path.Combine("c:/", 
                SpringAssembliesDeployer.PrivateBinPathPrefix + "foo/foo.dll");
            FileSystemEventArgs args = 
                new FileSystemEventArgs(WatcherChangeTypes.All, directory, "foo.dll");
            Assert.IsTrue(SpringAssembliesDeployer.DisallowFilter.Filter(args));
        }
	}
}
