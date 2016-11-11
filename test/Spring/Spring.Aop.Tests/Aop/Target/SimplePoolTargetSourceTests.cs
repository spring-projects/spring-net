using NUnit.Framework;

using Spring.Objects.Factory;
using Spring.Objects.Factory.Xml;
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
namespace Spring.Aop.Target
{
	
	/// <summary> Tests for pooling invoker interceptor
	/// TODO need to make these tests stronger: it's hard to
	/// make too many assumptions about a pool
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Federico Spinazzi (.Net)</author>
    [TestFixture]
    public class SimplePoolTargetSourceTests
	{
		
		/// <summary>Initial count value set in Object factory XML </summary>
		private const int INITIAL_COUNT = 10;
		
		private XmlObjectFactory objectFactory;
	
        [SetUp]
		public void SetUp()
		{
			objectFactory = new XmlObjectFactory(new ReadOnlyXmlTestResource("simplePoolTests.xml", GetType()));
		}
		
		/// <summary> We must simulate container shutdown, which should clear threads.</summary>
		[TearDown]
		public void TearDown()
		{
			// Will call pool.close()
			this.objectFactory.Dispose();
		}
		
		private void  Functionality(System.String name)
		{
			ISideEffectObject pooled = (ISideEffectObject) objectFactory.GetObject(name);
			Assert.AreEqual(INITIAL_COUNT, pooled.Count);
			pooled.doWork();
			Assert.AreEqual(INITIAL_COUNT + 1, pooled.Count);
			
			pooled = (ISideEffectObject) objectFactory.GetObject(name);
			// Just check that it works--we can't make assumptions
			// about the count
			pooled.doWork();
			//Assert.AreEqual(INITIAL_COUNT + 1, pooled.Count );
		}
		
		[Test]
        public virtual void  Functionality()
		{
			Functionality("pooled");
		}
		
        [Test]
		public virtual void  FunctionalityWithNoInterceptors()
		{
			Functionality("pooledNoInterceptors");
		}
		
        [Test]
		public virtual void  ConfigMixin()
		{
			ISideEffectObject pooled = (ISideEffectObject) objectFactory.GetObject("pooledWithMixin");
			Assert.AreEqual(INITIAL_COUNT, pooled.Count);
			PoolingConfig conf = (PoolingConfig) objectFactory.GetObject("pooledWithMixin");
			// TODO one invocation from setup
			// assertEquals(1, conf.getInvocations());
			pooled.doWork();
			//	assertEquals("No objects active", 0, conf.getActive());
			Assert.AreEqual(25, conf.MaxSize, "Correct target source");
			//		assertTrue("Some free", conf.getFree() > 0);
			//assertEquals(2, conf.getInvocations());
			Assert.AreEqual(25, conf.MaxSize);
		}
	}
}