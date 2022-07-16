#region License

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

#endregion

using System.Reflection;
using System.Resources;

using Spring.Util;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// A convenience class to create a
    /// <see cref="System.Resources.ResourceManager"/> given the resource base
    /// name and assembly name.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This is currently the preferred way of injecting resources into view
    /// tier components (such as Windows Forms GUIs and ASP.NET ASPX pages).
    /// A GUI component (typically a Windows Form) is injected with
    /// an <see cref="System.Resources.ResourceManager"/> instance, and can
    /// then proceed to use the various <c>GetXxx()</c> methods on the
    /// <see cref="System.Resources.ResourceManager"/> to retrieve images,
    /// strings, custom resources, etc.
    /// </p>
    /// </remarks>
    /// <author>Mark Pollack</author>
    /// <seealso cref="Spring.Objects.Factory.Config.AbstractFactoryObject.GetObject"/>
    /// <seealso cref="Spring.Objects.Factory.Config.AbstractFactoryObject.CreateInstance"/>
    /// <seealso cref="System.Resources.ResourceManager"/>
    [Serializable]
    public class ResourceManagerFactoryObject : AbstractFactoryObject
    {
        private string _baseName;
        private string _assemblyName;

        /// <summary>
        /// The root name of the resources.
        /// </summary>
        /// <remarks>
        /// <p>
        /// For example, the root name for the resource file named
        /// "MyResource.en-US.resources" is "MyResource".
        /// </p>
        /// <note>
        /// The namespace is also prefixed before the resource file name.
        /// </note>
        /// </remarks>
        public string BaseName
        {
            get { return _baseName; }
            set { _baseName = value; }
        }

        /// <summary>
        /// The string representation of the assembly that contains the resource.
        /// </summary>
        public string AssemblyName
        {
            get { return _assemblyName; }
            set { _assemblyName = value; }
        }

        /// <summary>
        /// The <see cref="System.Resources.ResourceManager"/> <see cref="System.Type"/>.
        /// </summary>
        public override Type ObjectType
        {
            get { return typeof(ResourceManager); }
        }

        /// <summary>
        /// Creates a <see cref="System.Resources.ResourceManager"/>.
        /// </summary>
        /// <exception cref="System.Exception">
        /// If an exception occured during object creation.
        /// </exception>
        /// <returns>The object returned by this factory.</returns>
        /// <seealso cref="Spring.Objects.Factory.Config.AbstractFactoryObject.GetObject"/>
        /// <seealso cref="Spring.Objects.Factory.Config.AbstractFactoryObject.CreateInstance"/>
        protected override object CreateInstance()
        {
            Assembly assembly;
            try
            {
                assembly = Assembly.Load(AssemblyName);
            }
            catch (FileLoadException)
            {
                throw new ArgumentException("Not able to load assembly with a given name [" + _assemblyName + "]");
            }

            if (assembly == null)
            {
                throw new ArgumentException("Not able to load assembly with a given name [" + _assemblyName + "]");
            }

            return new ResourceManager(_baseName, assembly);
        }

        /// <summary>
        /// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
        /// after it has set all object properties supplied
        /// (and satisfied the
        /// <see cref="Spring.Objects.Factory.IObjectFactoryAware"/>
        /// and <see cref="Spring.Context.IApplicationContextAware"/>
        /// interfaces).
        /// </summary>
        /// <exception cref="System.Exception">
        /// In the event of misconfiguration (such as failure to set an essential
        /// property) or if initialization fails.
        /// </exception>
        /// <seealso cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
        public override void AfterPropertiesSet()
        {
            if (StringUtils.IsNullOrEmpty(BaseName))
            {
                throw new ArgumentException("The 'BaseName' property for the resource is required.");
            }
            if (StringUtils.IsNullOrEmpty(AssemblyName))
            {
                throw new ArgumentException("The 'AssemblyName' property for the resource is required.");
            }
            base.AfterPropertiesSet();
        }
    }
}
