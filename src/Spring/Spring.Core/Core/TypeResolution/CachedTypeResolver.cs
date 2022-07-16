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

#region Imports

using System.Collections;
using System.Collections.Specialized;

using Spring.Util;

#endregion

namespace Spring.Core.TypeResolution
{
	/// <summary>
	/// Resolves (instantiates) a <see cref="System.Type"/> by it's (possibly
	/// assembly qualified) name, and caches the <see cref="System.Type"/>
	/// instance against the type name.
	/// </summary>
	/// <author>Rick Evans</author>
	/// <author>Bruno Baia</author>
    /// <author>Erich Eichinger</author>
	public class CachedTypeResolver : ITypeResolver
	{
		/// <summary>
		/// The cache, mapping type names (<see cref="System.String"/> instances) against
		/// <see cref="System.Type"/> instances.
		/// </summary>
		private IDictionary typeCache = new HybridDictionary();

		private ITypeResolver typeResolver;

		/// <summary>
		/// Creates a new instance of the <see cref="Spring.Core.TypeResolution.CachedTypeResolver"/> class.
		/// </summary>
		/// <param name="typeResolver">
		/// The <see cref="Spring.Core.TypeResolution.ITypeResolver"/> that this instance will delegate
		/// actual <see cref="System.Type"/> resolution to if a <see cref="System.Type"/>
		/// cannot be found in this instance's <see cref="System.Type"/> cache.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="typeResolver"/> is <see langword="null"/>.
		/// </exception>
		public CachedTypeResolver(ITypeResolver typeResolver)
		{
			AssertUtils.ArgumentNotNull(typeResolver, "typeResolver");
			this.typeResolver = typeResolver;
		}

		/// <summary>
		/// Resolves the supplied <paramref name="typeName"/> to a
		/// <see cref="System.Type"/>
		/// instance.
		/// </summary>
		/// <param name="typeName">
		/// The (possibly partially assembly qualified) name of a
		/// <see cref="System.Type"/>.
		/// </param>
		/// <returns>
		/// A resolved <see cref="System.Type"/> instance.
		/// </returns>
		/// <exception cref="System.TypeLoadException">
		/// If the supplied <paramref name="typeName"/> could not be resolved
		/// to a <see cref="System.Type"/>.
		/// </exception>
		public Type Resolve(string typeName)
		{
			if (StringUtils.IsNullOrEmpty(typeName))
			{
				throw BuildTypeLoadException(typeName);
			}
			Type type = null;
			try
			{
                lock (this.typeCache.SyncRoot)
                {
                    type = this.typeCache[typeName] as Type;
                    if (type == null)
                    {
                        type = this.typeResolver.Resolve(typeName);
                        this.typeCache[typeName] = type;
                    }
                }
			}
			catch (Exception ex)
			{
				if (ex is TypeLoadException)
				{
					throw;
				}
				throw BuildTypeLoadException(typeName, ex);
			}
			return type;
		}

		private static TypeLoadException BuildTypeLoadException(string typeName)
		{
			return new TypeLoadException("Could not load type from string value '" + typeName + "'.");
		}

        private static TypeLoadException BuildTypeLoadException(string typeName, Exception ex)
        {
            return new TypeLoadException("Could not load type from string value '" + typeName + "'.", ex);
        }
	}
}
