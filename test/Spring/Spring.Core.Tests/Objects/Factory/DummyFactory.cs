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

using Spring.Objects.Factory.Config;

#endregion

namespace Spring.Objects.Factory
{
	/// <summary>
	/// Simple factory to allow testing of IFactoryObject support in AbstractObjectFactory.
	/// Depending on whether its singleton property is set, it will return a singleton
	/// or a prototype instance.
	/// Implements the IInitializingObject interface, so we can check that factories get
	/// this lifecycle callback if they want.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Rick Evans (.NET)</author>
	public class DummyFactory :
		IFactoryObject,
		IObjectFactoryAware,
		IObjectNameAware,
		IInitializingObject,
		IDisposable
	{
		#region Constants

		public const string SINGLETON_NAME = "Factory singleton";

		#endregion

		#region Constructor (s) / Destructor

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.DummyFactory"/> class.
		/// </summary>
		public DummyFactory()
		{
			testObject = new TestObject();
			testObject.Name = SINGLETON_NAME;
			testObject.Age = 25;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Was this initialized by invocation of the
		/// AfterPropertiesSet() method from the IInitializingObject interface?
		/// </summary>
		public virtual bool WasInitialized
		{
			get { return initialized; }
		}

		public static bool WasPrototypeCreated
		{
			get { return prototypeCreated; }
		}

		public virtual bool PostProcessed
		{
			get { return postProcessed; }

			set { postProcessed = value; }
		}

		public virtual ITestObject OtherTestObject
		{
			get { return otherTestObject; }

			set { otherTestObject = value; }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Clear static state.
		/// </summary>
		public static void Reset()
		{
			prototypeCreated = false;
		}

		#endregion

		#region Fields

		/// <summary> Default is for factories to return a singleton instance.</summary>
		private bool singleton = true;

		private String objectName;
		private IAutowireCapableObjectFactory objectFactory;
		private bool postProcessed;
		private bool initialized;
		private static bool prototypeCreated;
		private TestObject testObject;
		private ITestObject otherTestObject;

		#endregion

		#region IFactoryObject Members

		public Type ObjectType
		{
			get
			{
//				if (!initialized)
//				{
//					throw new InvalidOperationException("'ObjectType' must not be called before AfterPropertiesSet()");
//				}
				return testObject.GetType();
			}
		}

		public object GetObject()
		{
//			if (!initialized)
//			{
//				throw new InvalidOperationException("GetObject() must not be called before AfterPropertiesSet()");
//			}

			if (IsSingleton)
			{
				return testObject;
			}
			else
			{
				TestObject prototype = new TestObject("Prototype created at " + DateTime.Now.Millisecond, 11);
				if (objectFactory != null)
				{
					objectFactory.ApplyObjectPostProcessorsBeforeInitialization(prototype, objectName);
				}
				prototypeCreated = true;
				return prototype;
			}
		}

		public bool IsSingleton
		{
			get { return singleton; }
			set { singleton = value; }
		}

		#endregion

		#region IObjectFactoryAware Members

		public IObjectFactory ObjectFactory
		{
			get { return objectFactory; }
			set
			{
				objectFactory = (IAutowireCapableObjectFactory) value;
				objectFactory.ApplyObjectPostProcessorsBeforeInitialization(testObject, objectName);
			}
		}

		#endregion

		#region IObjectNameAware Members

		public string ObjectName
		{
			get { return objectName; }
			set { objectName = value; }
		}

		#endregion

		#region IInitializingObject Members

		public void AfterPropertiesSet()
		{
			if (initialized)
			{
				throw new SystemException(
					"Cannot call AfterPropertiesSet twice on the one object.");
			}

			initialized = true;
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (testObject != null)
			{
				testObject.Name = null;
			}
		}

		#endregion
	}
}