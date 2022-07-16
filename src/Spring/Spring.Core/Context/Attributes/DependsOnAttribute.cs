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

namespace Spring.Context.Attributes
{
    /// <summary>
    /// objects on which the current object depends. Any objects specified are guaranteed to be
    /// created by the container before this object. Used infrequently in cases where a object
    /// does not explicitly depend on another through properties or constructor arguments,
    /// but rather depends on the side effects of another object's initialization.
    /// <para>Note: This attribute will not be inherited by child object definitions,
    /// hence it needs to be specified per concrete object definition.
    /// </para>
    /// <para>Using <see cref="DependsOnAttribute"/> at the class level has no effect unless component-scanning
    /// is being used. If a <see cref="DependsOnAttribute"/>-attributed class is declared via XML,
    /// <see cref="DependsOnAttribute"/> attribute metadata is ignored, and
    /// &lt;object depends-on="..."/&gt; is respected instead.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class DependsOnAttribute : Attribute
    {
        private string[] _name;

        /// <summary>
        /// Initializes a new instance of the DependsOn class.
        /// </summary>
        public DependsOnAttribute(string name)
            : this(new[] { name })
        {
        }

        /// <summary>
        /// Initializes a new instance of the DependsOn class.
        /// </summary>
        public DependsOnAttribute(params string[] name)
        {
            _name = name;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string[] Name
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }

    }
}
