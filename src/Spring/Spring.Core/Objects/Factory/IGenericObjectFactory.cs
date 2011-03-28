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

namespace Spring.Objects.Factory
{
	/// <summary>
	/// Interface defining a factory which can return an object instance
	/// (possibly shared or independent) when invoked. 
	/// </summary>
	/// <remarks>
	/// This interface is typically used to encapsulate a generic factory 
	/// which returns a new instance (prototype) on each invocation.
	/// It is similar to the <see cref="Spring.Objects.Factory.IFactoryObject"/>, but
	/// implementations of the aforementioned interface are normally meant to be defined
	/// as instances by the user in an <see cref="Spring.Objects.Factory.IObjectFactory"/>,
	/// while implementations of this class are normally meant to be fed as a property to
	/// other objects; as such, the
	/// <see cref="Spring.Objects.Factory.IGenericObjectFactory.GetObject"/> method
	/// has different exception handling behavior.
	/// </remarks>
	/// <author>Colin Sampaleanu</author>
	/// <author>Simon White (.NET)</author>
	public interface IGenericObjectFactory
	{
		/// <summary>
		/// Return an instance (possibly shared or independent)
		/// of the object managed by this factory.
		/// </summary>
		/// <returns>
		/// An instance of the object (should never be <see langword="null"/>).
		/// </returns>
		object GetObject();
	}
}
