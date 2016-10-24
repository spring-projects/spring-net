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
using System.Reflection;

#endregion

namespace Spring.Aop.Support
{
	/// <summary>
	/// Unit tests for the AttributeMatchMethodPointcut class.
	/// </summary>
	/// <author>Rick Evans</author>
    /// <author>Ronald Wildenberg</author>
	[TestFixture]
	public sealed class AttributeMatchMethodPointcutTests
	{
		[Test]
		public void InstantiationWithASunnyDayAttributeType()
		{
			new AttributeMatchMethodPointcut(typeof(SerializableAttribute));
		}

		[Test]
		public void InstantiationWithNonAttributeType()
		{
            Assert.Throws<ArgumentException>(() => new AttributeMatchMethodPointcut(GetType()));
		}

		[Test]
		public void AttributeSetterWithNonAttributeType()
		{
			AttributeMatchMethodPointcut cut = new AttributeMatchMethodPointcut();
            Assert.Throws<ArgumentException>(() => cut.Attribute = GetType());
		}

		[Test]
		public void AttributeSetterWithNullType()
		{
			AttributeMatchMethodPointcut cut = new AttributeMatchMethodPointcut();
			cut.Attribute = null; // must allow this (no Exception)...
		}

		[Test]
		public void AttributeSetterWithASunnyDayAttributeType()
		{
			AttributeMatchMethodPointcut cut = new AttributeMatchMethodPointcut();
			cut.Attribute = typeof(SerializableAttribute); // must allow this too (no Exception)...
		}

		[Test]
		public void MatchesWithASunnyDayAttributeTypeAndNoInheritance()
		{
			AttributeMatchMethodPointcut cut = new AttributeMatchMethodPointcut();
			cut.Attribute = typeof(MarkupAttribute);
			bool matches = cut.Matches(typeof(WithMarkup).GetMethod("Bing"), null);
			Assert.IsTrue(matches, "Method was decorated with the target attribute, so this must match.");
		}

		[Test]
		public void MatchesWithASunnyDayAttributeTypeAndInheritance()
		{
			AttributeMatchMethodPointcut cut = new AttributeMatchMethodPointcut();
			cut.Attribute = typeof(MarkupAttribute);
			bool matches = cut.Matches(typeof(InheritedWithMarkup).GetMethod("Bing"), null);
			Assert.IsTrue(matches, "Inherited method was decorated with the target attribute, so this must match.");
		}

		[Test]
		public void MatchesWithAMethodThatDontMatchTheAttributeTypeAndNoInheritance()
		{
			AttributeMatchMethodPointcut cut = new AttributeMatchMethodPointcut();
			cut.Attribute = typeof(MarkupAttribute);
			bool matches = cut.Matches(typeof(WithMarkup).GetMethod("RiloKiley"), null);
			Assert.IsFalse(matches, "Method was not decorated with the target attribute, so this must not match.");
		}

		[Test]
		public void MatchesWithAnInheritedMethodThatDontMatchTheAttributeTypeAndNoInheritance()
		{
			AttributeMatchMethodPointcut cut = new AttributeMatchMethodPointcut();
			cut.Attribute = typeof(MarkupAttribute);
			bool matches = cut.Matches(typeof(InheritedWithMarkup).GetMethod("RiloKiley"), null);
			Assert.IsFalse(matches, "Inherited method was not decorated with the target attribute, so this must not match.");
		}

        /// <summary>
        /// Confirms that without interfaces checking, a method that is implemented from an interface, will not match.
        /// </summary>
        [Test]
        public void MatchesWithAnInterfaceMethodThatMatchesTheAttributeTypeAndNoCheckInterfaces()
        {
            AttributeMatchMethodPointcut cut = new AttributeMatchMethodPointcut();
            cut.Attribute = typeof(MarkupAttribute);
            cut.CheckInterfaces = false;
            bool matches = cut.Matches(typeof(ImplementingClass).GetMethod("OtherTestMethod"), null);
            Assert.IsFalse(matches, "Implementing method was not decorated with the target attribute, so this must not match since CheckInterfaces is false.");
        }

        /// <summary>
        /// Confirms that with interface checking, a method that is implementing an interface method 
        /// where the attribute is defined will match.
        /// </summary>
        [Test]
        public void MatchesWithAnInterfaceMethodThatMatchesTheAttributeTypeAndCheckInterfaces()
        {
            AttributeMatchMethodPointcut cut = new AttributeMatchMethodPointcut();
            cut.Attribute = typeof(MarkupAttribute);
            cut.CheckInterfaces = true;
            bool matches = cut.Matches(typeof(ImplementingClass).GetMethod("OtherTestMethod"), null);
            Assert.IsTrue(matches, "Implementing method was not decorated with the target attribute, " + 
                "but the method from the interface was, so this must match.");
        }

        /// <summary>
        /// Confirms that with interfaces checking, a method that is indirectly implementing an interface method
        /// where the attribute is defined will match.
        /// </summary>
        [Test]
        public void MatchesWithAnIndirectInterfaceMethodThatMatchesTheAttributeTypeAndCheckInterfaces()
        {
            AttributeMatchMethodPointcut cut = new AttributeMatchMethodPointcut();
            cut.Attribute = typeof(MarkupAttribute);
            cut.CheckInterfaces = true;
            bool matches = cut.Matches(typeof(ImplementingClass).GetMethod("TestMethod", new Type[] { }), null);
            Assert.IsTrue(matches, "Implementing method was not decorated with the target attribute, but the" +
                " method from an indirectly implemented interface was, so this must match.");
        }

        /// <summary>
        /// Confirms that overloading methods do not match, whatever the attributes.
        /// </summary>
        [Test]
        public void MatchesWithAnOverloadedInterfaceMethod()
        {
            AttributeMatchMethodPointcut cut = new AttributeMatchMethodPointcut();
            cut.Attribute = typeof(MarkupAttribute);
            cut.CheckInterfaces = true;
            bool matches = cut.Matches(typeof(ImplementingClass).GetMethod("TestMethod", new Type[] { typeof(string) }), null);
            Assert.IsFalse(matches, "Overloaded method from an implemented interface is not decorated with" +
                " the attribute, so should not match.");
        }

        /// <summary>
        /// Confirms that methods, defined in a subclass of a class that implements an interface that
        /// has methods that have been decorated with an attribute, match.
        /// </summary>
        [Test]
        public void MatchesWithAnIndirectInterfaceMethodFromSubclassThatMatchesTheAttributeTypeAndCheckInterfaces()
        {
            AttributeMatchMethodPointcut cut = new AttributeMatchMethodPointcut();
            cut.Attribute = typeof(MarkupAttribute);
            cut.CheckInterfaces = true;
            bool matches = cut.Matches(typeof(InheritedImplementingClass).GetMethod("TestMethod", new Type[] { }), null);
            Assert.IsTrue(matches, "Implementing method from subclass was not decorated with the target attribute " +
                "but the method from an indirectly implemented interface was, so this must match.");
        }

        /// <summary>
        /// Confirms that methods, explicitly implemented in the derived classes will match
        /// </summary>
        [Test(Description="SPRNET-1314")]
        public void MatchesWhenExplicitlyImplemed()
        {
            AttributeMatchMethodPointcut cut = new AttributeMatchMethodPointcut();
            cut.Attribute = typeof(MarkupAttribute);
            cut.CheckInterfaces = true;

            // Only methods implemented expicitly are marked with attribute
            foreach (MethodInfo mi in typeof(ExplicitlyImplementingClass).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)) 
            {
                if (mi.Name.IndexOf('.') == -1) continue;
                bool matches = cut.Matches(mi, null);
                Assert.IsTrue(matches, "Explicitly implemented method must match");
            }

        }

        #region Helper classes definitions

        [AttributeUsage(AttributeTargets.Method)]
		private sealed class MarkupAttribute : Attribute {}

		private class WithMarkup 
		{
			[Markup]
			public void Bing() {}

			public void RiloKiley() {}
		}

		private sealed class InheritedWithMarkup : WithMarkup
		{
        }

        private interface SuperInterface
        {
            [Markup]
            void TestMethod();

            void TestMethod(string param);
        }

        private interface SubInterface : SuperInterface
        {
            [Markup]
            void OtherTestMethod();
        }

        private interface AnotherSuperInterface
        {
            [Markup]
            void TestMethod();
        }

        private class ImplementingClass : SubInterface
        {
            public void TestMethod() {}

            public void TestMethod(string param) {}

            public void OtherTestMethod() {}
        }

        private sealed class InheritedImplementingClass : ImplementingClass
        {
        }

        private class ExplicitlyImplementingClass : SuperInterface, AnotherSuperInterface
        {
            void AnotherSuperInterface.TestMethod() { }

            void SuperInterface.TestMethod() { }

            public void TestMethod(string param) {}
        }

        #endregion
    }
}