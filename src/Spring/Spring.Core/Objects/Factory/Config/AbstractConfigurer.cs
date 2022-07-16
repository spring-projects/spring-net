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

using Spring.Core;
using Spring.Core.TypeResolution;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Base class that provides common functionality needed for several IObjectFactoryPostProcessor
    /// implementations
    /// </summary>
    /// <author>Mark Pollack</author>
    [Serializable]
    public abstract class AbstractConfigurer : IPriorityOrdered, IObjectFactoryPostProcessor
    {
        private int order = Int32.MaxValue; // default: same as non-Ordered

        /// <summary>
        /// Return the order value of this object, with a higher value meaning
        /// greater in terms of sorting.
        /// </summary>
        /// <returns>The order value.</returns>
        /// <seealso cref="Spring.Core.IOrdered.Order"/>
        public int Order
        {
            get { return order; }
            set { order = value; }
        }

        /// <summary>
        /// Modify the application context's internal object factory after its
        /// standard initialization.
        /// </summary>
        /// <param name="factory">The object factory used by the application context.</param>
        /// <remarks>
        /// 	<p>
        /// All object definitions will have been loaded, but no objects will have
        /// been instantiated yet. This allows for overriding or adding properties
        /// even to eager-initializing objects.
        /// </p>
        /// </remarks>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of errors.
        /// </exception>
        public abstract void PostProcessObjectFactory(
            IConfigurableListableObjectFactory factory);

        /// <summary>
        /// Resolves the supplied <paramref name="value"/> into a
        /// <see cref="System.Type"/> instance.
        /// </summary>
        /// <param name="value">The object that is to be resolved into a <see cref="System.Type"/>
        /// instance.</param>
        /// <param name="errorContextSource">The error context source.</param>
        /// <param name="errorContext">The error context string.</param>
        /// <returns>A resolved <see cref="System.Type"/>.</returns>
        /// <remarks>
        /// 	<p>
        /// This (default) implementation supports resolving
        /// <see cref="System.String"/>s and <see cref="System.Type"/>s.
        /// Only override this method if you want to key your type alias
        /// on something other than <see cref="System.String"/>s
        /// and <see cref="System.Type"/>s.
        /// </p>
        /// </remarks>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If the supplied <paramref name="value"/> is <see langword="null"/>,
        /// or the supplied <paramref name="value"/> cannot be resolved.
        /// </exception>
        protected virtual Type ResolveRequiredType(object value, string errorContextSource, string errorContext)
        {
            Type requiredType = value as Type;
            if (requiredType == null)
            {
                string typeName = value as string;
                if (typeName != null)
                {
                    try
                    {
                        requiredType = TypeResolutionUtils.ResolveType(typeName);
                    }
                    catch (TypeLoadException ex)
                    {
                        throw new ObjectInitializationException(
                            string.Format(
                                "Could not load required type [{0}] for {1}.",
                                typeName, errorContext), ex);
                    }
                }
                else
                {
                    throw new ObjectInitializationException(
                        string.Format(
                            "Invalid value '{0}' for {1} - " +
                            "must be a System.String or System.Type.",
                            value, errorContext));
                }
            }
            return requiredType;
        }
    }
}
