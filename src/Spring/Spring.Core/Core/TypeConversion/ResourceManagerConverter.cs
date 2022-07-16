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
using System.Reflection;
using System.Resources;
using Spring.Core.TypeResolution;
using Spring.Util;

#endregion

namespace Spring.Core.TypeConversion
{
	/// <summary>
	/// Converts a two part string, (resource name, assembly name)
	/// to a ResourceManager instance.
	/// </summary>
	public class ResourceManagerConverter : TypeConverter
	{
		/// <summary>
		/// This constant represents the name of the folder/assembly containing global resources.
		/// </summary>
	    public static readonly string APP_GLOBALRESOURCES_ASSEMBLYNAME = "App_GlobalResources";

        #region Constructor (s) / Destructor
        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Core.TypeConversion.ResourceManagerConverter"/> class.
        /// </summary>
        public ResourceManagerConverter()
        {
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns whether this converter can convert an object of one
        /// <see cref="System.Type"/> to a
        /// <see cref="System.Resources.ResourceManager"/>
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
            if (sourceType == typeof(string)) 
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// Convert from a string value to a
        /// <see cref="System.Resources.ResourceManager"/> instance.
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
        /// A <see cref="System.Resources.ResourceManager"/>
        /// if successful. 
        /// </returns>
        /// <exception cref="ArgumentException">If the specified <paramref name="value"/> does not denote a valid resource</exception>
        public override object ConvertFrom (
            ITypeDescriptorContext context, 
            CultureInfo culture, object value) 
        {
            if (value is string) 
            {          
                // convert incoming string into ResourceManager...
                string[] resourceManagerDescription = StringUtils.DelimitedListToStringArray((string)value, ",");
                if (resourceManagerDescription.Length != 2)
                {
                    throw new ArgumentException ("The string to specify a ResourceManager must be a comma delimited list of length two.  i.e. resourcename, assembly parial name.");
                }
                string resourceName = resourceManagerDescription[0].Trim();
                if (resourceName != null && resourceName.Length == 0)
                {
                    throw new ArgumentException("Empty value set for the resource name in ResourceManager string.");
                }
                string assemblyName = resourceManagerDescription[1].Trim();
                if (assemblyName != null && assemblyName.Length == 0)
                {
                    throw new ArgumentException("Empty value set for the assembly name in ResourceManager string.");
                }
                if (assemblyName == APP_GLOBALRESOURCES_ASSEMBLYNAME)
                {
                    try
                    {
                        Type globalResourcesType = TypeResolutionUtils.ResolveType(resourceName);
                        // look both, NonPublic and Public properties (SPRNET-861)
                        PropertyInfo resourceManagerProperty = globalResourcesType.GetProperty("ResourceManager", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                        return (ResourceManager) resourceManagerProperty.GetValue(globalResourcesType, null);
                    }
                    catch (TypeLoadException ex)
                    {
                        throw new ArgumentException("Could not load resources '{0}'", resourceName, ex);
                    }
                }
                Assembly ass = Assembly.LoadWithPartialName(assemblyName);
                if (ass == null)
                {
                    throw new ArgumentException("Could not find assembly with name = '" + assemblyName + "'.");
                }
                return new ResourceManager(resourceName, ass);
            }
            return base.ConvertFrom (context, culture, value);
        }
        #endregion
	}
}
