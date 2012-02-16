#region License
/*
 * Copyright 2002-2010 the original author or authors.
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

using Spring.Util;

using NUnit.Framework;
#endregion

namespace Spring.Aop.Support
{
	/// <summary>
	/// Unit tests for the AbstractRegularExpressionMethodPointcut class.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Dmitriy Kopylenko</author>
	/// <author>Simon White (.NET)</author>
	public abstract class AbstractRegularExpressionMethodPointcutTests
	{
		private AbstractRegularExpressionMethodPointcut pointcut;

		[SetUp]
		protected void SetUp()
		{
			pointcut = GetRegexpMethodPointcut();
		}

		protected abstract AbstractRegularExpressionMethodPointcut GetRegexpMethodPointcut();

		[Test]
		public void NoPatternSupplied()
		{
			NoPatternSuppliedTests(pointcut);
		}

		[Test]
		public void SerializationWithNoPatternSupplied()
		{
			pointcut = (AbstractRegularExpressionMethodPointcut) SerializationTestUtils.SerializeAndDeserialize(pointcut);
			NoPatternSuppliedTests(pointcut);
		}

		protected void NoPatternSuppliedTests(AbstractRegularExpressionMethodPointcut rpc)
		{
			Assert.IsFalse(rpc.Matches(typeof(object).GetMethod("GetHashCode"), typeof(int)));
			Assert.IsFalse(rpc.Matches(typeof(object).GetMethod("GetType"), typeof(Type)));
			Assert.AreEqual(0, rpc.Patterns.Length);
		}

		[Test]
		public void ExactMatch()
		{
			pointcut.Pattern = "System.Object.GetHashCode";
			ExactMatchTests(pointcut);
			pointcut = (AbstractRegularExpressionMethodPointcut) SerializationTestUtils.SerializeAndDeserialize(pointcut);
			ExactMatchTests(pointcut);
		}


        protected void ExactMatchWithGenericTypeTests(AbstractRegularExpressionMethodPointcut rpc)
        {
            // assumes rpc.setPattern("System.Collections.Generic.List<string>");
            Assert.IsTrue(rpc.Matches(typeof(System.Collections.Generic.List<string>).GetMethod("Add"), typeof(int)));
            Assert.IsFalse(rpc.Matches(typeof(System.Collections.Generic.List<string>).GetMethod("GetType"), typeof(Type)));
        }

        [Test]
        public void ExactMatchWithGenericType()
        {
            pointcut.Pattern = "System.Collections.Generic.List<string>.Add";
            ExactMatchWithGenericTypeTests(pointcut);
            pointcut = (AbstractRegularExpressionMethodPointcut)SerializationTestUtils.SerializeAndDeserialize(pointcut);
            ExactMatchWithGenericTypeTests(pointcut);
        }

        [Test]
        public void WildcardWithGenericType()
        {
            pointcut.Pattern = ".*List<string>.Add";
            Assert.IsTrue(pointcut.Matches(typeof(System.Collections.Generic.List<string>).GetMethod("Add"), typeof(int)));
            Assert.IsFalse(pointcut.Matches(typeof(System.Collections.Generic.List<string>).GetMethod("GetType"), typeof(Type)));
        }

        [Test]
        public void WildcardForOneClassWithGenericType()
        {
            pointcut.Pattern = "System.Collections.*";
            Assert.IsTrue(pointcut.Matches(typeof(System.Collections.Generic.List<string>).GetMethod("Add"), typeof(int)));
            Assert.IsFalse(pointcut.Matches(typeof(System.Collections.Generic.List<string>).GetMethod("GetType"), typeof(Type)));
        }

        protected void ExactMatchTests(AbstractRegularExpressionMethodPointcut rpc)
        {
            // assumes rpc.setPattern("java.lang.Object.hashCode");
            Assert.IsTrue(rpc.Matches(typeof(object).GetMethod("GetHashCode"), typeof(int)));
            Assert.IsFalse(rpc.Matches(typeof(object).GetMethod("GetType"), typeof(Type)));
        }

		[Test]
		public void Wildcard()
		{
			pointcut.Pattern = ".*Object.GetHashCode";
			Assert.IsTrue(pointcut.Matches(typeof(object).GetMethod("GetHashCode"), typeof(int)));
			Assert.IsFalse(pointcut.Matches(typeof(object).GetMethod("GetType"), typeof(Type)));
		}
        
		[Test]
		public void WildcardForOneClass()
		{
			pointcut.Pattern = "System.Object.*";
			Assert.IsTrue(pointcut.Matches(typeof(object).GetMethod("GetHashCode"), typeof(int)));
			Assert.IsTrue(pointcut.Matches(typeof(object).GetMethod("GetType"), typeof(Type)));
		}

		[Test]
		public void MatchesObjectClass()
		{
			pointcut.Pattern = "Object.*";
			Assert.IsTrue(pointcut.Matches(typeof(Exception).GetMethod("GetHashCode"), typeof(TargetException)));
			// Doesn't match
			Assert.IsFalse(pointcut.Matches(typeof(Exception).GetMethod("ToString"), typeof(Exception)));
		}

	}
}
