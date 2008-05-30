#region License

/*
 * Copyright 2002-2005 the original author or authors.
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
	/// Summary description for DoubleBooleanConstructorObject.
	/// </summary>
	public class DoubleBooleanConstructorObject
	{
		private bool boolean1;
		private bool boolean2;

		public bool Boolean2
		{
			get { return this.boolean2; }
			set { this.boolean2 = value; }
		}

		public bool Boolean1
		{
			get { return this.boolean1; }
			set { this.boolean1 = value; }
		}

		public DoubleBooleanConstructorObject(bool b1, bool b2)
		{
			this.boolean1 = b1;
			this.boolean2 = b2;
		}
	}
}