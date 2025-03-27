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

#region Imports

using NUnit.Framework;
using Spring.Context.Support;
using Spring.Core.IO;
using Spring.Objects;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;

#endregion

namespace Spring.Context
{
	/// <summary>
	/// This class contains common mock implementations of Context interfaces,
	/// used for testing.
	/// </summary>
	[Serializable]
	public class MockContextAwareObject : MarshalByRefObject,
		IApplicationContextAware, IMessageSourceAware,  IResourceLoaderAware
	{
		private IApplicationContext _applicationContext;
		private IResourceLoader _resourceLoader;
		private IMessageSource _messageSource;

		public IApplicationContext GetApplicationContext()
		{
			return _applicationContext;
		}

		public IApplicationContext ApplicationContext
		{
			set { _applicationContext = value; }
			get { return _applicationContext; }
		}

		public IResourceLoader ResourceLoader
		{
			set { _resourceLoader = value; }
			get { return _resourceLoader; }
		}

		public IMessageSource MessageSource
		{
			get { return _messageSource; }
			set { _messageSource = value; }
		}
	}

	public class MockApplicationContext : AbstractApplicationContext
	{
		private string _mockName;
		private bool _isVerified;
		private DefaultListableObjectFactory factory;
	    private int expectedCloseCalls;
	    private int actualCloseCalls;

		public void SetCloseCalls(int expectedCalls)
		{
		    expectedCloseCalls = expectedCalls;
		}

		public MockApplicationContext() : this(null, null)
		{
		}

		public MockApplicationContext(string name) : this(name, null)
		{
			_mockName = name;
			factory = new DefaultListableObjectFactory();
        }

        /// <summary>
        /// Initializes a new instance of the MockApplicationContext class.
        /// </summary>
        public MockApplicationContext(IApplicationContext parentContext)
        {
            factory = new DefaultListableObjectFactory(parentContext);
        }

        public MockApplicationContext(string name, IApplicationContext parentContext) : base(name, true, parentContext)
		{
			_mockName = name;
			factory = new DefaultListableObjectFactory(GetInternalParentObjectFactory());
		}

        public override bool IsObjectNameInUse(string objectName)
        {
            return factory.IsObjectNameInUse(objectName);
        }

		public override IConfigurableListableObjectFactory ObjectFactory
		{
			get { return factory; }
		}

		protected override void RefreshObjectFactory()
		{
		}

		public void RegisterSingleton()
		{
			RootObjectDefinition mcaoDef = new RootObjectDefinition(typeof (MockContextAwareObject), new MutablePropertyValues(), true);
			factory.RegisterObjectDefinition("mcao-single", mcaoDef);
		}

		public void RegisterObject()
		{
			RootObjectDefinition mcaoDef = new RootObjectDefinition(typeof (MockContextAwareObject), new MutablePropertyValues(), false);
			factory.RegisterObjectDefinition("mcao-proto", mcaoDef);
		}

		public override void Dispose()
		{
		    actualCloseCalls++;
		}

		#region IMockObject Members

		public void NotImplemented(string methodName)
		{
			throw new NotImplementedException(methodName + " is not currently implemented");
		}

		public string MockName
		{
			get { return _mockName; }
			set { _mockName = value; }
		}

		#endregion

		#region IVerifiable Members

		public void Verify()
		{			
            Assert.AreEqual(actualCloseCalls, expectedCloseCalls, "Did not receive the expected Count for object " + MockName);
			_isVerified = true;
		}

		public bool IsVerified
		{
			get { return _isVerified; }
		}

		#endregion
	}

}