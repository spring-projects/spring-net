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

using System;

using FakeItEasy;

using NUnit.Framework;
using Spring.Core;
using Spring.Core.IO;
using Spring.Objects;
using Spring.Objects.Events;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;

namespace Spring.Context.Support
{
	[TestFixture]
	public class AbstractApplicationContextTests
	{
		private MockApplicationContext _context;

		[SetUp]
		public void Init()
		{
			EverythingAwareObject.InstanceCount = 0;
			EverythingAwareObjectFactoryPostProcessor.InstanceCount = 0;
			EverythingAwareObjectPostProcessor.InstanceCount = 0;
			_context = new MockApplicationContext("MockApplicationContextName");
		}

		[TearDown]
		public void Destroy()
		{
			_context.Dispose();
			_context = null;
		}


        public void ContextAwareDisplayName()
        {

        }

	    [Test]
	    public void ExecutesAllContextEventHandlersAndRethrowsExceptionsThrownDuringContextEventHandlingByDefault()
	    {
	        MockApplicationContext appCtx = new MockApplicationContext();
            bool secondHandlerExecuted = false;
            appCtx.ContextEvent += (sender, e) =>
            {
                throw new ApplicationException("dummy");
            };
            appCtx.ContextEvent += (sender,  e) =>
            {
                secondHandlerExecuted = true;
            };


	        ApplicationException resultException = null;
	        try
	        {
	            appCtx.PublishEvent(this, new ApplicationEventArgs());
                Assert.Fail();
	        }
	        catch (ApplicationContextException e)
	        {
	            resultException = (ApplicationException) e.GetBaseException();
	        }

            Assert.AreEqual("dummy", resultException.Message);
            Assert.IsTrue(secondHandlerExecuted);
        }

        [Test]
        public void DoesNotSearchParentContextForMessageSource()
        {
            IMessageSource msgSource = A.Fake<IMessageSource>();
            MockApplicationContext parentCtx = new MockApplicationContext("parentContext");
            parentCtx.ObjectFactory.RegisterSingleton(AbstractApplicationContext.MessageSourceObjectName, msgSource);
            MockApplicationContext childContext = new MockApplicationContext("childContext", parentCtx);
            parentCtx.Refresh();
            childContext.Refresh();

            Assert.AreNotSame( msgSource, childContext.MessageSource );
            Assert.AreSame(msgSource, parentCtx.MessageSource);
            Assert.AreEqual(msgSource, ((IHierarchicalMessageSource)childContext.MessageSource).ParentMessageSource);
        }

        [Test]
        public void DoesNotSearchParentContextForEventRegistry()
        {
            IEventRegistry eventRegistry = A.Fake<IEventRegistry>();
            MockApplicationContext parentCtx = new MockApplicationContext("parentContext");
            parentCtx.ObjectFactory.RegisterSingleton(AbstractApplicationContext.EventRegistryObjectName, eventRegistry);
            MockApplicationContext childContext = new MockApplicationContext("childContext", parentCtx);
            parentCtx.Refresh();
            childContext.Refresh();

            Assert.AreSame( eventRegistry, parentCtx.EventRegistry );
            Assert.AreNotSame( eventRegistry, childContext.EventRegistry );
        }

		/// <summary>
		/// Tests the case where there is an object in the context with the name of the
		/// default message source name, that is NOT an IMessageSource.
		/// </summary>
		[Test]
		public void InvalidMessageSourceObject()
		{
			RootObjectDefinition def = new RootObjectDefinition(typeof (TestObject));
			((DefaultListableObjectFactory) _context.ObjectFactory)
				.RegisterObjectDefinition(AbstractApplicationContext.MessageSourceObjectName, def);
			_context.Refresh();
			object foo = _context
				.GetObject(AbstractApplicationContext.MessageSourceObjectName);
			Assert.IsTrue(foo is ITestObject,
				"Registered non-IMessageSource object under the default message source name, but " +
				"failed to get it out of the context. Object retrieved is of type [ " + foo.GetType() + "].");
		}

		/// <summary>
		/// Tests the case where there is an object in the context with the name of the
		/// default event registry name, that is NOT an IEventRegistry.
		/// </summary>
		[Test]
		public void InvalidEventRegistryObject()
		{
			RootObjectDefinition def = new RootObjectDefinition(typeof (TestObject));
			((DefaultListableObjectFactory) _context.ObjectFactory)
				.RegisterObjectDefinition(AbstractApplicationContext.EventRegistryObjectName, def);
			_context.Refresh();
			object foo = _context
				.GetObject(AbstractApplicationContext.EventRegistryObjectName);
			Assert.IsTrue(foo is ITestObject,
				"Registered non-IEventRegistry object under the default message source name, but " +
				"failed to get it out of the context. Object retrieved is of type [ " + foo.GetType() + "].");
		}

		[Test]
		public void ContextAwareSingletonWasCalledBack()
		{
			_context.RegisterSingleton();
			_context.Refresh();
			MockContextAwareObject mcao1 = (MockContextAwareObject)_context.GetObject("mcao-single");
			Assert.IsTrue(mcao1.ApplicationContext == _context, "context");
			object mcao2 = _context.GetObject("mcao-single");
			Assert.IsTrue(mcao1 == mcao2, "same");
			Assert.IsTrue(_context.IsSingleton("mcao-single"), "singleton?");
		}

		[Test]
		public void ContextAwarePrototypeWasCalledBack()
		{
			_context.RegisterObject();
			_context.Refresh();
			MockContextAwareObject mcao1 = (MockContextAwareObject)_context.GetObject("mcao-proto");
			Assert.IsTrue(mcao1.ApplicationContext == _context, "context");
			Assert.IsTrue(! _context.IsSingleton("mcao-proto"), "singleton");
			MockContextAwareObject mcao2 = (MockContextAwareObject) _context.GetObject("mcao-proto");
			Assert.IsTrue(mcao1 != mcao2, "instance");

		}

		[Test]
		public void ContextAwareSingletonGetName()
		{
			_context.RegisterSingleton();
			_context.Refresh();
			MockContextAwareObject mcao1 = (MockContextAwareObject)_context.GetObject("mcao-single");
			Assert.AreEqual(mcao1.ApplicationContext.Name, "MockApplicationContextName");
		}

		[Test]
		public void ContextAwarePrototypeGetName()
		{
			_context.RegisterObject();
			_context.Refresh();
			MockContextAwareObject mcao1 = (MockContextAwareObject)_context.GetObject("mcao-proto");
			Assert.AreEqual(mcao1.ApplicationContext.Name, "MockApplicationContextName");
		}

		[Test]
		public void ParentNull()
		{
			Assert.IsNull(_context.ParentContext, "parent is not null");
		}

		[Test]
		public void ParentNotNullGrandparentNull()
		{
			IApplicationContext parentContext = new MockApplicationContext("MockApplicationContextParent");
			_context = new MockApplicationContext("MockApplicationContextName", parentContext);
			Assert.IsNotNull(_context.ParentContext, "parent is null");
			Assert.IsNull(_context.ParentContext.ParentContext, "parent is null");
		}

		#region OrderOfKnownProcessorInterfaces Utility Classes

		public enum ObjectProcessingState
		{
			SetObjectName,
			SetObjectFactory,
			SetApplicationContext,
			PostProcessObjectFactory,
			ObjectPostProcessorBeforeInitialization,
			ObjectPostProcessorAfterInitialization,
		}

		public class EverythingAwareObject :
			IObjectNameAware,
			IObjectFactoryAware,
			IApplicationContextAware
		{
			public static int InstanceCount = 0;

			private ObjectProcessingState currentState;

			public EverythingAwareObject()
			{
				InstanceCount++;
			}

			public EverythingAwareObject(int expectObjectPostProcessorInstances)
				:this()
			{
				// ensure postprocessor has been instantiated *before* this object
				Assert.AreEqual(expectObjectPostProcessorInstances, EverythingAwareObjectPostProcessor.InstanceCount);
			}

			public ObjectProcessingState CurrentState
			{
				get { return currentState;}
				set { currentState = value; }
			}

			public IApplicationContext ApplicationContext
			{
				get { throw new NotImplementedException(); }
				set
				{
					Assert.AreEqual(ObjectProcessingState.SetApplicationContext, this.CurrentState);
					this.CurrentState++;
				}
			}

			public string ObjectName
			{
				set
				{
					Assert.AreEqual(ObjectProcessingState.SetObjectName, this.CurrentState);
					this.CurrentState++;
				}
			}

		    private bool objectFactorySet = false;
			public IObjectFactory ObjectFactory
			{
				set
				{
                    // ignore multiple calls (due to OF also set during AbstractObjectFactory.AddObjectPostProcessor())
                    if (objectFactorySet)
                    {
                        return;
                    }
				    objectFactorySet = true;
					Assert.AreEqual(ObjectProcessingState.SetObjectFactory, this.CurrentState);
					this.CurrentState++;
				}
			}
		}

		public class EverythingAwareObjectFactoryPostProcessor : EverythingAwareObject,
			IObjectFactoryPostProcessor
		{
			public new static int InstanceCount = 0;

			public EverythingAwareObjectFactoryPostProcessor()
			{
				InstanceCount++;
			}

			public void PostProcessObjectFactory(IConfigurableListableObjectFactory factory)
			{
				Assert.AreEqual(ObjectProcessingState.PostProcessObjectFactory, CurrentState);
				CurrentState++;
			}
		}

		public class EverythingAwareObjectPostProcessor : EverythingAwareObjectFactoryPostProcessor,
			IObjectPostProcessor
		{
			public new static int InstanceCount = 0;

			public EverythingAwareObjectPostProcessor()
			{
				InstanceCount++;
			}

			public EverythingAwareObjectPostProcessor(int expectObjectFactoryPostProcessorInstances)
				:this()
			{
				// ensure factorypostprocessor has been instantiated *before* this object
				Assert.AreEqual(expectObjectFactoryPostProcessorInstances, EverythingAwareObjectFactoryPostProcessor.InstanceCount);
			}

			public object PostProcessBeforeInitialization(object instance, string name)
			{
				Assert.AreNotEqual(this, instance);
				Assert.AreEqual(ObjectProcessingState.ObjectPostProcessorBeforeInitialization, CurrentState);
				CurrentState++;
				return instance;
			}

			public object PostProcessAfterInitialization(object instance, string objectName)
			{
				Assert.AreNotEqual(this, instance);
				Assert.AreEqual(ObjectProcessingState.ObjectPostProcessorAfterInitialization, CurrentState);
				CurrentState++;
				return instance;
			}
		}

		#endregion

		[Test]
		public void OrderOfKnownProcessorInterfaces()
		{
			DefaultListableObjectFactory objectFactory = (DefaultListableObjectFactory)this._context.ObjectFactory;
			RootObjectDefinition def;
			def = new RootObjectDefinition(typeof(EverythingAwareObjectPostProcessor));
			objectFactory.RegisterObjectDefinition("everythingAwareObjectPostProcessor", def);
			_context.Refresh();
		}

		[Test]
		public void OrderOfKnownProcessorInstantiation()
		{
			DefaultListableObjectFactory objectFactory = (DefaultListableObjectFactory)this._context.ObjectFactory;
			RootObjectDefinition def;
			// note the order of registration (checks instantiation does not occur in order of registration)
			def = new RootObjectDefinition(typeof(EverythingAwareObject));
			def.ConstructorArgumentValues.AddIndexedArgumentValue(0, 1);
			objectFactory.RegisterObjectDefinition("everythingAwareObject", def);
			def = new RootObjectDefinition(typeof(EverythingAwareObjectPostProcessor));
			def.ConstructorArgumentValues.AddIndexedArgumentValue(0, 1);
			objectFactory.RegisterObjectDefinition("everythingAwareObjectPostProcessor", def);
			def = new RootObjectDefinition(typeof(EverythingAwareObjectFactoryPostProcessor));
			objectFactory.RegisterObjectDefinition("everythingAwareObjectFactoryPostProcessor", def);
			_context.Refresh();
		}

		[Test]
		public void DefaultObjectFactoryProcessorsDontGetAddedTwice()
		{
			MockApplicationContext myContext = new MockApplicationContext("myContext");
			DefaultListableObjectFactory objectFactory = (DefaultListableObjectFactory)myContext.ObjectFactory;
			Assert.AreEqual(0, objectFactory.ObjectPostProcessorCount);
			myContext.Refresh();
			int defaultProcessors = objectFactory.ObjectPostProcessors.Count;
			myContext.Refresh();
			Assert.AreEqual(defaultProcessors, objectFactory.ObjectPostProcessors.Count);
		}

        [Test]
        public void ThrowsCannotLoadObjectTypeExceptionOnInvalidTypename()
        {
          try
          {
            MockApplicationContext myContext = new MockApplicationContext("myContext");
            DefaultListableObjectFactory objectFactory = (DefaultListableObjectFactory)myContext.ObjectFactory;
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(objectFactory);
            reader.LoadObjectDefinitions(new StringResource(
                                           @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net'>  
	<object id='test2' type='DOESNOTEXIST' />
</objects>
"));
            myContext.Refresh();
          }
          catch (Exception e)
          {
//            Console.WriteLine(e);
            Assert.IsInstanceOf(typeof(CannotLoadObjectTypeException), e);
          }
        }
	}
}