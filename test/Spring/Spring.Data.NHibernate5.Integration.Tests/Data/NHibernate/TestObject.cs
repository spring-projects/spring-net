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


namespace Spring.Data.NHibernate
{
	/// <summary>
	/// TODO: 
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	public class TestObject 
	{
		#region Fields
        private int age;
        private string name;
        private int objectNumber;
         
		#endregion

		#region Constructor (s)
		/// <summary>
		/// Initializes a new instance of the <see cref="TestObject"/> class.
        /// </summary>
		public 	TestObject()
		{

		}

		#endregion

		#region Properties

	    public virtual int Age
	    {
	        get { return age; }
	        set { age = value; }
	    }

	    public virtual string Name
	    {
	        get { return name; }
	        set { name = value; }
	    }

	    public virtual int ObjectNumber
	    {
	        get { return objectNumber; }
	        set { objectNumber = value; }
	    }

	    #endregion

		#region Methods

		#endregion

	}
}
