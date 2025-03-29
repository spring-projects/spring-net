/*
 * Copyright 2002-2010 the original author or authors.
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

using System.Web;
using Spring.Web.UI;

namespace Spring.Web.Support;

/// <summary>
/// An <see cref="IResult"/> encapsulates concrete navigation logic. Usually executing a
/// result will invoke <see cref="HttpResponse.Redirect(string)"/> or <see cref="HttpServerUtility.Transfer(string, bool)"/>.
/// </summary>
/// <remarks>
/// For a larger example illustrating the customization of result processing <see cref="ResultFactoryRegistry"/>.
/// </remarks>
/// <seealso cref="ResultFactoryRegistry"/>
/// <seealso cref="IResult"/>
/// <seealso cref="Result"/>
/// <seealso cref="DefaultResultWebNavigator"/>
/// <seealso cref="Page.SetResult(string, object)"/>
/// <seealso cref="UserControl.SetResult(string, object)"/>
/// <author>Erich Eichinger</author>
public interface IResult
{
    /// <summary>
    /// Execute the result logic within the given <paramref name="context"/>.
    /// </summary>
    /// <param name="context">the context to evaluate this request in.</param>
    void Navigate(object context);

    /// <summary>
    /// Returns an url representation of the result logic within the given <paramref name="context"/>.
    /// </summary>
    /// <param name="context">the context to evaluate this request in.</param>
    /// <returns>the url corresponding to the result instance.</returns>
    /// <remarks>
    /// The returned url is not necessarily fully qualified nor absolute. Returned urls may be relative to the
    /// given context.<br/>
    /// To produce a client-usable url, consider applying e.g. <see cref="System.Web.UI.Control.ResolveUrl"/> or
    /// <see cref="System.Web.UI.Control.ResolveClientUrl"/> before writing the result url to the response.
    /// </remarks>
    string GetRedirectUri(object context);
}
