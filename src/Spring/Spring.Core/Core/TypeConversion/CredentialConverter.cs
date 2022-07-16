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

using System.Net;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

#endregion

namespace Spring.Core.TypeConversion
{
    /// <summary>
    /// Converts string representation of a credential for Web client authentication 
    /// into an instance of <see cref="System.Net.NetworkCredential"/>.
    /// </summary>
    /// <example>
    /// <p>
    /// Find below some examples of the XML formatted strings that this
    /// converter will sucessfully convert.
    /// </p>
    /// <code escaped="true">
    /// <property name="credentials" value="Spring\bbaia:sprnet"/>
    /// </code>
    /// <code escaped="true">
    /// <property name="credentials" value="bbaia:sprnet"/>
    /// </code>
    /// </example>
    /// <author>Bruno Baia</author>
    public class CredentialConverter : TypeConverter
    {
        private readonly static Regex credentialRegex = new Regex(
            @"(((?<domain>[\w_.]+)\\)?)(?<userName>([\w_.]+))((:(?<password>([\w_.]+)))?)",
            RegexOptions.Compiled);

        /// <summary>
        /// Can we convert from the sourcetype 
        /// to a <see cref="System.Net.NetworkCredential"/> instance ?
        /// </summary>
        /// <remarks>
        /// <p>
        /// Currently only supports conversion from a <see cref="System.String"/> instance.
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
        /// <returns><see langword="true"/> if the conversion is possible.</returns>
        public override bool CanConvertFrom(
            ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        /// <summary>
        /// Convert from a <see cref="System.String"/> value to an
        /// <see cref="System.Net.NetworkCredential"/> instance.
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
        /// A <see cref="System.Net.NetworkCredential"/> instance if successful.
        /// </returns>        
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                string credentials = (string)value;
                Match m = credentialRegex.Match(credentials);

                if (!m.Success || m.Value != credentials)
                {
                    throw new ArgumentException(String.Format("The credential '{0}' is not well-formed.", value));
                }

                // Get domain
                string domain = m.Groups["domain"].Value;

                // Get user name
                string userName = m.Groups["userName"].Value;

                // Get password
                string password = m.Groups["password"].Value;

                return new NetworkCredential(userName, password, domain);
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
