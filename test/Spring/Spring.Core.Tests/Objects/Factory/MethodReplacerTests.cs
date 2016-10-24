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
using System.Reflection;
using NUnit.Framework;
using Spring.Objects.Factory.Support;

#endregion

namespace Spring.Objects.Factory
{
	/// <summary>
	/// Unit tests for the method replacement functionality of the Spring IoC container.
	/// </summary>
	/// <remarks>
	/// <p>
	/// This encapsulates both generic method replacement and its specialization, the
	/// method lookup variant.
	/// </p>
	/// </remarks>
	/// <author>Rick Evans</author>
	[TestFixture]
	public sealed class MethodReplacerTests
	{
		[Test]
		public void SunnyDayReplaceMethod()
		{
			RootObjectDefinition replacerDef = new RootObjectDefinition(typeof (NewsFeedFactory));

			RootObjectDefinition managerDef = new RootObjectDefinition(typeof (ReturnsNullNewsFeedManager));
			managerDef.MethodOverrides.Add(new ReplacedMethodOverride("CreateNewsFeed", "replacer"));

			DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
			factory.RegisterObjectDefinition("manager", managerDef);
			factory.RegisterObjectDefinition("replacer", replacerDef);
			INewsFeedManager manager = (INewsFeedManager) factory["manager"];
			NewsFeed feed1 = manager.CreateNewsFeed();
			Assert.IsNotNull(feed1, "The CreateNewsFeed() method is not being replaced.");
			Assert.AreEqual(NewsFeedFactory.DefaultName, feed1.Name);
			NewsFeed feed2 = manager.CreateNewsFeed();
			// NewsFeedFactory always yields a new NewsFeed (see class definition below)...
			Assert.IsFalse(ReferenceEquals(feed1, feed2));
		}

        [Test]
        public void SunnyDayReplaceMethod_WithProtectedVirtual()
        {
            RootObjectDefinition replacerDef = new RootObjectDefinition(typeof(NewsFeedFactory));

            RootObjectDefinition managerDef = new RootObjectDefinition(typeof(ProtectedReturnsNullNewsFeedManagerWithVirtualMethod));
            managerDef.MethodOverrides.Add(new ReplacedMethodOverride("CreateNewsFeed", "replacer"));
            
            DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
            factory.RegisterObjectDefinition("manager", managerDef);
            factory.RegisterObjectDefinition("replacer", replacerDef);
            ProtectedReturnsNullNewsFeedManagerWithVirtualMethod manager = (ProtectedReturnsNullNewsFeedManagerWithVirtualMethod)factory["manager"];
            NewsFeed feed1 = manager.GrabNewsFeed();
            Assert.IsNotNull(feed1, "The protected CreateNewsFeed() method is not being replaced.");
            Assert.AreEqual(NewsFeedFactory.DefaultName, feed1.Name);
            NewsFeed feed2 = manager.GrabNewsFeed();
            // NewsFeedFactory always yields a new NewsFeed (see class definition below)...
            Assert.IsFalse(ReferenceEquals(feed1, feed2));
            
        }

		[Test]
		public void SunnyDayReplaceMethod_WithArgumentAcceptingReplacer()
		{
			RootObjectDefinition replacerDef = new RootObjectDefinition(typeof (NewsFeedFactory));

			RootObjectDefinition managerDef = new RootObjectDefinition(typeof (NewsFeedManagerWith_Replace_MethodThatTakesArguments));
			ReplacedMethodOverride theOverride = new ReplacedMethodOverride("CreateNewsFeed", "replacer");
			// we must specify parameter type fragments...
			theOverride.AddTypeIdentifier(typeof(string).FullName);
			managerDef.MethodOverrides.Add(theOverride);

			DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
			factory.RegisterObjectDefinition("manager", managerDef);
			factory.RegisterObjectDefinition("replacer", replacerDef);
			NewsFeedManagerWith_Replace_MethodThatTakesArguments manager = (NewsFeedManagerWith_Replace_MethodThatTakesArguments) factory["manager"];
			NewsFeed feed1 = manager.CreateNewsFeed("So sad... to be all alone in the world");
			Assert.IsNotNull(feed1, "The CreateNewsFeed() method is not being replaced.");
			Assert.AreEqual("So sad... to be all alone in the world", feed1.Name);
			NewsFeed feed2 = manager.CreateNewsFeed("Oh Muzzy!");
			// NewsFeedFactory always yields a new NewsFeed (see class definition below)...
			Assert.IsFalse(ReferenceEquals(feed1, feed2));
		}

		[Test]
		public void SunnyDayReplaceMethod_WithArgumentAcceptingReplacerWithNoOverloadingAndNoTypeFragmentsSpecified()
		{
			RootObjectDefinition replacerDef = new RootObjectDefinition(typeof (NewsFeedFactory));

			RootObjectDefinition managerDef = new RootObjectDefinition(typeof (NewsFeedManagerWith_Replace_MethodThatTakesArguments));
			ReplacedMethodOverride theOverride = new ReplacedMethodOverride("CreateNewsFeed", "replacer");
			// no need to specify parameter type fragments if we turn IsOverloaded off (the method is not overloaded)...
			theOverride.IsOverloaded = false;
			managerDef.MethodOverrides.Add(theOverride);

			DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
			factory.RegisterObjectDefinition("manager", managerDef);
			factory.RegisterObjectDefinition("replacer", replacerDef);
			NewsFeedManagerWith_Replace_MethodThatTakesArguments manager = (NewsFeedManagerWith_Replace_MethodThatTakesArguments) factory["manager"];
			NewsFeed feed1 = manager.CreateNewsFeed("So sad... to be all alone in the world");
			Assert.IsNotNull(feed1, "The CreateNewsFeed() method is not being replaced.");
			Assert.AreEqual("So sad... to be all alone in the world", feed1.Name);
			NewsFeed feed2 = manager.CreateNewsFeed("Oh Muzzy!");
			// NewsFeedFactory always yields a new NewsFeed (see class definition below)...
			Assert.IsFalse(ReferenceEquals(feed1, feed2));
		}

		[Test]
		public void SunnyDayReplaceMethod_WithReplacerThatReturnsVoid()
		{
			RootObjectDefinition replacerDef = new RootObjectDefinition(typeof (DoNothingReplacer));

			RootObjectDefinition managerDef = new RootObjectDefinition(typeof (NewsFeedManagerThatReturnsVoid));
			ReplacedMethodOverride theOverride = new ReplacedMethodOverride("DoSomething", "replacer");
			managerDef.MethodOverrides.Add(theOverride);

			DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
			factory.RegisterObjectDefinition("manager", managerDef);
			factory.RegisterObjectDefinition("replacer", replacerDef);
			NewsFeedManagerThatReturnsVoid manager = (NewsFeedManagerThatReturnsVoid) factory["manager"];
			manager.DoSomething();
		}

		/// <summary>
		/// We don't specify any type fragments, so the method overload check will never pass.
		/// </summary>
		[Test]
		public void SunnyDayReplaceMethod_WithArgumentAcceptingReplacerWithNoTypeFragmentsSpecified()
		{
			RootObjectDefinition replacerDef = new RootObjectDefinition(typeof (NewsFeedFactory));

			RootObjectDefinition managerDef = new RootObjectDefinition(typeof (NewsFeedManagerWith_Replace_MethodThatTakesArguments));
			managerDef.MethodOverrides.Add(new ReplacedMethodOverride("CreateNewsFeed", "replacer"));

			DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
			factory.RegisterObjectDefinition("manager", managerDef);
			factory.RegisterObjectDefinition("replacer", replacerDef);
			NewsFeedManagerWith_Replace_MethodThatTakesArguments manager = (NewsFeedManagerWith_Replace_MethodThatTakesArguments) factory["manager"];
            Assert.Throws<NotImplementedException>(() => manager.CreateNewsFeed("So sad... to be all alone in the world"));
		}

		/// <summary>
		/// A class that requires two lookup method injections.
		/// </summary>
		[Test]
		public void LookupMethodMultiple()
		{
			RootObjectDefinition feedDef = new RootObjectDefinition(typeof (NewsFeed));
			feedDef.IsSingleton = false;
			feedDef.PropertyValues.Add("name", "Bingo");

			RootObjectDefinition testObjectDef = new RootObjectDefinition(typeof (TestObject));
			testObjectDef.IsSingleton = false;
			testObjectDef.PropertyValues.Add("name", "Miki Nakatani");

			RootObjectDefinition managerDef = new RootObjectDefinition(typeof (TestObjectAndNewsFeedFactory));
			managerDef.MethodOverrides.Add(new LookupMethodOverride("CreateNewsFeed", "feed"));
			managerDef.MethodOverrides.Add(new LookupMethodOverride("CreateTestObject", "test"));

			DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
			factory.RegisterObjectDefinition("manager", managerDef);
			factory.RegisterObjectDefinition("feed", feedDef);
			factory.RegisterObjectDefinition("test", testObjectDef);
			TestObjectAndNewsFeedFactory manager = (TestObjectAndNewsFeedFactory) factory["manager"];

			INewsFeedManager newsFeedManager = manager;
			NewsFeed feed1 = newsFeedManager.CreateNewsFeed();
			Assert.IsNotNull(feed1, "The CreateNewsFeed() method is not being replaced.");
			NewsFeed feed2 = newsFeedManager.CreateNewsFeed();
			// assert that the object (prototype) is definitely being looked up each time...
			Assert.IsFalse(ReferenceEquals(feed1, feed2));

			ITestObjectFactory toFactory = manager;
			ITestObject to1 = toFactory.CreateTestObject();
			Assert.IsNotNull(to1, "The CreateTestObject() method is not being replaced.");
			ITestObject to2 = toFactory.CreateTestObject();
			// assert that the object (prototype) is definitely being looked up each time...
			Assert.IsFalse(ReferenceEquals(to1, to2));
		}

		/// <summary>
		/// A class that requires both lookup method and replace method injection.
		/// </summary>
		[Test]
		public void LookupAndReplaceMethod()
		{
			RootObjectDefinition replacerDef = new RootObjectDefinition(typeof (NewsFeedFactory));

			RootObjectDefinition testObjectDef = new RootObjectDefinition(typeof (TestObject));
			testObjectDef.IsSingleton = false;
			testObjectDef.PropertyValues.Add("name", "Miki Nakatani");

			RootObjectDefinition managerDef = new RootObjectDefinition(typeof (TestObjectAndNewsFeedFactory));
			managerDef.MethodOverrides.Add(new ReplacedMethodOverride("CreateNewsFeed", "replacer"));
			managerDef.MethodOverrides.Add(new LookupMethodOverride("CreateTestObject", "test"));

			DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
			factory.RegisterObjectDefinition("manager", managerDef);
			factory.RegisterObjectDefinition("replacer", replacerDef);
			factory.RegisterObjectDefinition("test", testObjectDef);
			TestObjectAndNewsFeedFactory manager = (TestObjectAndNewsFeedFactory) factory["manager"];

			INewsFeedManager newsFeedManager = manager;
			NewsFeed feed1 = newsFeedManager.CreateNewsFeed();
			Assert.IsNotNull(feed1, "The CreateNewsFeed() method is not being replaced.");
			NewsFeed feed2 = newsFeedManager.CreateNewsFeed();
			// assert that the object (prototype) is definitely being looked up each time...
			Assert.IsFalse(ReferenceEquals(feed1, feed2));

			ITestObjectFactory toFactory = manager;
			ITestObject to1 = toFactory.CreateTestObject();
			Assert.IsNotNull(to1, "The CreateTestObject() method is not being replaced.");
			ITestObject to2 = toFactory.CreateTestObject();
			// assert that the object (prototype) is definitely being looked up each time...
			Assert.IsFalse(ReferenceEquals(to1, to2));
		}

		/// <summary>
		/// Lookup method injection on an (interface) method that returns null.
		/// </summary>
		[Test]
		public void LookupMethodWithNullMethod()
		{
			try
			{
				RootObjectDefinition feedDef = new RootObjectDefinition(typeof (NewsFeed));
				feedDef.IsSingleton = false;
				feedDef.PropertyValues.Add("name", "Bingo");

				RootObjectDefinition managerDef = new RootObjectDefinition(typeof (ReturnsNullNewsFeedManager));
				managerDef.MethodOverrides.Add(new LookupMethodOverride("CreateNewsFeed", "feed"));

				DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
				factory.RegisterObjectDefinition("manager", managerDef);
				factory.RegisterObjectDefinition("feed", feedDef);
				INewsFeedManager manager = (INewsFeedManager) factory["manager"];
				NewsFeed feed1 = manager.CreateNewsFeed();
				Assert.IsNotNull(feed1, "The CreateNewsFeed() method is not being replaced.");
				NewsFeed feed2 = manager.CreateNewsFeed();
				// assert that the object (prototype) is definitely being looked up each time...
				Assert.IsFalse(ReferenceEquals(feed1, feed2));
			}
			catch (Exception ex)
			{
				Console.Out.WriteLine("ex = {0}", ex);
			}
		}

		/// <summary>
		/// Lookup method injection on an (interface) method that returns null.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The only difference from the previous (similarly named) test is that
		/// constructor arguments are passed to the (er) constructor as it (the
		/// dynamic subclass) is being instantiated by the container.
		/// </p>
		/// </remarks>
		[Test]
		public void LookupMethodWithNullMethodInstantiatedWithCtorArg()
		{
			RootObjectDefinition feedDef = new RootObjectDefinition(typeof (NewsFeed));
			feedDef.IsSingleton = false;
			feedDef.PropertyValues.Add("name", "Bingo");

			RootObjectDefinition managerDef = new RootObjectDefinition(typeof (ReturnsNullNewsFeedManager));
			managerDef.MethodOverrides.Add(new LookupMethodOverride("CreateNewsFeed", "feed"));
			managerDef.ConstructorArgumentValues.AddNamedArgumentValue("name", "Bingo");

			DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
			factory.RegisterObjectDefinition("manager", managerDef);
			factory.RegisterObjectDefinition("feed", feedDef);
			ReturnsNullNewsFeedManager manager = (ReturnsNullNewsFeedManager) factory["manager"];
			Assert.AreEqual("Bingo", manager.Name);
			NewsFeed feed1 = manager.CreateNewsFeed();
			Assert.IsNotNull(feed1, "The CreateNewsFeed() method is not being replaced.");
			NewsFeed feed2 = manager.CreateNewsFeed();
			// assert that the object (prototype) is definitely being looked up each time...
			Assert.IsFalse(ReferenceEquals(feed1, feed2));
		}

		[Test]
		public void LookupMethodWithAbstractMethod()
		{
			RootObjectDefinition feedDef = new RootObjectDefinition(typeof (NewsFeed));
			feedDef.IsSingleton = false;
			feedDef.PropertyValues.Add("name", "Bingo");

			RootObjectDefinition managerDef = new RootObjectDefinition(typeof (AbstractNewsFeedManager));
			managerDef.MethodOverrides.Add(new LookupMethodOverride("CreateNewsFeed", "feed"));

			DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
			factory.RegisterObjectDefinition("manager", managerDef);
			factory.RegisterObjectDefinition("feed", feedDef);
			INewsFeedManager manager = (INewsFeedManager) factory["manager"];
			NewsFeed feed1 = manager.CreateNewsFeed();
			Assert.IsNotNull(feed1, "The CreateNewsFeed() method is not being replaced.");
			NewsFeed feed2 = manager.CreateNewsFeed();
			// assert that the object (prototype) is definitely being looked up each time...
			Assert.IsFalse(ReferenceEquals(feed1, feed2));
		}

		/// <summary>
		/// Protected methods can also be method injected.
		/// </summary>
		[Test]
		public void LookupMethodWithVirtualProtectedMethod()
		{
			RootObjectDefinition feedDef = new RootObjectDefinition(typeof (NewsFeed));
			feedDef.IsSingleton = false;
			feedDef.PropertyValues.Add("name", "Bingo");

			RootObjectDefinition managerDef = new RootObjectDefinition(typeof (ProtectedReturnsNullNewsFeedManagerWithVirtualMethod));
			managerDef.MethodOverrides.Add(new LookupMethodOverride("CreateNewsFeed", "feed"));

			DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
			factory.RegisterObjectDefinition("manager", managerDef);
			factory.RegisterObjectDefinition("feed", feedDef);
			ProtectedReturnsNullNewsFeedManagerWithVirtualMethod manager = (ProtectedReturnsNullNewsFeedManagerWithVirtualMethod) factory["manager"];
			NewsFeed feed1 = manager.GrabNewsFeed();
			Assert.IsNotNull(feed1, "The CreateNewsFeed() method is not being replaced.");
			NewsFeed feed2 = manager.GrabNewsFeed();
			// assert that the object (prototype) is definitely being looked up each time...
			Assert.IsFalse(ReferenceEquals(feed1, feed2));
		}

		#region Rainy Day Scenarios

		[Test]
		public void FailWithLookupMethodOnSealedClass()
		{
			RootObjectDefinition feedDef = new RootObjectDefinition(typeof (NewsFeed));
			feedDef.IsSingleton = false;
			feedDef.PropertyValues.Add("name", "Bingo");

			RootObjectDefinition managerDef = new RootObjectDefinition(typeof (SealedReturnsNullNewsFeedManager));
			managerDef.MethodOverrides.Add(new LookupMethodOverride("CreateNewsFeed", "feed"));

			DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
			factory.RegisterObjectDefinition("manager", managerDef);
			factory.RegisterObjectDefinition("feed", feedDef);
            Assert.Throws<ObjectCreationException>(() => factory.GetObject("manager"));
		}

		[Test]
		public void FailWithReplaceMethodOnSealedClass()
		{
			RootObjectDefinition replacerDef = new RootObjectDefinition(typeof (NewsFeedFactory));

			RootObjectDefinition managerDef = new RootObjectDefinition(typeof (SealedReturnsNullNewsFeedManager));
			managerDef.MethodOverrides.Add(new ReplacedMethodOverride("CreateNewsFeed", "replacer"));

			DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
			factory.RegisterObjectDefinition("manager", managerDef);
			factory.RegisterObjectDefinition("replacer", replacerDef);
            Assert.Throws<ObjectCreationException>(() => factory.GetObject("manager"));
		}

		/// <summary>
		/// Tests that one cannot (obviously) override a protected method that has
		/// not been explicitly declared virtual on the original class declaration.
		/// </summary>
		[Test]
		public void FailOnNonVirtualProtectedMethod()
		{
			RootObjectDefinition feedDef = new RootObjectDefinition(typeof (NewsFeed));
			feedDef.IsSingleton = false;
			feedDef.PropertyValues.Add("name", "Bingo");

			RootObjectDefinition managerDef = new RootObjectDefinition(typeof (ProtectedReturnsNullNewsFeedManager));
			managerDef.MethodOverrides.Add(new LookupMethodOverride("CreateNewsFeed", "feed"));

			DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
			factory.RegisterObjectDefinition("manager", managerDef);
			factory.RegisterObjectDefinition("feed", feedDef);
            Assert.Throws<ObjectCreationException>(() => factory.GetObject("manager"));
		}

		/// <summary>
		/// Tests that one cannot (obviously) method inject a method with a void
		/// return type.
		/// </summary>
		[Test]
		public void FailOnMethodWithVoidReturnType()
		{
			RootObjectDefinition feedDef = new RootObjectDefinition(typeof (NewsFeed));
			feedDef.IsSingleton = false;
			feedDef.PropertyValues.Add("name", "Bingo");

			RootObjectDefinition managerDef = new RootObjectDefinition(typeof (ReturnsVoidNewsFeedManager));
			managerDef.MethodOverrides.Add(new LookupMethodOverride("CreateNewsFeed", "feed"));

			DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
			factory.RegisterObjectDefinition("manager", managerDef);
			factory.RegisterObjectDefinition("feed", feedDef);
            Assert.Throws<ObjectCreationException>(() => factory.GetObject("manager"));
		}

		/// <summary>
		/// Lookup methods cannot accept any arguments (replace methods can though).
		/// </summary>
		[Test]
		public void FailOnLookupMethodThatHasArguments()
		{
			RootObjectDefinition feedDef = new RootObjectDefinition(typeof (NewsFeed));
			feedDef.IsSingleton = false;
			feedDef.PropertyValues.Add("name", "Bingo");

			RootObjectDefinition managerDef = new RootObjectDefinition(typeof (NewsFeedManagerWithLookupMethodThatTakesArguments));
			managerDef.MethodOverrides.Add(new LookupMethodOverride("CreateNewsFeed", "feed"));

			DefaultListableObjectFactory factory = new DefaultListableObjectFactory();
			factory.RegisterObjectDefinition("manager", managerDef);
			factory.RegisterObjectDefinition("feed", feedDef);
            Assert.Throws<ObjectCreationException>(() => factory.GetObject("manager"));
		}

		#endregion
	}


	public interface INewsFeedManager
	{
		NewsFeed CreateNewsFeed();
	}

	public class ReturnsNullNewsFeedManager : INewsFeedManager
	{
		private string name;

		public ReturnsNullNewsFeedManager()
		{
		}

		public ReturnsNullNewsFeedManager(string name)
		{
			this.name = name;
		}

		public virtual NewsFeed CreateNewsFeed()
		{
			return null;
		}

		public string Name
		{
			get { return name; }
		}
	}

	public sealed class SealedReturnsNullNewsFeedManager : INewsFeedManager
	{
		public NewsFeed CreateNewsFeed()
		{
			return null;
		}
	}

	public class ProtectedReturnsNullNewsFeedManager
	{
		public NewsFeed GrabNewsFeed()
		{
			return CreateNewsFeed();
		}

		protected NewsFeed CreateNewsFeed()
		{
			return null;
		}
	}

	public class ProtectedReturnsNullNewsFeedManagerWithVirtualMethod
	{
		public NewsFeed GrabNewsFeed()
		{
			return CreateNewsFeed();
		}

		protected virtual NewsFeed CreateNewsFeed()
		{
			return null;
		}
	}

	/// <summary>
	/// Just why anyone would want to do method injection on a method with a
	/// void return type is a question best left for another (earlier) night.
	/// </summary>
	public class ReturnsVoidNewsFeedManager
	{
		public virtual void CreateNewsFeed()
		{
		}
	}

	public class NewsFeedManagerWithLookupMethodThatTakesArguments
	{
		public virtual NewsFeed CreateNewsFeed(string foo, int bar)
		{
			return null;
		}
	}

	public class NewsFeedManagerThatReturnsVoid
	{
		public virtual void DoSomething()
		{
		}
	}

	public class DoNothingReplacer : IMethodReplacer 
	{
		public object Implement(object target, MethodInfo method, params object[] arguments)
		{
			return null;
		}
	}

	public class NewsFeedManagerWith_Replace_MethodThatTakesArguments
	{
		public virtual NewsFeed CreateNewsFeed(string headline)
		{
			throw new NotImplementedException();
		}
	}

	public abstract class AbstractNewsFeedManager : INewsFeedManager
	{
		public abstract NewsFeed CreateNewsFeed();
	}

	public sealed class NewsFeedFactory : IMethodReplacer
	{
		public const string DefaultName = "All The News That's Fit To Print";

		public object Implement(object target, MethodInfo method, params object[] arguments)
		{
			if (arguments != null && arguments.Length > 0)
			{
				string headline = (string) arguments[0];
				return new NewsFeed(headline);
			}
			return new NewsFeed(DefaultName);
		}
	}

	public sealed class NewsFeed
	{
		private string name;

		public NewsFeed()
		{
		}

		public NewsFeed(string name)
		{
			this.name = name;
		}

		public string Name
		{
			get { return name; }
			set { name = value; }
		}
	}

	public interface ITestObjectFactory
	{
		ITestObject CreateTestObject();
	}

	public class TestObjectAndNewsFeedFactory : ITestObjectFactory, INewsFeedManager
	{
		public virtual ITestObject CreateTestObject()
		{
			throw new NotImplementedException();
		}

		public virtual NewsFeed CreateNewsFeed()
		{
			throw new NotImplementedException();
		}
	}
}