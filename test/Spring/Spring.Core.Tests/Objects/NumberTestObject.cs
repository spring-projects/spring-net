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

namespace Spring.Objects
{
	/// <summary>
	/// 
	/// </summary>
	/// <author>Juergen Hoeller</author>
	public sealed class NumberTestObject
	{
		private short _myShort;
		private int _myInt;
		private long _myLong;

		public short MyShort
		{
			get { return _myShort; }
			set { _myShort = value; }
		}

		public int MyInt
		{
			get { return _myInt; }
			set { _myInt = value; }
		}

		public long MyLong
		{
			get { return _myLong; }
			set { _myLong = value; }
		}

		public float MyFloat
		{
			get { return _myFloat; }
			set { _myFloat = value; }
		}

		public double MyDouble
		{
			get { return _myDouble; }
			set { _myDouble = value; }
		}

		private float _myFloat;
		private double _myDouble;
	}
}