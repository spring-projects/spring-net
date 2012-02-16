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

#region Imports

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using Spring.Reflection.Dynamic;
using Spring.Util;

#endregion

namespace Spring.Reflection.Dynamic
{
    #region IDynamicIndexer interface

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

    #endregion

    #region Safe wrapper

    /// <summary>
    /// Safe wrapper for the dynamic indexer.
    /// </summary>
    /// <remarks>
    /// <see cref="SafeIndexer"/> will attempt to use dynamic
    /// indexer if possible, but it will fall back to standard
    /// reflection if necessary.
    /// </remarks>    
    [Obsolete("Use SafeProperty instead", false)]
    public class SafeIndexer : IDynamicIndexer
    {
        private PropertyInfo indexerProperty;

        /// <summary>
        /// Internal PropertyInfo accessor.
        /// </summary>
        internal PropertyInfo IndexerProperty
        {
            get { return indexerProperty; }
        }

        private SafeProperty property;

        /// <summary>
        /// Creates a new instance of the safe indexer wrapper.
        /// </summary>
        /// <param name="indexerInfo">Indexer to wrap.</param>
        public SafeIndexer( PropertyInfo indexerInfo )
        {
            AssertUtils.ArgumentNotNull( indexerInfo, "You cannot create a dynamic indexer for a null value." );

            this.indexerProperty = indexerInfo;
            this.property = new SafeProperty( indexerInfo );
        }

        /// <summary>
        /// Gets the value of the dynamic indexer for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to get indexer value from.
        /// </param>
        /// <param name="index">
        /// Indexer arguments.
        /// </param>
        /// <returns>
        /// A indexer value.
        /// </returns>
        public object GetValue( object target, int index )
        {
            return property.GetValue( target, index );
        }

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
        public object GetValue( object target, object index )
        {
            return property.GetValue( target, index );
        }

        /// <summary>
        /// Gets the value of the dynamic indexer for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to get indexer value from.
        /// </param>
        /// <param name="index">
        /// Indexer arguments.
        /// </param>
        /// <returns>
        /// A indexer value.
        /// </returns>
        public object GetValue( object target, object[] index )
        {
            return property.GetValue( target, index );
        }

        /// <summary>
        /// Sets the value of the dynamic indexer for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to set indexer value on.
        /// </param>
        /// <param name="index">
        /// Indexer arguments.
        /// </param>
        /// <param name="value">
        /// A new indexer value.
        /// </param>
        public void SetValue( object target, int index, object value )
        {
            property.SetValue( target, value, index );
        }

        /// <summary>
        /// Sets the value of the dynamic indexer for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to set indexer value on.
        /// </param>
        /// <param name="index">
        /// Indexer arguments.
        /// </param>
        /// <param name="value">
        /// A new indexer value.
        /// </param>
        public void SetValue( object target, object index, object value )
        {
            property.SetValue( target, value, index );
        }

        /// <summary>
        /// Sets the value of the dynamic indexer for the specified target object.
        /// </summary>
        /// <param name="target">
        /// Target object to set indexer value on.
        /// </param>
        /// <param name="index">
        /// Indexer arguments.
        /// </param>
        /// <param name="value">
        /// A new indexer value.
        /// </param>
        public void SetValue( object target, object[] index, object value )
        {
            property.SetValue( target, value, index );
        }
    }

    #endregion

    /// <summary>
    /// Factory class for dynamic indexers.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Obsolete( "Use DynamicProperty instead", false )]
    public sealed class DynamicIndexer : BaseDynamicMember
    {
        /// <summary>
        /// Prevent instantiation
        /// </summary>
        private DynamicIndexer() { }

        /// <summary>
        /// Creates dynamic indexer instance for the specified <see cref="PropertyInfo"/>.
        /// </summary>
        /// <param name="indexer">Indexer info to create dynamic indexer for.</param>
        /// <returns>Dynamic indexer for the specified <see cref="PropertyInfo"/>.</returns>
        public static IDynamicIndexer Create( PropertyInfo indexer )
        {
            AssertUtils.ArgumentNotNull( indexer, "You cannot create a dynamic indexer for a null value." );

            IDynamicIndexer dynamicIndexer = new SafeIndexer( indexer );
            return dynamicIndexer;
        }
    }

} // namespace
