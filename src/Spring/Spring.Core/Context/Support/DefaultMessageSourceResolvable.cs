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

using System.Globalization;
using Spring.Util;

namespace Spring.Context.Support
{
	/// <summary>
	/// Default implementation of the
	/// <see cref="Spring.Context.IMessageSourceResolvable"/> interface.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Provides easy ways to store all the necessary values needed to resolve
	/// messages from an <see cref="Spring.Context.IMessageSource"/>.
	/// </p>
	/// </remarks>
	/// <author>Juergen Hoeller</author>
	/// <author>Griffin Caprio (.NET)</author>
	/// <seealso cref="Spring.Context.IMessageSource.GetMessage(IMessageSourceResolvable, CultureInfo)"/>
	[Serializable]
	public class DefaultMessageSourceResolvable : IMessageSourceResolvable
	{
		private IList<string> codes;
		private object[] arguments;
		private string defaultMessage;

		#region Constructor (s) / Destructor

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="DefaultMessageSourceResolvable"/> class
		/// using a single code.
		/// </summary>
		/// <param name="code">The message code to be resolved.</param>
		public DefaultMessageSourceResolvable(string code)
			: this(new string[] {code}, StringUtils.EmptyStrings, string.Empty)
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultMessageSourceResolvable"/> class.
        /// </summary>
        /// <param name="codes">The codes to be used to resolve this message</param>
        public DefaultMessageSourceResolvable(string[] codes)
            : this(codes, StringUtils.EmptyStrings, string.Empty)
        {
        }

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="DefaultMessageSourceResolvable"/> class
		/// using multiple codes.
		/// </summary>
		/// <param name="codes">The message codes to be resolved.</param>
		/// <param name="arguments">
		/// The arguments used to resolve the supplied <paramref name="codes"/>.
		/// </param>
		public DefaultMessageSourceResolvable(string[] codes, object[] arguments)
			: this(codes, arguments, string.Empty)
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="DefaultMessageSourceResolvable"/> class
		/// using multiple codes and a default message.
		/// </summary>
		/// <param name="codes">The message codes to be resolved.</param>
		/// <param name="arguments">
		/// The arguments used to resolve the supplied <paramref name="codes"/>.
		/// </param>
		/// <param name="defaultMessage">
		/// The default message used if no code could be resolved.
		/// </param>
		public DefaultMessageSourceResolvable(
			IList<string> codes, object[] arguments, string defaultMessage)
		{
			this.codes = codes;
			this.arguments = arguments;
			this.defaultMessage = defaultMessage;
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="DefaultMessageSourceResolvable"/> class
		/// from another resolvable.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is the <i>copy constructor</i> for the
		/// <see cref="DefaultMessageSourceResolvable"/> class.
		/// </p>
		/// </remarks>
		/// <param name="resolvable">
		/// The <see cref="Spring.Context.IMessageSourceResolvable"/> to be copied.
		/// </param>
		/// <exception cref="System.NullReferenceException">
		/// If the supplied <paramref name="resolvable"/> is <see langword="null"/>.
		/// </exception>
		public DefaultMessageSourceResolvable(IMessageSourceResolvable resolvable)
			: this(resolvable.GetCodes(), resolvable.GetArguments(), resolvable.DefaultMessage)
		{
		}

		#endregion

		/// <summary>
		/// Return the default code for this resolvable.
		/// </summary>
		/// <returns>
		/// The default code of this resolvable; this will be the last code in
		/// the codes array, or <see langword="null"/> if this instance has no
		/// codes.
		/// </returns>
		/// <seealso cref="GetCodes"/>
		public string LastCode
		{
			get
			{
				if (codes != null && codes.Count > 0)
				{
					return codes[codes.Count - 1];
				}
				else
				{
					return null;
				}
			}
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> representation of this
		/// <see cref="Spring.Context.IMessageSourceResolvable"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> representation of this
		/// <see cref="Spring.Context.IMessageSourceResolvable"/>.
		/// </returns>
		public override string ToString()
		{
			return Accept(new MessageSourceResolvableVisitor());
		}

		/// <summary>
		/// Calls the visit method on the supplied <paramref name="visitor"/>
		/// to output a <see cref="System.String"/> version of this class.
		/// </summary>
		/// <param name="visitor">The visitor to use.</param>
		/// <returns>
		/// A <see cref="System.String"/> representation of this
		/// <see cref="Spring.Context.IMessageSourceResolvable"/>.
		/// </returns>
		public string Accept(MessageSourceResolvableVisitor visitor)
		{
			return visitor.VisitMessageSourceResolvableString(this);
		}

		#region IMessageSourceResolvable Members

	    /// <summary>
	    /// Return the codes to be used to resolve this message, in the order
	    /// that they are to be tried.
	    /// </summary>
	    /// <returns>
	    /// A <see cref="System.String"/> array of codes which are associated
	    /// with this message.
	    /// </returns>
	    /// <seealso cref="Spring.Context.IMessageSourceResolvable.GetCodes"/>
	    public IList<string> GetCodes()
		{
			return codes;
		}

		/// <summary>
		/// Return the array of arguments to be used to resolve this message.
		/// </summary>
		/// <returns>
		/// An array of objects to be used as parameters to replace
		/// placeholders within the message text.
		/// </returns>
		/// <seealso cref="Spring.Context.IMessageSourceResolvable.GetArguments"/>
		public object[] GetArguments()
		{
			return arguments;
		}

		/// <summary>
		/// Return the default message to be used to resolve this message.
		/// </summary>
		/// <returns>
		/// The default message, or <see langword="null"/> if there is no
		/// default.
		/// </returns>
		/// <seealso cref="Spring.Context.IMessageSourceResolvable.DefaultMessage"/>
		public string DefaultMessage
		{
			get { return defaultMessage; }
		}

		#endregion
	}
}
