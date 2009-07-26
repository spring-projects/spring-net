#region License

/*
 * Copyright 2002-2004 the original author or authors.
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
using System.Collections;
using System.Globalization;
using DotNetMock;
using Spring.Context.Support;
using Spring.Core.IO;
using Spring.Objects;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;

#endregion

namespace Spring.Context
{
	/// <summary>
	/// This class contains common mock implementations of Context interfaces,
	/// used for testing.
	/// </summary>
	public class MockMessageSource : MockObject, IMessageSource
	{
		private ExpectationCounter _getMessageCalls = new ExpectationCounter("MockMessageSource.GetMessageCounter");
		private ExpectationString _getMessageCode = new ExpectationString("MockMessageSource.GetMessageCode");
        private ExpectationString _getMessageDefaultMessage = new ExpectationString("MockMessageSource.GetMessageDefaultMessage");
		private ExpectationArray _getMessageArguments = new ExpectationArray("MockMessageSource.GetMessageArguments");

		private string _getMessageReturn = null;


	    public MockMessageSource()
	    {
            // this is to ensure, that no expectations may be violated
	        _getMessageCalls.Expected = -1;
	        _getMessageCode.Expected = "something very, very unlikely " + Guid.NewGuid();
	        _getMessageDefaultMessage.Expected = "something very, very unlikely " + Guid.NewGuid();
	        _getMessageArguments.Expected = new object[] { "something very, very unlikely " + Guid.NewGuid() };
	    }

	    public void SetExpectedGetMessageCalls(int calls)
		{
			_getMessageCalls.Expected = calls;
		}

        public void SetExpectedGetMessageDefaultMessage(string defaultMessage)
        {
            _getMessageDefaultMessage.Expected = defaultMessage;
        }

		public void SetExpectedGetMessageArguments(object[] arguments)
		{
			_getMessageArguments.Expected = arguments;
		}

		public void SetExpectedGetMessageReturn(string message)
		{
			_getMessageReturn = message;
		}

		public void SetExpectedGetMessageCode(string code)
		{
			_getMessageCode.Expected = code;
		}

		#region IMessageSource Members

		public string GetMessage(IMessageSourceResolvable resolvable, CultureInfo culture)
		{
			_getMessageCalls.Inc();
			return _getMessageReturn;
		}

		public string GetMessage(string name, CultureInfo culture, params object[] args)
		{
			_getMessageCalls.Inc();
			_getMessageCode.Actual = name;
			_getMessageArguments.Actual = args;
			return _getMessageReturn;
		}

		public string GetMessage(string name)
		{
			_getMessageCalls.Inc();
			_getMessageCode.Actual = name;
			return _getMessageReturn;
		}

		public string GetMessage(string name, params object[] args)
		{
			_getMessageCalls.Inc();
			_getMessageCode.Actual = name;
			_getMessageArguments.Actual = args;
			return _getMessageReturn;
		}


	    public string GetMessage(string name, string defaultMessage, CultureInfo culture, params object[] arguments)
	    {
            _getMessageCalls.Inc();
            _getMessageCode.Actual = name;
	        _getMessageDefaultMessage.Actual = defaultMessage;
            _getMessageArguments.Actual = arguments;
            return _getMessageReturn;
	    }

	    public string GetMessage(string name, CultureInfo cultureInfo)
		{
			_getMessageCalls.Inc();
			_getMessageCode.Actual = name;
			return _getMessageReturn;
		}

		public object GetResourceObject(string name, CultureInfo culture)
		{
			return null;
		}

		public object GetResourceObject(string name)
		{
			return null;
		}

		public void ApplyResources(object value, string objectName, CultureInfo cultureInfo)
		{
		}

		#endregion
	}

	public class MockMessageResolvable : MockObject, IMessageSourceResolvable
	{
		private string[] _codes;
		private string _defaultMessage;
		private object[] _arguments;
		private ExpectationCounter _getCodesCalls = new ExpectationCounter("MockMessageSource.Codes");

		public void SetExpectedCodesCalls(int calls)
		{
			_getCodesCalls.Expected = calls;
		}

		public void SetCode(string code)
		{
			SetCodes(new string[] {code});
		}

		public void SetCodes(string[] codes)
		{
			_codes = codes;
		}

		public void SetDefaultMessage(String defaultMessage)
		{
			_defaultMessage = defaultMessage;
		}

		public void SetArguments(object[] arguments)
		{
			_arguments = arguments;
		}

		#region IMessageSourceResolvable Members

		public object[] GetArguments()
		{
			return _arguments;
		}

		public string[] GetCodes()
		{
			_getCodesCalls.Inc();
			return _codes;
		}

		public string DefaultMessage
		{
			get { return _defaultMessage; }
		}

		#endregion
	}

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

	public class MockApplicationContext : AbstractApplicationContext, IMockObject
	{
		private string _mockName;
		private bool _isVerified;
		private DefaultListableObjectFactory factory;
		private ExpectationCounter _closeCalls = new ExpectationCounter("MockConfigurableApplicationContext.CloseCalls");

		public void SetCloseCalls(int expectedCalls)
		{
			_closeCalls.Expected = expectedCalls;
		}

		public MockApplicationContext() : this(null, null)
		{
		}

		public MockApplicationContext(string name) : this(name, null)
		{
			_mockName = name;
			factory = new DefaultListableObjectFactory();
//			factory.AddObjectPostProcessor(new ApplicationContextAwareProcessor(this));
		}

		public MockApplicationContext(string name, IApplicationContext parentContext) : base(name, true, parentContext)
		{
			_mockName = name;
			factory = new DefaultListableObjectFactory(GetInternalParentObjectFactory());
//			factory.AddObjectPostProcessor(new ApplicationContextAwareProcessor(this));
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
			_closeCalls.Inc();
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
			Verifier.Verify(this);
			_isVerified = true;
		}

		public bool IsVerified
		{
			get { return _isVerified; }
		}

		#endregion
	}

	public class MockDefaultApplicationContext : MockObject, IApplicationContext
	{
		public void Dispose()
		{
		}

		#region IApplicationContext Members

		public DateTime StartupDate
		{
			get { return new DateTime(); }
		}

		public event ApplicationEventHandler ContextEvent;

		public long StartupDateMilliseconds
		{
			get { return 0; }
		}

		public string Name
		{
			get { return AbstractApplicationContext.DefaultRootContextName; }
			set
			{
			}
		}

		public IApplicationContext ParentContext
		{
			get { return null; }
			set
			{
			}
		}

		#endregion

		#region IListableObjectFactory Members

		public string[] GetObjectDefinitionNames(Type type)
		{
			return null;
		}

		public IObjectDefinition GetObjectDefinition(string name)
		{
			return null;
		}

		public IObjectDefinition GetObjectDefinition(string name, bool includeAncestors)
		{
			return null;
		}

		string[] IListableObjectFactory.GetObjectDefinitionNames()
		{
			return null;
		}

		public string[] GetObjectNamesForType(Type type)
		{
			return null;
		}

		public string[] GetObjectNamesForType(
			Type type, bool includePrototypes, bool includeFactoryObjects)
		{
			return null;
		}

		public IDictionary GetObjectsOfType(Type type)
		{
			return null;
		}

		public IDictionary GetObjectsOfType(
			Type type, bool includePrototypes, bool includeFactoryObjects)
		{
			return null;
		}

		public int ObjectDefinitionCount
		{
			get { return 0; }
		}

		public bool ContainsObjectDefinition(string name)
		{
			return false;
		}

		#endregion

		#region IObjectFactory Members

	    public bool IsCaseSensitive
	    {
	        get { return true; }
	    }

	    public object this[string name]
		{
			get { return null; }
		}

		public bool ContainsObject(string name)
		{
			return false;
		}

		public string[] GetAliases(string name)
		{
			return null;
		}

	    public object CreateObject(string name, Type requiredType, object[] arguments)
	    {
            return null;
	    }

		public object GetObject(string name, Type requiredType)
		{
			return null;
		}

		object IObjectFactory.GetObject(string name)
		{
			return null;
		}

	    public object GetObject(string name, object[] arguments)
	    {
	        return null;
	    }

	    public object GetObject(string name, Type requiredType, object[] arguments)
	    {
	        return null;
	    }

	    public bool IsSingleton(string name)
		{
			return false;
		}


	    public bool IsPrototype(string name)
	    {
	        return false;
	    }

	    public Type GetType(string name)
		{
			return null;
		}


	    public bool IsTypeMatch(string name, Type targetType)
	    {
	        return false;
	    }

	    public object ConfigureObject(object target)
		{
            return null;
        }

		public object ConfigureObject(object target, string name)
		{
            return null;
        }

        public object ConfigureObject(object target, string name, IObjectDefinition definition)
        {
            return null;
        }

		#endregion

		#region IHierarchicalObjectFactory Members

		public IObjectFactory ParentObjectFactory
		{
			get { return null; }
		}

	    public bool ContainsLocalObject(string name)
	    {
	        throw new NotImplementedException();
	    }

	    #endregion

		#region IMessageSource Members

		string IMessageSource.GetMessage(IMessageSourceResolvable resolvable, CultureInfo culture)
		{
			return null;
		}

		string IMessageSource.GetMessage(string name, CultureInfo culture, params object[] args)
		{
			return null;
		}

		string IMessageSource.GetMessage(string name)
		{
			return null;
		}

		string IMessageSource.GetMessage(string name, params object[] args)
		{
			return null;
		}

		string IMessageSource.GetMessage(string name, CultureInfo cultureInfo)
		{
			return null;
		}

		object IMessageSource.GetResourceObject(string name, CultureInfo culture)
		{
			return null;
		}

		object IMessageSource.GetResourceObject(string name)
		{
			return null;
		}


	    public string GetMessage(string name, string defaultMessage, CultureInfo culture, params object[] arguments)
	    {
	        return null;
	    }

	    void IMessageSource.ApplyResources(object value, string objectName, CultureInfo cultureInfo)
		{
		}

		#endregion

		#region IResourceLoader Members

		public IResource GetResource(string location)
		{
			return null;
		}

		#endregion

		#region IEventRegistry Members

		public void PublishEvents(object sourceObject)
		{
			throw new NotImplementedException();
		}

		public void Subscribe(object subscriber)
		{
			throw new NotImplementedException();
		}

		public void Subscribe(object subscriber, Type targetSourceType)
		{
			throw new NotImplementedException();
		}

		public void PublishEvent(object sender, ApplicationEventArgs e)
		{
			throw new NotImplementedException();
		}


	    public void Unsubscribe(object subscriber)
	    {
	        throw new NotImplementedException();
	    }

	    public void Unsubscribe(object subscriber, Type targetSourceType)
	    {
	        throw new NotImplementedException();
        }

        #endregion
    }
}