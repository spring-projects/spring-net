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

using System;
using System.IO;
using System.Text;
using NUnit.Framework;

#endregion

namespace Spring.Core.IO
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class StringResourceTests
    {
        [Test]
        public void EnsureDefaults()
        {
            Encoding enc = Encoding.Default;
            string FOO_CONTENT = "foo";
            string FOO_DESCRIPTION = "foo description";

            StringResource r = new StringResource(FOO_CONTENT);    
            Assert.AreEqual(FOO_CONTENT, r.Content);
            Assert.AreEqual(enc, r.Encoding);
            Assert.AreEqual(string.Empty, r.Description);

            enc = new UTF7Encoding();
            r = new StringResource(FOO_CONTENT, enc, FOO_DESCRIPTION);    
            Assert.AreEqual(FOO_CONTENT, r.Content);
            Assert.AreEqual(enc, r.Encoding);
            Assert.AreEqual(FOO_DESCRIPTION, r.Description);
        }

        [Test]
        public void ReturnsCorrectEncodedStream()
        {
            string FOO_CONTENT = "foo\u4567";
            StringResource r = new StringResource(FOO_CONTENT, Encoding.GetEncoding("utf-16"));
            Assert.AreEqual(FOO_CONTENT, r.Content);
            Stream istm = r.InputStream;
            Assert.IsTrue(istm.CanRead);

            byte[] chars = new byte[istm.Length];
            istm.Read(chars, 0, chars.Length);
            istm.Close();
            string result = Encoding.GetEncoding("utf-16").GetString( chars );
            Assert.AreEqual(FOO_CONTENT, result);
        }

        [Test]
        public void DoesntSupportRelativeResources()
        {
            StringResource r = new StringResource(string.Empty);
            Assert.Throws<NotSupportedException>(() => r.CreateRelative("foo"));
        }

        [Test]
        public void AcceptsNullContent()
        {
            Encoding utf7 = new UTF7Encoding();
            StringResource r = new StringResource(null, utf7);
            Assert.AreEqual(string.Empty, r.Content);
            Stream stm = r.InputStream;
            Assert.IsTrue(stm.CanRead);
            Assert.IsNotNull(stm);
            Assert.AreEqual(0, stm.Length);
            stm.Close();
        }

        [Test]
        public void AlwaysExists()
        {
            StringResource r = new StringResource(null);
            Assert.IsTrue(r.Exists);
            r = new StringResource(string.Empty);
            Assert.IsTrue(r.Exists);
            r = new StringResource("foo");
            Assert.IsTrue(r.Exists);
        }
    }
}