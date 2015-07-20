#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

namespace Spring.Reflection.Dynamic
{
    /// <summary>
    /// Defines methods that dynamic indexer class has to implement.
    /// </summary>
    public interface IDynamicIndexer
    {
        /// <summary>
        /// Gets the value of the dynamic indexer for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to get the indexer value from.
        /// </param>
        /// <param name="index">
        /// Indexer argument.
        /// </param>
        /// <returns>
        /// A indexer value.
        /// </returns>
        object GetValue( object target, int index );

        /// <summary>
        /// Gets the value of the dynamic indexer for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to get the indexer value from.
        /// </param>
        /// <param name="index">
        /// Indexer argument.
        /// </param>
        /// <returns>
        /// A indexer value.
        /// </returns>
        object GetValue( object target, object index );

        /// <summary>
        /// Gets the value of the dynamic indexer for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to get the indexer value from.
        /// </param>
        /// <param name="index">
        /// Indexer arguments.
        /// </param>
        /// <returns>
        /// A indexer value.
        /// </returns>
        object GetValue( object target, object[] index );

        /// <summary>
        /// Gets the value of the dynamic indexer for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to set the indexer value on.
        /// </param>
        /// <param name="index">
        /// Indexer argument.
        /// </param>
        /// <param name="value">
        /// A new indexer value.
        /// </param>
        void SetValue( object target, int index, object value );

        /// <summary>
        /// Gets the value of the dynamic indexer for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to set the indexer value on.
        /// </param>
        /// <param name="index">
        /// Indexer argument.
        /// </param>
        /// <param name="value">
        /// A new indexer value.
        /// </param>
        void SetValue( object target, object index, object value );

        /// <summary>
        /// Gets the value of the dynamic indexer for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to set the indexer value on.
        /// </param>
        /// <param name="index">
        /// Indexer arguments.
        /// </param>
        /// <param name="value">
        /// A new indexer value.
        /// </param>
        void SetValue( object target, object[] index, object value );
    }
}
