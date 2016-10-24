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
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Spring.Core.IO;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;

#endregion

namespace Spring.Context.Support
{
	[TestFixture]
	public class ApplicationObjectSupportTests
	{
		internal class MyApplicationObjectSupport : ApplicationObjectSupport
		{
			private bool _init = false;

			public MyApplicationObjectSupport() : base()
			{
			}

			public MyApplicationObjectSupport(IApplicationContext applicationContext) : base(applicationContext)
			{
			}

			protected override Type RequiredType
			{
				get { return typeof (MockApplicationContext); }
			}

			protected override void InitApplicationContext()
			{
				_init = true;
			}

			public bool Init
			{
				get { return _init; }
			}
		}

		internal class MyContext2 : IApplicationContext
		{
			public void Dispose()
			{
			}

			#region IApplicationContext Members

			public IApplicationContext ParentContext
			{
				get { return null; }
				set
				{
				}
			}

			public DateTime StartupDate
			{
				get { return new DateTime(); }
			}

#pragma warning disable 67
			public event ApplicationEventHandler ContextEvent;
#pragma warning restore 67

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

			#endregion

			#region IListableObjectFactory Members

            public IObjectDefinition GetObjectDefinition(string name)
            {
                return null;
            }

            public IObjectDefinition GetObjectDefinition(string name, bool includeAncestors)
            {
                return null;
            }

            public IList<string> GetObjectDefinitionNames(bool includeAncestors)
            {
                return null;
            }

            public string[] GetObjectDefinitionNames(Type type)
			{
				return null;
			}

			public IList<string> GetObjectNamesForType(Type type)
			{
				return null;
			}

		    public IList<string> GetObjectNames<T>()
		    {
		        return null;
		    }

		    public IList<string> GetObjectNamesForType(Type type, bool includePrototypes, bool includeFactoryObjects)
			{
				return null;
			}

		    public IList<string> GetObjectNames<T>(bool includePrototypes, bool includeFactoryObjects)
		    {
		        return null;
		    }

		    IList<string> IListableObjectFactory.GetObjectDefinitionNames()
			{
				return null;
			}

			public IDictionary<string, object> GetObjectsOfType(Type type)
			{
				return null;
			}

		    public IDictionary<string, T> GetObjects<T>()
		    {
		        return null;
		    }

		    public IDictionary<string, object> GetObjectsOfType(Type type, bool includePrototypes, bool includeFactoryObjects)
			{
				return null;
			}

		    public IDictionary<string, T> GetObjects<T>(bool includePrototypes, bool includeFactoryObjects)
		    {
		        return null;
		    }

		    public T GetObject<T>()
		    {
		        throw new NotImplementedException();
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

			public IList<string> GetAliases(string name)
			{
				return null;
			}

		    public bool IsTypeMatch<T>(string name)
		    {
                return false;
            }

		    public object CreateObject(string name, Type requiredType, object[] arguments)
		    {
		        return null;
		    }

		    public T CreateObject<T>(string name, object[] arguments)
		    {
		        return Activator.CreateInstance<T>();
		    }

		    public object GetObject(string name, Type requiredType)
			{
				return null;
			}

			object IObjectFactory.GetObject(string name)
			{
				return null;
			}

		    public T GetObject<T>(string name)
		    {
                return Activator.CreateInstance<T>();
            }

		    public object GetObject(string name, object[] arguments)
		    {
		        return null;
		    }

		    public T GetObject<T>(string name, object[] arguments)
		    {
                return Activator.CreateInstance<T>();
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
		        return false;
		    }

		    #endregion

			#region IMessageSource Members

			public string GetMessage(IMessageSourceResolvable resolvable, CultureInfo culture)
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

		    public string GetMessage(string name, string defaultMessage, CultureInfo culture, params object[] arguments)
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

			#region IEventRepository Members

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


		    public void Unsubscribe(object subscriber)
		    {
		        throw new NotImplementedException();
		    }

		    public void Unsubscribe(object subscriber, Type targetSourceType)
		    {
		        throw new NotImplementedException();
		    }

		    #endregion

			public void PublishEvent(object sender, ApplicationEventArgs e)
			{
				throw new NotImplementedException();
			}
		}

		internal class MyContext2Subclass : MyContext2
		{
		}

		internal class MyApplicationObjectSupportConcrete : ApplicationObjectSupport
		{
			private bool _init;

			public MyApplicationObjectSupportConcrete() : base()
			{
			}

			public MyApplicationObjectSupportConcrete(IApplicationContext applicationContext) : base(applicationContext)
			{
			}

			protected override void InitApplicationContext()
			{
				base.InitApplicationContext();
				_init = true;
			}

			public bool Init
			{
				get { return _init; }
			}
		}


		[Test]
		public void InvalidContextSubclass()
		{
			ApplicationObjectSupport support = new MyApplicationObjectSupport();
			Assert.Throws<ApplicationContextException>(() => support.ApplicationContext = new MyContext2());
		}

		[Test]
		public void ValidContextSubClassOfAContext()
		{
			MyApplicationObjectSupportConcrete support = new MyApplicationObjectSupportConcrete();
			support.ApplicationContext = new MyContext2();
		}

		[Test]
		public void ValidContextIApplicationContext()
		{
			MyApplicationObjectSupportConcrete support = new MyApplicationObjectSupportConcrete();
			support.ApplicationContext = new MyContext2();
			Assert.IsTrue(support.Init);
			Assert.IsNotNull(support.MessageSourceAccessor);
		}

		[Test]
		public void ValidContextSubClass()
		{
			MyApplicationObjectSupport support = new MyApplicationObjectSupport();
			support.ApplicationContext = new MockApplicationContext();
			Assert.IsTrue(support.Init);
		}

		[Test]
		public void ReinitWithSameContext()
		{
			MockApplicationContext ctx = new MockApplicationContext();
			ApplicationObjectSupport support = new MyApplicationObjectSupport(ctx);
			support.ApplicationContext = ctx;
			Assert.AreEqual(ctx, support.ApplicationContext);
		}

		[Test]
		public void ReinitWithDiffContext()
		{
			MockApplicationContext ctx = new MockApplicationContext();
			ApplicationObjectSupport support = new MyApplicationObjectSupport(ctx);
			Assert.Throws<ApplicationContextException>(() => support.ApplicationContext = new MockApplicationContext());
		}
	}
}