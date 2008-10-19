#region License

/*
 * Copyright © 2002-2008 the original author or authors.
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
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Web;
using Spring.Util;

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// This ResourceManager implementation swallows <see cref="MissingManifestResourceException"/>s and 
    /// simply returns <c>null</c> from <see cref="GetObject(string,CultureInfo"/> if no resource is found.
    /// </summary>
    /// <author>Erich Eichinger</author>
    internal class LocalResourceManager : ResourceManager
    {
        private string _virtualPath;
        private bool _isMissingManifest = false;

        public LocalResourceManager(string virtualPath) : base()
        {
            AssertUtils.ArgumentNotNull(virtualPath, "virtualPath");
            _virtualPath = virtualPath;
        }

//        public LocalResourceManager(string baseName, Assembly assembly) : base(baseName, assembly)
//        {}

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
            if (_isMissingManifest) return null;

            try
            {
                return HttpContext.GetLocalResourceObject(_virtualPath, name, culture);
                //return base.GetObject( name, culture );
            }
            catch (MissingManifestResourceException ex)
            {     
                _isMissingManifest = true;
            }
            return null;
        }
    }
}