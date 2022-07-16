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

using System.Collections;
using System.Collections.Specialized;
using System.Web;

#endregion

namespace Spring.Util
{
    /// <summary>
    /// Abstracts the underlying infrastructure of a HttpRequest
    /// </summary>
    public interface IVirtualEnvironment
    {
        /// <summary>
        /// The virtual (rooted) path of the current Application with trailing slash
        /// </summary>
        /// <remarks>
        /// For the site rooted applications, "/" will be returned, for all others "/..someappdir../"
        /// </remarks>
        string ApplicationVirtualPath { get; }
        /// <summary>
        /// The virtual (rooted) path of the current Request including <see cref="HttpRequest.PathInfo"/>
        /// </summary>
        string CurrentVirtualPath { get; }
        /// <summary>
        /// The virtual (rooted) path of the current Request without trailing <see cref="HttpRequest.PathInfo"/>
        /// </summary>
        string CurrentVirtualFilePath { get; }
        /// <summary>
        /// The virtual (rooted) path of the currently executing script
        /// </summary>
        /// <remarks>
        /// Normally this property is the same as <see cref="CurrentVirtualPath"/>.
        /// In case of <see cref="HttpServerUtility.Transfer(string,bool)"/>, this property returns the current script
        /// whereas CurrentVirtualPath returns the original script path.
        /// </remarks>
        string CurrentExecutionFilePath { get; }
        /// <summary>
        /// The query parameters
        /// </summary>
        NameValueCollection QueryString { get; }
        /// <summary>
        /// Maps a virtual path to it's physical location
        /// </summary>
        string MapPath( string virtualPath );
        /// <summary>
        /// Rewrites the <see cref="CurrentVirtualPath"/>, thus also affecting <see cref="MapPath"/>
        /// </summary>
        IDisposable RewritePath(string newVirtualPath, bool rebaseClientPath);
        /// <summary>
        /// Returns the current Session's variable dictionary
        /// </summary>
        ISessionState Session { get; }
        /// <summary>
        /// Returns the current Request's variable dictionary <see cref="HttpContext.Items"/>
        /// </summary>
        IDictionary RequestVariables { get; }
        /// <summary>
        /// Returns the current Request's parameter dictionary <see cref="HttpRequest.Params"/>
        /// </summary>
        NameValueCollection RequestParams { get; }
        /// <summary>
        /// Get the compiled type for the given virtual path
        /// </summary>
        /// <param name="absoluteVirtualPath">the absolute (=rooted) virtual path</param>
        /// <returns></returns>
        Type GetCompiledType(string absoluteVirtualPath);
        /// <summary>
        /// Creates an instance from the given virtual path
        /// </summary>
        /// <param name="absoluteVirtualPath">the absolute (=rooted) virtual path</param>
        /// <param name="requiredBaseType">the required base type </param>
        object CreateInstanceFromVirtualPath(string absoluteVirtualPath, Type requiredBaseType);
    }
}
