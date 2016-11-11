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

using NUnit.Framework;

#endregion

namespace Spring.Core
{
	/// <summary>
	/// Unit tests for the MethodReturnTypeCriteria class.
    /// </summary>
	[TestFixture]
    public sealed class MethodReturnTypeCriteriaTests
    {
        [Test]
        public void IsSatisfied () {
            MethodReturnTypeCriteria criteria = new MethodReturnTypeCriteria (typeof (bool));
            MethodInfo method = GetType ().GetMethod ("SomeKindOfWonderful", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsTrue (criteria.IsSatisfied (method));
        }

        private bool SomeKindOfWonderful () 
        {
            return true;
        }

        [Test]
        public void IsSatisfiedWithVoidByDefault () 
        {
            MethodReturnTypeCriteria criteria = new MethodReturnTypeCriteria ();
            MethodInfo method = GetType ().GetMethod ("IsSatisfied");
            Assert.IsTrue (criteria.IsSatisfied (method));
        }
        
        [Test]
        public void IsNotSatisfiedWithNull () {
            MethodReturnTypeCriteria criteria = new MethodReturnTypeCriteria ();
            Assert.IsFalse (criteria.IsSatisfied (null));
        }
	}
}
