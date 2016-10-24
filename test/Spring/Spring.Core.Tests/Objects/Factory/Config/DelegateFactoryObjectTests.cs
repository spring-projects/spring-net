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
using Spring.Util;

#endregion

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Unit tests for the DelegateFactoryObject class.
	/// </summary>
	/// <author>Rick Evans</author>
	[TestFixture]
	public sealed class DelegateFactoryObjectTests
	{
		[Test]
		public void StaticDelegateWithInstanceMethod()
		{
			DelegateFactoryObject fob = new DelegateFactoryObject();
			fob.DelegateType = typeof (PopHandler);
			fob.TargetType = typeof (OneThirstyDude);
			fob.MethodName = "HandlePop";
			fob.IsSingleton = false;
			fob.AfterPropertiesSet();
            Assert.Throws<ArgumentException>(() => fob.GetObject());
		}

		[Test]
		public void StaticDelegate()
		{
			DelegateFactoryObject fob = new DelegateFactoryObject();
			fob.DelegateType = typeof (PopHandler);
			fob.TargetType = typeof (OneThirstyDude);
			fob.MethodName = "StaticHandlePop";
			fob.IsSingleton = false;
			fob.AfterPropertiesSet();
			PopHandler popper = (PopHandler) fob.GetObject();
			Assert.IsNotNull(popper);
			Assert.AreEqual(fob.MethodName, popper.Method.Name);
		}

		[Test]
		public void InstanceSingletonDelegate()
		{
			DelegateFactoryObject fob = new DelegateFactoryObject();
			fob.DelegateType = typeof (PopHandler);
			OneThirstyDude dude = new OneThirstyDude();
			fob.TargetObject = dude;
			fob.MethodName = "HandlePop";
			fob.AfterPropertiesSet();
			PopHandler popper = (PopHandler) fob.GetObject();
			Assert.IsNotNull(popper);
			Assert.AreEqual(fob.MethodName, popper.Method.Name);
			string soda = "The Drink Of Champions";
			popper(this, soda);
			Assert.AreEqual(soda, dude.Soda);
			PopHandler other = (PopHandler) fob.GetObject();
			Assert.IsTrue(ReferenceEquals(popper, other));
		}

		[Test]
		public void InstancePrototypeDelegate()
		{
			DelegateFactoryObject fob = new DelegateFactoryObject();
			fob.IsSingleton = false;
			fob.DelegateType = typeof (PopHandler);
			OneThirstyDude dude = new OneThirstyDude();
			fob.TargetObject = dude;
			fob.MethodName = "HandlePop";
			fob.IsSingleton = false;
			fob.AfterPropertiesSet();
			PopHandler one = (PopHandler) fob.GetObject();
			PopHandler two = (PopHandler) fob.GetObject();
			Assert.IsFalse(ReferenceEquals(one, two));
		}

		[Test]
		public void ObjectType()
		{
			DelegateFactoryObject fob = new DelegateFactoryObject();
			Assert.IsNotNull(fob.ObjectType, "Should never be null (should default to typeof(Delegate)) ");
			Assert.AreEqual(typeof (Delegate), fob.ObjectType, "Not defaulting to typeof(Delegate).");
		}

		[Test]
		public void MissingDelegateType()
		{
			DelegateFactoryObject fob = new DelegateFactoryObject();
            Assert.Throws<ArgumentException>(() => fob.AfterPropertiesSet());
		}

		[Test]
		public void BadDelegateType()
		{
			DelegateFactoryObject fob = new DelegateFactoryObject();
			fob.DelegateType = DBNull.Value.GetType();
            Assert.Throws<ArgumentException>(() => fob.AfterPropertiesSet());
		}

		[Test]
		public void NullMethodName()
		{
			DelegateFactoryObject fob = new DelegateFactoryObject();
            fob.DelegateType = typeof (EventHandler);
            fob.TargetType = typeof (OneThirstyDude);
            Assert.Throws<ArgumentException>(() => fob.AfterPropertiesSet());
        }

        [Test]
        public void EmptyMethodName()
        {
            DelegateFactoryObject fob = new DelegateFactoryObject();
            fob.DelegateType = typeof (EventHandler);
            fob.TargetType = typeof (OneThirstyDude);
            fob.MethodName = string.Empty;
            Assert.Throws<ArgumentException>(() => fob.AfterPropertiesSet());
        }

        [Test]
        public void WhitespacedMethodName()
        {
            DelegateFactoryObject fob = new DelegateFactoryObject();
            fob.DelegateType = typeof (EventHandler);
            fob.TargetType = typeof (OneThirstyDude);
            fob.MethodName = "\n";
            Assert.Throws<ArgumentException>(() => fob.AfterPropertiesSet());
        }

		[Test]
		public void MissingATarget()
		{
			DelegateFactoryObject fob = new DelegateFactoryObject();
			fob.DelegateType = typeof (EventHandler);
			fob.MethodName = "I Love You Laura Palmer, I Really Do";
            Assert.Throws<ArgumentException>(() => fob.AfterPropertiesSet());
		}

		[Test]
		public void ChokesIfBothTargetTypeAndTargetObjectSupplied()
		{
			DelegateFactoryObject fob = new DelegateFactoryObject();
			fob.DelegateType = typeof (PopHandler);
			fob.TargetType = typeof (OneThirstyDude);
			fob.TargetObject = new OneThirstyDude();
			fob.MethodName = "I Love You Laura Palmer, I Really Do";
            Assert.Throws<ArgumentException>(() => fob.AfterPropertiesSet());
		}
	}
}