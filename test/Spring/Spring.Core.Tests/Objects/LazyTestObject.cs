#region License

/*
 * Copyright © 2002-2006 the original author or authors.
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

using System;
using System.Threading;

#endregion

namespace Spring.Objects
{
	/// <summary>
	/// Object to help test lazy instantiation of singletons from
	/// multiple threads.  See MultiThreadedLazyInit in 
	/// XmlObjectFactoryTests.
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	public class LazyTestObject 
	{
		#region Fields

        private static int count = 0;

		#endregion

		#region Constructor (s)
		/// <summary>
		/// Initializes a new instance of the <see cref="LazyTestObject"/> class
		/// and sleeps for 2 seconds.
        /// </summary>
		public LazyTestObject()
		{
            count++;
		    Thread.Sleep(2000);
		}

		#endregion

		#region Properties

	    public static int Count
	    {
	        get { return count; }
	        set { count = value; }
	    }

	    #endregion

		#region Methods

        public override string ToString()
        {
            return String.Format("LazyTestObject, Count = [{0}]", count);
        }
		#endregion

	}
}
