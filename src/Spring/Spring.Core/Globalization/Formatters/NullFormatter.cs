#region License

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

#endregion

namespace Spring.Globalization.Formatters
{
    /// <summary>
    /// Implementation of <see cref="IFormatter"/> that simply calls <see cref="object.ToString()"/>.
    /// </summary>
    /// <remarks>
    /// This formatter is a no-operation implementation.
    /// </remarks>
    /// <author>Erich Eichinger</author>
    public class NullFormatter : IFormatter
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NullFormatter"/> class.
        /// </summary>
        public NullFormatter()
        {
        }

        #endregion

        /// <summary>
        /// Converts the passed value to a string by calling <see cref="Object.ToString()"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>to string converted value.</returns>
        public string Format(object value)
        {
            return value != null ? value.ToString() : null;
        }

        /// <summary>
        /// Returns the passed string "as is".
        /// </summary>
        /// <param name="value">The value to return.</param>
        /// <returns>The value passed into this method.</returns>
        public object Parse(string value)
        {
            return value;
        }
    }
}
