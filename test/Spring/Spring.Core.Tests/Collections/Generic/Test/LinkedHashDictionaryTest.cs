#region License

/*
 * Copyright © 2015-2015 the original author or authors.
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

using System;
using System.Collections.Generic;
using System.Linq;
using Spring.Collections.Generic
using NUnit.Framework;

namespace Spring.Collections.Generic.Test
{
	/// <summary>
	/// This class contains tests for LinkedHashDictionary
	/// </summary>
	/// <author>Zbynek Vyskovsky, kvr@centrum.cz</author>
	[TestFixture]
	public class LinkedHashDictionaryTest
	{
		[Test]
		public void			TestSunnyDay()
		{
			IDictionary<int, string> td = new LinkedHashDictionary<int, string>();
			td.Add(0, "a");
			td.Add(1, "b");
			td.Add(2, "c");
			td.Add(1, "b2");
			td.Add(3, "d");
			td.Add(4, "e");
			Assert.IsTrue(td.ContainsKey(0));
			Assert.IsTrue(td.ContainsKey(1));
			Assert.IsTrue(td.ContainsKey(2));
			Assert.IsTrue(td.ContainsKey(3));
			Assert.IsTrue(td.ContainsKey(4));
			Assert.AreEqual("a", td[0]);
			Assert.AreEqual("b2", td[1]);
			int[] keys = td.Keys.ToArray();
			Assert.AreEqual(5, keys.Length);
			Assert.AreEqual(0, keys[0]);
			Assert.AreEqual(1, keys[1]);
			Assert.AreEqual(2, keys[2]);
			Assert.AreEqual(3, keys[3]);
			Assert.AreEqual(4, keys[4]);
			td.Remove(3);
			td.Remove(2);
			Assert.AreEqual(3, td.Count);
			td.Remove(1);
			Assert.AreEqual(2, td.Count);
			td.Remove(4);
			td.Remove(0);
			Assert.AreEqual(0, td.Count);
			Assert.AreEqual(0, td.Keys.Count);
		}
	}
}
