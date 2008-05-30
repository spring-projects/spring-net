#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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
using System.Collections;
using Common.Logging;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Data.Common;

#endregion

namespace Spring.Data
{
	/// <summary>
	/// TODO: 
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	/// <version>$Id: MappingAdoQueryTests.cs,v 1.3 2006/11/13 07:05:00 markpollack Exp $</version>
	[TestFixture]
	public class MappingAdoQueryTests 
	{
		#region Fields
	    IDbProvider dbProvider;
		#endregion

		#region Constants

		/// <summary>
		/// The shared ILog instance for this class (and derived classes). 
		/// </summary>
		protected static readonly ILog log =
			LogManager.GetLogger(typeof (MappingAdoQueryTests));

		#endregion

		#region Constructor (s)
		/// <summary>
		/// Initializes a new instance of the <see cref="MappingAdoQueryTests"/> class.
                /// </summary>
		public 	MappingAdoQueryTests()
		{

		}

		#endregion

		#region Properties

		#endregion

		#region Methods
        [SetUp]
        public void CreateDbProvider()
        {
            IApplicationContext ctx =
                new XmlApplicationContext("assembly://Spring.Data.Integration.Tests/Spring.Data/adoTemplateTests.xml");
            Assert.IsNotNull(ctx);
            dbProvider = ctx["DbProvider"] as IDbProvider;
            Assert.IsNotNull(dbProvider);
        }
	    
	    [Test]
	    public void MappingAdoQuery()
	    {
	        TestObjectQuery testObjectQuery = new TestObjectQuery(dbProvider);
	        IDictionary inParams = new Hashtable();
            inParams.Add("UName", "George");
	        IList testObjectList = testObjectQuery.QueryByNamedParam(inParams);
	        Assert.AreEqual(2, testObjectList.Count);
	    }



	    #endregion

        

	}
}
