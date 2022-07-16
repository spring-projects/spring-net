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

using System.Reflection;
using Spring.Util;

namespace Spring.Reflection.Dynamic
{
    #region IDynamicConstructor interface

    /// <summary>
    /// Defines constructors that dynamic constructor class has to implement.
    /// </summary>
    public interface IDynamicConstructor
    {
        /// <summary>
        /// Invokes dynamic constructor.
        /// </summary>
        /// <param name="arguments">
        /// Constructor arguments.
        /// </param>
        /// <returns>
        /// A constructor value.
        /// </returns>
        object Invoke(object[] arguments);
    }

    #endregion

    #region Safe wrapper

    /// <summary>
    /// Safe wrapper for the dynamic constructor.
    /// </summary>
    /// <remarks>
    /// <see cref="SafeConstructor"/> will attempt to use dynamic
    /// constructor if possible, but it will fall back to standard
    /// reflection if necessary.
    /// </remarks>
    public class SafeConstructor : IDynamicConstructor
    {
        private ConstructorInfo constructorInfo;

        #region Generated Function Cache

        private static readonly IDictionary<ConstructorInfo, ConstructorDelegate> constructorCache = new Dictionary<ConstructorInfo, ConstructorDelegate>();

        /// <summary>
        /// Obtains cached constructor info or creates a new entry, if none is found.
        /// </summary>
        private static ConstructorDelegate GetOrCreateDynamicConstructor(ConstructorInfo constructorInfo)
        {
            ConstructorDelegate method;
            if (!constructorCache.TryGetValue(constructorInfo, out method))
            {
                method = DynamicReflectionManager.CreateConstructor(constructorInfo);
                lock (constructorCache)
                {
                    constructorCache[constructorInfo] = method;
                }
            }
            return method;
        }

        #endregion

        private ConstructorDelegate constructor;

        /// <summary>
        /// Creates a new instance of the safe constructor wrapper.
        /// </summary>
        /// <param name="constructorInfo">Constructor to wrap.</param>
        public SafeConstructor(ConstructorInfo constructorInfo)
        {
            this.constructorInfo = constructorInfo;
            this.constructor = GetOrCreateDynamicConstructor(constructorInfo);
        }


        /// <summary>
        /// Invokes dynamic constructor.
        /// </summary>
        /// <param name="arguments">
        /// Constructor arguments.
        /// </param>
        /// <returns>
        /// A constructor value.
        /// </returns>
        public object Invoke(object[] arguments)
        {
            return constructor(arguments);
        }
    }

    #endregion

    /// <summary>
    /// Factory class for dynamic constructors.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class DynamicConstructor : BaseDynamicMember
    {
        /// <summary>
        /// Creates dynamic constructor instance for the specified <see cref="ConstructorInfo"/>.
        /// </summary>
        /// <param name="constructorInfo">Constructor info to create dynamic constructor for.</param>
        /// <returns>Dynamic constructor for the specified <see cref="ConstructorInfo"/>.</returns>
        public static IDynamicConstructor Create(ConstructorInfo constructorInfo)
        {
            AssertUtils.ArgumentNotNull(constructorInfo, "You cannot create a dynamic constructor for a null value.");

            return new SafeConstructor(constructorInfo);
        }
    }
}
