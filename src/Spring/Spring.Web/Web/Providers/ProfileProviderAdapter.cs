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

using System.Collections.Specialized;
using System.Configuration;
using System.Security.Permissions;
using System.Web;
using System.Web.Profile;
using Spring.Context.Support;

namespace Spring.Web.Providers
{
     /// <summary>
    /// Wrapper for <see cref="System.Web.Profile.ProfileProvider"/> class.
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
    public class ProfileProviderAdapter : ProfileProvider, IProfileProvider
     {
        #region Field

        /// <summary>
        /// Reference to wrapped provider (defined in Spring context).
        /// </summary>
        private ProfileProvider wrappedProvider;

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

                this.wrappedProvider = (ProfileProvider)WebApplicationContext.GetRootContext().GetObject(providerId);
                this.wrappedProvider.Initialize(name, config);
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

        #region System.Web.Profile.ProfileProvider members

         ///<summary>
         ///Returns the collection of settings property values for the specified application instance and settings property group.
         ///</summary>
         ///
         ///<returns>
         ///A <see cref="T:System.Configuration.SettingsPropertyValueCollection"></see> containing the values for the specified settings property group.
         ///</returns>
         ///
         ///<param name="context">A <see cref="T:System.Configuration.SettingsContext"></see> describing the current application use.</param>
         ///<param name="collection">A <see cref="T:System.Configuration.SettingsPropertyCollection"></see> containing the settings property group whose values are to be retrieved.</param><filterpriority>2</filterpriority>
         public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context,
                                                                           SettingsPropertyCollection collection)
         {
             return this.wrappedProvider.GetPropertyValues(context, collection);
         }

         ///<summary>
         ///Sets the values of the specified group of property settings.
         ///</summary>
         ///
         ///<param name="context">A <see cref="T:System.Configuration.SettingsContext"></see> describing the current application usage.</param>
         ///<param name="collection">A <see cref="T:System.Configuration.SettingsPropertyValueCollection"></see> representing the group of property settings to set.</param><filterpriority>2</filterpriority>
         public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
         {
             this.wrappedProvider.SetPropertyValues(context,collection);
         }

         ///<summary>
         ///Gets or sets the name of the currently running application.
         ///</summary>
         ///
         ///<returns>
         ///A <see cref="T:System.String"></see> that contains the application's shortened name, which does not contain a full path or extension, for example, SimpleAppSettings.
         ///</returns>
         ///<filterpriority>2</filterpriority>
         public override string ApplicationName
         {
             get { return this.wrappedProvider.ApplicationName; }
             set { this.wrappedProvider.ApplicationName = value; }
         }

         ///<summary>
         ///When overridden in a derived class, deletes profile properties and information for the supplied list of profiles.
         ///</summary>
         ///
         ///<returns>
         ///The number of profiles deleted from the data source.
         ///</returns>
         ///
         ///<param name="profiles">A <see cref="T:System.Web.Profile.ProfileInfoCollection"></see>  of information about profiles that are to be deleted.</param>
         public override int DeleteProfiles(ProfileInfoCollection profiles)
         {
             return this.wrappedProvider.DeleteProfiles(profiles);
         }

         ///<summary>
         ///When overridden in a derived class, deletes profile properties and information for profiles that match the supplied list of user names.
         ///</summary>
         ///
         ///<returns>
         ///The number of profiles deleted from the data source.
         ///</returns>
         ///
         ///<param name="usernames">A string array of user names for profiles to be deleted.</param>
         public override int DeleteProfiles(string[] usernames)
         {
             return this.wrappedProvider.DeleteProfiles(usernames);
         }

         ///<summary>
         ///When overridden in a derived class, deletes all user-profile data for profiles in which the last activity date occurred before the specified date.
         ///</summary>
         ///
         ///<returns>
         ///The number of profiles deleted from the data source.
         ///</returns>
         ///
         ///<param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"></see> values, specifying whether anonymous, authenticated, or both types of profiles are deleted.</param>
         ///<param name="userInactiveSinceDate">A <see cref="T:System.DateTime"></see> that identifies which user profiles are considered inactive. If the <see cref="P:System.Web.Profile.ProfileInfo.LastActivityDate"></see>  value of a user profile occurs on or before this date and time, the profile is considered inactive.</param>
         public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption,
                                                    DateTime userInactiveSinceDate)
         {
             return this.wrappedProvider.DeleteInactiveProfiles(authenticationOption, userInactiveSinceDate);
         }

         ///<summary>
         ///When overridden in a derived class, returns the number of profiles in which the last activity date occurred on or before the specified date.
         ///</summary>
         ///
         ///<returns>
         ///The number of profiles in which the last activity date occurred on or before the specified date.
         ///</returns>
         ///
         ///<param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"></see> values, specifying whether anonymous, authenticated, or both types of profiles are returned.</param>
         ///<param name="userInactiveSinceDate">A <see cref="T:System.DateTime"></see> that identifies which user profiles are considered inactive. If the <see cref="P:System.Web.Profile.ProfileInfo.LastActivityDate"></see>  of a user profile occurs on or before this date and time, the profile is considered inactive.</param>
         public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption,
                                                         DateTime userInactiveSinceDate)
         {
             return this.wrappedProvider.GetNumberOfInactiveProfiles(authenticationOption, userInactiveSinceDate);
         }

         ///<summary>
         ///When overridden in a derived class, retrieves user profile data for all profiles in the data source.
         ///</summary>
         ///
         ///<returns>
         ///A <see cref="T:System.Web.Profile.ProfileInfoCollection"></see> containing user-profile information for all profiles in the data source.
         ///</returns>
         ///
         ///<param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"></see> values, specifying whether anonymous, authenticated, or both types of profiles are returned.</param>
         ///<param name="totalRecords">When this method returns, contains the total number of profiles.</param>
         ///<param name="pageIndex">The index of the page of results to return.</param>
         ///<param name="pageSize">The size of the page of results to return.</param>
         public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption,
                                                              int pageIndex, int pageSize, out int totalRecords)
         {
             return this.wrappedProvider.GetAllProfiles(authenticationOption, pageIndex, pageSize, out totalRecords);
         }

         ///<summary>
         ///When overridden in a derived class, retrieves user-profile data from the data source for profiles in which the last activity date occurred on or before the specified date.
         ///</summary>
         ///
         ///<returns>
         ///A <see cref="T:System.Web.Profile.ProfileInfoCollection"></see> containing user-profile information about the inactive profiles.
         ///</returns>
         ///
         ///<param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"></see> values, specifying whether anonymous, authenticated, or both types of profiles are returned.</param>
         ///<param name="userInactiveSinceDate">A <see cref="T:System.DateTime"></see> that identifies which user profiles are considered inactive. If the <see cref="P:System.Web.Profile.ProfileInfo.LastActivityDate"></see>  of a user profile occurs on or before this date and time, the profile is considered inactive.</param>
         ///<param name="totalRecords">When this method returns, contains the total number of profiles.</param>
         ///<param name="pageIndex">The index of the page of results to return.</param>
         ///<param name="pageSize">The size of the page of results to return.</param>
         public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption,
                                                                      DateTime userInactiveSinceDate, int pageIndex,
                                                                      int pageSize, out int totalRecords)
         {
             return
                 this.wrappedProvider.GetAllInactiveProfiles(authenticationOption, userInactiveSinceDate, pageIndex, pageSize,
                                                 out totalRecords);
         }

         ///<summary>
         ///When overridden in a derived class, retrieves profile information for profiles in which the user name matches the specified user names.
         ///</summary>
         ///
         ///<returns>
         ///A <see cref="T:System.Web.Profile.ProfileInfoCollection"></see> containing user-profile information for profiles where the user name matches the supplied usernameToMatch parameter.
         ///</returns>
         ///
         ///<param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"></see> values, specifying whether anonymous, authenticated, or both types of profiles are returned.</param>
         ///<param name="totalRecords">When this method returns, contains the total number of profiles.</param>
         ///<param name="pageIndex">The index of the page of results to return.</param>
         ///<param name="usernameToMatch">The user name to search for.</param>
         ///<param name="pageSize">The size of the page of results to return.</param>
         public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption,
                                                                      string usernameToMatch, int pageIndex, int pageSize,
                                                                      out int totalRecords)
         {
             return
                 this.wrappedProvider.FindProfilesByUserName(authenticationOption, usernameToMatch, pageIndex, pageSize,
                                                 out totalRecords);
         }

         ///<summary>
         ///When overridden in a derived class, retrieves profile information for profiles in which the last activity date occurred on or before the specified date and the user name matches the specified user name.
         ///</summary>
         ///
         ///<returns>
         ///A <see cref="T:System.Web.Profile.ProfileInfoCollection"></see> containing user profile information for inactive profiles where the user name matches the supplied usernameToMatch parameter.
         ///</returns>
         ///
         ///<param name="authenticationOption">One of the <see cref="T:System.Web.Profile.ProfileAuthenticationOption"></see> values, specifying whether anonymous, authenticated, or both types of profiles are returned.</param>
         ///<param name="userInactiveSinceDate">A <see cref="T:System.DateTime"></see> that identifies which user profiles are considered inactive. If the <see cref="P:System.Web.Profile.ProfileInfo.LastActivityDate"></see> value of a user profile occurs on or before this date and time, the profile is considered inactive.</param>
         ///<param name="totalRecords">When this method returns, contains the total number of profiles.</param>
         ///<param name="pageIndex">The index of the page of results to return.</param>
         ///<param name="usernameToMatch">The user name to search for.</param>
         ///<param name="pageSize">The size of the page of results to return.</param>
         public override ProfileInfoCollection FindInactiveProfilesByUserName(
             ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate,
             int pageIndex, int pageSize, out int totalRecords)
         {
             return this.wrappedProvider.FindInactiveProfilesByUserName(authenticationOption, usernameToMatch, userInactiveSinceDate,
                                                            pageIndex, pageSize, out totalRecords);
         }


         #endregion

    }
}
