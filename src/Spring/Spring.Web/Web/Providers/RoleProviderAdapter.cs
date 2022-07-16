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
using System.Web.Security;
using Spring.Context.Support;

#endregion


namespace Spring.Web.Providers
{
    /// <summary>
    /// Wrapper for <see cref="System.Web.Security.RoleProvider"/> class.
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
    public class RoleProviderAdapter : RoleProvider, IRoleProvider
    {
        #region Field

        /// <summary>
        /// Reference to wrapped provider (defined in Spring context).
        /// </summary>
        private RoleProvider wrappedProvider;

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
            lock (this)
            {
                if (config == null) throw new ArgumentNullException("config");

                string providerId = config["providerId"];
                if (String.IsNullOrEmpty(providerId))
                    providerId = name;
                config.Remove("providerId");

                this.wrappedProvider = (RoleProvider)WebApplicationContext.GetRootContext().GetObject(providerId);
                this.wrappedProvider.Initialize(name,config);
            }
        }

        ///<summary>
        ///Gets the friendly name used to refer to the provider during configuration.
        ///</summary>
        ///
        ///<returns>
        ///The friendly name used to refer to the provider during configuration.
        ///</returns>
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
        public override string Description
        {
            get { return this.wrappedProvider.Description; }
        }

        #endregion

        #region System.Web.Security.RoleProvider members


        ///<summary>
        ///Gets a value indicating whether the specified user is in the specified role for the configured applicationName.
        ///</summary>
        ///
        ///<returns>
        ///true if the specified user is in the specified role for the configured applicationName; otherwise, false.
        ///</returns>
        ///
        ///<param name="username">The user name to search for.</param>
        ///<param name="roleName">The role to search in.</param>
        public override bool IsUserInRole(string username, string roleName)
        {
            return this.wrappedProvider.IsUserInRole(username, roleName);
        }

        ///<summary>
        ///Gets a list of the roles that a specified user is in for the configured applicationName.
        ///</summary>
        ///
        ///<returns>
        ///A string array containing the names of all the roles that the specified user is in for the configured applicationName.
        ///</returns>
        ///
        ///<param name="username">The user to return a list of roles for.</param>
        public override string[] GetRolesForUser(string username)
        {
            return this.wrappedProvider.GetRolesForUser(username);
        }

        ///<summary>
        ///Adds a new role to the data source for the configured applicationName.
        ///</summary>
        ///
        ///<param name="roleName">The name of the role to create.</param>
        public override void CreateRole(string roleName)
        {
            this.wrappedProvider.CreateRole(roleName);
        }

        ///<summary>
        ///Removes a role from the data source for the configured applicationName.
        ///</summary>
        ///
        ///<returns>
        ///true if the role was successfully deleted; otherwise, false.
        ///</returns>
        ///
        ///<param name="throwOnPopulatedRole">If true, throw an exception if roleName has one or more members and do not delete roleName.</param>
        ///<param name="roleName">The name of the role to delete.</param>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            return this.wrappedProvider.DeleteRole(roleName, throwOnPopulatedRole);
        }

        ///<summary>
        ///Gets a value indicating whether the specified role name already exists in the role data source for the configured applicationName.
        ///</summary>
        ///
        ///<returns>
        ///true if the role name already exists in the data source for the configured applicationName; otherwise, false.
        ///</returns>
        ///
        ///<param name="roleName">The name of the role to search for in the data source. </param>
        public override bool RoleExists(string roleName)
        {
            return this.wrappedProvider.RoleExists(roleName);
        }

        ///<summary>
        ///Adds the specified user names to the specified roles for the configured applicationName.
        ///</summary>
        ///
        ///<param name="roleNames">A string array of the role names to add the specified user names to. </param>
        ///<param name="usernames">A string array of user names to be added to the specified roles. </param>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            this.wrappedProvider.AddUsersToRoles(usernames,roleNames);
        }

        ///<summary>
        ///Removes the specified user names from the specified roles for the configured applicationName.
        ///</summary>
        ///
        ///<param name="roleNames">A string array of role names to remove the specified user names from. </param>
        ///<param name="usernames">A string array of user names to be removed from the specified roles. </param>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            this.wrappedProvider.RemoveUsersFromRoles(usernames,roleNames);
        }

        ///<summary>
        ///Gets a list of users in the specified role for the configured applicationName.
        ///</summary>
        ///
        ///<returns>
        ///A string array containing the names of all the users who are members of the specified role for the configured applicationName.
        ///</returns>
        ///
        ///<param name="roleName">The name of the role to get the list of users for. </param>
        public override string[] GetUsersInRole(string roleName)
        {
            return this.wrappedProvider.GetUsersInRole(roleName);
        }

        ///<summary>
        ///Gets a list of all the roles for the configured applicationName.
        ///</summary>
        ///
        ///<returns>
        ///A string array containing the names of all the roles stored in the data source for the configured applicationName.
        ///</returns>
        ///
        public override string[] GetAllRoles()
        {
            return this.wrappedProvider.GetAllRoles();
        }

        ///<summary>
        ///Gets an array of user names in a role where the user name contains the specified user name to match.
        ///</summary>
        ///
        ///<returns>
        ///A string array containing the names of all the users where the user name matches usernameToMatch and the user is a member of the specified role.
        ///</returns>
        ///
        ///<param name="usernameToMatch">The user name to search for.</param>
        ///<param name="roleName">The role to search in.</param>
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            return this.wrappedProvider.FindUsersInRole(roleName, usernameToMatch);
        }

        ///<summary>
        ///Gets or sets the name of the application to store and retrieve role information for.
        ///</summary>
        ///
        ///<returns>
        ///The name of the application to store and retrieve role information for.
        ///</returns>
        ///
        public override string ApplicationName
        {
            get { return this.wrappedProvider.ApplicationName; }
            set { this.wrappedProvider.ApplicationName = value; }
        }

        #endregion

    }
}
