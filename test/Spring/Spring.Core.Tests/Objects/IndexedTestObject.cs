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

using System.Collections;
using Spring.Collections;

#endregion

namespace Spring.Objects
{
	/// <summary>
	/// Another simple object for testing purposes, containing some collection properties.
	/// </summary>
	public class IndexedTestObject
	{
		private TestObject[] array;
		private IList list;
		private ISet set;
		private IDictionary map;

		public TestObject[] Array
		{
			get { return array; }
			set { array = value; }
		}

		public IList List
		{
			get { return list; }
			set { list = value; }
		}

		public ISet Set
		{
			get { return set; }
			set { set = value; }
		}

		public IDictionary Map
		{
			get { return map; }
			set { map = value; }
		}

		public IndexedTestObject()
		{
			TestObject to0 = new TestObject("name0", 0);
			TestObject to1 = new TestObject("name1", 0);
			TestObject to2 = new TestObject("name2", 0);
			TestObject to3 = new TestObject("name3", 0);
			TestObject to4 = new TestObject("name4", 0);
			TestObject to5 = new TestObject("name5", 0);
			TestObject to6 = new TestObject("name6", 0);
			TestObject to7 = new TestObject("name7", 0);
			array = new TestObject[] {to0, to1};
			list = new ArrayList();
			list.Add(to2);
			list.Add(to3);
			set = new HashedSet();
			set.Add(to6);
			set.Add(to7);
			map = new Hashtable();
			map["key1"] = to4;
			map["key2"] = to5;
		}
	}
}