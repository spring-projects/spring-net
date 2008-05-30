#region Licence

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
using System.Data;
using System.Data.SqlClient;
using NUnit.Framework;

#endregion

namespace SpringAir.Data.Ado
{
	/// <summary>
	/// Unit tests for the ReflectiveDbConnectionFactory class.
	/// </summary>
	/// <author>Rick Evans (.NET)</author>
	/// <version>$Id: ReflectiveDbConnectionFactoryTests.cs,v 1.1 2005/10/30 09:06:40 springboy Exp $</version>
	[TestFixture]
	public sealed class ReflectiveDbConnectionFactoryTests
	{
		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void CtorWithNullConnectionString()
		{
			new ReflectiveDbConnectionFactory(typeof(SqlConnection), null);
		}
		
		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void CtorWithEmptyConnectionString()
		{
			new ReflectiveDbConnectionFactory(typeof(SqlConnection), string.Empty);
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void CtorWithWhitespacedConnectionString()
		{
			new ReflectiveDbConnectionFactory(typeof(SqlConnection), "\n");
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void CtorWithNullConnectionType()
		{
			new ReflectiveDbConnectionFactory(null, "a.connection.string");
		}

		[Test]
		[ExpectedException(typeof (ArgumentException))]
		public void CtorWithBadConnectionType()
		{
			new ReflectiveDbConnectionFactory(typeof(string), "a.connection.string");
		}

		[Test]
		public void CtorSunnyDay()
		{
			new ReflectiveDbConnectionFactory(typeof(SqlConnection), "a.connection.string");
		}
	}
}
