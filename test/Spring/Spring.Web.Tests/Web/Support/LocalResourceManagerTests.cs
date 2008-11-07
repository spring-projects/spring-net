#region License

/*
 * Copyright © 2002-2008 the original author or authors.
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

using System;
using NUnit.Framework;
using NUnitAspEx;

#endregion

#if NET_2_0

namespace Spring.Web.Support
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Erich Eichinger</author>
    [AspTestFixture("/Test", "/Spring/Web/Support/LocalResourceManagerTests")]
    public class LocalResourceManagerTests
    {
        [Test]
        public void ReturnsWithResources()
        {
            AspTestClient client = new AspTestClient();
            string result = client.GetPage("WithResources.aspx");
            Assert.AreEqual("<span id=\"Result\"></span>", result);            
        }

        [Test]
        public void ReturnsWithoutResources()
        {
            AspTestClient client = new AspTestClient();
            string result = client.GetPage("WithoutResources.aspx");
            Assert.AreEqual("OK", result);            
        }
    }
}

#endif // NET_2_0