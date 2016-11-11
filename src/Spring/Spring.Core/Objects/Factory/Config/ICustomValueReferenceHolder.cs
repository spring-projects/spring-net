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

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// May be used to store custom value references in object definition properties.
    /// </summary>
    /// <see cref="Spring.Objects.Factory.Support.ObjectDefinitionValueResolver"/>
    /// <author>Erich Eichinger</author>
    public interface ICustomValueReferenceHolder
    {
        /// <summary>
        /// </summary>
        /// <param name="objectFactory">
        /// the object factory holding the given object definition
        /// </param>
        /// <param name="name">
        /// The name of the object that is having the value of one of its properties resolved.
        /// </param>
        /// <param name="definition">
        /// The definition of the named object.
        /// </param>
        /// <param name="argumentName">
        /// The name of the property the value of which is being resolved.
        /// </param>
        /// <param name="argumentValue">
        /// The value of the property that is being resolved.
        /// </param>
        object Resolve(IObjectFactory objectFactory, string name, IObjectDefinition definition, string argumentName, object argumentValue);
    }
}