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
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

#endregion

namespace Spring.Collections
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class CaseInsensitiveHashtableTests
    {
        private static object SerializeDeserializeObject(object exp)
        {
            byte[] data;
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                formatter.Serialize(ms, exp);
                ms.Flush();
                data = ms.ToArray();
            }

            using (MemoryStream ms = new MemoryStream(data))
            {
                exp = formatter.Deserialize(ms);
            }

            return exp;
        }

        [Test]
        public void IsSerializable()
        {
            CaseInsensitiveHashtable storiginal = new CaseInsensitiveHashtable();
            storiginal.Add("key", "value");
            CaseInsensitiveHashtable st = (CaseInsensitiveHashtable)SerializeDeserializeObject(storiginal);
            Assert.AreNotSame(storiginal, st);
            Assert.AreEqual("value", st["KEY"]);
            Assert.AreEqual(1, st.Count);
        }

        [Test]
        public void AcceptsNonStringKeys()
        {
            CaseInsensitiveHashtable st = new CaseInsensitiveHashtable();

            object key = new object();
            st.Add(key, "value");
            Assert.AreEqual(1, st.Count);            
            Assert.AreEqual("value", st[key]);
            Assert.IsNull(st[new object()]);
        }

        [Test]
        public void IgnoresCase()
        {
            CaseInsensitiveHashtable st = new CaseInsensitiveHashtable();
            st.Add("key", "value");
            Assert.AreEqual("value", st["KEY"]);
            st["KeY"] = "value2";
            Assert.AreEqual(1, st.Count);
            Assert.AreEqual("value2", st["key"]);

            try
            {
                st.Add("KEY", "value2");
                Assert.Fail();
            }
            catch (ArgumentException)
            { }

            Hashtable ht = new Hashtable();
            ht.Add("key", "value");
            ht.Add("KEY", "value");
            try
            {
                st = new CaseInsensitiveHashtable(ht, CultureInfo.InvariantCulture);
                Assert.Fail();
            }
            catch (ArgumentException)
            { }
        }

        [Test]
        public void InitializeFromOtherCopiesValues()
        {
            Hashtable ht = new Hashtable();
            ht["key"] = "value";
            ht["key2"] = "value2";

            CaseInsensitiveHashtable st = new CaseInsensitiveHashtable(ht, CultureInfo.InvariantCulture);
            Assert.AreEqual(2, st.Count);
            ht.Remove("key");
            Assert.AreEqual(1, ht.Count);
            Assert.AreEqual(2, st.Count);
        }

        /// <summary>
        /// On my NB gives
        /// Duration: 00:00:11.0937500
        /// Duration: 00:00:05.3593750
        /// </summary>
        [Test, Explicit]
        public void ComparePerformance()
        {
            const int runs = 30000000;
            StopWatch watch = new StopWatch();

            Hashtable ht = CollectionsUtil.CreateCaseInsensitiveHashtable();
            for (int i = 0; i < 1000000; i++) ht.Add(Guid.NewGuid().ToString(), "val"); // gen. higher number of elements results in OOM exception????
            CaseInsensitiveHashtable ciht = new CaseInsensitiveHashtable(ht, CultureInfo.InvariantCulture);

            using (watch.Start("Duration: {0}"))
            {
                for (int i = 0; i < runs; i++)
                {
                    object v = ht["somekey"];
                }
            }

            using (watch.Start("Duration: {0}"))
            {
                for (int i = 0; i < runs; i++)
                {
                    object v = ciht["somekey"];
                }
            }
        }
    }
}