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

#region Imports

using System.Text;

using NUnit.Framework;

#endregion

namespace Spring.Util
{
	/// <summary>
	/// Unit tests for Spring.Util.Properties.
	/// </summary>
	[TestFixture]
	public class PropertiesTests
	{
        [Test]
        public void Instantiation ()
        {
            Properties root = new Properties();
            root.Add ("foo", "this");
            root.Add ("bar", "is");
            Properties props = new Properties(root);
			props.SetProperty("myPropertyKey", "myPropertyValue");
            Assert.AreEqual (3, props.Count);
            Assert.AreEqual ("this", props.GetProperty ("foo"));
            Assert.AreEqual ("is", props.GetProperty ("bar"));
            Assert.AreEqual ("myPropertyValue", props.GetProperty ("myPropertyKey"));
        }

        [Test]
        public void GetPropertyWithDefault ()
        {
            Properties props = new Properties();
            props.Add ("foo", "this");
            props.Add ("bar", "is");
            Assert.AreEqual ("this", props.GetProperty ("foo"));
            Assert.AreEqual ("is", props.GetProperty ("bar"));
            Assert.AreEqual ("it", props.GetProperty ("baz", "it"));
        }

        [Test]
        public void Remove ()
        {
            Properties props = new Properties();
            props.Add ("foo", "this");
            props.Add ("bar", "is");
            Assert.AreEqual (2, props.Count);
            props.Remove ("foo");
            Assert.AreEqual (1, props.Count);
            Assert.IsFalse (props.ContainsKey ("foo"));
        }

        [Test]
        public void Store ()
        {
            Properties props = new Properties();
            props.Add ("foo", "this");
            props.Add ("bar", "is");
            props.Add ("baz", "it");
            FileInfo file = new FileInfo ("properties.test");
            try 
            {
                // write 'em out with the specified header...
                using (Stream cout = file.OpenWrite ()) 
                {
                    props.Store (cout, "My Properties");
                }
            }
            finally 
            {
                try 
                {
                    file.Delete ();
                } 
                catch (IOException)
                {
                }
            }
        }

        [Test]
        public void ListAndLoad()
        {
            Properties props = new Properties();
            props.Add ("foo", "this");
            props.Add ("bar", "is");
            props.Add ("baz", "it");
            FileInfo file = new FileInfo ("properties.test");
            try 
            {
                // write 'em out...
                using (Stream cout = file.OpenWrite ()) 
                {
                    props.List (cout);
                }
                // read 'em back in...
                using (Stream cin = file.OpenRead ()) 
                {
                    props = new Properties ();
                    props.Load (cin);
                    Assert.AreEqual (3, props.Count);
                    Assert.AreEqual ("this", props.GetProperty ("foo"));
                    Assert.AreEqual ("is", props.GetProperty ("bar"));
                    Assert.AreEqual ("it", props.GetProperty ("baz", "it"));
                }
            }
            finally 
            {
                try 
                {
                    file.Delete ();
                } 
                catch (IOException)
                {
                }
            }
        }

		[Test]
		public void SimpleProperties()
		{
			string input = "key1=value1\r\nkey2:value2\r\n\r\n# a comment line\r\n   leadingspace : true";
			Stream s = new MemoryStream(Encoding.ASCII.GetBytes(input));
			Properties props = new Properties();
			props.Load(s);

			Assert.IsTrue("value1".Equals(props["key1"]));
			Assert.IsTrue("value2".Equals(props["key2"]));
			Assert.IsTrue("true".Equals(props["leadingspace"]));
		}

        [Test]
        public void WhitespaceProperties()
        {
            string input = "key1 =\t\nkey2:\nkey3";

            Stream s = new MemoryStream(Encoding.ASCII.GetBytes(input));
            Properties props = new Properties();
            props.Load(s);

            Assert.AreEqual(string.Empty, props["key1"], "key1 should have empty value");
            Assert.AreEqual(string.Empty, props["key2"], "key2 should have empty value");
            Assert.IsTrue(props.ContainsKey("key3"));
            Assert.IsNull(props["key3"]);
        }

		[Test]
		public void Continuation()
		{
			string input = "continued = this is a long value element \\\r\nthat uses continuation \\\r\n    xxx";
			Stream s = new MemoryStream(Encoding.ASCII.GetBytes(input));
			Properties props = new Properties();
			props.Load(s);

			Assert.IsTrue("this is a long value element that uses continuation xxx".Equals(props["continued"]));
		}

		[Test]
		public void SeperatorEscapedWithinKey()
		{
			string input = "\\" + ":key:newvalue";
			Stream s = new MemoryStream(Encoding.ASCII.GetBytes(input));
			Properties props = new Properties();
			props.Load(s);

			Assert.IsTrue("newvalue".Equals(props[":key"]));
		}

		[Test]
		public void EscapedCharactersInValue()
		{
			string input = "escaped=test\\ttest";
			Stream s = new MemoryStream(Encoding.ASCII.GetBytes(input));
			Properties props = new Properties();
			props.Load(s);

			Assert.IsTrue("test\ttest".Equals(props["escaped"]));
		}

	}
}
