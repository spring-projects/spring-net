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

using System.Collections.Specialized;
using System.Security.Permissions;
using System.Web;
using Spring.Context.Support;

#endregion

namespace Spring.Web.Providers
{
    /// <summary>
    /// Wrapper for <see cref="System.Web.SiteMapProvider "/> class.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Configuration for this provider <b>requires</b> <c>providerId</c> element set in web.config file,
    /// as the Id of wrapped provider (defined in the Spring context).
    /// </p>
    /// </remarks>
    /// <author>Damjan Tomic</author>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class SiteMapProviderAdapter : SiteMapProvider, ISiteMapProvider
    {
        #region Field

        /// <summary>
        /// Reference to wrapped provider (defined in Spring context).
        /// </summary>
        private SiteMapProvider wrappedProvider;

        #endregion

        #region ProviderBase members

        ///<summary>
        ///Initializes the provider.
        ///</summary>
        ///
        ///<param name="config">A collection of the name/value pairs representing the provider-specific
        /// attributes specified in the configuration for this provider.
        /// The <c>providerId</c> attribute may be used to override the name being used for looking up an object definition.
        /// </param>
        ///<param name="name">The friendly name of the provider.</param>
        ///<exception cref="T:System.ArgumentNullException">The <paramref name="name"/> or <paramref name="config"/> is null.</exception>
        ///<exception cref="T:System.InvalidOperationException">An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"></see> on a provider after the provider has already been initialized.</exception>
        ///<exception cref="T:System.ArgumentException">The <paramref name="name"/> has a length of zero or providerId attribute is not set.</exception>
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null) throw new ArgumentNullException("config");

            string providerId = config["providerId"];
            if (String.IsNullOrEmpty(providerId))
                providerId = name;
            config.Remove("providerId");

            this.wrappedProvider = (System.Web.SiteMapProvider)WebApplicationContext.GetRootContext().GetObject(providerId);
            this.wrappedProvider.Initialize(name,config);
        }

        ///<summary>
        ///Gets the friendly name used to refer to the provider during configuration.
        ///</summary>
        ///
        ///<returns>
        ///The friendly name used to refer to the provider during configuration.
        ///</returns>
        ///
        public override string Name
        {
            get { return this.wrappedProvider.Name; }
        }

        ///<summary>
        ///Gets a brief, friendly description suitable for display in administrative tools or other user interfaces (UIs).
        ///</summary>
        ///
        ///<returns>
        ///A brief, friendly description suitable for display in administrative tools or other UIs.
        ///</returns>
        ///
        public override string Description
        {
            get { return this.wrappedProvider.Description; }
        }

        #endregion

        #region System.Web.SiteMapProvider members

        ///<summary>
        ///Retrieves a <see cref="T:System.Web.SiteMapNode"></see> object that represents the currently requested page using the specified <see cref="T:System.Web.HttpContext"></see> object.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.Web.SiteMapNode"></see> that represents the currently requested page; otherwise, null, if no corresponding <see cref="T:System.Web.SiteMapNode"></see> can be found in the <see cref="T:System.Web.SiteMapNode"></see> or if the page context is null.
        ///</returns>
        ///
        ///<param name="context">The <see cref="T:System.Web.HttpContext"></see> used to match node information with the URL of the requested page.</param>
        public override SiteMapNode FindSiteMapNode(HttpContext context)
        {
            return this.wrappedProvider.FindSiteMapNode(context);
        }

        ///<summary>
        ///Retrieves a <see cref="T:System.Web.SiteMapNode"></see> object based on a specified key.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.Web.SiteMapNode"></see> that represents the page identified by key; otherwise, null, if no corresponding <see cref="T:System.Web.SiteMapNode"></see> is found or if security trimming is enabled and the <see cref="T:System.Web.SiteMapNode"></see> cannot be returned for the current user. The default is null.
        ///</returns>
        ///
        ///<param name="key">A lookup key with which a <see cref="T:System.Web.SiteMapNode"></see> is created.</param>
        public override SiteMapNode FindSiteMapNodeFromKey(string key)
        {
            return this.wrappedProvider.FindSiteMapNodeFromKey(key);
        }

        ///<summary>
        ///When overridden in a derived class, retrieves a <see cref="T:System.Web.SiteMapNode"></see> object that represents the page at the specified URL.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.Web.SiteMapNode"></see> that represents the page identified by rawURL; otherwise, null, if no corresponding <see cref="T:System.Web.SiteMapNode"></see> is found or if security trimming is enabled and the <see cref="T:System.Web.SiteMapNode"></see> cannot be returned for the current user.
        ///</returns>
        ///
        ///<param name="rawUrl">A URL that identifies the page for which to retrieve a <see cref="T:System.Web.SiteMapNode"></see>. </param>
        public override SiteMapNode FindSiteMapNode(string rawUrl)
        {
            return this.wrappedProvider.FindSiteMapNode(rawUrl);
        }

        ///<summary>
        ///When overridden in a derived class, retrieves the child nodes of a specific <see cref="T:System.Web.SiteMapNode"></see>.
        ///</summary>
        ///
        ///<returns>
        ///A read-only <see cref="T:System.Web.SiteMapNodeCollection"></see> that contains the immediate child nodes of the specified <see cref="T:System.Web.SiteMapNode"></see>; otherwise, null or an empty collection, if no child nodes exist.
        ///</returns>
        ///
        ///<param name="node">The <see cref="T:System.Web.SiteMapNode"></see> for which to retrieve all child nodes. </param>
        public override SiteMapNodeCollection GetChildNodes(SiteMapNode node)
        {
            return this.wrappedProvider.GetChildNodes(node);
        }

        ///<summary>
        ///Provides an optimized lookup method for site map providers when retrieving the node for the currently requested page and fetching the parent and ancestor site map nodes for the current page.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.Web.SiteMapNode"></see> that represents the currently requested page; otherwise, null, if the <see cref="T:System.Web.SiteMapNode"></see> is not found or cannot be returned for the current user.
        ///</returns>
        ///
        ///<param name="upLevel">The number of ancestor site map node generations to get. A value of -1 indicates that all ancestors might be retrieved and cached by the provider.</param>
        ///<exception cref="T:System.ArgumentOutOfRangeException">upLevel is less than -1.</exception>
        public override SiteMapNode GetCurrentNodeAndHintAncestorNodes(int upLevel)
        {
            return this.wrappedProvider.GetCurrentNodeAndHintAncestorNodes(upLevel);
        }

        ///<summary>
        ///Provides an optimized lookup method for site map providers when retrieving the node for the currently requested page and fetching the site map nodes in the proximity of the current node.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.Web.SiteMapNode"></see> that represents the currently requested page; otherwise, null, if the <see cref="T:System.Web.SiteMapNode"></see> is not found or cannot be returned for the current user.
        ///</returns>
        ///
        ///<param name="upLevel">The number of ancestor <see cref="T:System.Web.SiteMapNode"></see> generations to fetch. 0 indicates no ancestor nodes are retrieved and -1 indicates that all ancestors might be retrieved and cached by the provider.</param>
        ///<param name="downLevel">The number of child <see cref="T:System.Web.SiteMapNode"></see> generations to fetch. 0 indicates no descendant nodes are retrieved and a -1 indicates that all descendant nodes might be retrieved and cached by the provider.</param>
        ///<exception cref="T:System.ArgumentOutOfRangeException">upLevel or downLevel is less than -1.</exception>
        public override SiteMapNode GetCurrentNodeAndHintNeighborhoodNodes(int upLevel, int downLevel)
        {
            return this.wrappedProvider.GetCurrentNodeAndHintNeighborhoodNodes(upLevel, downLevel);
        }

        ///<summary>
        ///When overridden in a derived class, retrieves the parent node of a specific <see cref="T:System.Web.SiteMapNode"></see> object.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.Web.SiteMapNode"></see> that represents the parent of node; otherwise, null, if the <see cref="T:System.Web.SiteMapNode"></see> has no parent or security trimming is enabled and the parent node is not accessible to the current user.
        ///</returns>
        ///
        ///<param name="node">The <see cref="T:System.Web.SiteMapNode"></see> for which to retrieve the parent node. </param>
        public override SiteMapNode GetParentNode(SiteMapNode node)
        {
            return this.wrappedProvider.GetParentNode(node);
        }

        ///<summary>
        ///Provides an optimized lookup method for site map providers when retrieving an ancestor node for the currently requested page and fetching the descendant nodes for the ancestor.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.Web.SiteMapNode"></see> that represents an ancestor <see cref="T:System.Web.SiteMapNode"></see> of the currently requested page; otherwise, null, if the current or ancestor <see cref="T:System.Web.SiteMapNode"></see> is not found or cannot be returned for the current user.
        ///</returns>
        ///
        ///<param name="relativeDepthFromWalkup">The number of descendant node levels to retrieve from the target ancestor node. </param>
        ///<param name="walkupLevels">The number of ancestor node levels to traverse when retrieving the requested ancestor node. </param>
        ///<exception cref="T:System.ArgumentOutOfRangeException">walkupLevels or relativeDepthFromWalkup is less than 0.</exception>
        public override SiteMapNode GetParentNodeRelativeToCurrentNodeAndHintDownFromParent(int walkupLevels,
                                                                                            int relativeDepthFromWalkup)
        {
            return this.wrappedProvider.GetParentNodeRelativeToCurrentNodeAndHintDownFromParent(walkupLevels, relativeDepthFromWalkup);
        }

        ///<summary>
        ///Provides an optimized lookup method for site map providers when retrieving an ancestor node for the specified <see cref="T:System.Web.SiteMapNode"></see> object and fetching its child nodes.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.Web.SiteMapNode"></see> that represents an ancestor of node; otherwise, null, if the current or ancestor <see cref="T:System.Web.SiteMapNode"></see> is not found or cannot be returned for the current user.
        ///</returns>
        ///
        ///<param name="relativeDepthFromWalkup">The number of descendant node levels to retrieve from the target ancestor node.</param>
        ///<param name="node">The <see cref="T:System.Web.SiteMapNode"></see> that acts as a reference point for walkupLevels and relativeDepthFromWalkup. </param>
        ///<param name="walkupLevels">The number of ancestor node levels to traverse when retrieving the requested ancestor node.</param>
        ///<exception cref="T:System.ArgumentOutOfRangeException">The value specified for walkupLevels or relativeDepthFromWalkup is less than 0.</exception>
        ///<exception cref="T:System.ArgumentNullException">node is null.</exception>
        public override SiteMapNode GetParentNodeRelativeToNodeAndHintDownFromParent(SiteMapNode node, int walkupLevels,
                                                                                     int relativeDepthFromWalkup)
        {
            return this.wrappedProvider.GetParentNodeRelativeToNodeAndHintDownFromParent(node, walkupLevels, relativeDepthFromWalkup);
        }

        ///<summary>
        /// This method is marked as protected and should never be called.
        ///</summary>
        ///<exception cref="NotSupportedException"></exception>
        ///<returns>
        ///A <see cref="T:System.Web.SiteMapNode"></see> that represents the root node of the set of nodes that the current provider manages.
        ///</returns>
        ///
        protected override SiteMapNode GetRootNodeCore()
        {
            throw new NotSupportedException("This method should never be called.");
        }

        ///<summary>
        ///Provides a method that site map providers can override to perform an optimized retrieval of one or more levels of parent and ancestor nodes, relative to the specified <see cref="T:System.Web.SiteMapNode"></see> object.
        ///</summary>
        ///
        ///<param name="upLevel">The number of ancestor <see cref="T:System.Web.SiteMapNode"></see> generations to fetch. 0 indicates no ancestor nodes are retrieved and -1 indicates that all ancestors might be retrieved and cached.</param>
        ///<param name="node">The <see cref="T:System.Web.SiteMapNode"></see> that acts as a reference point for upLevel.</param>
        ///<exception cref="T:System.ArgumentOutOfRangeException">upLevel is less than -1.</exception>
        ///<exception cref="T:System.ArgumentNullException">node is null.</exception>
        public override void HintAncestorNodes(SiteMapNode node, int upLevel)
        {
            this.wrappedProvider.HintAncestorNodes(node, upLevel);
        }

        ///<summary>
        ///Provides a method that site map providers can override to perform an optimized retrieval of nodes found in the proximity of the specified node.
        ///</summary>
        ///
        ///<param name="upLevel">The number of ancestor <see cref="T:System.Web.SiteMapNode"></see> generations to fetch. 0 indicates no ancestor nodes are retrieved and -1 indicates that all ancestors (and their descendant nodes to the level of node) might be retrieved and cached.</param>
        ///<param name="downLevel">The number of descendant <see cref="T:System.Web.SiteMapNode"></see> generations to fetch. 0 indicates no descendant nodes are retrieved and -1 indicates that all descendant nodes might be retrieved and cached.</param>
        ///<param name="node">The <see cref="T:System.Web.SiteMapNode"></see> that acts as a reference point for upLevel.</param>
        ///<exception cref="T:System.ArgumentOutOfRangeException">upLevel or downLevel is less than -1.</exception>
        ///<exception cref="T:System.ArgumentNullException">node is null.</exception>
        public override void HintNeighborhoodNodes(SiteMapNode node, int upLevel, int downLevel)
        {
            this.wrappedProvider.HintNeighborhoodNodes(node, upLevel, downLevel);
        }

        ///<summary>
        ///Retrieves a Boolean value indicating whether the specified <see cref="T:System.Web.SiteMapNode"></see> object can be viewed by the user in the specified context.
        ///</summary>
        ///
        ///<returns>
        ///true if security trimming is enabled and node can be viewed by the user or security trimming is not enabled; otherwise, false.
        ///</returns>
        ///
        ///<param name="context">The <see cref="T:System.Web.HttpContext"></see> that contains user information.</param>
        ///<param name="node">The <see cref="T:System.Web.SiteMapNode"></see> that is requested by the user.</param>
        ///<exception cref="T:System.ArgumentNullException">context is null.- or -node is null.</exception>
        public override bool IsAccessibleToUser(HttpContext context, SiteMapNode node)
        {
            return this.wrappedProvider.IsAccessibleToUser(context, node);
        }

        ///<summary>
        ///Gets the <see cref="T:System.Web.SiteMapNode"></see> object that represents the currently requested page.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.Web.SiteMapNode"></see> that represents the currently requested page; otherwise, null, if the <see cref="T:System.Web.SiteMapNode"></see> is not found or cannot be returned for the current user.
        ///</returns>
        ///
        public override SiteMapNode CurrentNode
        {
            get { return this.wrappedProvider.CurrentNode; }
        }

        ///<summary>
        ///Gets or sets the parent <see cref="T:System.Web.SiteMapProvider"></see> object of the current provider.
        ///</summary>
        ///
        ///<returns>
        ///The parent provider of the current <see cref="T:System.Web.SiteMapProvider"></see>.
        ///</returns>
        ///
        public override System.Web.SiteMapProvider ParentProvider
        {
            get { return this.wrappedProvider.ParentProvider; }
            set { this.wrappedProvider.ParentProvider = value; }
        }

        ///<summary>
        ///Gets the root <see cref="T:System.Web.SiteMapProvider"></see> object in the current provider hierarchy.
        ///</summary>
        ///
        ///<returns>
        ///An <see cref="T:System.Web.SiteMapProvider"></see> that is the top-level site map provider in the provider hierarchy that the current provider belongs to.
        ///</returns>
        ///
        ///<exception cref="T:System.Configuration.Provider.ProviderException">There is a circular reference to the current site map provider. </exception>
        public override System.Web.SiteMapProvider RootProvider
        {
            get { return this.wrappedProvider.RootProvider; }
        }

        ///<summary>
        ///Gets the root <see cref="T:System.Web.SiteMapNode"></see> object of the site map data that the current provider represents.
        ///</summary>
        ///
        ///<returns>
        ///The root <see cref="T:System.Web.SiteMapNode"></see> of the current site map data provider. The default implementation performs security trimming on the returned node.
        ///</returns>
        ///
        public override SiteMapNode RootNode
        {
            get { return this.wrappedProvider.RootNode; }
        }

        #endregion

    }
}
