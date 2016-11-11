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

using System;
using System.Reflection;

using NUnit.Framework;

#endregion

namespace Spring.Core
{
	/// <summary>
	/// Unit tests for the MethodParametersCountCriteria class.
    /// </summary>
    /// <author>Rick Evans</author>
	[TestFixture]
    public sealed class MethodParametersCountCriteriaTests
    {
        [Test]
        public void Instantiation () 
        {
            MethodParametersCountCriteria criteria = new MethodParametersCountCriteria ();
            Assert.AreEqual (0, criteria.ExpectedParameterCount);
            criteria = new MethodParametersCountCriteria (10);
            Assert.AreEqual (10, criteria.ExpectedParameterCount);
        }

        [Test]
        public void InstantiationBailsWithParameterCountSetToLessThanZero () 
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new MethodParametersCountCriteria (-10));
        }

        [Test]
        public void BailsWhenExpectedParameterCountSetToLessThanZero () 
        {
            MethodParametersCountCriteria criteria = new MethodParametersCountCriteria ();
            Assert.Throws<ArgumentOutOfRangeException>(() => criteria.ExpectedParameterCount = -12);
        }

        [Test]
        public void IsSatisfiedWithNoParameter () 
        {
            MethodParametersCountCriteria criteria = new MethodParametersCountCriteria ();
            MethodInfo method = GetType ().GetMethod ("NoParameter", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsTrue (criteria.IsSatisfied (method));

            criteria = new MethodParametersCountCriteria(0);
            method = GetType().GetMethod("NoParameter", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsTrue(criteria.IsSatisfied(method));

            criteria = new MethodParametersCountCriteria();
            criteria.ExpectedParameterCount = 0;
            method = GetType().GetMethod("NoParameter", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsTrue(criteria.IsSatisfied(method));
        }

        [Test]
        public void IsSatisfiedWithOneParameter () 
        {
            MethodParametersCountCriteria criteria = new MethodParametersCountCriteria (1);
            MethodInfo method = GetType().GetMethod("OneParameter", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsTrue (criteria.IsSatisfied (method));
        }

        [Test]
        public void IsSatisfiedWithTwoParameters () 
        {
            MethodParametersCountCriteria criteria = new MethodParametersCountCriteria(2);
            MethodInfo method = GetType().GetMethod("TwoParameters", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsTrue (criteria.IsSatisfied (method));
        }

        [Test]
        public void IsSatisfiedWithParamsParameters () 
        {
            MethodParametersCountCriteria criteria = new MethodParametersCountCriteria(1);
            MethodInfo method = GetType().GetMethod("ParamsParameters", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsFalse(criteria.IsSatisfied(method));

            criteria = new MethodParametersCountCriteria(2);
            method = GetType().GetMethod("ParamsParameters", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsTrue(criteria.IsSatisfied(method));

            criteria = new MethodParametersCountCriteria (3);
            method = GetType().GetMethod("ParamsParameters", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsTrue (criteria.IsSatisfied (method));

            criteria = new MethodParametersCountCriteria(5);
            method = GetType().GetMethod("ParamsParameters", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsTrue(criteria.IsSatisfied(method));
        }

        [Test]
        public void IsNotSatisfiedWithNull () 
        {
            MethodParametersCountCriteria criteria = new MethodParametersCountCriteria ();
            Assert.IsFalse (criteria.IsSatisfied (null));
        }

        // some methods for testing signatures...
        public void NoParameter () 
        {
        }

        public void OneParameter (int foo) 
        {
        }

        public void TwoParameters (int foo, int bar) 
        {
        }

        public void ParamsParameters(int foo, int bar, params string[] strs)
        {
        }
	}
}
