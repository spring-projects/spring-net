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
using System.Text;
using NUnit.Framework;
using Spring.Core.IO;
using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Services.WindowsService.Common.Deploy.FileSystem
{
    [TestFixture]
    public class RegularExpressionFilterConfigurerTest
    {

        string xml = String.Format (@"
<objects xmlns='http://www.springframework.net' 
		xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' 
		xsi:schemaLocation='http://www.springframework.net http://www.springframework.net/xsd/spring-objects.xsd'>
    <object name='watcher' type='Spring.Services.WindowsService.Common.Deploy.FileSystem.FileSystemApplicationWatcher' singleton='false'>
      <constructor-arg><ref object='{0}'/></constructor-arg>
      <constructor-arg><ref object='configurer'/></constructor-arg>
    </object>
    <object 
        name='configurer' 
        type='Spring.Services.WindowsService.Common.Deploy.FileSystem.RegularExpressionFilterConfigurer' 
        singleton='false'>
      <property name='includes'>
        <list>
          <value>bin/**/*.*</value>
        </list>
      </property>
      <property name='excludes'>
        <list>
          <value>**/*.log</value>
        </list>
      </property>
    </object>
</objects>
", DefaultApplicationWatcherFactory.InjectedApplicationName);

        private string xmlFile;

        [TearDown]
        public void TearDown ()
        {
            File.Delete(xmlFile);
        }

        [SetUp]
        public void SetUp()
        {
            xmlFile = Application.WatcherXml;
            using (StreamWriter w = File.CreateText(xmlFile))
            {
                w.WriteLine(xml);
            }
        }

        [Test]
        public void CreationViaARegexFilterConfigurerPrependsApplicationFullPathToRegex()
        {
            IApplication application = new Application(Path.GetFullPath("."));
            FileSystemApplicationWatcher w =
                (FileSystemApplicationWatcher) DefaultApplicationWatcherFactory.Instance.CreateApplicationWatcher(application);
            Assert.AreEqual(1, w.DisallowFilters.Count);
            Assert.AreEqual(1, w.AllowFilters.Count);
        	RegularExpressionFilter filter1 = w.AllowFilters[0] as RegularExpressionFilter;
            Assert.AreEqual(
                PathMatcher.ForwardifySlashes(application.FullPath + "/bin/**/*.*"), filter1.Patterns[0]);
        	RegularExpressionFilter filter2 = w.DisallowFilters[0] as RegularExpressionFilter;
            Assert.AreEqual(
                PathMatcher.ForwardifySlashes(application.FullPath + "/**/*.log"), filter2.Patterns[0]);
        }

        [Test]
        public void AnObjectCanBeInjectedFromCodeIntoTheContext ()
        {
            string inject = @"
<objects xmlns='http://www.springframework.net' 
		xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' 
		xsi:schemaLocation='http://www.springframework.net http://www.springframework.net/xsd/spring-objects.xsd'>

    <object 
        name='inject' 
        type='Spring.Services.WindowsService.Common.Deploy.FileSystem.Inject' 
        singleton='false'>
      <property name='Application'>
        <ref object='app'/>
      </property>
    </object>
</objects>
";            
            XmlObjectFactory factory = new XmlObjectFactory(new InputStreamResource(new MemoryStream(Encoding.Default.GetBytes(inject)), ""));
            IApplication app = new Application("foo");
            factory.RegisterSingleton("app", app);
            Inject injected = (Inject) factory.GetObject("inject");
            Assert.IsNotNull(injected);
            Assert.IsNotNull(injected.Application);
            Assert.AreEqual(app, injected.Application);
        }

    }

    public class Inject
    {
        IApplication application;
        public IApplication Application
        {
            get { return application; }
            set { application = value; }
        }
    }
}
