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

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// Tests the MediaType class according to http://www.iana.org/assignments/media-types/
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class MimeMediaTypeTests
    {
        [Test]
        public void CreateInstance()
        {
            Assert.AreEqual( MimeMediaType.Application.Octet, new MimeMediaType() );
            MimeMediaType mt = new MimeMediaType("ApPlicatioN");
            Assert.AreEqual("application", mt.ContentType);
            Assert.AreEqual("*", mt.SubType);

            Assert.AreEqual("*", new MimeMediaType("ApPlicatioN", null).SubType);
            Assert.AreEqual("*", new MimeMediaType("ApPlicatioN", "").SubType);
            Assert.AreEqual("bla", new MimeMediaType("ApPlicatioN", "bla").SubType);
        }

        [Test]
        public void ToStringReturnsMimeTypeString()
        {
            Assert.AreEqual("application/subtype", new MimeMediaType("ApPlicatioN", "SubType").ToString());
        }

        [Test]
        public void Parse()
        {
            MimeMediaType mt = MimeMediaType.Parse("text/html");
            Assert.AreEqual("text", mt.ContentType);
            Assert.AreEqual("html", mt.SubType);

            try { mt = MimeMediaType.Parse(null); Assert.Fail(); }
            catch (ArgumentNullException) { /* ok */ }
            try { mt = MimeMediaType.Parse(""); Assert.Fail(); }
            catch (ArgumentException) { /* ok */ }
            try { mt = MimeMediaType.Parse("aasdfaDF"); Assert.Fail(); }
            catch (ArgumentException) { /* ok */ }
            try { mt = MimeMediaType.Parse("*"); Assert.Fail(); }
            catch (ArgumentException) { /* ok */ }
        }

        [Test]
        public void Equals()
        {
            MimeMediaType mt = MimeMediaType.Parse("text/html");
            Assert.AreEqual(MimeMediaType.Text.Html, mt);
            Assert.AreEqual(MimeMediaType.Parse("text/html"), mt);

            Assert.AreNotEqual(MimeMediaType.Parse("text/xml"), mt);
        }

        [Test]
        public void GetHashcode()
        {
            MimeMediaType mt = MimeMediaType.Parse("text/html");
            Assert.AreEqual(MimeMediaType.Text.Html.GetHashCode(), mt.GetHashCode());
            Assert.AreEqual(MimeMediaType.Parse("text/html").GetHashCode(), mt.GetHashCode());

            Assert.AreNotEqual(MimeMediaType.Parse("text/xml").GetHashCode(), mt.GetHashCode());
        }
    }
}
