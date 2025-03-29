/*
 * Copyright � 2002-2011 the original author or authors.
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

using System.ServiceModel;
using Microsoft.Extensions.Logging;
using Spring.Objects.Factory;

namespace Spring.ServiceModel.Activation;

/// <summary>
/// Factory that provides instances of <see cref="ServiceHost" />
/// to host objects created with Spring's IoC container.
/// </summary>
/// <author>Bruno Baia</author>
public class ServiceHostFactoryObject : IFactoryObject, IInitializingObject, IObjectFactoryAware, IDisposable
{
    private static readonly ILogger<ServiceHostFactoryObject> LOG = LogManager.GetLogger<ServiceHostFactoryObject>();

    private string _targetName;
    private bool _useServiceProxyTypeCache = true;
    private Uri[] _baseAddresses = new Uri[] { };

    /// <summary>
    /// The owning factory.
    /// </summary>
    private IObjectFactory objectFactory;

    /// <summary>
    /// The <see cref="Spring.ServiceModel.SpringServiceHost" /> instance managed by this factory.
    /// </summary>
    protected SpringServiceHost springServiceHost;

    /// <summary>
    /// Gets or sets the name of the target object that should be exposed as a service.
    /// </summary>
    /// <value>
    /// The name of the target object that should be exposed as a service.
    /// </value>
    public string TargetName
    {
        get { return _targetName; }
        set { _targetName = value; }
    }

    /// <summary>
    /// Gets or sets the base addresses for the hosted service.
    /// </summary>
    /// <value>
    /// The base addresses for the hosted service.
    /// </value>
    public Uri[] BaseAddresses
    {
        get { return _baseAddresses; }
        set { _baseAddresses = value; }
    }

    /// <summary>
    /// Controls, whether the underlying <see cref="SpringServiceHost"/> should cache
    /// the generated proxy types. Defaults to <c>true</c>.
    /// </summary>
    public bool UseServiceProxyTypeCache
    {
        get { return _useServiceProxyTypeCache; }
        set { _useServiceProxyTypeCache = value; }
    }

    /// <summary>
    /// Creates a new instance of the
    /// <see cref="Spring.ServiceModel.Activation.ServiceHostFactoryObject"/> class.
    /// </summary>
    public ServiceHostFactoryObject()
    {
    }

    /// <summary>
    /// Callback that supplies the owning factory to an object instance.
    /// </summary>
    /// <value>
    /// Owning <see cref="Spring.Objects.Factory.IObjectFactory"/>
    /// (may not be <see langword="null"/>). The object can immediately
    /// call methods on the factory.
    /// </value>
    /// <remarks>
    /// <p>
    /// Invoked after population of normal object properties but before an init
    /// callback like <see cref="Spring.Objects.Factory.IInitializingObject"/>'s
    /// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
    /// method or a custom init-method.
    /// </p>
    /// </remarks>
    /// <exception cref="Spring.Objects.ObjectsException">
    /// In case of initialization errors.
    /// </exception>
    public virtual IObjectFactory ObjectFactory
    {
        protected get { return this.objectFactory; }
        set { this.objectFactory = value; }
    }

    /// <summary>
    /// Return a <see cref="Spring.ServiceModel.SpringServiceHost" /> instance
    /// managed by this factory.
    /// </summary>
    /// <returns>
    /// An instance of <see cref="Spring.ServiceModel.SpringServiceHost" />
    /// managed by this factory.
    /// </returns>
    public virtual object GetObject()
    {
        return springServiceHost;
    }

    /// <summary>
    /// Return the <see cref="System.Type"/> of object that this
    /// <see cref="Spring.Objects.Factory.IFactoryObject"/> creates.
    /// </summary>
    public virtual Type ObjectType
    {
        get { return typeof(SpringServiceHost); }
    }

    /// <summary>
    /// Always returns <see langword="false"/>
    /// </summary>
    public virtual bool IsSingleton
    {
        get { return false; }
    }

    /// <summary>
    /// Publish the object.
    /// </summary>
    public virtual void AfterPropertiesSet()
    {
        ValidateConfiguration();

        springServiceHost = new SpringServiceHost(TargetName, objectFactory, UseServiceProxyTypeCache, BaseAddresses);

        springServiceHost.Open();

        if (LOG.IsEnabled(LogLevel.Information))
        {
            LOG.LogInformation(String.Format("The service '{0}' is ready and can now be accessed.", TargetName));
        }
    }

    /// <summary>
    /// Close the SpringServiceHost
    /// </summary>
    public void Dispose()
    {
        if (springServiceHost != null)
        {
            springServiceHost.Close();
        }
    }

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    protected virtual void ValidateConfiguration()
    {
        if (TargetName == null)
        {
            throw new ArgumentException("The TargetName property is required.");
        }
    }
}
