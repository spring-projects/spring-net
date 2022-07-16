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
using System.Globalization;

using Spring.Core.IO;

#endregion

namespace Spring.Core.TypeConversion
{
	/// <summary>
	/// Converter for <see cref="System.IO.Stream"/> to directly set a
	/// <see cref="System.IO.Stream"/> property.
	/// </summary>
	/// <author>Jurgen Hoeller</author>
	/// <author>Mark Pollack (.NET)</author>
	public class StreamConverter : TypeConverter
	{
        private ResourceConverter resourceConverter;

        #region Constructors
        /// <summary>
        /// Create a new StreamConverter using the default
        /// <see cref="Spring.Core.IO.ResourceConverter"/>.
        /// </summary>
		public StreamConverter() : this (new ResourceConverter())
		{
		}

        /// <summary>
        /// Create a new StreamConverter using the given
        /// <see cref="Spring.Core.IO.ResourceConverter"/>.
        /// </summary>
        /// <param name="resourceConverter">
        /// The <see cref="Spring.Core.IO.ResourceConverter"/> to use.</param>
        public StreamConverter(ResourceConverter resourceConverter)
        {
            this.resourceConverter = resourceConverter == null
                ? new ResourceConverter() : resourceConverter;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns whether this converter can convert an object of one
        /// <see cref="System.Type"/> to a <see cref="System.IO.Stream"/>
        /// </summary>
        /// <remarks>
        /// <p>
        /// Currently only supports conversion from a
        /// <see cref="System.String"/> instance.
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
        public override bool CanConvertFrom (
            ITypeDescriptorContext context,
            Type sourceType)
        {
            if (sourceType == typeof (string))
            {
                return true;
            }
            return base.CanConvertFrom (context, sourceType);
        }

        /// <summary>
        /// Convert from a string value to a <see cref="System.IO.Stream"/> instance.
        /// </summary>
        /// <param name="context">
        /// A <see cref="System.ComponentModel.ITypeDescriptorContext"/>
        /// that provides a format context.
        /// </param>
        /// <param name="culture">
        /// The <see cref="System.Globalization.CultureInfo"/> to use
        /// as the current culture.
        /// </param>
        /// <param name="val">
        /// The value that is to be converted.
        /// </param>
        /// <returns>
        /// A <see cref="System.IO.Stream"/> if successful.
        /// </returns>
        public override object ConvertFrom (
            ITypeDescriptorContext context,
            CultureInfo culture, object val)
        {
            if (val is string)
            {
                IResource resource = (IResource) resourceConverter.ConvertFrom(context, culture, val);
                return resource.InputStream;
            }
            return base.ConvertFrom (context, culture, val);
        }
        #endregion
	}
}
