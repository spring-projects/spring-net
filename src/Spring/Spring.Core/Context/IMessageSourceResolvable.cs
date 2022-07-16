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

namespace Spring.Context
{
    /// <summary>
    /// Describes objects that are suitable for message resolution in a
    /// <see cref="Spring.Context.IMessageSource"/>.
    /// </summary>
    /// <remarks>
    /// <p>
	/// Spring.NET's own validation error classes implement this interface.
    /// </p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
	/// <author>Mark Pollack (.NET)</author>
    /// <seealso cref="Spring.Context.IMessageSource.GetMessage(IMessageSourceResolvable, CultureInfo)"/>
    /// <seealso cref="Spring.Context.Support.DefaultMessageSourceResolvable"/>
    public interface IMessageSourceResolvable
	{
        /// <summary>
        /// Return the codes to be used to resolve this message, in the order
        /// that they are to be tried.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The last code will therefore be the default one.
        /// </p>
        /// </remarks>
        /// <returns>
        /// A <see cref="System.String"/> array of codes which are associated
        /// with this message.
        /// </returns>
        IList<string> GetCodes();

        /// <summary>
        /// Return the array of arguments to be used to resolve this message.
        /// </summary>
        /// <returns>
        /// An array of objects to be used as parameters to replace
        /// placeholders within the message text.
        /// </returns>
        object[] GetArguments();

        /// <summary>
        /// Return the default message to be used to resolve this message.
        /// </summary>
        /// <returns>
        /// The default message, or <see langword="null"/> if there is no
        /// default.
        /// </returns>
        string DefaultMessage { get; }
	}
}
