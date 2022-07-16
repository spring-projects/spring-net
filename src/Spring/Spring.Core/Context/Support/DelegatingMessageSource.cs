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

using Spring.Util;

#endregion

namespace Spring.Context.Support
{
    /// <summary>
    /// Empty <see cref="Spring.Context.IMessageSource"/> implementation that
    /// simply delegates all method calls to it's parent
    /// <see cref="Spring.Context.IMessageSource"/>.
    /// </summary>
    /// <remarks>
    /// <p>
    /// If no parent <see cref="Spring.Context.IMessageSource"/> is available,
    /// no messages will be resolved (and a
    /// <see cref="Spring.Context.NoSuchMessageException"/> will be thrown).
    /// </p>
    /// <p>
    /// Used as placeholder <see cref="Spring.Context.IMessageSource"/> by the
    /// <see cref="Spring.Context.Support.AbstractApplicationContext"/> class,
    /// if the context definition doesn't define its own
    /// <see cref="Spring.Context.IMessageSource"/>. Not intended for direct use
    /// in applications.
    /// </p>
    /// </remarks>
    /// <author>Juergan Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
    /// <seealso cref="Spring.Context.Support.AbstractApplicationContext"/>
    public class DelegatingMessageSource : IHierarchicalMessageSource
    {
        #region Fields

        private IMessageSource _parentMessageSource;

        #endregion

        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Context.Support.DelegatingMessageSource"/> class.
        /// </summary>
        public DelegatingMessageSource()
        {}

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Context.Support.DelegatingMessageSource"/> class.
        /// </summary>
        /// <param name="parentMessageSource">
        /// The parent message source used to try and resolve messages that
        /// this object can't resolve.
        /// </param>
        public DelegatingMessageSource(IMessageSource parentMessageSource)
        {
            ParentMessageSource = parentMessageSource;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The parent message source used to try and resolve messages that
        /// this object can't resolve.
        /// </summary>
        /// <seealso cref="Spring.Context.IHierarchicalMessageSource.ParentMessageSource"/>
        public IMessageSource ParentMessageSource
        {
            get
            {
                if (_parentMessageSource == null)
                {
                    _parentMessageSource = new SpecialCaseNullMessageSource();
                }
                return _parentMessageSource;
            }
            set { _parentMessageSource = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Resolve the message identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the message to resolve.</param>
        /// <returns>
        /// The resolved message if the lookup was successful (see above for
        /// the return value in the case of an unsuccessful lookup).
        /// </returns>
        /// <exception cref="Spring.Context.NoSuchMessageException">
        /// If the message could not be resolved.
        /// </exception>
        /// <seealso cref="Spring.Context.IMessageSource.GetMessage(string)"/>
        public string GetMessage(string name)
        {
            return ParentMessageSource.GetMessage(name);
        }

        /// <summary>
        /// Resolve the message identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
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
        /// <exception cref="Spring.Context.NoSuchMessageException">
        /// If the message could not be resolved.
        /// </exception>
        /// <seealso cref="Spring.Context.IMessageSource.GetMessage(string, object[])"/>
        public string GetMessage(string name, params object[] arguments)
        {
            return ParentMessageSource.GetMessage(name, arguments);
        }

        /// <summary>
        /// Resolve the message identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the message to resolve.</param>
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
        /// <seealso cref="Spring.Context.IMessageSource.GetMessage(string, CultureInfo)"/>
        public string GetMessage(string name, CultureInfo culture)
        {
            return ParentMessageSource.GetMessage(name, culture);
        }

        /// <summary>
        /// Resolve the message identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
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
        /// <exception cref="Spring.Context.NoSuchMessageException">
        /// If the message could not be resolved.
        /// </exception>
        /// <seealso cref="Spring.Context.IMessageSource.GetMessage(string, CultureInfo, object[])"/>
        public string GetMessage(string name, CultureInfo culture, params object[] arguments)
        {
            return ParentMessageSource.GetMessage(name, culture, arguments);
        }

        /// <summary>
        /// Resolve the message identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the message to resolve.</param>
        /// <param name="defaultMessage">The default message.</param>
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
        /// <exception cref="Spring.Context.NoSuchMessageException">
        /// If the message could not be resolved.
        /// </exception>
        /// <seealso cref="Spring.Context.IMessageSource.GetMessage(string, CultureInfo, object[])"/>
        public string GetMessage(string name, string defaultMessage, CultureInfo culture, params object[] arguments)
        {
            return ParentMessageSource.GetMessage(name, defaultMessage, culture, arguments);
        }

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
        /// <exception cref="Spring.Context.NoSuchMessageException">
        /// If the message could not be resolved.
        /// </exception>
        /// <seealso cref="Spring.Context.IMessageSource.GetMessage(IMessageSourceResolvable, CultureInfo)"/>
        public string GetMessage(IMessageSourceResolvable resolvable, CultureInfo culture)
        {
            return ParentMessageSource.GetMessage(resolvable, culture);
        }

        /// <summary>
        /// Gets a localized resource object identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
        /// <param name="name">
        /// The name of the resource object to resolve.
        /// </param>
        /// <returns>
        /// The resolved object, or <see langword="null"/> if not found.
        /// </returns>
        /// <seealso cref="Spring.Context.IMessageSource.GetResourceObject(string)"/>
        public object GetResourceObject(string name)
        {
            return ParentMessageSource.GetResourceObject(name);
        }

        /// <summary>
        /// Gets a localized resource object identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
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
        /// <seealso cref="Spring.Context.IMessageSource.GetResourceObject(string, CultureInfo)"/>
        public object GetResourceObject(string name, CultureInfo culture)
        {
            return ParentMessageSource.GetResourceObject(name, culture);
        }

        /// <summary>
        /// Applies resources to object properties.
        /// </summary>
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
        /// <seealso cref="Spring.Context.IMessageSource.ApplyResources(object, string, CultureInfo)"/>
        public void ApplyResources(object value, string objectName, CultureInfo culture)
        {
            ParentMessageSource.ApplyResources(value, objectName, culture);
        }

        #endregion

        #region Inner Class : SpecialCaseNullMessageSource

        private sealed class SpecialCaseNullMessageSource : IMessageSource
        {
            public string GetMessage(string name)
            {
                return GetMessage(name, (object[]) null);
            }

            public string GetMessage(string name, params object[] arguments)
            {
                return GetMessage(name, CultureInfo.CurrentUICulture, null);
            }

            public string GetMessage(string name, CultureInfo culture)
            {
                return GetMessage(name, culture, null);
            }

            public string GetMessage(string name, CultureInfo culture, params object[] arguments)
            {
                throw new NoSuchMessageException(name, culture);
            }

            public string GetMessage(string name, string defaultMessage, CultureInfo culture, params object[] arguments)
            {
                throw new NoSuchMessageException(name, culture);
            }


            public string GetMessage(IMessageSourceResolvable resolvable, CultureInfo culture)
            {
                if (StringUtils.HasText(resolvable.DefaultMessage))
                {
                    return resolvable.DefaultMessage;
                }
                IList<string> codes = resolvable.GetCodes();
                string code = (codes != null && codes.Count > 0 ? codes[0] : string.Empty);
                throw new NoSuchMessageException(code, culture);
            }

            public object GetResourceObject(string name)
            {
                return GetResourceObject(name, CultureInfo.CurrentUICulture);
            }

            public object GetResourceObject(string name, CultureInfo culture)
            {
                throw new ApplicationContextException(
                    string.Format(
                        "Cannot lookup the named resource '{0}' for locale '{1}' " +
                            ": no IMessageSource in context.",
                        name, culture));
            }

            public void ApplyResources(object value, string objectName, CultureInfo culture)
            {
                throw new ApplicationContextException(
                    string.Format(
                        "Cannot apply [{0}] resource to object '{1}' for locale '{2}' " +
                            ": no IMessageSource in context.",
                        value, objectName, culture));
            }
        }

        #endregion
    }
}
