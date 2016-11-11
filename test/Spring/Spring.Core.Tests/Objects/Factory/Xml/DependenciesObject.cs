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



#endregion

namespace Spring.Objects.Factory.Xml
{
	/// <summary>
	/// Simple object used to check dependency checking.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Rick Evans (.NET)</author>
	public class DependenciesObject
	{
		#region Constructor (s) / Destructor

		/// <summary>
		/// Creates a new instance of the  DependenciesObject class.
		/// </summary>
		public DependenciesObject()
		{
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

		public ITestObject Spouse
		{
			get { return spouse; }
			set { this.spouse = value; }
		}

		#endregion

		#region Fields

		private int age;
		private string name;
		private ITestObject spouse;

		#endregion
	}
}