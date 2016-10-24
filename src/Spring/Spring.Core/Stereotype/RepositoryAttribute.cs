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

namespace Spring.Stereotype
{
    /// <summary>
    /// Indicates that an annotated class is a "Repository" (or "DAO").
    /// </summary>
    /// <remarks>
    /// A class with this attribute is eligible for Spring DataAccessException translation.  A class
    /// with the Repository attribute is also clarified as to its role in the overall application
    /// architecture for the purpose of tools, aspects, etc.
    /// <para>
    /// This attribute also serves as a specialization of the ComponentAttribute, allowing implementation
    /// classes to be autodetected in future releases through assembly scanning.
    /// </para>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Jueren Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    /// <seealso cref="ComponentAttribute"/>   
    public class RepositoryAttribute : ComponentAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryAttribute"/> class.
        /// </summary>
        public RepositoryAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the repository.</param>
        public RepositoryAttribute(string name) : base(name)
        {
        }


    }
}