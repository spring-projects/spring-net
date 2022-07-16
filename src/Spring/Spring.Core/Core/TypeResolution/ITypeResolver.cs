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

#endregion

namespace Spring.Core.TypeResolution
{
	/// <summary>
	/// Resolves a <see cref="System.Type"/> by name.
	/// </summary>
	/// <remarks>
	/// <p>
	/// The rationale behind the creation of this interface is to centralise
	/// the resolution of type names to <see cref="System.Type"/> instances
	/// beyond that offered by the plain vanilla
	/// <see cref="System.Type.GetType(string)"/> method call.
	/// </p>
	/// </remarks>
	/// <author>Rick Evans</author>
	public interface ITypeResolver
	{
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
		Type Resolve(string typeName);
	}
}
