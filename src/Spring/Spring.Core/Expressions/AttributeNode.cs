#region License

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

#endregion

using System.Runtime.Serialization;

using Spring.Core.TypeResolution;

namespace Spring.Expressions
{
    /// <summary>
    /// Represents parsed attribute node in the navigation expression.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class AttributeNode : ConstructorNode
    {
        /// <summary>
        /// Create a new instance
        /// </summary>
        public AttributeNode()
        {
        }

        /// <summary>
        /// Create a new instance from SerializationInfo
        /// </summary>
        protected AttributeNode(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Tries to determine attribute type based on the specified
        /// attribute type name.
        /// </summary>
        /// <param name="typeName">
        /// Attribute type name to resolve.
        /// </param>
        /// <returns>
        /// Resolved attribute type.
        /// </returns>
        /// <exception cref="TypeLoadException">
        /// If type cannot be resolved.
        /// </exception>
        protected override Type GetObjectType(string typeName)
        {
            Type type;

            try
            {
                type = base.GetObjectType(typeName);
            }
            catch (TypeLoadException)
            {
                if (typeName.EndsWith("Attribute"))
                {
                    throw;
                }
                type = TypeResolutionUtils.ResolveType(typeName + "Attribute");
            }

            return type;
        }
    }
}
