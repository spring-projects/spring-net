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

using NUnit.Framework;
using NUnitAspEx;
using NUnitAspEx.Core;

namespace Spring.TestSupport
{
    /// <summary>
    /// </summary>
    /// <author>Erich Eichinger</author>
    public abstract class WebApplicationTests
    {
        private readonly string virtualPath;
        private readonly string relativePhysicalPath;
        private IAspFixtureHost host;

        public IAspFixtureHost Host
        {
            get { return host; }
        }

        protected WebApplicationTests(string virtualPath, string relativePhysicalPath)
        {
            this.virtualPath = virtualPath;
            this.relativePhysicalPath = relativePhysicalPath;
        }

        [OneTimeSetUp]
        public virtual void TestFixtureSetup()
        {
            host = AspFixtureHost.CreateInstance(virtualPath, relativePhysicalPath, this);
        }

        [OneTimeTearDown]
        public virtual void TestFixtureTearDown()
        {
            host = AspFixtureHost.ReleaseInstance(host);
        }

    }
}