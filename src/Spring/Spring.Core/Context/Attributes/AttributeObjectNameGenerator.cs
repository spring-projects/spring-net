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
using Spring.Objects.Factory.Support;

namespace Spring.Context.Attributes
{
    /// <summary>
    /// Default Name Generator for attribute driven component scan.
    /// 
    /// First choice is the provided name of the Component attribute.
    /// Fallback is the short type name.
    /// </summary>
    public class AttributeObjectNameGenerator : IObjectNameGenerator
    {
        /// <summary>
        /// Generates an object name for the given object definition.
        /// </summary>
        /// <param name="definition">The object definition to generate a name for.</param>
        /// <param name="registry">The object definitions registry that the given definition is
        ///             supposed to be registerd with</param>
        /// <returns>
        /// the generated object name
        /// </returns>
        public string GenerateObjectName(IObjectDefinition definition, IObjectDefinitionRegistry registry)
        {
            if (definition is ScannedGenericObjectDefinition)
            {
                string objectName = ((ScannedGenericObjectDefinition) definition).ComponentName;
                if (!string.IsNullOrEmpty(objectName))
                    return objectName;
            }
            return BuildDefaultObjectName(definition);
        }

        private string BuildDefaultObjectName(IObjectDefinition definition)
        {
            return definition.ObjectType.FullName;
        }
    }
}
