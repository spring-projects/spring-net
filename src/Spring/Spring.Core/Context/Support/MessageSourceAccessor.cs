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

#region Imports

using System.Globalization;

#endregion

namespace Spring.Context.Support
{
	/// <summary>
	/// Helper class for easy access to messages from an
	/// <see cref="Spring.Context.IMessageSource"/>, providing various
	/// overloaded <c>GetMessage</c> methods.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Available from
	/// <see cref="Spring.Context.Support.ApplicationObjectSupport"/>, but also
	/// reusable as a standalone helper to delegate to in application objects.
	/// </p>
	/// </remarks>
	/// <author>Juergen Hoeller</author>
	/// <author>Griffin Caprio (.NET)</author>
	/// <seealso cref="Spring.Context.IMessageSource"/>
	/// <seealso cref="Spring.Context.Support.ApplicationObjectSupport"/>
	public class MessageSourceAccessor
	{
		private IMessageSource _messageSource;
		private CultureInfo _defaultCultureInfo;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Context.Support.MessageSourceAccessor"/> class
		/// that uses the current <see cref="System.Globalization.CultureInfo.CurrentUICulture"/>
		/// for all locale specific lookups.
		/// </summary>
		/// <param name="messageSource">
		/// The <see cref="Spring.Context.IMessageSource"/> to use to locate messages.
		/// </param>
		public MessageSourceAccessor(IMessageSource messageSource)
			: this(messageSource, CultureInfo.CurrentUICulture)
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Context.Support.MessageSourceAccessor"/> class
		/// </summary>
		/// <param name="messageSource">
		/// The <see cref="Spring.Context.IMessageSource"/> to use to locate
		/// messages.
		/// </param>
		/// <param name="cultureInfo">
		/// The <see cref="System.Globalization.CultureInfo"/> to use for
		/// locale specific messages.
		/// </param>
		public MessageSourceAccessor(
			IMessageSource messageSource, CultureInfo cultureInfo)
		{
			_messageSource = messageSource;
			_defaultCultureInfo = cultureInfo;
		}

		/// <summary>
		/// Retrieve the message for the given code and the default
		/// <see cref="System.Globalization.CultureInfo"/>.
		/// </summary>
		/// <param name="code">The code of the message.</param>
		/// <returns>The message.</returns>
		public string GetMessage(string code)
		{
			return _messageSource.GetMessage(code, _defaultCultureInfo);
		}

		/// <summary>
		/// Retrieve the message for the given code and the given
		/// <see cref="System.Globalization.CultureInfo"/>.
		/// </summary>
		/// <param name="code">The code of the message.</param>       
		/// <param name="cultureInfo">
		/// The <see cref="System.Globalization.CultureInfo"/> to use for
		/// lookups.
		/// </param>
		/// <returns>The message.</returns>
		public string GetMessage(string code, CultureInfo cultureInfo)
		{
			return _messageSource.GetMessage(code, cultureInfo);
		}

		/// <summary>
		/// Retrieve the message for the given code and the default
		/// <see cref="System.Globalization.CultureInfo"/>.
		/// </summary>
		/// <param name="code">The code of the message.</param>
		/// <param name="args">
		/// The arguments for the message, or <see langword="null"/> if none.
		/// </param>
		/// <returns>The message.</returns>
		/// <exception cref="NoSuchMessageException">
		/// If the message could not be found.
		/// </exception>
		public string GetMessage(string code, params object[] args)
		{
			return _messageSource.GetMessage(code, _defaultCultureInfo, args);
		}

		/// <summary>
		/// Retrieve the message for the given code and the given
		/// <see cref="System.Globalization.CultureInfo"/>.
		/// </summary>
		/// <param name="code">The code of the message.</param>      
		/// <param name="cultureInfo">
		/// The <see cref="System.Globalization.CultureInfo"/> to use for
		/// lookups.
		/// </param>
		/// <param name="args">
		/// The arguments for the message, or <see langword="null"/> if none.
		/// </param>
		/// <returns>The message.</returns>
		/// <exception cref="NoSuchMessageException">
		/// If the message could not be found.
		/// </exception>
		public string GetMessage(
			string code, CultureInfo cultureInfo, params object[] args)
		{
			return _messageSource.GetMessage(code, cultureInfo, args);
		}

		/// <summary>
		/// Retrieve a mesage using the given
		/// <see cref="Spring.Context.IMessageSourceResolvable"/>.
		/// </summary>
		/// <param name="resolvable">
		/// The <see cref="Spring.Context.IMessageSourceResolvable"/>.
		/// </param>
		/// <returns>The message.</returns>
		/// <exception cref="NoSuchMessageException">
		/// If the message could not be found.
		/// </exception>
		public string GetMessage(IMessageSourceResolvable resolvable)
		{
			return GetMessage(resolvable, _defaultCultureInfo);
		}

		/// <summary>
		/// Retrieve a mesage using the given
		/// <see cref="Spring.Context.IMessageSourceResolvable"/> in the given
		/// <see cref="System.Globalization.CultureInfo"/>.
		/// </summary>
		/// <param name="resolvable">
		/// The <see cref="Spring.Context.IMessageSourceResolvable"/>.
		/// </param>     
		/// <param name="cultureInfo">
		/// The <see cref="System.Globalization.CultureInfo"/> to use for
		/// lookups.
		/// </param>
		/// <returns>The message</returns>
		/// <exception cref="NoSuchMessageException">
		/// If the message could not be found.
		/// </exception>
		public string GetMessage(
			IMessageSourceResolvable resolvable, CultureInfo cultureInfo)
		{
			return _messageSource.GetMessage(resolvable, cultureInfo);
		}
	}
}