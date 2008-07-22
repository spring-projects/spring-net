#region License

/*
 * Copyright 2002-2006 the original author or authors.
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

#if (!NET_1_0 && !MONO)

#region Imports

using System;
using System.Collections;
using System.EnterpriseServices;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

using Spring.Objects.Factory;
using Spring.Util;

#endregion

namespace Spring.EnterpriseServices
{
    /// <summary>
    /// Exports specified components as ServicedComponents.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This class will create ServicedComponent wrapper for each of the
    /// specified components and register them with the Component Services.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class EnterpriseServicesExporter : IInitializingObject, IObjectFactoryAware
    {
        #region Fields

        private IObjectFactory objectFactory;

        private IList components = new ArrayList();
        private string applicationName;
        private string applicationId;
        private ActivationOption activationMode = ActivationOption.Library;
        private string description;
        private ApplicationAccessControlAttribute accessControl;
        private ApplicationQueuingAttribute applicationQueuing;
        private IList roles;

        private string assemblyName;

        #endregion

        #region Constructor(s) / Destructor

        /// <summary>
        /// Creates new enterprise services exporter.
        /// </summary>
        public EnterpriseServicesExporter()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets list of components to export.
        /// </summary>
        public IList Components
        {
            get { return components; }
            set { components = value; }
        }

        /// <summary>
        /// Gets or sets COM+ application name.
        /// </summary>
        public string ApplicationName
        {
            get { return applicationName; }
            set { applicationName = value; }
        }

        /// <summary>
        /// Gets or sets application identifier (GUID). Defaults to generated GUID if not specified.
        /// </summary>
        public string ApplicationId
        {
            get { return applicationId; }
            set { applicationId = value; }
        }

        /// <summary>
        /// Gets or sets application activation mode, which can be either <b>Server</b> or <b>Library</b> (default).
        /// </summary>
        public ActivationOption ActivationMode
        {
            get { return activationMode; }
            set { activationMode = value; }
        }

        /// <summary>
        /// Gets or sets application description.
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        /// <summary>
        /// Gets or sets access control attribute.
        /// </summary>
        public ApplicationAccessControlAttribute AccessControl
        {
            get { return accessControl; }
            set { accessControl = value; }
        }

        /// <summary>
        /// Gets or sets application queuing attribute.
        /// </summary>
        public ApplicationQueuingAttribute ApplicationQueuing
        {
            get { return applicationQueuing; }
            set { applicationQueuing = value; }
        }

        /// <summary>
        /// Gets or sets application roles.
        /// </summary>
        public IList Roles
        {
            get { return roles; }
            set { roles = value; }
        }

        /// <summary>
        /// Gets or sets name of the generated assembly that will contain serviced components.
        /// </summary>
        public string Assembly
        {
            get { return assemblyName; }
            set { assemblyName = value; }
        }

        #endregion

        #region IInitializingObject Members

        /// <summary>
        /// Called by Spring container after object is configured in order to initialize it.
        /// </summary>
        public void AfterPropertiesSet()
        {
            if (roles != null && roles.Count > 0)
            {
                RefreshRoles();
            }
            Export();
        }

        #endregion

        #region IObjectFactoryAware Members

        /// <summary>
        /// Sets object factory instance.
        /// </summary>
        public IObjectFactory ObjectFactory
        {
            set { objectFactory = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates ServicedComponent wrappers for the specified components and registers
        /// them with COM+ Component Services.
        /// </summary>
        public virtual void Export()
        {
            AssemblyName an = new AssemblyName();
            an.Name = assemblyName;
            an.Version = new Version("1.0.0.0");
            an.KeyPair = new StrongNameKeyPair(GetKeyPair());
            AssemblyBuilder proxyAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder module = proxyAssembly.DefineDynamicModule(assemblyName, assemblyName + ".dll", true);
            ApplyAssemblyAttributes(proxyAssembly);

            foreach (ServicedComponentExporter definition in components)
            {
                definition.CreateWrapperType(module, objectFactory);
            }

            proxyAssembly.Save(assemblyName + ".dll");

            RegistrationConfig config = new RegistrationConfig();
            config.Application = applicationName;
            config.AssemblyFile = AppDomain.CurrentDomain.DynamicDirectory + assemblyName + ".dll";
            config.InstallationFlags = InstallationFlags.FindOrCreateTargetApplication;

            RegistrationHelper regHelper = new RegistrationHelper();
            regHelper.InstallAssemblyFromConfig(ref config);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Reads key pair from embedded resource.
        /// </summary>
        /// <returns>Key pair as a byte array.</returns>
        private byte[] GetKeyPair()
        {
            using (Stream keys = GetType().Assembly.GetManifestResourceStream("Spring.EnterpriseServices.EnterpriseServices.keys"))
            {
                byte[] bytes = new byte[keys.Length];
                keys.Read(bytes, 0, (int) keys.Length);
                return bytes;
            }
        }

        /// <summary>
        /// Applies custom attributes to generated assembly.
        /// </summary>
        /// <param name="assembly">Dynamic assembly to apply attributes to.</param>
        private void ApplyAssemblyAttributes(AssemblyBuilder assembly)
        {
            assembly.SetCustomAttribute(ReflectionUtils.CreateCustomAttribute(typeof(ApplicationNameAttribute), applicationName));
            assembly.SetCustomAttribute(ReflectionUtils.CreateCustomAttribute(typeof(ApplicationActivationAttribute), activationMode));
            if (applicationId != null)
            {
                assembly.SetCustomAttribute(ReflectionUtils.CreateCustomAttribute(typeof(ApplicationIDAttribute), applicationId));
            }
            if (description != null)
            {
                assembly.SetCustomAttribute(ReflectionUtils.CreateCustomAttribute(typeof(DescriptionAttribute), description));
                assembly.SetCustomAttribute(ReflectionUtils.CreateCustomAttribute(typeof(AssemblyDescriptionAttribute), description));
            }
            if (accessControl != null)
            {
                assembly.SetCustomAttribute(ReflectionUtils.CreateCustomAttribute(accessControl));
            }
            if (applicationQueuing != null)
            {
                assembly.SetCustomAttribute(ReflectionUtils.CreateCustomAttribute(applicationQueuing));
            }
            if (roles != null)
            {
                foreach (SecurityRoleAttribute role in roles)
                {
                    assembly.SetCustomAttribute(ReflectionUtils.CreateCustomAttribute(typeof(SecurityRoleAttribute),
                                                                                      new object[] {role.Role}, role));
                }
            }
        }

        /// <summary>
        /// Replaces roles expressed using string with appropriate SecurityRoleAttribute instance.
        /// </summary>
        private void RefreshRoles()
        {
            for (int i = 0; i < roles.Count; i++)
            {
                object role = roles[i];
                if (role is string)
                {
                    roles[i] = ParseRole((string) role);
                }
            }
        }

        /// <summary>
        /// Parses string representation of SecurityRoleAttribute.
        /// </summary>
        /// <param name="roleString">Role definition string.</param>
        /// <returns>Configured SecurityRoleAttribute instance.</returns>
        private SecurityRoleAttribute ParseRole(string roleString)
        {
            string[] parts = roleString.Split(':');
            SecurityRoleAttribute role = new SecurityRoleAttribute(parts[0].Trim());
            if (parts.Length > 1)
            {
                role.Description = parts[1].Trim();
            }
            if (parts.Length > 2)
            {
                role.SetEveryoneAccess = bool.Parse(parts[2].Trim());
            }

            return role;
        }

        #endregion
    }
}

#endif // (!NET_1_0)
