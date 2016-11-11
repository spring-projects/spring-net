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
using NUnit.Framework;

#endregion

namespace SpringAir.Data.Ado
{
	/// <summary>
	/// Integration tests for the ADO.NET-backed FlightDao class.
	/// </summary>
	/// <author>Rick Evans (.NET)</author>
	/// <version>$Id: FlightDaoTests.cs,v 1.1 2005/10/30 09:06:40 springboy Exp $</version>
	[TestFixture]
	public sealed class FlightDaoTests
	{
		[Test]
		public void CtorWithNullAircraftDao()
		{
            Assert.Throws<ArgumentNullException>(() => new FlightDao(null));
		}
	}
}