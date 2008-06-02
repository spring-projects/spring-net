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

using NUnit.Framework;
using Spring.Globalization;

#endregion

namespace Spring.TestSupport
{
    /// <summary>
    /// The base class for tests to run within a TestWebContext
    /// </summary>
    /// <author>Erich Eichinger</author>
    public abstract class TestWebContextTests
    {
        [SetUp]
        public void SetUp()
        {
            // ensure, uiCulture and culture are set to different cultures
            CultureTestScope.Set();
            TestWebContext.Create("/apppath", "testpage.aspx");
            DoSetUp();
        }

        [TearDown]
        public void TearDown()
        {
            DoTearDown();
            TestWebContext.Release();
            CultureTestScope.Reset();
        }

        protected virtual void DoSetUp()
        {            
        }

        protected void DoTearDown()
        {
        }
    }
}