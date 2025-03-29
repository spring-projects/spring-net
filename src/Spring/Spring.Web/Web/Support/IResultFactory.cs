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
/// A result factory is responsible for create an <see cref="IResult"/> instance from a given string representation.
/// </summary>
/// <remarks>
/// For a larger example illustrating the customization of result processing, <see cref="ResultFactoryRegistry"/>.
/// </remarks>
/// <seealso cref="ResultFactoryRegistry"/>
/// <seealso cref="IResult"/>
/// <seealso cref="Result"/>
/// <seealso cref="DefaultResultWebNavigator"/>
/// <author>Erich Eichinger</author>
public interface IResultFactory
{
    /// <summary>
    /// Create an <see cref="IResult"/> instance from the given string representation.
    /// </summary>
    /// <param name="resultMode">the resultMode that caused triggering this factory.</param>
    /// <param name="resultText">the remainder string to be interpreted and converted into an <see cref="IResult"/>.</param>
    /// <returns>An <see cref="IResult"/> instance. Must never be null!</returns>
    /// <remarks>
    /// Note to implementors: This method must never return null. Instead exceptions should be thrown.
    /// </remarks>
    IResult CreateResult(string resultMode, string resultText);
}
