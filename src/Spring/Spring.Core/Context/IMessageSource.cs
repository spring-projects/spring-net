#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

using System.Globalization;

namespace Spring.Context {
    /// <summary>
    /// Describes an object that can resolve messages.
    /// </summary>
    /// <remarks>
    /// <p> 
    /// This enables the parameterization and internationalization of messages.
    /// </p>
    /// <p>
    /// Spring.NET provides one out-of-the-box implementation for production
    /// use:
    /// <ul>
    /// <li><see cref="Spring.Context.Support.ResourceSetMessageSource"/>.</li>
    /// </ul>
    /// </p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    /// <author>Aleksandar Seovic (.NET)</author>
    /// <seealso cref="Spring.Context.Support.ResourceSetMessageSource"/>
    public interface IMessageSource {
        /// <summary>
        /// Resolve the message identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If the lookup is not successful, implementations are permitted to
        /// take one of two actions.
        /// </p>
        /// <list type="bullet">
        /// <item>
        /// <description>Throw an exception.</description>
        /// </item>
        /// <item>
        /// <description>
        /// Return the supplied <paramref name="name"/> as is.
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="name">The name of the message to resolve.</param>
        /// <returns>
        /// The resolved message if the lookup was successful (see above for
        /// the return value in the case of an unsuccessful lookup).
        /// </returns>
        string GetMessage(string name);

        /// <summary>
        /// Resolve the message identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If the lookup is not successful, implementations are permitted to
        /// take one of two actions.
        /// </p>
        /// <list type="bullet">
        /// <item>
        /// <description>Throw an exception.</description>
        /// </item>
        /// <item>
        /// <description>
        /// Return the supplied <paramref name="name"/> as is.
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="name">The name of the message to resolve.</param>
        /// <param name="arguments">
        /// The array of arguments that will be filled in for parameters within
        /// the message, or <see langword="null"/> if there are no parameters
        /// within the message. Parameters within a message should be
        /// referenced using the same syntax as the format string for the
        /// <see cref="System.String.Format(string,object[])"/> method.
        /// </param>
        /// <returns>
        /// The resolved message if the lookup was successful (see above for
        /// the return value in the case of an unsuccessful lookup).
        /// </returns>
        string GetMessage(string name, params object[] arguments);

        /// <summary>
        /// Resolve the message identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
        /// <remarks>
        /// Note that the fallback behavior based on CultureInfo seem to 
        /// have a bug that is fixed by installed .NET 1.1 Service Pack 1.  
        /// <p>
        /// If the lookup is not successful, implementations are permitted to
        /// take one of two actions.
        /// </p>
        /// <list type="bullet">
        /// <item>
        /// <description>Throw an exception.</description>
        /// </item>
        /// <item>
        /// <description>
        /// Return the supplied <paramref name="name"/> as is.
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="name">The name of the message to resolve.</param>
        /// <param name="culture">
        /// The <see cref="System.Globalization.CultureInfo"/> that represents
        /// the culture for which the resource is localized.
        /// </param>
        /// <returns>
        /// The resolved message if the lookup was successful (see above for
        /// the return value in the case of an unsuccessful lookup).
        /// </returns>
        string GetMessage(string name, CultureInfo culture);

        /// <summary>
        /// Resolve the message identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
        /// <remarks>
        /// Note that the fallback behavior based on CultureInfo seem to 
        /// have a bug that is fixed by installed .NET 1.1 Service Pack 1.      
        /// <p>
        /// If the lookup is not successful, implementations are permitted to
        /// take one of two actions.
        /// </p>
        /// <list type="bullet">
        /// <item>
        /// <description>Throw an exception.</description>
        /// </item>
        /// <item>
        /// <description>
        /// Return the supplied <paramref name="name"/> as is.
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="name">The name of the message to resolve.</param>
        /// <param name="culture">
        /// The <see cref="System.Globalization.CultureInfo"/> that represents
        /// the culture for which the resource is localized.
        /// </param>
        /// <param name="arguments">
        /// The array of arguments that will be filled in for parameters within
        /// the message, or <see langword="null"/> if there are no parameters
        /// within the message. Parameters within a message should be
        /// referenced using the same syntax as the format string for the
        /// <see cref="System.String.Format(string,object[])"/> method.
        /// </param>
        /// <returns>
        /// The resolved message if the lookup was successful (see above for
        /// the return value in the case of an unsuccessful lookup).
        /// </returns>
        string GetMessage(string name, CultureInfo culture, params object[] arguments);

        /// <summary>
        /// Resolve the message identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
        /// <remarks>
        /// Note that the fallback behavior based on CultureInfo seem to 
        /// have a bug that is fixed by installed .NET 1.1 Service Pack 1.      
        /// <p>
        /// If the lookup is not successful, implementations are permitted to
        /// take one of two actions.
        /// </p>
        /// <list type="bullet">
        /// <item>
        /// <description>Throw an exception.</description>
        /// </item>
        /// <item>
        /// <description>
        /// Return the supplied <paramref name="name"/> as is.
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="name">The name of the message to resolve.</param>
        /// <param name="defaultMessage">The default message if name is not found.</param>
        /// <param name="culture">
        /// The <see cref="System.Globalization.CultureInfo"/> that represents
        /// the culture for which the resource is localized.
        /// </param>
        /// <param name="arguments">
        /// The array of arguments that will be filled in for parameters within
        /// the message, or <see langword="null"/> if there are no parameters
        /// within the message. Parameters within a message should be
        /// referenced using the same syntax as the format string for the
        /// <see cref="System.String.Format(string,object[])"/> method.
        /// </param>
        /// <returns>
        /// The resolved message if the lookup was successful (see above for
        /// the return value in the case of an unsuccessful lookup).
        /// </returns>
        string GetMessage(string name, string defaultMessage, CultureInfo culture, params object[] arguments);

        /// <summary>
        /// Resolve the message using all of the attributes contained within
        /// the supplied <see cref="Spring.Context.IMessageSourceResolvable"/>
        /// argument.
        /// </summary>
        /// <param name="resolvable">
        /// The value object storing those attributes that are required to
        /// properly resolve a message.
        /// </param>
        /// <param name="culture">
        /// The <see cref="System.Globalization.CultureInfo"/> that represents
        /// the culture for which the resource is localized.
        /// </param>
        /// <returns>
        /// The resolved message if the lookup was successful (see above for
        /// the return value in the case of an unsuccessful lookup).
        /// </returns>
        /// <exception cref="Spring.Context.NoSuchMessageException">
        /// If the message could not be resolved.
        /// </exception>
        string GetMessage(IMessageSourceResolvable resolvable, CultureInfo culture);

        /// <summary>
        /// Gets a localized resource object identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method must use the
        /// <see cref="System.Globalization.CultureInfo.CurrentUICulture"/>
        /// value to obtain a resource.
        /// </p>
        /// <p>
        /// Examples of resources that may be resolved by this method include
        /// (but are not limited to) objects such as icons and bitmaps.
        /// </p>
        /// </remarks>
        /// <param name="name">
        /// The name of the resource object to resolve.
        /// </param>
        /// <returns>
        /// The resolved object, or <see langword="null"/> if not found.
        /// </returns>
        object GetResourceObject(string name);

        /// <summary>
        /// Gets a localized resource object identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Examples of resources that may be resolved by this method include
        /// (but are not limited to) objects such as icons and bitmaps.
        /// </p>
        /// </remarks>
        /// <param name="name">
        /// The name of the resource object to resolve.
        /// </param>
        /// <param name="culture">
        /// The <see cref="System.Globalization.CultureInfo"/> with which the
        /// resource is associated.
        /// </param>
        /// <returns>
        /// The resolved object, or <see langword="null"/> if not found.
        /// </returns>
        object GetResourceObject(string name, CultureInfo culture);

        /// <summary>
        /// Applies resources to object properties.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Resource key names are of the form <c>objectName.propertyName</c>.
        /// </p>
        /// </remarks>
        /// <param name="value">
        /// An object that contains the property values to be applied.
        /// </param>
        /// <param name="objectName">
        /// The base name of the object to use for key lookup.
        /// </param>
        /// <param name="culture">
        /// The <see cref="System.Globalization.CultureInfo"/> with which the
        /// resource is associated.
        /// </param>
        void ApplyResources(object value, string objectName, CultureInfo culture);
    }
}