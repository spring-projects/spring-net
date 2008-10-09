#region License

/*
 * Copyright 2002-2008 the original author or authors.
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

namespace Spring.Stereotype
{
    /// <summary>
    /// Indicates that an annotated class is a "Service" (e.g. a business service facade).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This attribute also serves as a specialization of the ComponentAttribute, allowing implementation
    /// classes to be autodetected in future releases through assembly scanning.
    /// </para>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public class ServiceAttribute : ComponentAttribute
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceAttribute"/> class.
        /// </summary>
        public ServiceAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public ServiceAttribute(string name) : base(name)
        {
        }


    }
}