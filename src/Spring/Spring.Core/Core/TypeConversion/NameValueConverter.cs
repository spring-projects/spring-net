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

using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Xml;

#endregion

namespace Spring.Core.TypeConversion
{
	/// <summary>
	/// Custom <see cref="System.ComponentModel.TypeConverter"/> implementation for
	/// <see cref="System.Collections.Specialized.NameValueCollection"/> objects.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Handles conversion from an <b>XML formatted string</b> to a
	/// <see cref="System.Collections.Specialized.NameValueCollection"/> object
	/// (see below for an example of the expected XML format).
	/// </p>
	/// <p>
	/// This converter must be registered before it will be available. Standard
	/// converters in this namespace are automatically registered by the
	/// <see cref="Spring.Objects.ObjectWrapper"/> class.
	/// </p>
	/// </remarks>
	/// <example>
	/// <p>
	/// Find below some examples of the XML formatted strings that this
	/// converter will sucessfully convert. Note that the name of the top level
	/// (document) element is quite arbitrary... it is only the content that
	/// matters (and which must be in the format
	/// <c>&lt;add key="..." value="..."/&gt;</c>. For your continued sanity
	/// though, you may wish to standardize on the top level name of
	/// <c>'dictionary'</c> (although you are of course free to not do so).
	/// </p>
	/// <code escaped="true">
	/// <dictionary>
	///		<add key="host" value="localhost"/>
	///		<add key="port" value="8080"/>
	/// </dictionary>
	/// </code>
	/// <p>
	/// The following example uses a different top level (document) element
	/// name, but is equivalent to the first example.
	/// </p>
	/// <code escaped="true">
	/// <web-configuration-parameters>
	///		<add key="host" value="localhost"/>
	///		<add key="port" value="8080"/>
	/// </web-configuration-parameters>
	/// </code>
	/// </example>
	/// <author>Rod Johnson</author>
	/// <author>Juergen Hoeller</author>
	/// <author>Simon White (.NET)</author>
	public class NameValueConverter : TypeConverter
	{
		#region Constructor (s) / Destructor

		/// <summary>
		/// Creates a new instance of the
        /// <see cref="Spring.Core.TypeConversion.NameValueConverter"/> class.
		/// </summary>
		public NameValueConverter()
		{
		}

		#endregion

		/// <summary>
		/// Returns whether this converter can convert an object of one
		/// <see cref="System.Type"/> to a
		/// <see cref="System.Collections.Specialized.NameValueCollection"/>
		/// </summary>
		/// <remarks>
		/// <p>
		/// Currently only supports conversion from an
		/// <b>XML formatted</b> <see cref="System.String"/> instance.
		/// </p>
		/// </remarks>
		/// <param name="context">
		/// A <see cref="System.ComponentModel.ITypeDescriptorContext"/>
		/// that provides a format context.
		/// </param>
		/// <param name="sourceType">
		/// A <see cref="System.Type"/> that represents the
		/// <see cref="System.Type"/> you want to convert from.
		/// </param>
		/// <returns>True if the conversion is possible.</returns>
		public override bool CanConvertFrom(
			ITypeDescriptorContext context,
			Type sourceType)
		{
			if (sourceType == typeof (string))
			{
				return true;
			}
			return base.CanConvertFrom(context, sourceType);
		}

		/// <summary>
		/// Convert from a string value to a
		/// <see cref="System.Collections.Specialized.NameValueCollection"/> instance.
		/// </summary>
		/// <param name="context">
		/// A <see cref="System.ComponentModel.ITypeDescriptorContext"/>
		/// that provides a format context.
		/// </param>
		/// <param name="culture">
		/// The <see cref="System.Globalization.CultureInfo"/> to use
		/// as the current culture. 
		/// </param>
		/// <param name="value">
		/// The value that is to be converted.
		/// </param>
		/// <returns>
		/// A <see cref="System.Collections.Specialized.NameValueCollection"/>
		/// if successful. 
		/// </returns>
		public override object ConvertFrom(
			ITypeDescriptorContext context,
			CultureInfo culture, object value)
		{
			string text = value as string;
			if (text != null)
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(text);
				return new NameValueSectionHandler()
					.Create(null, null, doc.DocumentElement);
			}
			return base.ConvertFrom(context, culture, value);
		}
	}
}
