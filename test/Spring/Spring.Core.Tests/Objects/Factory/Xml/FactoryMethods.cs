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
	/// Test class for Spring.NET's ability to create objects using static
	/// factory methods, rather than constructors.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Rick Evans (.NET)</author>
	public class FactoryMethods
	{
		#region Constructor (s) / Destructor

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.Xml.FactoryMethods"/> class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Constructor is private: not for use outside this class, even by the IoC container.
		/// </p>
		/// </remarks>
		private FactoryMethods(TestObject obj, string name, int number)
		{
			_object = obj;
			_name = name;
			_number = number;
		}

		#endregion

		#region Properties

		public TestObject Object
		{
			get { return _object; }
		}

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public string Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public int Number
		{
			get { return _number; }
		}

		#endregion

		#region Methods

		public static FactoryMethods DefaultInstance()
		{
			TestObject obj = new TestObject();
			obj.Name = "defaultInstance";
			return new FactoryMethods(obj, "default", 0);
		}

		public static FactoryMethods NewInstance(TestObject obj)
		{
			return new FactoryMethods(obj, "default", 0);
		}

		public static FactoryMethods NewInstance(TestObject obj, string name, int num)
		{
			return new FactoryMethods(obj, name, num);
		}

		#endregion

		#region Fields

		private int _number;
		private TestObject _object;
		private string _name = "default";
		private string _value;

		#endregion
	}
}