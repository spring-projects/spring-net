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

using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;

#endregion

namespace Spring.Data.NHibernate
{
	[TestFixture]
	public class NativeNHTests 
	{

		#region Constructor (s)
		/// <summary>
		/// Initializes a new instance of the <see cref="NativeNHTests"/> class.
                /// </summary>
		public 	NativeNHTests()
		{

		}

		#endregion


		#region Methods

	    [Test]
	    public void CreateNative()
	    {
            IApplicationContext ctx;
            string assemblyName = GetType().Assembly.GetName().Name;
            ctx = new XmlApplicationContext("assembly://" + assemblyName + "/Spring.Data.NHibernate/templateTests.xml");

            ITestObjectDao dao = (ITestObjectDao)ctx.GetObject("nativeNHTestObjectDao");
	        
            TestObject toGeorge = new TestObject();
            toGeorge.Name = "George";
            toGeorge.Age = 34;
            dao.Create(toGeorge);

        }
        #endregion

    }
}
