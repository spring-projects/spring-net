#region License

// /*
//  * Copyright 2018 the original author or authors.
//  *
//  * Licensed under the Apache License, Version 2.0 (the "License");
//  * you may not use this file except in compliance with the License.
//  * You may obtain a copy of the License at
//  *
//  *      http://www.apache.org/licenses/LICENSE-2.0
//  *
//  * Unless required by applicable law or agreed to in writing, software
//  * distributed under the License is distributed on an "AS IS" BASIS,
//  * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  * See the License for the specific language governing permissions and
//  * limitations under the License.
//  */

#endregion

using Spring.Objects.Factory.Config;

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// Post-processor callback interface for <i>merged</i> bean definitions at runtime.
    /// <see cref="IObjectPostProcessor"/> implementations may implement this sub-interface in order
    /// to post-process the merged bean definition (a processed copy of the original object
    /// definition) that the Spring <see cref="IObjectFactory" /> uses to create a object instance.
    /// </summary>
    /// <remarks>
    /// The <see cref="PostProcessMergedObjectDefinition" /> method may for example introspect
    /// the object definition in order to prepare some cached metadata before post-processing
    /// actual instances of a object. It is also allowed to modify the object definition but
    /// <i>only</i> for definition properties which are actually intended for concurrent
    /// modification. Essentially, this only applies to operations defined on the
    /// <see cref="RootObjectDefinition" /> itself but not to the properties of its base classes.
    /// </remarks>
    public interface IMergedObjectDefinitionPostProcessor : IObjectPostProcessor
    {
        /// <summary>
        /// Post-process the given merged object definition for the specified object.
        /// </summary>
        /// <param name="objectDefinition">The merged object definition for the object</param>
        /// <param name="objectType">The actual type of the managed object instance</param>
        /// <param name="objectName">The name of the object</param>
        /// <see cref="AbstractAutowireCapableObjectFactory.ApplyMergedObjectDefinitionPostProcessors" />
        void PostProcessMergedObjectDefinition(RootObjectDefinition objectDefinition, Type objectType, string objectName);

        /// <summary>
        /// A notification that the object definition for the specified name has been reset,
        /// and that this post-processor should clear any metadata for the affected object.
        /// </summary>
        /// <remarks>
        /// The default implementation is empty.
        /// </remarks>
        /// <param name="objectName">The name of the object</param>
        void ResetObjectDefinition(string objectName);
    }
}
