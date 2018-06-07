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

using System.Diagnostics;
using NUnit.Framework;
using NUnitAspEx.Client;
using Spring.TestSupport;

namespace Spring.Web.Support
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    [Ignore("Trouble with running under .NET 4.5")]
    public class LocalResourceManagerTests : WebApplicationTests
    {
        public LocalResourceManagerTests()
            : base("/Test", "/Spring/Web/Support/LocalResourceManagerTests")
        {}

        [Test]
        public void ReturnsWithResources()
        {
            HttpWebClient client = Host.CreateWebClient();
            string result = client.GetPage("WithResources.aspx");
            Trace.Write(result);
            Assert.AreEqual("<span id=\"Result\">FromResource</span>", result);            
        }

        [Test]
        public void ReturnsWithoutResources()
        {
            HttpWebClient client = Host.CreateWebClient();
            string result = client.GetPage("WithoutResources.aspx");
            Assert.AreEqual("OK", result);            
        }
    }
}
