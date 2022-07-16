#region License

/*
 * Copyright 2002-2010 the original author or authors.
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
using Common.Logging;

namespace Spring.Context.Support
{
    /// <summary>
    /// Abstract implementation of the <see cref="Spring.Context.IHierarchicalMessageSource"/> interface,
    /// implementing common handling of message variants, making it easy
    /// to implement a specific strategy for a concrete <see cref="Spring.Context.IMessageSource"/>.
    /// </summary>
    /// <remarks>
    /// <p>Subclasses must implement the abstract <code>ResolveObject</code>
    /// method.</p>
    /// <p><b>Note:</b> By default, message texts are only parsed through
    /// String.Format if arguments have been passed in for the message. In case
    /// of no arguments, message texts will be returned as-is. As a consequence,
    /// you should only use String.Format escaping for messages with actual
    /// arguments, and keep all other messages unescaped.
    /// </p>
    /// <p>Supports not only IMessageSourceResolvables as primary messages
    /// but also resolution of message arguments that are in turn
    /// IMessageSourceResolvables themselves.
    /// </p>
    /// <p>This class does not implement caching of messages per code, thus
    /// subclasses can dynamically change messages over time. Subclasses are
    /// encouraged to cache their messages in a modification-aware fashion,
    /// allowing for hot deployment of updated messages.
    /// </p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Griffin Caprio (.NET)</author>
    /// <author>Harald Radi (.NET)</author>
    /// <seealso cref="Spring.Context.IMessageSourceResolvable"/>
    /// <seealso cref="Spring.Context.IMessageSource"/>
    /// <seealso cref="Spring.Context.IHierarchicalMessageSource"/>
    public abstract class AbstractMessageSource : IHierarchicalMessageSource
    {
        #region Fields

        /// <summary>
        /// holds the logger instance shared with subclasses.
        /// </summary>
        protected readonly ILog log;

        private IMessageSource parentMessageSource;
        private bool useCodeAsDefaultMessage = false;

        #endregion

        #region Constructor

		/// <summary>
		/// Initializes this instance.
		/// </summary>
        protected AbstractMessageSource()
        {
            log = LogManager.GetLogger(GetType());
        }

        #endregion

        #region Properties


        /// <summary>Gets or Sets a value indicating whether to use the message code as 
        /// default message instead of throwing a NoSuchMessageException. 
        /// Useful for development and debugging. Default is "false".
        /// </summary>
        /// <remarks>
        /// <p>Note: In case of a IMessageSourceResolvable with multiple codes
        /// (like a FieldError) and a MessageSource that has a parent MessageSource,
        /// do <i>not</i> activate "UseCodeAsDefaultMessage" in the <i>parent</i>:
        /// Else, you'll get the first code returned as message by the parent,
        /// without attempts to check further codes.</p>
        /// <p>To be able to work with "UseCodeAsDefaultMessage" turned on in the parent,
        /// AbstractMessageSource contains special checks
        /// to delegate to the internal <code>GetMessageInternal</code> method if available.
        /// In general, it is recommended to just use "UseCodeAsDefaultMessage" during
        /// development and not rely on it in production in the first place, though.</p>
        /// <p>Alternatively, consider overriding the <code>GetDefaultMessage</code>
        /// method to return a custom fallback message for an unresolvable code.</p>
        /// </remarks>
        /// <value>
        /// 	<c>true</c> if use the message code as default message instead of 
        /// throwing a NoSuchMessageException; otherwise, <c>false</c>.
        /// </value>
        public bool UseCodeAsDefaultMessage
        {
            get { return useCodeAsDefaultMessage; }
            set { useCodeAsDefaultMessage = value; }
        }

        #endregion

        #region IHierarchicalMessageSource Members

        /// <summary>
        /// The parent message source used to try and resolve messages that
        /// this object can't resolve.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// 	<p>
        /// If the value of this property is <see langword="null"/> then no
        /// further resolution is possible.
        /// </p>
        /// </remarks>
        public IMessageSource ParentMessageSource
        {
            get { return parentMessageSource; }
            set { parentMessageSource = value; }
        }

        /// <summary>
        /// Resolve the message identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the message to resolve.</param>
        /// <returns>
        /// The resolved message if the lookup was successful (see above for
        /// the return value in the case of an unsuccessful lookup).
        /// </returns>
        /// <remarks>
        /// If the lookup is not successful throw NoSuchMessageException
        /// </remarks>
        public string GetMessage(string name)
        {
            return GetMessage(name, CultureInfo.CurrentUICulture, null);
        }

        /// <summary>
        /// Resolve the message identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the message to resolve.</param>
        /// <param name="culture">The <see cref="System.Globalization.CultureInfo"/> that represents
        /// the culture for which the resource is localized.</param>
        /// <returns>
        /// The resolved message if the lookup was successful (see above for
        /// the return value in the case of an unsuccessful lookup).
        /// </returns>
        /// <remarks>
        /// Note that the fallback behavior based on CultureInfo seem to
        /// have a bug that is fixed by installed .NET 1.1 Service Pack 1.
        /// <p>
        /// If the lookup is not successful, implementations are permitted to
        /// take one of two actions.
        /// </p>
        /// If the lookup is not successful throw NoSuchMessageException
        /// </remarks>
        public string GetMessage(string name, CultureInfo culture)
        {
            return GetMessage(name, culture, null);
        }

        /// <summary>
        /// Resolve the message identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the message to resolve.</param>
        /// <param name="arguments">The array of arguments that will be filled in for parameters within
        /// the message, or <see langword="null"/> if there are no parameters
        /// within the message. Parameters within a message should be
        /// referenced using the same syntax as the format string for the
        /// <see cref="System.String.Format(string,object[])"/> method.</param>
        /// <returns>
        /// The resolved message if the lookup was successful (see above for
        /// the return value in the case of an unsuccessful lookup).
        /// </returns>
        /// <remarks>
        /// If the lookup is not successful throw NoSuchMessageException
        /// </remarks>
        public string GetMessage(string name, params object[] arguments)
        {
            return GetMessage(name, CultureInfo.CurrentUICulture, arguments);
        }

        /// <summary>
        /// Resolve the message identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the message to resolve.</param>
        /// <param name="culture">The <see cref="System.Globalization.CultureInfo"/> that represents
        /// the culture for which the resource is localized.</param>
        /// <param name="arguments">The array of arguments that will be filled in for parameters within
        /// the message, or <see langword="null"/> if there are no parameters
        /// within the message. Parameters within a message should be
        /// referenced using the same syntax as the format string for the
        /// <see cref="System.String.Format(string,object[])"/> method.</param>
        /// <returns>
        /// The resolved message if the lookup was successful (see above for
        /// the return value in the case of an unsuccessful lookup).
        /// </returns>
        /// <remarks>
        /// Note that the fallback behavior based on CultureInfo seem to
        /// have a bug that is fixed by installed .NET 1.1 Service Pack 1.
        /// <p>
        /// If the lookup is not successful throw NoSuchMessageException.
        /// </p>
        /// </remarks>        
        public string GetMessage(string name, CultureInfo culture, params object[] arguments)
        {
            string msg = GetMessageInternal(name, arguments, culture);
            if (msg != null) return msg;
            string fallback = GetDefaultMessage(name);
            if (fallback != null) return fallback;
            throw new NoSuchMessageException(name, culture);
        }

        /// <summary>
        /// Resolve the message identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the message to resolve.</param>
        /// <param name="defaultMessage">The default message if name is not found.</param>
        /// <param name="culture">The <see cref="System.Globalization.CultureInfo"/> that represents
        /// the culture for which the resource is localized.</param>
        /// <param name="arguments">The array of arguments that will be filled in for parameters within
        /// the message, or <see langword="null"/> if there are no parameters
        /// within the message. Parameters within a message should be
        /// referenced using the same syntax as the format string for the
        /// <see cref="System.String.Format(string,object[])"/> method.</param>
        /// <returns>
        /// The resolved message if the lookup was successful (see above for
        /// the return value in the case of an unsuccessful lookup).
        /// </returns>
        /// <remarks>
        /// Note that the fallback behavior based on CultureInfo seem to
        /// have a bug that is fixed by installed .NET 1.1 Service Pack 1.
        /// <p>
        /// If the lookup is not successful throw NoSuchMessageException
        /// </p>
        /// </remarks>
        public string GetMessage(string name, string defaultMessage, CultureInfo culture, params object[] arguments)
        {
            string msg = GetMessageInternal(name, arguments, culture);
            if (msg != null) return msg;
            if (defaultMessage == null)
            {
                string fallback = GetDefaultMessage(name);
                if (fallback != null) return fallback;
            }
            return RenderDefaultMessage(defaultMessage, arguments, culture);
        }

        /// <summary>
        /// Resolve the message using all of the attributes contained within
        /// the supplied <see cref="Spring.Context.IMessageSourceResolvable"/>
        /// argument.
        /// </summary>
        /// <param name="resolvable">The value object storing those attributes that are required to
        /// properly resolve a message.</param>
        /// <param name="culture">The <see cref="System.Globalization.CultureInfo"/> that represents
        /// the culture for which the resource is localized.</param>
        /// <returns>
        /// The resolved message if the lookup was successful.
        /// </returns>
        /// <exception cref="Spring.Context.NoSuchMessageException">
        /// If the message could not be resolved.
        /// </exception>
        public string GetMessage(IMessageSourceResolvable resolvable, CultureInfo culture)
        {
            IList<string> codes = resolvable.GetCodes();
            if (codes == null) codes = new string[0];
            for (int i = 0; i < codes.Count; i++)
            {
                string msg = GetMessageInternal(codes[i], resolvable.GetArguments(), culture);
                if (msg != null) return msg;
            }
            if (resolvable.DefaultMessage != null)
                return RenderDefaultMessage(resolvable.DefaultMessage, resolvable.GetArguments(), culture);
            if (codes.Count > 0)
            {
                string fallback = GetDefaultMessage(codes[0]);
                if (fallback != null) return fallback;
            }
            throw new NoSuchMessageException(codes.Count > 0 ? codes[codes.Count - 1] : null, culture);
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
            object resource = GetResourceInternal(name, CultureInfo.CurrentUICulture);
            if (resource != null) return resource;
            if (ParentMessageSource != null)
                return ParentMessageSource.GetResourceObject(name, CultureInfo.CurrentUICulture);
            return null;
        }

        /// <summary>
        /// Gets a localized resource object identified by the supplied
        /// <paramref name="name"/>.
        /// </summary>
        /// <remarks>
        /// Note that the fallback behavior based on CultureInfo seem to 
        /// have a bug that is fixed by installed .NET 1.1 Service Pack 1.  
        /// </remarks>	    
        /// <param name="name">
        /// The name of the resource object to resolve.
        /// </param>
        /// <param name="culture">
        /// The <see cref="System.Globalization.CultureInfo"/> with which the
        /// resource is associated.
        /// </param>
        /// <returns>
        /// The resolved object, or <see langword="null"/> if not found.  If
        /// the resource name resolves to null, then in .NET 1.1 the return
        /// value will be String.Empty whereas in .NET 2.0 it will return
        /// null.
        /// </returns>
        /// <seealso cref="Spring.Context.IMessageSource.GetResourceObject(string, CultureInfo)"/>
        public object GetResourceObject(string name, CultureInfo culture)
        {
            object resource = GetResourceInternal(name, culture);
            if (resource != null) return resource;
            if (ParentMessageSource != null) return ParentMessageSource.GetResourceObject(name, culture);
            return null;
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
        public void ApplyResources(
            object value, string objectName, CultureInfo culture)
        {
            ApplyResourcesInternal(value, objectName, culture);
            if (ParentMessageSource != null) ParentMessageSource.ApplyResources(value, objectName, culture);
        }

        #endregion

        #region Protected Methods

        /// <summary>Resolve the given code and arguments as message in the given culture,
        /// returning null if not found. Does <i>not</i> fall back to the code
        /// as default message. Invoked by GetMessage methods.
        /// </summary>
        /// <param name="code">The code to lookup up, such as 'calculator.noRateSet'.</param>
        /// <param name="args"> array of arguments that will be filled in for params
        /// within the message.</param>
        /// <param name="culture">The <see cref="System.Globalization.CultureInfo"/> with which the
        /// resource is associated.</param>
        /// <returns>
        /// The resolved message if the lookup was successful.
        /// </returns>
        protected string GetMessageInternal(string code, object[] args, CultureInfo culture)
        {
            if (code == null) return null;
            if (culture == null) culture = CultureInfo.CurrentUICulture;

            if ((args != null && args.Length > 0))
            {
                // Resolve arguments eagerly, for the case where the message
                // is defined in a parent MessageSource but resolvable arguments
                // are defined in the child MessageSource.
                args = ResolveArguments(args, culture);
            }

            string message = ResolveMessage(code, culture);

            if (message != null) return FormatMessage(message, args, culture);

            // Not found -> check parent, if any.
            return GetMessageFromParent(code, args, culture);
        }


        /// <summary>
        /// Try to retrieve the given message from the parent MessageSource, if any.
        /// </summary>
        /// <param name="code">The code to lookup up, such as 'calculator.noRateSet'.</param>
        /// <param name="args"> array of arguments that will be filled in for params
        /// within the message.</param>
        /// <param name="culture">The <see cref="System.Globalization.CultureInfo"/> with which the
        /// resource is associated.</param>
        /// <returns>
        /// The resolved message if the lookup was successful.
        /// </returns>
        protected string GetMessageFromParent(string code, object[] args, CultureInfo culture)
        {
            if (ParentMessageSource != null)
            {
                AbstractMessageSource parent = ParentMessageSource as AbstractMessageSource;
                if (parent != null)
                {
                    // Call internal method to avoid getting the default code back
                    // in case of "useCodeAsDefaultMessage" being activated.
                    return parent.GetMessageInternal(code, args, culture);
                }
                else
                {
                    // Check parent MessageSource, returning null if not found there.
                    return ParentMessageSource.GetMessage(code, null, culture, args);
                }
            }
            // Not found in parent either.
            return null;
        }


        /// <summary>
        /// Return a fallback default message for the given code, if any.
        /// </summary>
        /// <remarks>
        /// Default is to return the code itself if "UseCodeAsDefaultMessage"
        /// is activated, or return no fallback else. In case of no fallback,
        /// the caller will usually receive a NoSuchMessageException from GetMessage
        /// </remarks>
        /// <param name="code">The code to lookup up, such as 'calculator.noRateSet'.</param>
        /// <returns>The default message to use, or null if none.</returns>
        protected virtual string GetDefaultMessage(string code)
        {
            if (UseCodeAsDefaultMessage) return code;
            return null;
        }



        /// <summary>
        /// Renders the default message string.  The default message is passed in as specified by the
        /// caller and can be rendered into a fully formatted default message shown to the user.
        /// </summary>
        /// <remarks>Default implementation passed he String for String.Format resolving any 
        /// argument placeholders found in them.  Subclasses may override this method to plug
        /// in custom processing of default messages.
        /// </remarks>
        /// <param name="defaultMessage">The default message.</param>
        /// <param name="args">The array of agruments that will be filled in for parameter
        /// placeholders within the message, or null if none.</param>
        /// <param name="culture">The <see cref="System.Globalization.CultureInfo"/> with which the
        /// resource is associated.</param>
        /// <returns>The rendered default message (with resolved arguments)</returns>
        protected virtual string RenderDefaultMessage(string defaultMessage, object[] args, CultureInfo culture)
        {
            return FormatMessage(defaultMessage, args, culture);
        }

        /// <summary>
        /// Format the given default message String resolving any 
        /// agrument placeholders found in them.
        /// </summary>
        /// <param name="msg">The message to format.</param>
        /// <param name="args">The array of agruments that will be filled in for parameter
        /// placeholders within the message, or null if none.</param>
        /// <param name="culture">The <see cref="System.Globalization.CultureInfo"/> with which the
        /// resource is associated.</param>
        /// <returns>The formatted message (with resolved arguments)</returns>
        protected virtual string FormatMessage(string msg, object[] args, CultureInfo culture)
        {
            if (msg == null || ((args == null || args.Length == 0))) return msg;
            return String.Format(culture, msg, args);
        }


        /// <summary>
        /// Search through the given array of objects, find any
        /// MessageSourceResolvable objects and resolve them.
        /// </summary>
        /// <remarks>
        /// Allows for messages to have MessageSourceResolvables as arguments.
        /// </remarks>
        /// 
        /// <param name="args">The array of arguments for a message.</param>
        /// <param name="culture">The <see cref="System.Globalization.CultureInfo"/> with which the
        /// resource is associated.</param>
        /// <returns>An array of arguments with any IMessageSourceResolvables resolved</returns>
        protected virtual object[] ResolveArguments(object[] args, CultureInfo culture)
        {
            if (args == null) return new object[0];
            object[] resolvedArgs = new object[args.Length];

            for (int i = 0; i < args.Length; i++)
            {
                IMessageSourceResolvable resolvable = args[i] as IMessageSourceResolvable;
                if (resolvable != null) resolvedArgs[i] = GetMessage(resolvable, culture);
                else resolvedArgs[i] = args[i];
            }

            return resolvedArgs;
        }

		/// <summary>
		/// Gets the specified resource (e.g. Icon or Bitmap).
		/// </summary>
		/// <param name="name">The name of the resource to resolve.</param>
		/// <param name="cultureInfo">
		/// The <see cref="System.Globalization.CultureInfo"/> to resolve the
		/// code for.
		/// </param>
		/// <returns>The resource if found. <see langword="null"/> otherwise.</returns>
        protected object GetResourceInternal(string name, CultureInfo cultureInfo)
        {
            if (cultureInfo == null) cultureInfo = CultureInfo.CurrentUICulture;
            if (name == null) return null;
            return ResolveObject(name, cultureInfo);
        }

		/// <summary>
		/// Applies resources from the given name on the specified object.
		/// </summary>
		/// <param name="value">
		/// An object that contains the property values to be applied.
		/// </param>
		/// <param name="objectName">
		/// The base name of the object to use for key lookup.
		/// </param>
		/// <param name="cultureInfo">
		/// The <see cref="System.Globalization.CultureInfo"/> with which the
		/// resource is associated.
		/// </param>
		protected void ApplyResourcesInternal(object value, string objectName, CultureInfo cultureInfo)
        {
            if (cultureInfo == null) cultureInfo = CultureInfo.CurrentUICulture;
            ApplyResourcesToObject(value, objectName, cultureInfo);
        }

        #endregion

        #region Protected Abstract Methods

        /// <summary>
        /// Subclasses must implement this method to resolve a message.
        /// </summary>
        /// <param name="code">The code to lookup up, such as 'calculator.noRateSet'.</param>
        /// <param name="cultureInfo">The <see cref="System.Globalization.CultureInfo"/> with which the
        /// resource is associated.</param>
        /// <returns>The resolved message from the backing store of message data.</returns>
        protected abstract string ResolveMessage(string code, CultureInfo cultureInfo);

        /// <summary>
        /// Resolves an object (typically an icon or bitmap).
        /// </summary>
        /// <remarks>
        /// <p>
        /// Subclasses must implement this method to resolve an object.
        /// </p>
        /// </remarks>
        /// <param name="code">The code of the object to resolve.</param>
        /// <param name="cultureInfo">
        /// The <see cref="System.Globalization.CultureInfo"/> to resolve the
        /// code for.
        /// </param>
        /// <returns>
        /// The resolved object or <see langword="null"/> if not found.
        /// </returns>
        protected abstract object ResolveObject(string code, CultureInfo cultureInfo);

        /// <summary>
        /// Applies resources to object properties.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Subclasses must implement this method to apply resources
        /// to an arbitrary object.
        /// </p>
        /// </remarks>
        /// <param name="value">
        /// An object that contains the property values to be applied.
        /// </param>
        /// <param name="objectName">
        /// The base name of the object to use for key lookup.
        /// </param>
        /// <param name="cultureInfo">
        /// The <see cref="System.Globalization.CultureInfo"/> with which the
        /// resource is associated.
        /// </param>
        protected abstract void ApplyResourcesToObject(object value, string objectName, CultureInfo cultureInfo);


        #endregion

    }
}
