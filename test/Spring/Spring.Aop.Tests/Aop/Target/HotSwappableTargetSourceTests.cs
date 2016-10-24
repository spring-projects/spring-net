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
using Spring.Objects.Factory;
using Spring.Objects.Factory.Xml;

#endregion

namespace Spring.Aop.Target
{
	/// <summary>
	/// Unit tests for the HotSwappableTargetSource class.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Federico Spinazzi (.NET)</author>
	[TestFixture]
	public sealed class HotSwappableTargetSourceTests
	{
		/// <summary>Initial count value set in Object factory XML </summary>
		private const int INITIAL_COUNT = 10;

		private IObjectFactory ObjectFactory;

		[SetUp]
		public void SetUp()
		{
			this.ObjectFactory = new XmlObjectFactory(
				new ReadOnlyXmlTestResource("hotSwapTests.xml", GetType()));
		}

		/// <summary>
		/// We must simulate container shutdown, which should clear threads.
		/// </summary>
		[TearDown]
		public void TearDown()
		{
			this.ObjectFactory.Dispose();
		}

		[Test]
		[Category("Integration")]
		public void ValidSwaps()
		{
			ISideEffectObject target1 = (ISideEffectObject) ObjectFactory.GetObject("target1");
			ISideEffectObject target2 = (ISideEffectObject) ObjectFactory.GetObject("target2");

			ISideEffectObject proxied = (ISideEffectObject) ObjectFactory.GetObject("swappable");
			//	assertEquals(target1, ((Advised) proxied).getTarget());
			Assert.AreEqual(target1.Count, proxied.Count);
			proxied.doWork();
			Assert.AreEqual(INITIAL_COUNT + 1, proxied.Count);

			HotSwappableTargetSource swapper = (HotSwappableTargetSource) ObjectFactory.GetObject("swapper");
			Object old = swapper.Swap(target2);
			Assert.AreEqual(target1, old, "Correct old target was returned");

			// TODO should be able to make this assertion: need to fix target handling
			// in AdvisedSupport
			//assertEquals(target2, ((Advised) proxied).getTarget());

			Assert.AreEqual(20, proxied.Count);
			proxied.doWork();
			Assert.AreEqual(21, target2.Count);

			// Swap it back
			swapper.Swap(target1);
			Assert.AreEqual(target1.Count, proxied.Count);
		}

		[Test]
		public void RejectsSwapToNull()
		{
			HotSwappableTargetSource source = new HotSwappableTargetSource(null);
            Assert.Throws<ArgumentNullException>(() => source.Swap(null));
		}

		[Test]
		public void SwapDoesIndeedReturnTheOldTarget()
		{
			HotSwappableTargetSource source = new HotSwappableTargetSource(this);
			object foo = source.Swap(new SideEffectObject());
			Assert.IsTrue(object.ReferenceEquals(this, foo),
				"Swap() is not returning the old target.");
		}

		[Test]
		public void InstantiationWithNullIsOk()
		{
			new HotSwappableTargetSource(null);
		}

		[Test]
		public void InstantiationWithInitialTarget()
		{
			HotSwappableTargetSource source = new HotSwappableTargetSource(this);
			object foo = source.GetTarget();
			Assert.IsTrue(object.ReferenceEquals(this, foo),
				"Ctor is not storing the supplied target.");
		}
	}
}