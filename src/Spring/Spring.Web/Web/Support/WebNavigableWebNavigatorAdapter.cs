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

namespace Spring.Web.Support;

/// <summary>
/// Adapts a concrete <see cref="IWebNavigator"/> instance as a <see cref="IWebNavigable"/>.
/// </summary>
/// <author>Erich Eichinger</author>
public class WebNavigableWebNavigatorAdapter : IWebNavigable
{
    private readonly IWebNavigator _resultNavigator;

    /// <summary>
    /// Create a new adapter instance, wrapping the specified <paramref name="resultNavigator"/>
    /// into a <see cref="IWebNavigable"/> interface.
    /// </summary>
    /// <param name="resultNavigator">the <see cref="IWebNavigator"/> instance to be adapted. May be null.</param>
    public WebNavigableWebNavigatorAdapter(IWebNavigator resultNavigator)
    {
        _resultNavigator = resultNavigator;
    }

    /// <summary>
    /// Returns the wrapped <see cref="IWebNavigator"/> that was passed into <see cref="WebNavigableWebNavigatorAdapter(IWebNavigator)"/>.
    /// </summary>
    public IWebNavigator WebNavigator
    {
        get { return _resultNavigator; }
    }
}
