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
	/// Simple template superclass for <see cref="Spring.Objects.Factory.IFactoryObject"/>
	/// implementations that allows for the creation of a singleton or a prototype
	/// instance (depending on a flag).
	/// </summary>
	/// <remarks>
	/// If the value of the
	/// <see cref="Spring.Objects.Factory.IFactoryObject.IsSingleton"/>
	/// property is <see langword="true"/> (this is the default), this class
	/// will create a single instance of it's object upon initialization and
	/// subsequently return the singleton instance; else, this class will
	/// create a new instance each time (prototype mode). Subclasses must
	/// implement the <see langword="abstract"/>
	/// <see cref="Spring.Objects.Factory.Config.AbstractFactoryObject.CreateInstance"/>
	/// template method to actually create objects.
	/// </remarks>
	/// <author>Juergen Hoeller</author>
	/// <author>Keith Donald</author>
	/// <author>Simon White (.NET)</author>
    [Serializable]
    public abstract class AbstractFactoryObject : IFactoryObject, IInitializingObject, IDisposable
	{
		private bool isSingleton = true;
		private object singletonInstance;

		/// <summary>
		/// Is the object managed by this factory a singleton or a prototype?
		/// </summary>
		/// <remarks>
		/// <p>
		/// Please note that changing the value of this property <b>after</b>
		/// this factory object instance has been created by an enclosing
		/// Spring.NET IoC container really is a programming error. This
		/// property should really only be set once, prior to the invocation
		/// of the
		/// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
		/// callback method.
		/// </p>
		/// </remarks>
		/// <see cref="Spring.Objects.Factory.IFactoryObject.IsSingleton"/>
		public bool IsSingleton
		{
			get { return this.isSingleton; }
			set { this.isSingleton = value; }
		}

		/// <summary>
		/// Return the <see cref="System.Type"/> of object that this
		/// <see cref="Spring.Objects.Factory.IFactoryObject"/> creates, or
		/// <see langword="null"/> if not known in advance.
		/// </summary>
		/// <see cref="Spring.Objects.Factory.IFactoryObject.ObjectType"/>
		public abstract Type ObjectType { get; }

		/// <summary>
		/// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
		/// after it has injected all of an object's dependencies.
		/// </summary>
		/// <exception cref="System.Exception">
		/// In the event of misconfiguration (such as the failure to set a
		/// required property) or if initialization fails.
		/// </exception>
		/// <seealso cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
		public virtual void AfterPropertiesSet()
		{
			if (this.isSingleton && this.singletonInstance == null)
			{
				this.singletonInstance = CreateInstance();
			}
		}

		/// <summary>
		/// Return an instance (possibly shared or independent) of the object
		/// managed by this factory.
		/// </summary>
		/// <returns>
		/// An instance (possibly shared or independent) of the object managed by
		/// this factory.
		/// </returns>
		/// <seealso cref="Spring.Objects.Factory.IFactoryObject.GetObject"/>
		public object GetObject()
		{
			if (this.isSingleton)
			{
				return this.singletonInstance;
			}
			else
			{
				return CreateInstance();
			}
		}

		/// <summary>
		/// Template method that subclasses must override to construct
		/// the object returned by this factory.
		/// </summary>
		/// <remarks>
		/// Invoked once immediately after the initialization of this
		/// <see cref="Spring.Objects.Factory.IFactoryObject"/> in the case of
		/// a singleton; else, on each call to the
		/// <see cref="Spring.Objects.Factory.IFactoryObject.GetObject"/>
		/// method.
		/// </remarks>
		/// <exception cref="System.Exception">
		/// If an exception occured during object creation.
		/// </exception>
		/// <returns>
		/// A distinct instance of the object created by this factory.
		/// </returns>
		protected abstract object CreateInstance();

		/// <summary>
		/// Performs cleanup on any cached singleton object.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Only makes sense in the context of a singleton object.
		/// </p>
		/// </remarks>
		/// <see cref="Spring.Objects.Factory.IFactoryObject.IsSingleton"/>
		/// <seealso cref="System.IDisposable.Dispose"/>
		public virtual void Dispose()
		{
			if (this.isSingleton && this.singletonInstance != null)
			{
				IDisposable disposableSingletonInstance = this.singletonInstance as IDisposable;
				if (disposableSingletonInstance != null)
				{
					disposableSingletonInstance.Dispose();
				}
			}
		}
	}
}
