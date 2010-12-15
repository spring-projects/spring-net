#region License

/*
 * Copyright © 2002-2010 the original author or authors.
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

using System.Text.RegularExpressions;
using NUnit.Framework;

#endregion

namespace Spring.Core
{
	[TestFixture]
	public class ComposedCriteriaTests : ComposedCriteria
	{
		public ComposedCriteriaTests() : base() {}
		public ComposedCriteriaTests( ICriteria criteria ) : base( criteria ) {}
		[Test]
		public void IsSatisfiedWithNoCriteria()
		{
			ComposedCriteriaTests composedCriteria = new ComposedCriteriaTests();
			Assert.IsTrue(composedCriteria.IsSatisfied("foo"));
		}

		[Test]
		public void SatifiesMyUpperCaseCriteria()
		{
			ComposedCriteriaTests composedCriteria = new ComposedCriteriaTests(new MyUpperCaseCriteria());
			Assert.IsTrue(composedCriteria.IsSatisfied("HELLO"));
			Assert.IsFalse(composedCriteria.IsSatisfied("hello"));
		}

		[Test]
		public void SatifiesTwoCriteria()
		{
			ComposedCriteriaTests composedCriteria = new ComposedCriteriaTests();
			composedCriteria.Add(new MyUpperCaseCriteria());
			composedCriteria.Add(new MyStringCriteria());
			Assert.IsTrue(composedCriteria.IsSatisfied("HELLO"));
			Assert.IsFalse(composedCriteria.IsSatisfied("GOODBYE"));
			Assert.IsTrue( composedCriteria.Criteria.Count == 2 );
		}

		internal class MyUpperCaseCriteria : ICriteria
		{
			public bool IsSatisfied(object datum)
			{
				return Regex.Match((string) datum, "[A-Z]").Success;
			}
		}

		internal class MyStringCriteria : ICriteria
		{
			public bool IsSatisfied(object datum)
			{
				return datum.ToString().ToLower() == "hello";
			}
		}

	}
}