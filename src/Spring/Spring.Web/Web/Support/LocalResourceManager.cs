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

#region Imports

using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Security;
using System.Security.Permissions;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using Spring.Reflection.Dynamic;
using Spring.Util;

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// This ResourceManager implementation swallows <see cref="MissingManifestResourceException"/>s and
    /// simply returns <c>null</c> from <see cref="ResourceManager.GetObject(string,CultureInfo)"/> if no resource is found.
    /// </summary>
    /// <author>Erich Eichinger</author>
    internal abstract class LocalResourceManager : ResourceManager
    {
        private delegate IResourceProvider GetResourceProviderDelegate( TemplateControl control );
        private static readonly GetResourceProviderDelegate getLocalResourceProvider;
        private static readonly Type LocalResXResourceProviderFactoryType;
        private static readonly Type LocalResXResourceProviderType;
        private static readonly ResourceProviderFactory LocalResXResourceProviderFactory;
        private static readonly SafeMethod fnGetLocalResourceAssembly;

        /// <summary>
        /// Avoid beforeFieldInit
        /// </summary>
        static LocalResourceManager()
        {
            LocalResXResourceProviderFactoryType = typeof(IResourceProvider).Assembly.GetType("System.Web.Compilation.ResXResourceProviderFactory", true);
            LocalResXResourceProviderType = typeof(IResourceProvider).Assembly.GetType("System.Web.Compilation.LocalResXResourceProvider", true);

            GetResourceProviderDelegate fnGetResourceProvider = null;
            ResourceProviderFactory rpf = null;
            SafeMethod glra = null;
            SecurityCritical.ExecutePrivileged(new PermissionSet(PermissionState.Unrestricted), delegate
            {
                fnGetResourceProvider = (GetResourceProviderDelegate)Delegate.CreateDelegate(typeof(GetResourceProviderDelegate), typeof(ResourceExpressionBuilder).GetMethod("GetLocalResourceProvider", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(TemplateControl) }, null));
                rpf = (ResourceProviderFactory)Activator.CreateInstance( LocalResXResourceProviderFactoryType, true );
                glra = new SafeMethod(LocalResXResourceProviderType.GetMethod("GetLocalResourceAssembly", BindingFlags.Instance | BindingFlags.NonPublic));
            });
            getLocalResourceProvider = fnGetResourceProvider;
            LocalResXResourceProviderFactory = rpf;
            fnGetLocalResourceAssembly = glra;
        }

        internal static ResourceManager GetLocalResourceManager( TemplateControl control )
        {
            IResourceProvider localResourceProvider = GetLocalResourceProvider( control );
            if (localResourceProvider == null)
            {
                return null;
            }

            if (localResourceProvider.GetType() == LocalResXResourceProviderType)
            {
                Assembly localResourceAssembly = GetLocalResourceAssembly( control );
                if (localResourceAssembly != null)
                {
                    return new LocalResXAssemblyResourceManager(control, localResourceAssembly);
                }
                else
                {
                    return null;
                }
            }

            return new ResourceProviderResourceManager(localResourceProvider);
        }

        internal static IResourceProvider GetLocalResourceProvider( TemplateControl templateControl )
        {
            IResourceProvider localResourceProvider = getLocalResourceProvider( templateControl );
            return localResourceProvider;
        }

        internal static Assembly GetLocalResourceAssembly( TemplateControl control )
        {
            object localResXResourceProvider = LocalResXResourceProviderFactory.CreateLocalResourceProvider( control.AppRelativeVirtualPath );
            Assembly localResourceAssembly = (Assembly)fnGetLocalResourceAssembly.Invoke( localResXResourceProvider, null );
            return localResourceAssembly;
        }

        #region ResourceProviderResourceManager

        private class ResourceProviderResourceManager : ResourceManager
        {
            private bool _hasException;
            private readonly IResourceProvider _resourceProvider;

            public ResourceProviderResourceManager(IResourceProvider resourceProvider)
            {
                _resourceProvider = resourceProvider;
            }

            ///<summary>
            ///Returns the value of the specified <see cref="T:System.Object"></see> resource.
            ///</summary>
            ///<returns>
            ///The value of the resource localized for the caller's current culture settings. If a match is not possible, null is returned. The resource value can be null.
            ///</returns>
            ///<param name="name">The name of the resource to get. </param>
            ///<exception cref="T:System.ArgumentNullException">The name parameter is null. </exception>
            public override object GetObject( string name )
            {
                return this.GetObject( name, null );
            }

            ///<summary>
            ///Gets the value of the <see cref="T:System.Object"></see> resource localized for the specified culture.
            ///</summary>
            ///<returns>
            ///The value of the resource, localized for the specified culture. If a "best match" is not possible, null is returned.
            ///</returns>
            ///<param name="culture">The <see cref="T:System.Globalization.CultureInfo"></see> object that represents the culture for which the resource is localized. Note that if the resource is not localized for this culture, the lookup will fall back using the culture's <see cref="P:System.Globalization.CultureInfo.Parent"></see> property, stopping after checking in the neutral culture.If this value is null, the <see cref="T:System.Globalization.CultureInfo"></see> is obtained using the culture's <see cref="P:System.Globalization.CultureInfo.CurrentUICulture"></see> property. </param>
            ///<param name="name">The name of the resource to get. </param>
            ///<exception cref="T:System.ArgumentNullException">The name parameter is null. </exception>
            public override object GetObject( string name, CultureInfo culture )
            {
                if (_hasException)
                    return null;

                try
                {
                    return _resourceProvider.GetObject(name, culture);
                }
                catch (Exception)
                {
                    _hasException = true;
                }
                return null;
            }

            public override ResourceSet GetResourceSet( CultureInfo culture, bool createIfNotExists, bool tryParents )
            {
                ResourceSet resourceSet = null;
                if (culture == CultureInfo.InvariantCulture)
                {
                    resourceSet = new ResourceSet(_resourceProvider.ResourceReader);
                }
                return resourceSet;
            }
        }

        #endregion

        #region LocalResXAssemblyResourceManager

        private class LocalResXAssemblyResourceManager : ResourceManager
        {
            private bool _isMissingManifest = false;

            public LocalResXAssemblyResourceManager( TemplateControl templateControl, Assembly localResourceAssembly )
                : base(
                    VirtualPathUtility.GetFileName( templateControl.AppRelativeVirtualPath ), localResourceAssembly )
            {
                AssertUtils.ArgumentNotNull( templateControl, "templateControl" );
                AssertUtils.ArgumentNotNull( localResourceAssembly, "localResourceAssembly" );
            }

            ///<summary>
            ///Returns the value of the specified <see cref="T:System.Object"></see> resource.
            ///</summary>
            ///<returns>
            ///The value of the resource localized for the caller's current culture settings. If a match is not possible, null is returned. The resource value can be null.
            ///</returns>
            ///<param name="name">The name of the resource to get. </param>
            ///<exception cref="T:System.ArgumentNullException">The name parameter is null. </exception>
            public override object GetObject( string name )
            {
                return this.GetObject( name, null );
            }

            ///<summary>
            ///Gets the value of the <see cref="T:System.Object"></see> resource localized for the specified culture.
            ///</summary>
            ///<returns>
            ///The value of the resource, localized for the specified culture. If a "best match" is not possible, null is returned.
            ///</returns>
            ///<param name="culture">The <see cref="T:System.Globalization.CultureInfo"></see> object that represents the culture for which the resource is localized. Note that if the resource is not localized for this culture, the lookup will fall back using the culture's <see cref="P:System.Globalization.CultureInfo.Parent"></see> property, stopping after checking in the neutral culture.If this value is null, the <see cref="T:System.Globalization.CultureInfo"></see> is obtained using the culture's <see cref="P:System.Globalization.CultureInfo.CurrentUICulture"></see> property. </param>
            ///<param name="name">The name of the resource to get. </param>
            ///<exception cref="T:System.ArgumentNullException">The name parameter is null. </exception>
            public override object GetObject( string name, CultureInfo culture )
            {
                if (_isMissingManifest)
                    return null;

                try
                {
                    return base.GetObject( name, culture );
                }
                catch (MissingManifestResourceException)
                {
                    _isMissingManifest = true;
                }
                return null;
            }
        }

        #endregion
    }
}
