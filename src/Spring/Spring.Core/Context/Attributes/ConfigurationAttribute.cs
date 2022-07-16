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

using Spring.Stereotype;

namespace Spring.Context.Attributes
{
    /// <summary>
    /// Indicates that a class declares one or more <see cref="ObjectDefAttribute"/> methods and may be processed
    /// by the Spring container to generate object definitions and service requests for those objects
    /// at runtime.
    ///
    /// <para>Configuration is meta-annotated as a <see cref="ComponentAttribute"/>, therefore Configuration
    /// classes are candidates for component-scanning.
    /// </para>
    /// <para>May be used in conjunction with the <see cref="LazyAttribute"/> attribute to indicate that all object
    /// methods declared within this class are by default lazily initialized.
    ///</para>
    /// <h3>Constraints</h3>
    /// <ul>
    ///    <li>Configuration classes must be non-sealed</li>
    ///    <li>Configuration classes must have a default/no-arg constructor</li>
    /// </ul>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigurationAttribute : ComponentAttribute
    {

        /// <summary>
        /// Initializes a new instance of the ConfigurationAttribute class.
        /// </summary>
        public ConfigurationAttribute()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the Configuration class.
        /// </summary>
        /// <param name="name"></param>
        public ConfigurationAttribute(string name)
        {
            Name = name;
        }
        
    }
}
