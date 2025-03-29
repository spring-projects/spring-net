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

namespace Spring.Web.Support;

/// <summary>
/// This result factory implementation creates <see cref="Result"/> instances from a given string representation.
/// </summary>
/// <remarks>
/// For a larger example illustrating the customization of result processing, <see cref="ResultFactoryRegistry"/>.
/// </remarks>
/// <seealso cref="ResultFactoryRegistry"/>
/// <seealso cref="IResult"/>
/// <seealso cref="Result"/>
/// <seealso cref="DefaultResultWebNavigator"/>
/// <author>Erich Eichinger</author>
public class DefaultResultFactory : IResultFactory
{
    /// <summary>
    /// Create a new <see cref="Result"/> from the specified <paramref name="resultText"/>.
    /// </summary>
    /// <param name="resultMode">the result mode.</param>
    /// <param name="resultText">the string representation of the result.</param>
    /// <returns>the <see cref="Result"/> instance created from <paramref name="resultText"/>.</returns>
    public IResult CreateResult(string resultMode, string resultText)
    {
        return new Result(resultMode, resultText);
    }
}