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
using Common.Logging;
using Spring.Util;

#endregion

namespace Spring.Core.IO
{
	/// <summary>
	/// Custom type converter for <see cref="Spring.Core.IO.IResource"/> instances.
	/// </summary>
	/// <remarks>
	/// <p>
	/// A resource path may contain placeholder variables of the form <c>${...}</c>
	/// that will be expended to <b>environment variables</b>.
	/// </p>
	/// <p>
	/// Currently only supports conversion from a <see cref="System.String"/>
	/// instance.
	/// </p>
	/// </remarks>
	/// <example>
	/// <p>
	/// On Win9x boxes, this resource path, <c>${userprofile}\objects.xml</c> will
	/// be expanded at runtime with the value of the <c>'userprofile'</c> environment
	/// variable substituted for the <c>'${userprofile}'</c> portion of the path.
	/// </p>
	/// <code escaped="true">
	/// // assuming a user called Rick, running on a plain vanilla Windows XP setup...
	/// // this resource path...
	/// 
	/// ${userprofile}\objects.xml
	/// 
	/// // will become (after expansion)...
	/// 
	/// C:\Documents and Settings\Rick\objects.xml
	/// </code>
	/// </example>
	/// <author>Mark Pollack</author>
	/// <seealso cref="Spring.Core.IO.IResourceLoader"/>
	/// <seealso cref="System.ComponentModel.TypeConverter"/>
	public class ResourceConverter : TypeConverter
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof (ResourceConverter));
		private IResourceLoader _resourceLoader;
		
		#region Constructor (s) / Destructor

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Core.IO.ResourceConverter"/> class.
		/// </summary>
		public ResourceConverter()
		{
			_resourceLoader = new ConfigurableResourceLoader();
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Core.IO.ResourceConverter"/> class using the specified resourceLoader.
		/// </summary>
		/// <param name="resourceLoader">the underlying IResourceLoader to be used to resolve resources</param>
		public ResourceConverter( IResourceLoader resourceLoader )
		{
			AssertUtils.ArgumentNotNull( resourceLoader, "resourceLoader" );
			_resourceLoader = resourceLoader;			
		}
		#endregion

		/// <summary>
		/// Returns whether this converter can convert an object of one
		/// <see cref="System.Type"/> to a <see cref="Spring.Core.IO.IResource"/>
		/// </summary>
		/// <param name="context">
		/// A <see cref="System.ComponentModel.ITypeDescriptorContext"/>
		/// that provides a format context.
		/// </param>
		/// <param name="sourceType">
		/// A <see cref="System.Type"/> that represents the
		/// <see cref="System.Type"/> you want to convert from.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the conversion is possible.
		/// </returns>
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
		/// <see cref="Spring.Core.IO.IResource"/> instance.
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
		/// An <see cref="Spring.Core.IO.IResource"/> if successful. 
		/// </returns>
		/// <exception cref="System.UriFormatException">
		/// If the resource name objectained form the supplied
		/// <paramref name="value"/> is malformed.
		/// </exception>
		/// <exception cref="System.Exception">
		/// In the case of any errors arising from the instantiation of the
		/// returned <see cref="Spring.Core.IO.IResource"/> instance.
		/// </exception>
		public override object ConvertFrom(
			ITypeDescriptorContext context,
			CultureInfo culture, object value)
		{
			string resource = value as string;
			if (resource != null)
			{
				return GetResourceLoader().GetResource(ResolvePath(resource));
			}
			return base.ConvertFrom(context, culture, value);
		}

		/// <summary>
		/// Resolve the given path, replacing placeholder values with
		/// corresponding property values if necessary.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This implementation resolves environment variables only.
		/// </p>
		/// </remarks>
		/// <param name="path">The original resource path.</param>
		/// <returns>The resolved resource path.</returns>
		protected virtual string ResolvePath(string path)
		{
			// quite inefficient, but cost is only ever paid once at startup...
			IList<string> expressions = StringUtils.GetAntExpressions(path);
			foreach (string expression in expressions)
			{
				string environmentValue
					= Environment.GetEnvironmentVariable(expression);
				if (environmentValue != null)
				{
					path = StringUtils.SetAntExpression(
						path, expression, environmentValue);
				}
				else
				{
					#region Instrumentation

					if (_log.IsWarnEnabled)
					{
						_log.Warn(string.Format(
							CultureInfo.InvariantCulture,
							"Could not resolve placeholder '{0}' in resource path " +
								"'{1}' as an environment variable.", expression, path));
					}

					#endregion
				}
			}
			return path;
		}

		/// <summary>
		/// Return the <see cref="Spring.Core.IO.IResourceLoader"/> used to
		/// resolve the string.
		/// </summary>
		/// <returns>
		/// The <see cref="Spring.Core.IO.IResourceLoader"/> used to resolve
		/// the string.
		/// </returns>
		protected internal virtual IResourceLoader GetResourceLoader()
		{
			return _resourceLoader;
		}
	}
}
