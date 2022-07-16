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

using Spring.Util;

#endregion

namespace Spring.Aop.Support
{
	/// <summary>
	/// Simple <see cref="Spring.Aop.ITypeFilter"/> implementation that matches
	/// all classes classes (and any derived subclasses) of a give root
	/// <see cref="System.Type"/>.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
    [Serializable]
    public class RootTypeFilter : ITypeFilter
	{
		private Type _rootType;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="RootTypeFilter"/> for the supplied
		/// <paramref name="rootType"/>.
		/// </summary>
		/// <param name="rootType">The root <see cref="System.Type"/>.</param>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="rootType"/> is <see langword="null"/>.
		/// </exception>
		public RootTypeFilter(Type rootType)
		{
			AssertUtils.ArgumentNotNull(rootType, "rootType");
			_rootType = rootType;
		}

		/// <summary>
		/// Should the pointcut apply to the supplied
		/// <see cref="System.Type"/>?
		/// </summary>
		/// <remarks>
		/// <p>
		/// Returns <see langword="true"/> if the supplied <paramref name="type"/>
		/// can be assigned to the root <see cref="System.Type"/>.
		/// </p>
		/// </remarks>
		/// <param name="type">
		/// The candidate <see cref="System.Type"/>.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the advice should apply to the supplied
		/// <paramref name="type"/>
		/// </returns>
		public virtual bool Matches(Type type)
		{
			return _rootType.IsAssignableFrom(type);
		}
	}
}
