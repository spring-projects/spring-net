#region License

/*
 * Copyright © 2010-2011 the original author or authors.
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

using Spring.Objects.Factory.Support;

namespace Spring.Context.Support
{
    /// <summary>
    /// ApplicationContext that can scan to identify object definitions
    /// </summary>
    public class CodeConfigApplicationContext : GenericApplicationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Spring.Context.Support.GenericApplicationContext"/> class.
        /// </summary>
        public CodeConfigApplicationContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Spring.Context.Support.GenericApplicationContext"/> class.
        /// </summary>
        /// <param name="caseSensitive">if set to <c>true</c> names in the context are case sensitive.</param>
        public CodeConfigApplicationContext(bool caseSensitive)
            : base(caseSensitive)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Spring.Context.Support.GenericApplicationContext"/> class.
        /// </summary>
        /// <param name="objectFactory">The object factory instance to use for this context.</param>
        public CodeConfigApplicationContext(DefaultListableObjectFactory objectFactory)
            : base(objectFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Spring.Context.Support.GenericApplicationContext"/> class.
        /// </summary>
        /// <param name="parent">The parent application context.</param>
        public CodeConfigApplicationContext(IApplicationContext parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Spring.Context.Support.GenericApplicationContext"/> class.
        /// </summary>
        /// <param name="name">The name of the application context.</param><param name="caseSensitive">if set to <c>true</c> names in the context are case sensitive.</param><param name="parent">The parent application context.</param>
        public CodeConfigApplicationContext(string name, bool caseSensitive, IApplicationContext parent)
            : base(name, caseSensitive, parent)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Spring.Context.Support.GenericApplicationContext"/> class.
        /// </summary>
        /// <param name="objectFactory">The object factory to use for this context</param><param name="parent">The parent applicaiton context.</param>
        public CodeConfigApplicationContext(DefaultListableObjectFactory objectFactory, IApplicationContext parent)
            : base(objectFactory, parent)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Spring.Context.Support.GenericApplicationContext"/> class.
        /// </summary>
        /// <param name="name">The name of the application context.</param><param name="caseSensitive">if set to <c>true</c> names in the context are case sensitive.</param><param name="parent">The parent application context.</param><param name="objectFactory">The object factory to use for this context</param>
        public CodeConfigApplicationContext(string name, bool caseSensitive, IApplicationContext parent,
                                            DefaultListableObjectFactory objectFactory)
            : base(name, caseSensitive, parent, objectFactory)
        {
        }
    }
}