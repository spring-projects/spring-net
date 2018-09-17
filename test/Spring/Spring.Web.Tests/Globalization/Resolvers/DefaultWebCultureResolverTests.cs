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

using System.Globalization;
using NUnit.Framework;

#endregion

namespace Spring.Globalization.Resolvers
{
    /// <summary>
    /// Tests DefaultWebCultureResolver behaviour.
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class DefaultWebCultureResolverTests
    {
        #region TestDefaultWebCultureResolver utility class

        /// <summary>
        /// Override GetRequestLanguage() to return test language instead of HttpRequest.UserLanguages[0]
        /// </summary>
        public class TestDefaultWebCultureResolver : DefaultWebCultureResolver
        {
            private string _requestLanguage;

            // convenience setter method
            public TestDefaultWebCultureResolver SetRequestLanguage(string requestLanguage)
            {
                _requestLanguage = requestLanguage;
                return this;
            }

            // override to return our test language instead of HttpRequest.UserLanguages[0]!
            protected override string GetRequestLanguage()
            {
                return _requestLanguage;
            }
        }

        #endregion

        private readonly CultureInfo EXPECTED_NEUTRALCULTURE = new CultureInfo("fr");

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            // ensure, uiCulture and culture are set to different cultures
            CultureTestScope.Set();
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            CultureTestScope.Reset();
        }

        [Test]
        public void DefaultCultureDefaultsToNull()
        {
            TestDefaultWebCultureResolver r = new TestDefaultWebCultureResolver();
            Assert.IsNull(r.DefaultCulture);
        }

        [Test]
        public void AlwaysReturnsDefaultCultureIfDefaultCultureIsSet()
        {
            TestDefaultWebCultureResolver r = new TestDefaultWebCultureResolver();
            r.DefaultCulture = EXPECTED_NEUTRALCULTURE;

            r.SetRequestLanguage(null);
            Assert.AreEqual(EXPECTED_NEUTRALCULTURE, r.ResolveCulture());

            r.SetRequestLanguage("de");
            Assert.AreEqual(EXPECTED_NEUTRALCULTURE, r.ResolveCulture());
        }

        [Test]
        public void ReturnsRequestCultureIfNoDefaultCulture()
        {
            TestDefaultWebCultureResolver r = new TestDefaultWebCultureResolver();
            
            r.SetRequestLanguage(EXPECTED_NEUTRALCULTURE.Name);
            Assert.AreEqual(EXPECTED_NEUTRALCULTURE, r.ResolveCulture());
        }

        [Test]
        public void ReturnsCurrentUICultureIfNoDefaultCultureIsSetAndNoOrInvalidRequestLanguage()
        {
            TestDefaultWebCultureResolver r = new TestDefaultWebCultureResolver();

            r.SetRequestLanguage(null);
            Assert.AreEqual(CultureInfo.CurrentUICulture, r.ResolveCulture());
            
            r.SetRequestLanguage("invalid culture name");
            Assert.AreEqual(CultureInfo.CurrentUICulture, r.ResolveCulture());
        }
    }
}