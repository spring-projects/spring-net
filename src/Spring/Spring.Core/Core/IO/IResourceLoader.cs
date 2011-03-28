#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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



#endregion

namespace Spring.Core.IO
{
    /// <summary>
    /// Describes an object that can load
    /// <see cref="Spring.Core.IO.IResource"/>s.
    /// </summary>
    /// <remarks>
    /// <p>
    /// An <see cref="Spring.Context.IApplicationContext"/> implementation is
    /// generally required to support the functionality described by this
    /// interface.
    /// </p>
    /// <p>
    /// The <see cref="Spring.Core.IO.ConfigurableResourceLoader"/> class is a
    /// standalone implementation that is usable outside an
    /// <see cref="Spring.Context.IApplicationContext"/>; the aforementioned
    /// class is also used by the
    /// <see cref="Spring.Core.IO.ResourceConverter"/> class.
    /// </p>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    /// <seealso cref="Spring.Core.IO.IResource"/>
    /// <seealso cref="Spring.Core.IO.ResourceConverter"/>
    /// <seealso cref="Spring.Core.IO.ConfigurableResourceLoader"/>
    public interface IResourceLoader
    {
    	/// <summary>
    	/// Return an <see cref="Spring.Core.IO.IResource"/> handle for the
    	/// specified resource.
    	/// </summary>
    	/// <remarks>
    	/// <p>
    	/// The handle should always be a reusable resource descriptor; this
    	/// allows one to make repeated calls to the underlying
    	/// <see cref="Spring.Core.IO.IInputStreamSource.InputStream"/>.
    	/// </p>
    	/// <p>
    	/// <ul>
    	/// <li>
    	/// <b>Must</b> support fully qualified URLs, e.g. "file:C:/test.dat".
    	/// </li>
    	/// <li>
    	/// Should support relative file paths, e.g. "test.dat" (this will be
    	/// implementation-specific, typically provided by an
    	/// <see cref="Spring.Context.IApplicationContext"/> implementation).
    	/// </li>
    	/// </ul>
    	/// </p>
    	/// <note>
    	/// An <see cref="Spring.Core.IO.IResource"/> handle does not imply an
    	/// existing resource; you need to check the value of an
    	/// <see cref="Spring.Core.IO.IResource"/>'s
    	/// <see cref="Spring.Core.IO.IResource.Exists"/> property to determine
    	/// conclusively whether or not the resource actually exists.
    	/// </note>
    	/// </remarks>
    	/// <param name="location">The resource location.</param>
    	/// <returns>
    	/// An appropriate <see cref="Spring.Core.IO.IResource"/> handle.
    	/// </returns>
    	/// <seealso cref="Spring.Core.IO.IResource"/>
    	/// <seealso cref="Spring.Core.IO.IResource.Exists"/>
    	/// <seealso cref="Spring.Context.IApplicationContext"/>
    	IResource GetResource(string location);
    }
}
