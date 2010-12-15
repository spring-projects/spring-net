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

using System;
using System.Security.Policy;
using NUnit.Framework;

namespace Spring.Context.Support
{
	[TestFixture]
	public sealed class ApplicationContextAwareProcessorTests
	{
		[Test]
		public void AttachResourceLoader()
		{
			MockApplicationContext ctx = new MockApplicationContext("MockApplicationContext");
			ApplicationContextAwareProcessor processor = new ApplicationContextAwareProcessor(ctx);
			MockContextAwareObject obj = new MockContextAwareObject();
			Assert.IsNull(obj.ResourceLoader, "ResourceLoader Does Not Equal");
			MockContextAwareObject obj2 = (MockContextAwareObject) processor.PostProcessBeforeInitialization(obj, "MyContextAwareObject");
			Assert.AreEqual(ctx, obj2.ResourceLoader, "ResourceLoader Does Not Equal");
		}

		[Test]
		public void DoNotAttachResourceLoaderForRegularObject()
		{
			MockApplicationContext ctx = new MockApplicationContext();
			ApplicationContextAwareProcessor processor = new ApplicationContextAwareProcessor(ctx);
			object obj = new object();
			object obj1 = processor.PostProcessBeforeInitialization(obj, "MyContextAwareObject");
			Assert.AreEqual(obj, obj1, "Objects don't equal");
		}

		[Test]
		public void AttachContext()
		{
			MockApplicationContext ctx = new MockApplicationContext();
			ApplicationContextAwareProcessor processor = new ApplicationContextAwareProcessor(ctx);
			MockContextAwareObject obj = new MockContextAwareObject();
			Assert.IsNull(obj.GetApplicationContext(), "Context Does Not Equal");
			MockContextAwareObject obj2 = (MockContextAwareObject) processor.PostProcessBeforeInitialization(obj, "MyContextAwareObject");
			Assert.AreEqual(ctx, obj2.GetApplicationContext(), "Context Does Not Equal");
		}

		[Test]
		public void DoNotAttachContextForRegularObject()
		{
			MockApplicationContext ctx = new MockApplicationContext();
			ApplicationContextAwareProcessor processor = new ApplicationContextAwareProcessor(ctx);
			object obj = new object();
			object obj1 = processor.PostProcessBeforeInitialization(obj, "MyContextAwareObject");
			Assert.AreEqual(obj, obj1, "Objects don't equal");
		}

		[Test]
		public void AfterInitReturnsSameInstanceAsWasPassedIn()
		{
			MockApplicationContext ctx = new MockApplicationContext();
			ApplicationContextAwareProcessor processor = new ApplicationContextAwareProcessor(ctx);
			object obj = new object();
			object obj1 = processor.PostProcessAfterInitialization(obj, "MyContextAwareObject");
			Assert.AreEqual(obj, obj1, "Objects don't equal");
		}

		[Test]
		public void AlwaysIgnoresProxiedMessageSourceAwareObjects()
		{
			PostProcessTProxiedObject(new ProcessedObjectChecker(
				_AlwaysIgnoresProxiedMessageSourceAwareObjects));
		}

		private void _AlwaysIgnoresProxiedMessageSourceAwareObjects(MockContextAwareObject obj) 
		{
			Assert.IsNull(obj.MessageSource,
				"Transparent proxy IMessageSourceAware object was not ignored (must be).");
		}

		[Test]
		public void AlwaysIgnoresProxiedResourceLoaderAwareObjects()
		{
			PostProcessTProxiedObject(new ProcessedObjectChecker(
				_AlwaysIgnoresProxiedResourceLoaderAwareObjects));
		}

		private void _AlwaysIgnoresProxiedResourceLoaderAwareObjects(MockContextAwareObject obj) 
		{
			Assert.IsNull(obj.ResourceLoader,
				"Transparent proxy IResourceLoaderAware object was not ignored (must be).");
		}

		[Test]
		public void AlwaysIgnoresProxiedApplicationContextAwareAwareObjects()
		{
			PostProcessTProxiedObject(new ProcessedObjectChecker(
				_AlwaysIgnoresProxiedApplicationContextAwareAwareObjects));
		}

		private void _AlwaysIgnoresProxiedApplicationContextAwareAwareObjects(MockContextAwareObject obj) 
		{
			Assert.IsNull(obj.ApplicationContext,
				"Transparent proxy IApplicationContextAwareAware object was not ignored (must be).");
		}

		private void PostProcessTProxiedObject(ProcessedObjectChecker test) 
		{
			AppDomain domain = null;
			try
			{
				AppDomainSetup setup = new AppDomainSetup();
				setup.ApplicationBase = Environment.CurrentDirectory;
				domain = AppDomain.CreateDomain("Spring", new Evidence(AppDomain.CurrentDomain.Evidence), setup);
				object foo = domain.CreateInstanceAndUnwrap(GetType().Assembly.FullName, typeof(MockContextAwareObject).FullName);
	
				MockApplicationContext ctx = new MockApplicationContext();
				ApplicationContextAwareProcessor processor = new ApplicationContextAwareProcessor(ctx);
				MockContextAwareObject afterFoo = (MockContextAwareObject) processor.PostProcessBeforeInitialization(foo, "MyContextAwareObject");
				test(afterFoo);
			}
			finally
			{
				try
				{
					AppDomain.Unload(domain);
				}
				catch (Exception ex)
				{
					Console.Write("Error unloading AppDomain used during testing : " + ex);
				}
			}
		}

		private delegate void ProcessedObjectChecker(MockContextAwareObject obj);
	}
}