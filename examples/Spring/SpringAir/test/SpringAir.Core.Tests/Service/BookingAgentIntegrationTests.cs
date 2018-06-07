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

using NUnit.Framework;

#endregion

namespace SpringAir.Service
{
	/// <summary>
	/// Integration tests for the IBookingAgent service.
	/// </summary>
	/// <author>Keith Donald</author>
	/// <author>Rick Evans (.NET)</author>
	/// <version>$Id: BookingAgentIntegrationTests.cs,v 1.1 2005/07/26 20:10:23 springboy Exp $</version>
	[TestFixture]
    public sealed class BookingAgentIntegrationTests
    {
        #region SetUp
        
        /// <summary>
        /// The setup logic executed before the execution of this test fixture.
        /// </summary>
        [OneTimeSetUp]
        public void FixtureSetUp()
        {
        }

        /// <summary>
        /// The setup logic executed before the execution of each individual test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
        }
        
        #endregion

        #region TearDown
        
        /// <summary>
        /// The tear down logic executed after the execution of each individual test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
        }

        /// <summary>
        /// The tear down logic executed after the entire test fixture has executed.
        /// </summary>
        [OneTimeTearDown]
        public void FixtureTearDown()
        {
        }
        
        #endregion

        [Test]
        public void SuggestItinerariesSuccess() {

		}

		[Test]
		public void SuggestItinerariesWhenNoneExist() 
		{

		}
	}
}
