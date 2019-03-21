#region License

/*
 * Copyright � 2002-2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System.Collections.Specialized;
using System.Web.Security;

namespace Spring.Web.Providers
{
    /// <summary>
    /// An Interface for <see cref="MembershipProviderAdapter"/> class.
    /// </summary>
    /// <author>Damjan Tomic</author>
    public interface IMembershipProvider
    {
        ///<summary>
        ///Initializes the provider.
        ///</summary>
        ///
        ///<param name="config">A collection of the name/value pairs representing the provider-specific 
        /// attributes specified in the configuration for this provider.
        /// The <c>providerId</c> attribute is mandatory.
        /// </param>
        ///<param name="name">The friendly name of the provider.</param>
        ///<exception cref="T:System.ArgumentNullException">The <paramref name="name"/> or <paramref name="config"/> is null.</exception>
        ///<exception cref="T:System.InvalidOperationException">An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"></see> on a provider after the provider has already been initialized.</exception>
        ///<exception cref="T:System.ArgumentException">The <paramref name="name"/> has a length of zero or providerId attribute is not set.</exception>
        void Initialize(string name, NameValueCollection config);

        ///<summary>
        ///Gets the friendly name used to refer to the provider during configuration.
        ///</summary>
        ///
        ///<returns>
        ///The friendly name used to refer to the provider during configuration.
        ///</returns>
        ///
        string Name { get; }

        ///<summary>
        ///Gets a brief, friendly description suitable for display in administrative tools or other user interfaces (UIs).
        ///</summary>
        ///
        ///<returns>
        ///A brief, friendly description suitable for display in administrative tools or other UIs.
        ///</returns>
        ///
        string Description { get; }

        ///<summary>
        ///Adds a new membership user to the data source.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.Web.Security.MembershipUser"></see> object populated with the information for the newly created user.
        ///</returns>
        ///
        ///<param name="isApproved">Whether or not the new user is approved to be validated.</param>
        ///<param name="passwordAnswer">The password answer for the new user</param>
        ///<param name="username">The user name for the new user. </param>
        ///<param name="providerUserKey">The unique identifier from the membership data source for the user.</param>
        ///<param name="password">The password for the new user. </param>
        ///<param name="passwordQuestion">The password question for the new user.</param>
        ///<param name="email">The e-mail address for the new user.</param>
        ///<param name="status">A <see cref="T:System.Web.Security.MembershipCreateStatus"></see> enumeration value indicating whether the user was created successfully.</param>
        MembershipUser CreateUser(string username, string password, string email,
                                  string passwordQuestion, string passwordAnswer, bool isApproved,
                                  object providerUserKey, out MembershipCreateStatus status);

        ///<summary>
        ///Processes a request to update the password question and answer for a membership user.
        ///</summary>
        ///
        ///<returns>
        ///true if the password question and answer are updated successfully; otherwise, false.
        ///</returns>
        ///
        ///<param name="newPasswordQuestion">The new password question for the specified user. </param>
        ///<param name="newPasswordAnswer">The new password answer for the specified user. </param>
        ///<param name="username">The user to change the password question and answer for. </param>
        ///<param name="password">The password for the specified user. </param>
        bool ChangePasswordQuestionAndAnswer(string username, string password,
                                             string newPasswordQuestion, string newPasswordAnswer);

        ///<summary>
        ///Gets the password for the specified user name from the data source.
        ///</summary>
        ///
        ///<returns>
        ///The password for the specified user name.
        ///</returns>
        ///
        ///<param name="username">The user to retrieve the password for. </param>
        ///<param name="answer">The password answer for the user. </param>
        string GetPassword(string username, string answer);

        ///<summary>
        ///Processes a request to update the password for a membership user.
        ///</summary>
        ///
        ///<returns>
        ///true if the password was updated successfully; otherwise, false.
        ///</returns>
        ///
        ///<param name="newPassword">The new password for the specified user. </param>
        ///<param name="oldPassword">The current password for the specified user. </param>
        ///<param name="username">The user to update the password for. </param>
        bool ChangePassword(string username, string oldPassword, string newPassword);

        ///<summary>
        ///Resets a user's password to a new, automatically generated password.
        ///</summary>
        ///
        ///<returns>
        ///The new password for the specified user.
        ///</returns>
        ///
        ///<param name="username">The user to reset the password for. </param>
        ///<param name="answer">The password answer for the specified user. </param>
        string ResetPassword(string username, string answer);

        ///<summary>
        ///Updates information about a user in the data source.
        ///</summary>
        ///
        ///<param name="user">A <see cref="T:System.Web.Security.MembershipUser"></see> object that represents the user to update and the updated information for the user. </param>
        void UpdateUser(MembershipUser user);

        ///<summary>
        ///Verifies that the specified user name and password exist in the data source.
        ///</summary>
        ///
        ///<returns>
        ///true if the specified username and password are valid; otherwise, false.
        ///</returns>
        ///
        ///<param name="username">The name of the user to validate. </param>
        ///<param name="password">The password for the specified user. </param>
        bool ValidateUser(string username, string password);

        ///<summary>
        ///Clears a lock so that the membership user can be validated.
        ///</summary>
        ///
        ///<returns>
        ///true if the membership user was successfully unlocked; otherwise, false.
        ///</returns>
        ///
        ///<param name="userName">The membership user to clear the lock status for.</param>
        bool UnlockUser(string userName);

        ///<summary>
        ///Gets information from the data source for a user based on the unique identifier for the membership user. Provides an option to update the last-activity date/time stamp for the user.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.Web.Security.MembershipUser"></see> object populated with the specified user's information from the data source.
        ///</returns>
        ///
        ///<param name="providerUserKey">The unique identifier for the membership user to get information for.</param>
        ///<param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
        MembershipUser GetUser(object providerUserKey, bool userIsOnline);

        ///<summary>
        ///Gets information from the data source for a user. Provides an option to update the last-activity date/time stamp for the user.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.Web.Security.MembershipUser"></see> object populated with the specified user's information from the data source.
        ///</returns>
        ///
        ///<param name="username">The name of the user to get information for. </param>
        ///<param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user. </param>
        MembershipUser GetUser(string username, bool userIsOnline);

        ///<summary>
        ///Gets the user name associated with the specified e-mail address.
        ///</summary>
        ///
        ///<returns>
        ///The user name associated with the specified e-mail address. If no match is found, return null.
        ///</returns>
        ///
        ///<param name="email">The e-mail address to search for. </param>
        string GetUserNameByEmail(string email);

        ///<summary>
        ///Removes a user from the membership data source. 
        ///</summary>
        ///
        ///<returns>
        ///true if the user was successfully deleted; otherwise, false.
        ///</returns>
        ///
        ///<param name="username">The name of the user to delete.</param>
        ///<param name="deleteAllRelatedData">true to delete data related to the user from the database; false to leave data related to the user in the database.</param>
        bool DeleteUser(string username, bool deleteAllRelatedData);

        ///<summary>
        ///Gets a collection of all the users in the data source in pages of data.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.Web.Security.MembershipUserCollection"></see> collection that contains a page of pageSize<see cref="T:System.Web.Security.MembershipUser"></see> objects beginning at the page specified by pageIndex.
        ///</returns>
        ///
        ///<param name="totalRecords">The total number of matched users.</param>
        ///<param name="pageIndex">The index of the page of results to return. pageIndex is zero-based.</param>
        ///<param name="pageSize">The size of the page of results to return.</param>
        MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords);

        ///<summary>
        ///Gets the number of users currently accessing the application.
        ///</summary>
        ///
        ///<returns>
        ///The number of users currently accessing the application.
        ///</returns>
        ///
        int GetNumberOfUsersOnline();

        ///<summary>
        ///Gets a collection of membership users where the user name contains the specified user name to match.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.Web.Security.MembershipUserCollection"></see> collection that contains a page of pageSize<see cref="T:System.Web.Security.MembershipUser"></see> objects beginning at the page specified by pageIndex.
        ///</returns>
        ///
        ///<param name="totalRecords">The total number of matched users.</param>
        ///<param name="pageIndex">The index of the page of results to return. pageIndex is zero-based.</param>
        ///<param name="usernameToMatch">The user name to search for.</param>
        ///<param name="pageSize">The size of the page of results to return.</param>
        MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize,
                                                 out int totalRecords);

        ///<summary>
        ///Gets a collection of membership users where the e-mail address contains the specified e-mail address to match.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.Web.Security.MembershipUserCollection"></see> collection that contains a page of pageSize<see cref="T:System.Web.Security.MembershipUser"></see> objects beginning at the page specified by pageIndex.
        ///</returns>
        ///
        ///<param name="totalRecords">The total number of matched users.</param>
        ///<param name="pageIndex">The index of the page of results to return. pageIndex is zero-based.</param>
        ///<param name="emailToMatch">The e-mail address to search for.</param>
        ///<param name="pageSize">The size of the page of results to return.</param>
        MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize,
                                                  out int totalRecords);

        ///<summary>
        ///Indicates whether the membership provider is configured to allow users to retrieve their passwords.
        ///</summary>
        ///
        ///<returns>
        ///true if the membership provider is configured to support password retrieval; otherwise, false. The default is false.
        ///</returns>
        ///
        bool EnablePasswordRetrieval { get; }

        ///<summary>
        ///Indicates whether the membership provider is configured to allow users to reset their passwords.
        ///</summary>
        ///
        ///<returns>
        ///true if the membership provider supports password reset; otherwise, false. The default is true.
        ///</returns>
        ///
        bool EnablePasswordReset { get; }

        ///<summary>
        ///Gets a value indicating whether the membership provider is configured to require the user to answer a password question for password reset and retrieval.
        ///</summary>
        ///
        ///<returns>
        ///true if a password answer is required for password reset and retrieval; otherwise, false. The default is true.
        ///</returns>
        ///
        bool RequiresQuestionAndAnswer { get; }

        ///<summary>
        ///The name of the application using the custom membership provider.
        ///</summary>
        ///
        ///<returns>
        ///The name of the application using the custom membership provider.
        ///</returns>
        ///
        string ApplicationName { get; set; }

        ///<summary>
        ///Gets the number of invalid password or password-answer attempts allowed before the membership user is locked out.
        ///</summary>
        ///
        ///<returns>
        ///The number of invalid password or password-answer attempts allowed before the membership user is locked out.
        ///</returns>
        ///
        int MaxInvalidPasswordAttempts { get; }

        ///<summary>
        ///Gets the number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        ///</summary>
        ///
        ///<returns>
        ///The number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        ///</returns>
        ///
        int PasswordAttemptWindow { get; }

        ///<summary>
        ///Gets a value indicating whether the membership provider is configured to require a unique e-mail address for each user name.
        ///</summary>
        ///
        ///<returns>
        ///true if the membership provider requires a unique e-mail address; otherwise, false. The default is true.
        ///</returns>
        ///
        bool RequiresUniqueEmail { get; }

        ///<summary>
        ///Gets a value indicating the format for storing passwords in the membership data store.
        ///</summary>
        ///
        ///<returns>
        ///One of the <see cref="T:System.Web.Security.MembershipPasswordFormat"></see> values indicating the format for storing passwords in the data store.
        ///</returns>
        ///
        MembershipPasswordFormat PasswordFormat { get; }

        ///<summary>
        ///Gets the minimum length required for a password.
        ///</summary>
        ///
        ///<returns>
        ///The minimum length required for a password. 
        ///</returns>
        ///
        int MinRequiredPasswordLength { get; }

        ///<summary>
        ///Gets the minimum number of special characters that must be present in a valid password.
        ///</summary>
        ///
        ///<returns>
        ///The minimum number of special characters that must be present in a valid password.
        ///</returns>
        ///
        int MinRequiredNonAlphanumericCharacters { get; }

        ///<summary>
        ///Gets the regular expression used to evaluate a password.
        ///</summary>
        ///
        ///<returns>
        ///A regular expression used to evaluate a password.
        ///</returns>
        ///
        string PasswordStrengthRegularExpression { get; }
    }
}
