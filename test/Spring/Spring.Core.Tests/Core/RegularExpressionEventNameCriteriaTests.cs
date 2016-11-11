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

#region Imports

using System.Reflection;

using Spring.Objects;

using NUnit.Framework;

#endregion

namespace Spring.Core
{
	/// <summary>
	/// Unit tests for the RegularExpressionEventNameCriteria class.
    /// </summary>
	[TestFixture]
    public class RegularExpressionEventNameCriteriaTests
    {
        [Test]
        public void IsSatisfied () {
        	RegularExpressionEventNameCriteria criteria = new RegularExpressionEventNameCriteria("Click");
            EventInfo evt = typeof(TestObject).GetEvent("Click");
            Assert.IsTrue (criteria.IsSatisfied (evt));
        }

        [Test]
        public void IsNotSatisfiedWithGarbage () 
        {
        	RegularExpressionEventNameCriteria criteria = new RegularExpressionEventNameCriteria ("BingoBango");
            EventInfo evt = typeof(TestObject).GetEvent("Click");
            Assert.IsFalse (criteria.IsSatisfied (evt));
        }

        [Test]
        public void IsNotSatisfiedWithNull () 
        {
            RegularExpressionEventNameCriteria criteria = new RegularExpressionEventNameCriteria("Click");
            Assert.IsFalse (criteria.IsSatisfied (null));
        }

        [Test]
        public void IsSatisfiedWithAnythingByDefault () 
        {
        	RegularExpressionEventNameCriteria criteria = new RegularExpressionEventNameCriteria ();
            EventInfo evt = typeof (TestObject).GetEvent ("Click");
            Assert.IsTrue (criteria.IsSatisfied (evt));
        }
	}
}
