

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

namespace Spring.Objects.Factory.Xml
{
	/// <summary>
	/// Simple object used to test object creation via constuctors with
	/// simple types.
	/// </summary>
	/// <author>Jurgen Hoeller</author>
	/// <author>Mark Pollack (.NET)</author>
	public class SingleSimpleTypeConstructorObject
	{
		private bool singleBoolean;

		private bool secondBoolean;

		private string testString;

		/// <summary>
		/// Create an instance using the singleBoolean
		/// </summary>
		/// <param name="singleBoolean">true or false</param>
		public SingleSimpleTypeConstructorObject(bool singleBoolean)
		{
			this.singleBoolean = singleBoolean;
		}

		/// <summary>
		/// Create an instance using the second boolean and a string
		/// </summary>
		/// <param name="testString">test value</param>
		/// <param name="secondBoolean">true or false</param>
		public SingleSimpleTypeConstructorObject(string testString, bool secondBoolean)
		{
			this.testString = testString;
			this.secondBoolean = secondBoolean;
		}


		/// <summary>
		/// Property TestString (string)
		/// </summary>
		public string TestString
		{
			get { return this.testString; }
		}

		/// <summary>
		/// Property SecondBoolean (bool)
		/// </summary>
		public bool SecondBoolean
		{
			get { return this.secondBoolean; }
		}

		/// <summary>
		/// Property SingleBoolean (bool)
		/// </summary>
		public bool SingleBoolean
		{
			get { return this.singleBoolean; }
		}


	}
}