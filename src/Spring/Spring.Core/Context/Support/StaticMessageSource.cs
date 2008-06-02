#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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

using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Text;

#endregion

namespace Spring.Context.Support
{
    /// <summary>
    /// Simple implementation of <see cref="Spring.Context.IMessageSource"/>
    /// that allows messages to be held in an object and added programmatically.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Mainly useful for testing.
	/// </p>
	/// <p>
	/// This <see cref="Spring.Context.IMessageSource"/> supports internationalization.
	/// </p>
	/// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
	/// <author>Griffin Caprio (.NET)</author>
	/// <seealso cref="Spring.Context.Support.DelegatingMessageSource"/>
    public class StaticMessageSource : AbstractMessageSource
    {
        private Hashtable _messages;
        private Hashtable _objects;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Context.Support.StaticMessageSource"/> class.
		/// </summary>
        public StaticMessageSource()
        {
            _messages = new Hashtable();
            _objects = new Hashtable();
        }

		/// <summary>
		/// Returns a format string.
		/// </summary>
		/// <param name="code">The code of the message to resolve.</param>
		/// <param name="cultureInfo">
		/// The <see cref="System.Globalization.CultureInfo"/> to resolve the
		/// code for.
		/// </param>
		/// <returns>
		/// A format string or <see langword="null"/> if not found.
		/// </returns>
		/// <seealso cref="Spring.Context.Support.AbstractMessageSource.ResolveMessage(string, CultureInfo)"/>
		protected override string ResolveMessage(string code, CultureInfo cultureInfo)
        {
            return (string) _messages[GetLookupKey(code, cultureInfo)];
        }

		/// <summary>
		/// Resolves an object (typically an icon or bitmap).
		/// </summary>
		/// <param name="code">The code of the object to resolve.</param>
		/// <param name="cultureInfo">
		/// The <see cref="System.Globalization.CultureInfo"/> to resolve the
		/// code for.
		/// </param>
		/// <returns>
		/// The resolved object or <see langword="null"/> if not found.
		/// </returns>
		/// <seealso cref="Spring.Context.Support.AbstractMessageSource.ResolveObject(string, CultureInfo)"/>
        protected override object ResolveObject(string code, CultureInfo cultureInfo)
        {
            return _objects[GetLookupKey(code, cultureInfo)];
        }


	// *** NOTE Don't use cref for ComponentResourceManager as it doesn't
	//          exist on 1.0
	//

        /// <summary>
        /// Applies resources to object properties.
        /// </summary>
		/// <remarks>
		/// <p>
		/// Uses a System.ComponentModel.ComponentResourceManager
		/// internally to apply resources to object properties. Resource key
		/// names are of the form <c>objectName.propertyName</c>.
		/// </p>
        /// <p>
        /// This feature is not currently supported on version 1.0 of the .NET platform.
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
		/// <exception cref="System.NotSupportedException">
		/// This feature is not currently supported on version 1.0 of the .NET platform.
		/// </exception>
		/// <seealso cref="Spring.Context.Support.AbstractMessageSource.ApplyResourcesToObject(object, string, CultureInfo)"/>
		protected override void ApplyResourcesToObject(object value, string objectName, CultureInfo cultureInfo)
		{
#if !NET_1_0
		    if(value != null) 
		    {
		        new ComponentResourceManager(value.GetType()).ApplyResources(value, objectName, cultureInfo);
		    }
#else
		    throw new System.NotSupportedException("Operation not supported in .NET 1.0 Release.");
#endif 
		}

        /// <summary>
        /// Associate the supplied <paramref name="messageFormat"/> with the
        /// supplied <paramref name="code"/>.
        /// </summary>
        /// <param name="code">The lookup code.</param>
        /// <param name="culture">
        /// The <see cref="System.Globalization.CultureInfo"/> to resolve the
        /// code for.
        /// </param>
        /// <param name="messageFormat">
        /// The message format associated with this lookup code.
        /// </param>
        public void AddMessage(
            string code, CultureInfo culture, string messageFormat)
        {
            _messages.Add(GetLookupKey(code, culture), messageFormat);
        }

        /// <summary>
        /// Associate the supplied <paramref name="value"/> with the
        /// supplied <paramref name="code"/>.
        /// </summary>
        /// <param name="code">The lookup code.</param>
        /// <param name="cultureInfo">
        /// The <see cref="System.Globalization.CultureInfo"/> to resolve the
        /// code for.
        /// </param>
        /// <param name="value">
        /// The object associated with this lookup code.
        /// </param>
        public void AddObject(string code, CultureInfo cultureInfo, object value)
        {
            _objects.Add(GetLookupKey(code, cultureInfo), value);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> representation of this
        /// message source.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> containing all of this message
        /// source's messages.
        /// </returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append(GetType().Name);
            output.Append(" : ");
            foreach (string code in _messages.Keys)
            {
                output.AppendFormat("['{0}' : '{1}']", code, _messages[code]);
            }
            return output.ToString();
        }

        private static string GetLookupKey(string code, CultureInfo culture)
        {
            return new StringBuilder(code).Append("_").Append(culture).ToString();
        }
    }
}