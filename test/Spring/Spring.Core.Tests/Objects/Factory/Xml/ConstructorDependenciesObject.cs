#region License

/*
 * Copyright 2004 the original author or authors.
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



#endregion

namespace Spring.Objects.Factory.Xml
{
	/// <summary>
	/// Simple object used to check constructor dependency checking.
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans (.NET)</author>
	public class ConstructorDependenciesObject
	{
		#region Constructor (s) / Destructor

		public ConstructorDependenciesObject(int age)
		{
			this.age = age;
		}

		public ConstructorDependenciesObject(string name)
		{
			this.name = name;
		}

		public ConstructorDependenciesObject(ITestObject spouse1)
		{
			this.spouse1 = spouse1;
		}

		public ConstructorDependenciesObject(ITestObject spouse1, ITestObject spouse2)
		{
			this.spouse1 = spouse1;
			this.spouse2 = spouse2;
		}

		public ConstructorDependenciesObject(ITestObject spouse1, ITestObject spouse2, IndexedTestObject other)
		{
			this.spouse1 = spouse1;
			this.spouse2 = spouse2;
			this.other = other;
		}

		#endregion

		#region Properties

		public int Age
		{
			get { return age; }

			set { this.age = value; }
		}

		public string Name
		{
			get { return name; }
			set { this.name = value; }
		}

		public ITestObject Spouse1
		{
			get { return spouse1; }
		}

		public ITestObject Spouse2
		{
			get { return spouse2; }
		}

		public IndexedTestObject Other
		{
			get { return other; }
		}

		#endregion

		#region Fields

		private int age;
		private string name;
		private ITestObject spouse1;
		private ITestObject spouse2;
		private IndexedTestObject other;

		#endregion
	}
}