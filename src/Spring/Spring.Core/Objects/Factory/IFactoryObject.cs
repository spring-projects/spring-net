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

namespace Spring.Objects.Factory
{
	/// <summary>
	/// Interface to be implemented by objects used within an
	/// <see cref="Spring.Objects.Factory.IObjectFactory"/> that are themselves
	/// factories.
	/// </summary>
	/// <remarks>
	/// <p>
	/// If an object implements this interface, it is used as a factory,
	/// not directly as an object. <see cref="Spring.Objects.Factory.IFactoryObject"/>s
	/// can support singletons and prototypes
	/// (<see cref="Spring.Objects.Factory.IFactoryObject.IsSingleton"/>)...
	/// please note that an <see cref="Spring.Objects.Factory.IFactoryObject"/>
	/// itself can only ever be a singleton. It is a logic error to configure an
	/// <see cref="Spring.Objects.Factory.IFactoryObject"/> itself to be a prototype.
	/// </p>
	/// <note type="caution">
	/// An object that implements this interface cannot be used as a normal object.
	/// </note>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans (.NET)</author>
	public interface IFactoryObject
	{
	    /// <summary>
	    /// Return an instance (possibly shared or independent) of the object
	    /// managed by this factory.
	    /// </summary>
	    /// <remarks>
	    /// <note type="caution">
	    /// If this method is being called in the context of an enclosing IoC container and
	    /// returns <see langword="null"/>, the IoC container will consider this factory
	    /// object as not being fully initialized and throw a corresponding (and most
	    /// probably fatal) exception.
	    /// </note>
	    /// </remarks>
	    /// <returns>
	    /// An instance (possibly shared or independent) of the object managed by
	    /// this factory.
	    /// </returns>
	    object GetObject();

	    /// <summary>
	    /// Return the <see cref="System.Type"/> of object that this
	    /// <see cref="Spring.Objects.Factory.IFactoryObject"/> creates, or
	    /// <see langword="null"/> if not known in advance.
	    /// </summary>
	    Type ObjectType { get; }

	    /// <summary>
	    /// Is the object managed by this factory a singleton or a prototype?
	    /// </summary>
	    bool IsSingleton { get; }
	}
}
