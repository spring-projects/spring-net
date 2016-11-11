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

namespace Spring.Data.Northwind
{
	/// <author>Mark Pollack (.NET)</author>
	public class Shipper 
	{
		#region Fields

	    public int Id
	    {
	        get { return id; }
	        set { id = value; }
	    }

	    public string Name
	    {
	        get { return name; }
	        set { name = value; }
	    }

	    public string Phone
	    {
	        get { return phone; }
	        set { phone = value; }
	    }

	    private int id;
	    private string name;
	    private string phone;
		#endregion

		#region Constructor (s)
		/// <summary>
		/// Initializes a new instance of the <see cref="Shipper"/> class.
                /// </summary>
		public 	Shipper()
		{

		}

	    public Shipper(int id, string name, string phone)
	    {
	        this.id = id;
	        this.name = name;
	        this.phone = phone;
	    }

	    #endregion

		#region Properties

		#endregion

		#region Methods

		#endregion

	}
}
