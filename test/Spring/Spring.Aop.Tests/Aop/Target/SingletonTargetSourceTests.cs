#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

namespace Spring.Aop.Target
{
	/// <summary>
	/// Unit tests for the SingletonTargetSource class.
	/// </summary>
	/// <author>Rick Evans</author>
	[TestFixture]
	public sealed class SingletonTargetSourceTests
	{
		[Test]
		public void InstantiationWithNullTargetSource()
		{
            Assert.Throws<ArgumentNullException>(() => new SingletonTargetSource(null));
		}

		[Test]
		public void TargetType()
		{
			SingletonTargetSource source = new SingletonTargetSource(this);
			Assert.AreEqual(GetType(), source.TargetType, "Wrong TargetType being returned.");
		}

		[Test]
		public void IsStatic()
		{
			SingletonTargetSource source = new SingletonTargetSource(this);
			Assert.IsTrue(source.IsStatic, "Must be static.");
		}

		[Test]
		public void GetTarget()
		{
			SingletonTargetSource source = new SingletonTargetSource(this);
			Assert.IsTrue(object.ReferenceEquals(source.GetTarget(), this),
				"Same target source reference not being returned by GetTarget().");
		}

		[Test]
		public void EqualsSameInstance()
		{
			SingletonTargetSource lhs = new SingletonTargetSource(this);
			SingletonTargetSource rhs = new SingletonTargetSource(this);
			Assert.AreEqual(lhs, rhs, "Equals() not correct for same instance comparison.");
		}

		[Test]
		public void EqualsNullInstance()
		{
			SingletonTargetSource lhs = new SingletonTargetSource(this);
			Assert.IsFalse(lhs.Equals(null), "Equals() not correct for null instance comparison.");
		}
	}
}