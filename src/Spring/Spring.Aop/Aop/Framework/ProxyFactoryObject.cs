#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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

using AopAlliance.Aop;
using AopAlliance.Intercept;
using Common.Logging;
using Spring.Aop.Framework.Adapter;
using Spring.Aop.Support;
using Spring.Aop.Target;
using Spring.Core;
using Spring.Core.TypeResolution;
using Spring.Objects.Factory;
using Spring.Util;

#endregion

namespace Spring.Aop.Framework
{
	/// <summary>
	/// <see cref="Spring.Objects.Factory.IFactoryObject"/> implementation to
	/// source AOP proxies from a Spring.NET IoC container (an
	/// <see cref="Spring.Objects.Factory.IObjectFactory"/>).
	/// </summary>
	/// <remarks>
	/// <p>
	/// <see cref="AopAlliance.Intercept.IInterceptor"/>s and
	/// <see cref="Spring.Aop.IAdvisor"/>s are identified by a list of object
	/// names in the current container.</p>
	/// <p>
	/// Global interceptors and advisors can be added at the factory level
	/// (that is, outside the context of a
	/// <see cref="Spring.Aop.Framework.ProxyFactoryObject"/> definition). The
	/// specified interceptors and advisors are expanded in an interceptor list
	/// (see
	/// <see cref="Spring.Aop.Framework.ProxyFactoryObject.InterceptorNames"/>)
	/// where an <c>'xxx*'</c> wildcard-style entry is included in the list,
	/// matching the given prefix with the object names. For example,
	/// <c>'global*'</c> would match both <c>'globalObject1'</c> and
	/// <c>'globalObjectBar'</c>, and <c>'*'</c> would match all defined
	/// interceptors. The matching interceptors get applied according to their
	/// returned order value, if they implement the
	/// <see cref="Spring.Core.IOrdered"/> interface. An interceptor name list
	/// may not conclude with a global <c>'xxx*'</c> pattern, as global
	/// interceptors cannot invoke targets.
	/// </p>
	/// <p>
	/// It is possible to cast a proxy obtained from this factory to an
	/// <see cref="Spring.Aop.Framework.IAdvised"/> reference, or to obtain the
	/// <see cref="Spring.Aop.Framework.ProxyFactoryObject"/> reference and
	/// programmatically manipulate it. This won't work for existing prototype
	/// references, which are independent... however, it will work for prototypes
	/// subsequently obtained from the factory. Changes to interception will
	/// work immediately on singletons (including existing references).
	/// However, to change interfaces or the target it is necessary to obtain a
	/// new instance from the surrounding container. This means that singleton
	/// instances obtained from the factory do not have the same object
	/// identity... however, they do have the same interceptors and target, and
	/// changing any reference will change all objects.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Juergen Hoeller</author>
	/// <author>Federico Spinazzi (.NET)</author>
	/// <author>Choy Rim (.NET)</author>
	/// <author>Mark Pollack (.NET)</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	/// <seealso cref="Spring.Aop.Framework.ProxyFactoryObject.InterceptorNames"/>
	/// <seealso cref="Spring.Aop.Framework.ProxyFactoryObject.ProxyInterfaces"/>
	/// <seealso cref="AopAlliance.Intercept.IMethodInterceptor"/>
	/// <seealso cref="Spring.Aop.IAdvisor"/>
	/// <seealso cref="Spring.Aop.Target.SingletonTargetSource"/>
    [Serializable]
    public class ProxyFactoryObject
		: AdvisedSupport, IFactoryObject, IObjectFactoryAware, IAdvisedSupportListener
	{
		#region Fields

		/// <summary>
		/// The shared <see cref="Common.Logging.ILog"/> instance for this class.
		/// </summary>
		private static readonly ILog logger = LogManager.GetLogger(typeof (ProxyFactoryObject));

	    /// <summary>
		/// Is the object managed by this factory a singleton or a prototype?
		/// </summary>
		private bool singleton = true;

	    /// <summary>
	    /// This suffix in a value in an interceptor list indicates to expand globals.
	    /// </summary>
	    public const string GlobalInterceptorSuffix = "*";

	    /// <summary>
		/// The cached instance if this proxy factory object is a singleton.
		/// </summary>
		private object singletonInstance;

		/// <summary>
		/// The owning object factory (which cannot be changed after this object is initialized).
		/// </summary>
		private IObjectFactory objectFactory;

		/// <summary> 
		/// The mapping from an <see cref="Spring.Aop.IPointcut"/> or interceptor
		/// to an object name (or <see lang="null"/>), depending on where it was
		/// sourced from.
		/// </summary>
		/// <remarks>
		/// <p>
		/// If it's sourced from object name, it will need to be
		/// refreshed each time a new prototype instance is created.
		/// </p>
		/// </remarks>
		private IDictionary sourceDictionary = new Hashtable();

		/// <summary>
		/// Names of interceptors and pointcut objects in the factory.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Default is for globals expansion only.
		/// </p>
		/// </remarks>
		private string[] interceptorNames = null;

		/// <summary>
		/// Names of introductions and pointcut objects in the factory.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Default is for globals expansion only.
		/// </p>
		/// </remarks>
		private string[] introductionNames = null;

		/// <summary>
		/// The name of the target object(in the enclosing
		/// <see cref="Spring.Objects.Factory.IObjectFactory"/>).
		/// </summary>
		private string targetName = null;

		#endregion

        #region Properties

        /// <summary>
		/// Sets the names of the interfaces that are to be implemented by the proxy.
		/// </summary>
		/// <value>
		/// The names of the interfaces that are to be implemented by the proxy.
		/// </value>
		/// <exception cref="Spring.Aop.Framework.AopConfigException">
		/// If the supplied value (or any of its elements) is <see langword="null"/>;
		/// or if any of the element values is not the (assembly qualified) name of
		/// an interface type.
		/// </exception>
		public virtual string[] ProxyInterfaces
		{
            set
            {
                try
                {
                    Interfaces = TypeResolutionUtils.ResolveInterfaceArray(value);
                }
                catch (Exception ex)
                {
                    throw new AopConfigException("Bad value passed to the ProxyInterfaces property (see inner exception).", ex);
                }
            }
		}

		/// <summary>
		/// Sets the name of the target object being proxied.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Only works when the
		/// <see cref="Spring.Aop.Framework.ProxyFactoryObject.ObjectFactory"/>
		/// property is set; it is a logic error on the part of the programmer
		/// if this value is set and the accompanying
		/// <see cref="Spring.Objects.Factory.IObjectFactory"/> is not also set.
		/// </p>
		/// </remarks>
		/// <value>
		/// The name of the target object being proxied.
		/// </value>
		public virtual string TargetName
		{
			set { this.targetName = value; }
		}

		/// <summary> 
		/// Sets the list of <see cref="AopAlliance.Intercept.IMethodInterceptor"/> and
		/// <see cref="Spring.Aop.IAdvisor"/> object names.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This property must always be set (configured) when using a
		/// <see cref="Spring.Aop.Framework.ProxyFactoryObject"/> in an
		/// <see cref="Spring.Objects.Factory.IObjectFactory"/> context.
		/// </p>
		/// </remarks>
		/// <value>
		/// The list of <see cref="AopAlliance.Intercept.IMethodInterceptor"/> and
		/// <see cref="Spring.Aop.IAdvisor"/> object names.
		/// </value>
		/// <seealso cref="AopAlliance.Intercept.IInterceptor"/>
		/// <seealso cref="Spring.Aop.IAdvisor"/>
		/// <seealso cref="Spring.Objects.Factory.IObjectFactory"/>
		/// <seealso cref="Spring.Objects.Factory.IObjectFactoryAware.ObjectFactory"/>
		public virtual string[] InterceptorNames
		{
			set { this.interceptorNames = value; }
		}

		/// <summary> 
		/// Sets the list of introduction object names. 
		/// </summary>
		/// <remarks>
		/// <p>
		/// Only works when the
		/// <see cref="Spring.Aop.Framework.ProxyFactoryObject.ObjectFactory"/>
		/// property is set; it is a logic error on the part of the programmer
		/// if this value is set and the accompanying
		/// <see cref="Spring.Objects.Factory.IObjectFactory"/> is not supplied.
		/// </p>
		/// </remarks>
		/// <value>
		/// The list of introduction object names. .
		/// </value>
		public virtual string[] IntroductionNames
		{
			set { this.introductionNames = value; }
        }

        #endregion

        #region IFactoryObjectAware implementation

        /// <summary>
		/// Callback that supplies the owning factory to an object instance.
		/// </summary>
		/// <value>
		/// Owning <see cref="Spring.Objects.Factory.IObjectFactory"/>
		/// (may not be <see langword="null"/>). The object can immediately
		/// call methods on the factory.
		/// </value>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// In case of initialization errors.
		/// </exception>
		/// <seealso cref="Spring.Objects.Factory.IObjectFactory"/>
		/// <seealso cref="Spring.Objects.Factory.IObjectFactoryAware.ObjectFactory"/>
		public virtual IObjectFactory ObjectFactory
		{
			set
			{
				this.objectFactory = value;

				#region Instrumentation

				if (logger.IsDebugEnabled)
				{
					logger.Debug("Setting IObjectFactory. Will configure target, interceptors and introductions...");
				}

				#endregion

				ConfigureAdvisorChain();
				ConfigureIntroductions();

				#region Instrumentation

				if (logger.IsDebugEnabled)
				{
					logger.Debug("ProxyFactoryObject config: " + this);
				}

				#endregion

				if (IsSingleton)
				{
					if (this.targetName != null)
					{
						TargetSource = NamedObjectToTargetSource(this.objectFactory.GetObject(this.targetName));
					}

					// eagerly initialize the shared singleton instance...
					this.singletonInstance = CreateAopProxy().GetProxy();

					// must listen to superclass advice and interface change
					// events to recache singleton instance if necessary...
					AddListener(this);
				}
			}
        }

        #endregion

        #region IFactoryObject implementation

        /// <summary> 
		/// Creates an instance of the AOP proxy to be returned by this factory
		/// </summary>
		/// <remarks>
		/// <p>
		/// Invoked when clients obtain objects from this factory object. The
		/// (proxy) instance will be cached for a singleton, and created on each
		/// call to <see cref="Spring.Aop.Framework.ProxyFactoryObject.GetObject"/>
		/// for a prototype.
		/// </p>
		/// </remarks>
		/// <returns>
		/// A fresh AOP proxy reflecting the current state of this factory.
		/// </returns>
		/// <seealso cref="Spring.Objects.Factory.IFactoryObject.GetObject()"/>
		public virtual object GetObject()
		{
		    lock(this.SyncRoot)
		    {
		        return (this.IsSingleton ? GetSingletonInstance() : NewPrototypeInstance());
		    }
		}

		/// <summary>
		/// Return the <see cref="System.Type"/> of the proxy. 
		/// </summary>
		/// <remarks>
		/// Will check the singleton instance if already created, 
		/// else fall back to the proxy interface (if a single one),
		/// the target bean type, or the TargetSource's target class.
		/// </remarks>
		/// Return the <see cref="System.Type"/> of object that this
		/// <see cref="Spring.Objects.Factory.IFactoryObject"/> creates, or
		/// <see langword="null"/> if not known in advance.
		public virtual Type ObjectType
		{
			get 
			{ 
				if (this.singletonInstance != null) 
				{
					return this.singletonInstance.GetType();
				}
				else if (Interfaces.Length == 1) 
				{
					return Interfaces[0];
				}
				else if (this.targetName != null && this.objectFactory != null) 
				{
					return this.objectFactory.GetType(this.targetName);
				}
				else 
				{
					return TargetSource.TargetType;
				}
			}
		}

		/// <summary>
		/// Is the object managed by this factory a singleton or a prototype?
		/// </summary>
		public virtual bool IsSingleton
		{
			get { return this.singleton; }
			set { this.singleton = value; }
        }

        #endregion

        #region Private Methods

        private object NewPrototypeInstance()
		{
			RefreshAdvisorChain();
			RefreshTarget();
			RefreshIntroductions();

			// in the case of a prototype, we need to give the proxy
			// an independent instance of the configuration...

			#region Instrumentation

			if (logger.IsDebugEnabled)
			{
				logger.Debug("Creating copy of prototype ProxyFactoryObject config: " + this);
			}

			#endregion

			AdvisedSupport copy = new AdvisedSupport();
			copy.CopyConfigurationFrom(this);

			#region Instrumentation

			if (logger.IsDebugEnabled)
			{
				logger.Debug("Copy has config: " + copy);
			}

			#endregion

			object generatedProxy = copy.CreateAopProxy().GetProxy();
		    this.ProxyType = copy.ProxyType;
		    this.ProxyConstructor = copy.ProxyConstructor;
		    return generatedProxy;
		}

		/// <summary>Create the advisor (interceptor) chain.</summary>
		/// <remarks>
		/// The advisors that are sourced from an ObjectFactory will be refreshed each time
		/// a new prototype instance is added. Interceptors added programmatically through 
		/// the factory API are unaffected by such changes.
		/// </remarks>
		private void ConfigureAdvisorChain()
		{
			if (this.interceptorNames == null || this.interceptorNames.Length == 0)
			{
				return;
			}

			// materialize interceptor chain from object names...
			for (int i = 0; i < this.interceptorNames.Length; ++i)
			{
				string name = this.interceptorNames[i];

				if(name == null) 
				{
					throw new AopConfigException("Found null interceptor name value in the InterceptorNames list; check your configuration.");
				}

				#region Instrumentation

				if (logger.IsDebugEnabled)
				{
					logger.Debug("Configuring interceptor '" + name + "'");
				}

				#endregion

				if (name.EndsWith(GlobalInterceptorSuffix))
				{
					IListableObjectFactory lof = this.objectFactory as IListableObjectFactory;
					if (lof == null)
					{
						// TODO : test this...
						throw new AopConfigException(
							"Can only use global advisors or interceptors in conjunction with an IListableObjectFactory.");
					}
					else
					{
						AddGlobalAdvisor((IListableObjectFactory) this.objectFactory,
						                 name.Substring(0, (name.Length - GlobalInterceptorSuffix.Length)));
						continue;
					}
				}
                
				else if (i == this.interceptorNames.Length - 1 &&
					this.targetName == null &&
					this.m_targetSource == EmptyTargetSource.Empty)
				{
					// the last name in the chain may be an IAdvisor/IAdvice or a target/ITargetSource;
					// unfortunately we don't know; we must look at type of the object...
					if (!IsNamedObjectAnAdvisorOrAdvice(name))
					{
						this.targetName = name;
						continue;
					}
				}
                
				object advice = null;
				if(this.IsSingleton ||
					this.objectFactory.IsSingleton(name)) 
				{
					advice = this.objectFactory.GetObject(name);
				}
				else
				{
					advice = this.objectFactory.GetObject(name);
				}
                if (advice is IAdvisors)
                {
                    IAdvisors advisors = (IAdvisors)advice;
                    foreach (object advisor in advisors.Advisors)
                    {
                        AddAdvisor(advisor, name);
                    }
                }
                else
                {
                    AddAdvisor(advice, name);
                }
			}
		}

        
		private bool IsNamedObjectAnAdvisorOrAdvice(string name)
		{
			Type namedObjectType = this.objectFactory.GetType(name);
			if (namedObjectType != null)
			{
				return typeof(IAdvisors).IsAssignableFrom(namedObjectType)
                    || typeof(IAdvisor).IsAssignableFrom(namedObjectType)
                    || typeof(IAdvice).IsAssignableFrom(namedObjectType);
			}
			// treat it as an IAdvisor if we can't tell...
			return true;
		}
        

		/// <summary>
		/// Configures introductions for this proxy.
		/// </summary>
		private void ConfigureIntroductions()
		{
			if (this.introductionNames == null || this.introductionNames.Length == 0)
			{
				return;
			}

			// Materialize introductions from object names...
			for (int i = 0; i < this.introductionNames.Length; ++i)
			{
				string name = this.introductionNames[i];

				#region Instrumentation

				if (logger.IsDebugEnabled)
				{
					logger.Debug("Configuring introduction '" + name + "'");
				}

				#endregion

				if (name.EndsWith(GlobalInterceptorSuffix))
				{
					if (!(this.objectFactory is IListableObjectFactory))
					{
						throw new AopConfigException("Can only use global introductions with a ListableObjectFactory");
					}
					else
					{
						AddGlobalIntroduction((IListableObjectFactory) this.objectFactory,
						                      name.Substring(0, (name.Length - GlobalInterceptorSuffix.Length)));
					}
				}
				else
				{
					// add a named introduction
					object introduction = this.objectFactory.GetObject(this.introductionNames[i]);
					AddIntroduction(introduction, this.introductionNames[i]);
				}
			}
		}

		/// <summary> Add all global interceptors and pointcuts.</summary>
		private void AddGlobalAdvisor(IListableObjectFactory objectFactory, string prefix)
		{
            string[] globalAspectNames =
                ObjectFactoryUtils.ObjectNamesForTypeIncludingAncestors(objectFactory, typeof(IAdvisors));
            string[] globalAdvisorNames =
				ObjectFactoryUtils.ObjectNamesForTypeIncludingAncestors(objectFactory, typeof(IAdvisor));
			string[] globalInterceptorNames =
				ObjectFactoryUtils.ObjectNamesForTypeIncludingAncestors(objectFactory, typeof(IInterceptor));
            IList objects = new ArrayList();
			IDictionary names = new Hashtable();

            for (int i = 0; i < globalAspectNames.Length; i++)
            {
                string name = globalAspectNames[i];
                if (name.StartsWith(prefix))
                {
                    IAdvisors advisors = (IAdvisors) objectFactory.GetObject(name);
                    foreach (object advisor in advisors.Advisors)
                    {
                        // exclude introduction advisors from interceptor list
                        if (!(advisor is IIntroductionAdvisor))
                        {
                            objects.Add(advisor);
                            names[advisor] = name;
                        }
                    }
                }
            }
            for (int i = 0; i < globalAdvisorNames.Length; i++)
			{
				string name = globalAdvisorNames[i];
				if (name.StartsWith(prefix))
				{
					object obj = objectFactory.GetObject(name);
					// exclude introduction advisors from interceptor list
					if (!(obj is IIntroductionAdvisor))
					{
						objects.Add(obj);
						names[obj] = name;
					}
				}
			}
			for (int i = 0; i < globalInterceptorNames.Length; i++)
			{
				string name = globalInterceptorNames[i];
				if (name.StartsWith(prefix))
				{
					object obj = objectFactory.GetObject(name);
					objects.Add(obj);
					names[obj] = name;
				}
			}
			((ArrayList) objects).Sort(new OrderComparator());
			foreach (object obj in objects)
			{
				string name = (string) names[obj];
				AddAdvisor(obj, name);
			}
		}

		/// <summary> Add all global introductions.</summary>
		private void AddGlobalIntroduction(IListableObjectFactory objectFactory, string prefix)
		{
            string[] globalAspectNames =
                ObjectFactoryUtils.ObjectNamesForTypeIncludingAncestors(objectFactory, typeof(IAdvisors));
            string[] globalAdvisorNames =
				ObjectFactoryUtils.ObjectNamesForTypeIncludingAncestors(objectFactory, typeof (IAdvisor));
			string[] globalIntroductionNames =
				ObjectFactoryUtils.ObjectNamesForTypeIncludingAncestors(objectFactory, typeof (IAdvice));
			IList objects = new ArrayList();
			IDictionary names = new Hashtable();

            for (int i = 0; i < globalAspectNames.Length; i++)
            {
                string name = globalAspectNames[i];
                if (name.StartsWith(prefix))
                {
                    IAdvisors advisors = (IAdvisors)objectFactory.GetObject(name);
                    foreach (object advisor in advisors.Advisors)
                    {
                        // only include introduction advisors
                        if (advisor is IIntroductionAdvisor)
                        {
                            objects.Add(advisor);
                            names[advisor] = name;
                        }
                    }
                }
            }
            for (int i = 0; i < globalAdvisorNames.Length; i++)
			{
				string name = globalAdvisorNames[i];
				if (name.StartsWith(prefix))
				{
					object obj = objectFactory.GetObject(name);
					// only include introduction advisors
					if (obj is IIntroductionAdvisor)
					{
						objects.Add(obj);
						names[obj] = name;
					}
				}
			}
			for (int i = 0; i < globalIntroductionNames.Length; i++)
			{
				string name = globalIntroductionNames[i];
				if (name.StartsWith(prefix))
				{
					object obj = objectFactory.GetObject(name);
					// exclude other advice types
					if (!(obj is IInterceptor || obj is IBeforeAdvice || obj is IAfterReturningAdvice))
					{
						objects.Add(obj);
						names[obj] = name;
					}
				}
			}
			((ArrayList) objects).Sort(new OrderComparator());
			foreach (object obj in objects)
			{
				string name = (string) names[obj];
				AddIntroduction(obj, name);
			}
		}

		/// <summary> Add the given interceptor or pointcut to the interceptor list.</summary>
		/// <param name="next">interceptor or pointcut to add</param>
		/// <param name="name">object name from which we obtained this object in our owning object factory</param>
		private void AddAdvisor(object next, string name)
		{
			#region Instrumentation

			if (logger.IsDebugEnabled)
			{
				logger.Debug("Adding advisor with name '" + name + "'.");
			}

			#endregion

			IAdvisor advisor = NamedObjectToAdvisor(next);
			AddAdvisor(advisor);

			// Record the pointcut as descended from the given object name.
			// This allows us to refresh the interceptor list, which we'll need to
			// do if we have to create a new prototype instance. Otherwise the new
			// prototype instance wouldn't be truly independent, because it might
			// reference the original instances of prototype interceptors.
			this.sourceDictionary[advisor] = name;
		}

		/// <summary>Add the introduction to the introduction list.</summary>
		/// <remarks>
		/// If specified parameter is IIntroducionAdvisor it is added directly, otherwise it is wrapped
		/// with DefaultIntroductionAdvisor first.
		/// </remarks>
		/// <param name="introduction">introducion to add</param>
		/// <param name="name">object name from which we obtained this object in our owning object factory</param>
		private void AddIntroduction(object introduction, string name)
		{
			logger.Debug("Adding introduction with name [" + name + "]");
			IIntroductionAdvisor advisor = NamedObjectToIntroduction(introduction);
			AddIntroduction(advisor);

			// Record the introduction as descended from the given object name.
			// This allows us to refresh the introduction list, which we'll need to
			// do if we have to create a new prototype instance. Otherwise the new
			// prototype instance wouldn't be truly independent, because it might
			// reference the original instances of prototype introductions.
			this.sourceDictionary[advisor] = name;
		}

		/// <summary> Refresh named objects from the interceptor chain.
		/// We need to do this every time a new prototype instance is returned,
		/// to return distinct instances of prototype interfaces and pointcuts.
		/// </summary>
		private void RefreshAdvisorChain()
		{
			IAdvisor[] advisors = Advisors;
			for (int i = 0; i < advisors.Length; ++i)
			{
				string objectName = (string) this.sourceDictionary[advisors[i]];
				if (objectName != null)
				{
					#region Instrumentation

					if (logger.IsDebugEnabled)
					{
						logger.Debug("Refreshing object named '" + objectName + "'");
					}

					#endregion

					IAdvisor refreshedAdvisor
						= NamedObjectToAdvisor(this.objectFactory.GetObject(objectName));
					ReplaceAdvisor(advisors[i], refreshedAdvisor);
					this.sourceDictionary.Remove(advisors[i]);
					// keep name mapping up to date...
					this.sourceDictionary[refreshedAdvisor] = objectName;
				}
				else
				{
					// We can't throw an exception here, as the user may have added additional
					// pointcuts programmatically we don't know about
					logger.Info(
						"Cannot find object name for Advisor [" + advisors[i] + "] when refreshing advisor chain");
				}
			}
		}

		/// <summary>
		/// Refreshes target object for prototype instances.
		/// </summary>
		private void RefreshTarget()
		{
			#region Instrumentation

			if (logger.IsDebugEnabled)
			{
				logger.Debug("Refreshing target with name '" + this.targetName + "'");
			}

			#endregion

			if (StringUtils.IsNullOrEmpty(this.targetName))
			{
				// TODO test...
				throw new AopConfigException("Target name cannot be null (or composed wholly of whitespace) for prototype factory.");
			}
			object target = this.objectFactory.GetObject(this.targetName);
			TargetSource = NamedObjectToTargetSource(target);
		}

		/// <summary> Refresh named objects from the interceptor chain.
		/// We need to do this every time a new prototype instance is returned,
		/// to return distinct instances of prototype interfaces and pointcuts.
		/// </summary>
		private void RefreshIntroductions()
		{
			IIntroductionAdvisor[] introductions = Introductions;
			for (int i = 0; i < introductions.Length; i++)
			{
				string objectName = (string) this.sourceDictionary[introductions[i]];
				if (objectName != null)
				{
					logger.Info("Refreshing introduction named '" + objectName + "'");
					object obj = this.objectFactory.GetObject(objectName);
					IIntroductionAdvisor refreshedIntroduction = NamedObjectToIntroduction(obj);

					ReplaceIntroduction(i, refreshedIntroduction);
					this.sourceDictionary.Remove(introductions[i]);
					this.sourceDictionary[refreshedIntroduction] = objectName;
				}
				else
				{
					// We can't throw an exception here, as the user may have added additional
					// introductions programmatically we don't know about
					logger.Info(
						"Cannot find object name for Introduction [" + introductions[i] +
							"] when refreshing introduction list");
				}
			}
		}

		/// <summary>Wraps pointcut or interceptor with appropriate advisor</summary>
		/// <param name="next">pointcut or interceptor that needs to be wrapped with advisor</param>
		/// <returns>Advisor</returns>
		private IAdvisor NamedObjectToAdvisor(object next)
		{
			return GlobalAdvisorAdapterRegistry.Instance.Wrap(next);
		}

		/// <summary>Wraps target with SingletonTargetSource if necessary</summary>
		/// <param name="target">target or target source object</param>
		/// <returns>target source passed or target wrapped with SingletonTargetSource</returns>
		private ITargetSource NamedObjectToTargetSource(object target)
		{
			if (target is ITargetSource)
			{
				return (ITargetSource) target;
			}
			else
			{
				// It's an object that needs target source around it.
				return new SingletonTargetSource(target);
			}
		}

		/// <summary>Wraps introduction with IIntroductionAdvisor if necessary</summary>
		/// <param name="introduction">object to wrap</param>
		/// <returns>Introduction advisor</returns>
		private IIntroductionAdvisor NamedObjectToIntroduction(object introduction)
		{
			if (introduction is IIntroductionAdvisor)
			{
				return (IIntroductionAdvisor) introduction;
			}
			else
			{
				return new DefaultIntroductionAdvisor((IAdvice) introduction);
			}
		}

		private object GetSingletonInstance()
		{
			if (this.singletonInstance == null)
			{
				this.singletonInstance = CreateAopProxy().GetProxy();
			}
			return this.singletonInstance;
        }

        #endregion

        #region IAdvisedSupportListener implementation

        /// <seealso cref="Spring.Aop.Framework.IAdvisedSupportListener.Activated(Spring.Aop.Framework.AdvisedSupport)">
		/// </seealso>
		public virtual void Activated(AdvisedSupport advisedSupport)
		{
		}

		/// <summary> No need to do anything when advice change, proxy can handle those changes by itself.</summary>
		/// <seealso cref="Spring.Aop.Framework.IAdvisedSupportListener.AdviceChanged(Spring.Aop.Framework.AdvisedSupport)">
		/// </seealso>
		public virtual void AdviceChanged(AdvisedSupport advisedSupport)
		{
		}

		/// <summary>Implementation of listener for AdvisedSupport.InterfacesChanged event</summary>
		/// <param name="advisedSupport">event source</param>
		public virtual void InterfacesChanged(AdvisedSupport advisedSupport)
		{
			logger.Info("Implemented interfaces have changed; reseting singleton instance");
			this.singletonInstance = null;
		    this.ProxyType = null;
		    this.ProxyConstructor = null;
		}

		#endregion
	}
}