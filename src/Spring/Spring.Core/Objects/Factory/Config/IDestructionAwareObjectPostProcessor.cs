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

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Subinterface of
	/// <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/> that adds
	/// a before-destruction callback.
	/// </summary>
	/// <remarks>
	/// The typical usage will be to invoke custom destruction callbacks on
	/// specific object types, matching corresponding initialization callbacks.
	/// </remarks>
	/// <author>Juergen Hoeller</author>
	/// <author>Simon White (.NET)</author>
	public interface IDestructionAwareObjectPostProcessor : IObjectPostProcessor
	{
		/// <summary>
		/// Apply this
		/// <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/> to the
		/// given new object instance before its destruction. Can invoke custom
		/// destruction callbacks.
		/// </summary>
		/// <param name="instance">The new object instance.</param>
		/// <param name="name">The name of the object.</param>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// In case of errors.
		/// </exception>
		void PostProcessBeforeDestruction (object instance, string name);
	}
}
