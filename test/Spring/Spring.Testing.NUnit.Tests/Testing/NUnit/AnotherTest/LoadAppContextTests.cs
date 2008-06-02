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

#if !NET_1_0

#region Imports

using System;
using NUnit.Framework;

#endregion

namespace Spring.Testing.NUnit.Another
{
    /// <summary>
    /// This class should be the first one executed that inherits from BaseSpringTests to be loaded by
    /// NUnit. The second one should be CachedAppContextTests.  The package name starting with the
    /// letter 'A' is on purpose, since this seems to match how NUnit iterates over tests in this
    /// assembly.
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class LoadAppContextTests : BaseTestAppContextTests
    {

        /// <summary>
        /// Loads the application context.  If this test fails, see if you can coerce NUnit to 
        /// execut it before CachedAppContextTests, or check if application context caching in 
        /// Spring.Testing.NUnit is broken.
        /// </summary>
        [Test]
        public void LoadApplicationContext()
        {
            Assert.AreEqual(1, LoadCount, "Either caching of application context is broken or this test was executed before CachedAppContextTests.");            
        }

        
    }
}
#endif