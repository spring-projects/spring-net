#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

#endregion

namespace Spring.Data
{
    [TestFixture]
	public class DTCTests 
	{
        private IApplicationContext ctx;

        [SetUp]
        public void SetUp()
        {
            //BasicConfigurator.Configure();
            //Console.WriteLine("Hello");
            //LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter(new NameValueCollection());

            //WELLKNOWN: NamespaceParserRegistry.RegisterParser(typeof(TxNamespaceParser));
            //WELLKNOWN: NamespaceParserRegistry.RegisterParser(typeof(AopNamespaceParser));
            string ctxName = "DTCAppContext.xml"; // for .NET 2.0
            //string ctxName = "DTC1.1AppContext.xml"; // for .NET 1.1
            ctx =
                new XmlApplicationContext("assembly://Spring.Data.Integration.Tests/Spring.Data/" + ctxName);            
        }

        [Test]
#if NETCOREAPP
        [Ignore("Not supported on .NET Core")]
#endif
        public void DeclarativeWithAttributes()
        {
            IAccountManager mgr = ctx["accountManager"] as IAccountManager;
            mgr.DoTransfer(100, 100);
        }

	}
}
