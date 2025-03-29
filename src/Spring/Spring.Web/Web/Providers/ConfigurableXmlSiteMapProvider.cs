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

using System.Collections.Specialized;
using System.Security.Permissions;
using System.Web;
using System.Web.Security;

namespace Spring.Web.Providers;

/// <summary>
/// A spring-configurable version of <see cref="SqlRoleProvider"/>
/// </summary>
/// <author>Erich Eichinger</author>
[AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
[AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
public class ConfigurableXmlSiteMapProvider : XmlSiteMapProvider, ISiteMapProvider
{
    private bool initialized;
    private string siteMapFile;
    private NameValueCollection parameters;

    /// <summary>
    /// The XML file to be used for reading in the sitemap
    /// </summary>
    public string SiteMapFile
    {
        get { return this.siteMapFile; }
        set { this.siteMapFile = value; }
    }

    /// <summary>
    /// A collection of the name/value pairs representing the provider-specific
    /// attributes specified in the configuration for this provider.
    /// </summary>
    public NameValueCollection Parameters
    {
        get { return this.parameters; }
        set { this.parameters = value; }
    }

    ///<summary>
    ///Initializes the provider.
    ///</summary>
    ///
    ///<param name="config">
    /// <para>
    /// A collection of the name/value pairs representing the provider-specific
    /// attributes specified in the configuration for this provider.
    /// </para>
    /// Values may be overridden by specifying them in <see cref="Parameters"/> list.
    /// </param>
    ///<param name="name">The friendly name of the provider.</param>
    ///<exception cref="T:System.ArgumentNullException">The <paramref name="name"/> or <paramref name="config"/> is null.</exception>
    ///<exception cref="T:System.InvalidOperationException">An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"></see> on a provider after the provider has already been initialized.</exception>
    ///<exception cref="T:System.ArgumentException">The <paramref name="name"/> has a length of zero or providerId attribute is not set.</exception>
    public override void Initialize(string name, NameValueCollection config)
    {
        lock (this)
        {
            if (initialized) return;

            if (parameters != null)
            {
                foreach (string key in parameters.Keys)
                {
                    config[key] = parameters[key];
                }
            }

            config["siteMapFile"] = this.siteMapFile;
            base.Initialize(name, config);

            initialized = true;
        }
    }
}
