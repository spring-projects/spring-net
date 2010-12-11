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

using Spring.Objects.Factory.Config;

namespace Spring.Objects.Factory.Support
{

    /// <summary>
    /// Strategy interface for generating object names for object definitions
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public interface IObjectNameGenerator
    {
        /// <summary>
        /// Generates an object name for the given object definition.
        /// </summary>
        /// <param name="definition">The object definition to generate a name for.</param>
        /// <param name="registry">The object definitions registry that the given definition is
        /// supposed to be registerd with</param>
        /// <returns>the generated object name</returns>
        string GenerateObjectName(IObjectDefinition definition, IObjectDefinitionRegistry registry);
    }
}
