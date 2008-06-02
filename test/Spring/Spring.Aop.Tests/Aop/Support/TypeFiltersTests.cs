#region License

/*
 * Copyright 2002-2004 the original author or authors.
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
using Spring.Objects;

#endregion

namespace Spring.Aop.Support
{
	/// <summary>
	/// Unit tests for the TypeFilters class.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Choy Rim (.NET)</author>
	[TestFixture]
	public sealed class TypeFiltersTests
	{
		private ITypeFilter exceptionFilter = new RootTypeFilter(typeof (Exception));
		private ITypeFilter itoFilter = new RootTypeFilter(typeof (ITestObject));
		private ITypeFilter hasRootCauseFilter = new RootTypeFilter(typeof (StackOverflowException));

		[Test]
		public void Union()
		{
			Assert.IsTrue(exceptionFilter.Matches(typeof (SystemException)));
			Assert.IsFalse(exceptionFilter.Matches(typeof (TestObject)));
			Assert.IsFalse(itoFilter.Matches(typeof (Exception)));
			Assert.IsTrue(itoFilter.Matches(typeof (TestObject)));
			ITypeFilter union = TypeFilters.Union(exceptionFilter, itoFilter);
			Assert.IsTrue(union.Matches(typeof (SystemException)));
			Assert.IsTrue(union.Matches(typeof (TestObject)));
		}

		[Test]
		public void Intersection()
		{
			Assert.IsTrue(exceptionFilter.Matches(typeof (SystemException)));
			Assert.IsTrue(hasRootCauseFilter.Matches(typeof (StackOverflowException)));

			ITypeFilter intersection = TypeFilters.Intersection(exceptionFilter, hasRootCauseFilter);
			Assert.IsFalse(intersection.Matches(typeof (SystemException)));
			Assert.IsFalse(intersection.Matches(typeof (TestObject)));
			Assert.IsTrue(intersection.Matches(typeof (StackOverflowException)));
		}
	}
}