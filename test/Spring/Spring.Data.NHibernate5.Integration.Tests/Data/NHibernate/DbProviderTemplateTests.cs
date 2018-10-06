#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

#region Imports

using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Data.Common;

#endregion

namespace Spring.Data.NHibernate
{
    /// <summary>
    /// This class contains tests for execution of misc Hibernate usage, including userCredentialsDbProvider
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class DbProviderTemplateTests
    {
        private IApplicationContext ctx;
        private UserCredentialsDbProvider userCredentialsDbProvider;

        [SetUp]
        public void SetUp()
        {
            //BasicConfigurator.Configure();
            string assemblyName = GetType().Assembly.GetName().Name;
            ctx = new XmlApplicationContext("assembly://" + assemblyName + "/Spring.Data.NHibernate/dbProviderTemplateTests.xml");

            userCredentialsDbProvider = ctx["DbProvider"] as UserCredentialsDbProvider;
            Assert.IsNotNull(userCredentialsDbProvider);
        }

        [Test]
        [Ignore("Trouble running on Appveyor")]
        public void UserCredentialsDbProvider()
        {
            ITestObjectDao dao = (ITestObjectDao)ctx["testObjectDaoTransProxy"];

            userCredentialsDbProvider.SetCredentialsForCurrentThread("User ID=springqa", "Password=springqa");
            TestObject toGeorge = new TestObject();
            toGeorge.Name = "George";
            toGeorge.Age = 33;
            dao.Create(toGeorge);

            userCredentialsDbProvider.SetCredentialsForCurrentThread("User ID=springqa2", "Password=springqa2");

            TestObject toMary = new TestObject();
            toMary.Name = "Mary";
            toMary.Age = 34;
            dao.Create(toMary);
        }




    }
}