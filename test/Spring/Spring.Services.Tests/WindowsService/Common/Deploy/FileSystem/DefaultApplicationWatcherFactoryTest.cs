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
using NUnit.Framework;

namespace Spring.Services.WindowsService.Common.Deploy.FileSystem
{
    [TestFixture]
    public class DefaultApplicationWatcherFactoryTest
    {
        string xml = String.Format(@"
<objects xmlns='http://www.springframework.net' 
		xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' 
		xsi:schemaLocation='http://www.springframework.net https://www.springframework.net/xsd/spring-objects.xsd'>
    <object name='watcher' type='Spring.Services.WindowsService.Common.Deploy.FileSystem.FileSystemApplicationWatcher' singleton='false'>
      <constructor-arg><ref object='{0}'/></constructor-arg>
      <constructor-arg><ref object='configurer'/></constructor-arg>
    </object>

    <object name='configurer' type='Spring.Services.WindowsService.Common.Deploy.FileSystem.RegularExpressionFilterConfigurer' singleton='false'>
        <property name='includes'>
        <list>
          <value>**/**</value>
          <value>{1}</value>
          <value>{2}</value>
        </list>
      </property>
    </object>
</objects>
", DefaultApplicationWatcherFactory.InjectedApplicationName, 
            "${spring.services.application.name}", 
            "${spring.services.application.fullpath}");

        private string xmlFile;
        private IApplication application;
        private IApplicationWatcher w;

        [TearDown]
        public void TearDown ()
        {
            File.Delete(xmlFile);
        }

        [SetUp]
        public void SetUp()
        {
            xmlFile = Application.WatcherXml;
            using (StreamWriter writer = File.CreateText(xmlFile))
            {
                writer.WriteLine(xml);
            }
            application = new Application(Path.GetFullPath("."));
            w = DefaultApplicationWatcherFactory.Instance.CreateApplicationWatcher(application); 
        }

        [Test]
        public void InjectTheApplicationToBeWatchedInTheApplicationContextDefinedForTheWatcher ()
        {
            Assert.IsNotNull(w);
            Assert.IsTrue(w is FileSystemApplicationWatcher, "not a " + typeof(FileSystemApplicationWatcher));
            Assert.AreSame(application, w.Application);
        }
    }
}