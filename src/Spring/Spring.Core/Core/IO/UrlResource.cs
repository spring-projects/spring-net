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
using Spring.Util;

#endregion

namespace Spring.Core.IO
{
	/// <summary>
    /// A <see cref="System.Uri"/> backed resource 
    /// on top of <see cref="System.Net.WebRequest"/>
	/// </summary>
	/// <remarks>
	/// <p>
	/// Obviously supports resolution as a <see cref="System.Uri"/>, and also
	/// as a <see cref="System.IO.FileInfo"/> in the case of the <c>"file:"</c>
	/// protocol.
	/// </p>
	/// </remarks>
	/// <example>
	/// <p>
	/// Some examples of the strings that can be used to initialize a new
	/// instance of the <see cref="Spring.Core.IO.UrlResource"/> class
	/// include...
	/// <list type="bullet">
	/// <item>
	/// <description>file:///Config/objects.xml</description>
	/// </item>
	/// <item>
	/// <description>http://www.mycompany.com/services.txt</description>
	/// </item>
	/// </list>
	/// </p>
	/// </example>
	/// <author>Juergen Hoeller</author>
	/// <author>Leonardo Susatyo (.NET)</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	/// <seealso cref="Spring.Core.IO.IResource"/>
	/// <seealso cref="Spring.Core.IO.IResourceLoader"/>
	/// <seealso cref="Spring.Core.IO.ConfigurableResourceLoader"/>
	public class UrlResource : AbstractResource
	{
		private Uri _uri;
        private WebRequest _webRequest;
		private string _rootLocation;
		private string _resourcePath;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Core.IO.UrlResource"/> class.
		/// </summary>
		/// <example>
		/// <p>
		/// Some examples of the values that the <paramref name="resourceName"/>
		/// can typically be expected to hold include...
		/// <list type="bullet">
		/// <item>
		/// <description>file:///Config/objects.xml</description>
		/// </item>
		/// <item>
		/// <description>http://www.mycompany.com/services.txt</description>
		/// </item>
		/// </list>
		/// </p>
		/// </example>
		/// <param name="resourceName">
		/// A string representation of the <see cref="System.Uri"/> resource.
		/// </param>
		public UrlResource(string resourceName) : base(resourceName)
		{
            if (resourceName.StartsWith("file:///"))
            {
                resourceName = resourceName.Substring("file:///".Length);
            }

			this._uri = new Uri(resourceName);
			_rootLocation = _uri.Host;
			if (!_uri.IsDefaultPort)
			{
				_rootLocation += ":" + _uri.Port;
			}
			_resourcePath = _uri.AbsolutePath;
			int n = _resourcePath.LastIndexOf('/');
			if (n > 0)
			{
				_resourcePath = _resourcePath.Substring(1, n - 1);
			}
			else
			{
				_resourcePath = null;
			}
            _webRequest = WebRequest.Create(_uri);
		}

        /// <summary>
        /// Returns the <see cref="System.Net.WebRequest"/> instance 
        /// used for the resource resolution.
        /// </summary>
        /// <value>
        /// A <see cref="System.Net.WebRequest"/> instance.
        /// </value>
        /// <seealso cref="System.Net.HttpWebRequest"/>
        /// <seealso cref="System.Net.FileWebRequest"/>
        public WebRequest WebRequest
        {
            get { return _webRequest; }
        }

		/// <summary>
		/// Return an <see cref="System.IO.Stream"/> for this resource.
		/// </summary>
		/// <value>
		/// An <see cref="System.IO.Stream"/>.
		/// </value>
		/// <exception cref="System.IO.IOException">
		/// If the stream could not be opened.
		/// </exception>
		/// <seealso cref="Spring.Core.IO.IInputStreamSource"/>
		public override Stream InputStream
		{
			get { return _webRequest.GetResponse().GetResponseStream(); }
		}

		/// <summary>
		/// Returns the <see cref="System.Uri"/> handle for this resource.
		/// </summary>
		/// <value>
		/// The <see cref="System.Uri"/> handle for this resource.
		/// </value>
		/// <exception cref="System.IO.IOException">
		/// If the resource is not available or cannot be exposed as a
		/// <see cref="System.Uri"/>.
		/// </exception>
		/// <seealso cref="Spring.Core.IO.IResource.Uri"/>
		public override Uri Uri
		{
			get { return _uri; }
		}

		/// <summary>
		/// Returns a <see cref="System.IO.FileInfo"/> handle for this resource.
		/// </summary>
		/// <value>
		/// The <see cref="System.IO.FileInfo"/> handle for this resource.
		/// </value>
		/// <exception cref="System.IO.FileNotFoundException">
		/// If the resource is not available on a filesystem.
		/// </exception>
		/// <seealso cref="Spring.Core.IO.IResource.File"/>
		public override FileInfo File
		{
			get
			{
				if (_uri.IsFile)
				{
					return new FileInfo(_uri.AbsolutePath);
				}
				throw new FileNotFoundException(Description +
					" cannot be resolved to absolute file path - " +
					"resource does not use 'file:' protocol." );
			}
		}

		/// <summary>
		/// Does this <see cref="Spring.Core.IO.IResource"/> support relative
		/// resource retrieval?
		/// </summary>
		/// <remarks>
		/// <p>
		/// This implementation does support relative resource retrieval, and
		/// so will always return <see langword="true"/>.
		/// </p>
		/// </remarks>
		/// <value>
		/// <see langword="true"/> if this
		/// <see cref="Spring.Core.IO.IResource"/> supports relative resource
		/// retrieval.
		/// </value>
		/// <seealso cref="Spring.Core.IO.AbstractResource.SupportsRelativeResources"/>
		protected override bool SupportsRelativeResources
		{
			get { return true; }
		}

		/// <summary>
		/// Gets the root location of the resource.
		/// </summary>
		/// <value>
		/// The root location of the resource.
		/// </value>
		/// <seealso cref="Spring.Core.IO.AbstractResource.RootLocation"/>
		protected override string RootLocation
		{
			get { return _rootLocation; }
		}

		/// <summary>
		/// Gets the current path of the resource.
		/// </summary>
		/// <value>
		/// The current path of the resource.
		/// </value>
		/// <seealso cref="Spring.Core.IO.AbstractResource.ResourcePath"/>
		protected override string ResourcePath
		{
			get { return _resourcePath; }
		}

		/// <summary>
		/// Gets those characters that are valid path separators for the
		/// resource type.
		/// </summary>
		/// <value>
		/// Those characters that are valid path separators for the resource
		/// type.
		/// </value>
		/// <seealso cref="Spring.Core.IO.AbstractResource.PathSeparatorChars"/>
		protected override char[] PathSeparatorChars
		{
			get { return new char[] {'/'}; }
		}

		/// <summary>
		/// Returns a description for this resource.
		/// </summary>
		/// <value>
		/// A description for this resource.
		/// </value>
		/// <seealso cref="Spring.Core.IO.IResource.Description"/>
		public override string Description
		{
			get { return StringUtils.Surround("URL [", Uri, "]"); }
		}

        /// <summary>
        /// Does the supplied <paramref name="resourceName"/> relative ?
        /// </summary>
        /// <param name="resourceName">
        /// The name of the resource to test.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if resource name is relative; 
        /// otherwise <see langword="false"/>.
        /// </returns>
        protected override bool IsRelativeResource(string resourceName)
        {
            return true;
        }
	}
}
