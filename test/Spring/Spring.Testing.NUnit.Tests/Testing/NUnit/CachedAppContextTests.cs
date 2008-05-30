#if !NET_1_0
#region License

/*
 * Copyright © 2002-2007 the original author or authors.
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

#endregion

namespace Spring.Testing.NUnit
{
    /// <summary>
    /// This class should be the second one executed that inherits from BaseSpringTests to be loaded by
    /// NUnit.  The first one should be LoadAppContextTests.
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <version>$Id: CachedAppContextTests.cs,v 1.2 2007/08/21 19:26:14 markpollack Exp $</version>
    [TestFixture]
    public class CachedAppContextTests : BaseTestAppContextTests
    {

        /// <summary>
        /// Loads the cached application context.  If this test fails, see if you can coerce NUnit to 
        /// execut it before CachedAppContextTests, or check if application context caching in 
        /// Spring.Testing.NUnit is broken.
        /// </summary>
        [Test]
        public void CachedApplicationContext()
        {
            Assert.AreEqual(0, LoadCount);
        }

        
    }
}
#endif