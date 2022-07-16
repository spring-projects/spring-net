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
using System.Resources;
using System.Text;

using Spring.Objects.Factory;
using Spring.Core.TypeConversion;

#endregion

namespace Spring.Context.Support
{
	/// <summary>
	/// An <see cref="Spring.Context.IMessageSource"/> implementation that
	/// accesses resources from .resx / .resource files.
	/// </summary>
    /// <remarks>Note that for the method 
    /// GetResourceObject if the resource name resolves to null, then in 
    /// .NET 1.1 the return value will be String.Empty whereas 
    /// in .NET 2.0 it will return null.</remarks>
	/// <author>Griffin Caprio (.NET)</author>
	/// <author>Mark Pollack (.NET)</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	public class ResourceSetMessageSource : AbstractMessageSource, IInitializingObject
	{
		#region Fields

		private Dictionary<string, object> _cachedResources;
		private IList<object> _resourceManagers;

		#endregion

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Context.Support.ResourceSetMessageSource"/> class.
		/// </summary>
		public ResourceSetMessageSource()
		{
			_cachedResources = new Dictionary<string, object>();
			_resourceManagers = new List<object>();
		}

		/// <summary>
		/// The collection of <see cref="System.Resources.ResourceManager"/>s
		/// in this <see cref="Spring.Context.Support.ResourceSetMessageSource"/>.
		/// </summary>
		public IList<object> ResourceManagers
		{
			get { return _resourceManagers; }
			set { _resourceManagers = value; }
		}

		/// <summary>
		/// Resolves a given code by searching through each assembly name in
		/// the base names array.
		/// </summary>
		/// <param name="code">The code to resolve.</param>      
		/// <param name="cultureInfo">
		/// The <see cref="System.Globalization.CultureInfo"/> to use for lookups.
		/// </param>
		/// <returns>The message from the resource set.</returns>
		protected override string ResolveMessage(
			string code, CultureInfo cultureInfo)
		{
			string message = null;
			for (int i = 0; message == null & i < _resourceManagers.Count; i++)
			{
				message = ResolveObject((ResourceManager) _resourceManagers[i], code, cultureInfo) as string;
			}
			return message;
		}

		/// <summary>
		/// Resolves a given code by searching through each assembly name in the array.
		/// </summary>
		/// <param name="code">The code to resolve.</param>      
		/// <param name="cultureInfo">
		/// The <see cref="System.Globalization.CultureInfo"/> to use for lookups.
		/// </param>
		/// <returns>The object from the resource set.</returns>
		protected override object ResolveObject(string code, CultureInfo cultureInfo)
		{
			object obj = null;

			for (int i = 0; obj == null & i < _resourceManagers.Count; i++)
			{
				obj = ResolveObject((ResourceManager) _resourceManagers[i], code, cultureInfo);
			}
			return obj;
		}

		// *** NOTE Don't use cref for ComponentResourceManager as it doesn't
		//          exist on 1.0
		//

		/// <summary>
		/// Uses a System.ComponentModel.ComponentResourceManager
		/// to apply resources to object properties.
		/// Resource key names are of the form objectName.propertyName
		/// </summary>
		/// <remarks>
        /// This feature is not currently supported on version 1.0 of the .NET platform.
		/// </remarks>
		/// <param name="value">
		/// An object that contains the property values to be applied.
		/// </param>
		/// <param name="objectName">
		/// The base name of the object to use for the key lookup.
		/// </param>      
		/// <param name="culture">
		/// The <see cref="System.Globalization.CultureInfo"/> to use for lookups.
		/// If <cref lang="null"/>, uses the
		/// <see cref="System.Globalization.CultureInfo.CurrentUICulture"/> value.
		/// </param>
		/// <exception cref="System.NotSupportedException">
		/// This feature is not currently supported on version 1.0 of the .NET platform.
		/// </exception>
		protected override void ApplyResourcesToObject(object value, string objectName, CultureInfo culture)
		{
		    if(value != null) 
		    {
		        ComponentResourceManager crm = new ComponentResourceManager(value.GetType());
		        crm.ApplyResources(value, objectName, culture);
		    }
		}

	    /// <summary>
	    /// Resolves a code into an object given a base name.
	    /// </summary>
	    /// <param name="resourceManager">The <see cref="System.Resources.ResourceManager"/> to search.</param>
	    /// <param name="code">The code to resolve.</param>      
	    /// <param name="cultureInfo">
	    /// The <see cref="System.Globalization.CultureInfo"/> to use for lookups.
	    /// </param>
	    /// <returns>The object from the resource file.</returns>
	    protected object ResolveObject(ResourceManager resourceManager, string code, CultureInfo cultureInfo)
	    {
            string cacheKey = code + "." + cultureInfo.Name;
            object resource;

            if (!_cachedResources.TryGetValue(cacheKey, out resource))
            {
                resource = resourceManager.GetObject(code, cultureInfo);
                if (resource != null)
                {
                    lock (_cachedResources)
                    {
                        _cachedResources[cacheKey] = resource;
                    }
                }
            }

            return resource;
	    }

	    /// <summary>
	    /// Returns a <see cref="System.String"/> representation of the
	    /// <see cref="Spring.Context.Support.ResourceSetMessageSource"/>.
	    /// </summary>
	    /// <returns>A <see cref="System.String"/> representation of the
	    /// <see cref="Spring.Context.Support.ResourceSetMessageSource"/>.</returns>
	    public override string ToString()
	    {
	        StringBuilder sb = new StringBuilder();
	        sb.Append(GetType().Name);
	        sb.Append(" with ResourceManagers of base names = [");
	        foreach (ResourceManager rm in ResourceManagers)
	        {
	            sb.Append(rm.BaseName);
	            sb.Append(",");
	        }
	        string s = sb.ToString();
	        return s.TrimEnd(new char[] {','}) + "]";
	    }

	    /// <summary>
	    /// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
	    /// after it has set all object properties supplied.
	    /// </summary>
	    /// <remarks>
	    /// <p>
	    /// The list may contain objects of type <see cref="System.String"/> or
	    /// <see cref="System.Resources.ResourceManager"/>. <see cref="System.String"/> types
	    /// are converted to <see cref="System.Resources.ResourceManager"/> instances using the notation
	    /// resourcename, assembly partial name.
	    /// </p>
	    /// </remarks>
	    /// <exception cref="System.ArgumentException">
	    /// If the conversion from a <see cref="System.String"/> to a
	    /// <see cref="System.Resources.ResourceManager"/> can't be performed.
	    /// </exception>
	    public void AfterPropertiesSet()
	    {
	        ResourceManagerConverter cvt = new ResourceManagerConverter();
	        for (int i = 0; i < _resourceManagers.Count; i++)
	        {
	            object o = _resourceManagers[i];
	            if (o is String)
	            {
	                _resourceManagers[i] = cvt.ConvertFrom((string) o);
	            }
	            else if (!(o is ResourceManager))
	            {
	                throw new ArgumentException("Only Types of string and ResourceManager are allowed.  Type " + o.GetType() + " was set instead.");
	            }
	        }
	    }
	}
}
