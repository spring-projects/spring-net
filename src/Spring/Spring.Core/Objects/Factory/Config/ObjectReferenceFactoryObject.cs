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

using Spring.Util;

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// An <see cref="Spring.Objects.Factory.IFactoryObject"/> implementation
	/// that exposes an arbitrary target object under a different name.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Usually, the target object will reside in a different object
	/// definition file, using this
	/// <see cref="Spring.Objects.Factory.IFactoryObject"/> to link it in
	/// and expose it under a different name. Effectively, this corresponds
	/// to an alias for the target object.
	/// </p>
	/// <note>
	/// For XML based object definition files, a <code>&lt;alias&gt;</code>
	/// tag is available that effectively achieves the same.
	/// </note>
	/// </remarks>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans (.NET)</author>
	/// <seealso cref="Spring.Objects.Factory.IFactoryObject"/>
    [Serializable]
    public sealed class ObjectReferenceFactoryObject
		: IFactoryObject, IObjectFactoryAware
	{
		private string _targetObjectName;
		private IObjectFactory _objectFactory;

        /// <summary>
        /// Initialize a new default instance
        /// </summary>
	    public ObjectReferenceFactoryObject()
	    {
	    }
        /// <summary>
        /// Initialize this instance with the predefined <paramref name="targetObjectName"/> and <paramref name="objectFactory"/>.
        /// </summary>
        /// <param name="targetObjectName"></param>
        /// <param name="objectFactory"></param>
	    public ObjectReferenceFactoryObject(string targetObjectName, IObjectFactory objectFactory)
	    {
	        this.TargetObjectName = targetObjectName;
	        this.ObjectFactory = objectFactory;
	    }

	    /// <summary>
		/// The name of the target object.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The target object may potentially be defined in a different object
		/// definition file.
		/// </p>
		/// </remarks>
		/// <value>The name of the target object.</value>
		public string TargetObjectName
		{
			set { _targetObjectName = value; }
		}

		/// <summary>
		/// Return an instance (possibly shared or independent) of the object
		/// managed by this factory.
		/// </summary>
		/// <returns>
		/// An instance (possibly shared or independent) of the object managed by
		/// this factory.
		/// </returns>
		/// <seealso cref="Spring.Objects.Factory.IFactoryObject.GetObject()"/>
		public object GetObject()
		{
			return _objectFactory.GetObject(_targetObjectName);
		}

		/// <summary>
		/// Return the type of object that this
		/// <see cref="Spring.Objects.Factory.IFactoryObject"/> creates, or
		/// <see langword="null"/> if not known in advance.
		/// </summary>
		/// <seealso cref="Spring.Objects.Factory.IFactoryObject.ObjectType"/>
		public Type ObjectType
		{
			get { return _objectFactory.GetType(_targetObjectName); }
		}

		/// <summary>
		/// Is the object managed by this factory a singleton or a prototype?
		/// </summary>
		/// <seealso cref="Spring.Objects.Factory.IFactoryObject.IsSingleton"/>
		public bool IsSingleton
		{
			get { return _objectFactory.IsSingleton(_targetObjectName); }
		}

		/// <summary>
		/// Callback that supplies the owning factory to an object instance.
		/// </summary>
		/// <value>
		/// The owning <see cref="Spring.Objects.Factory.IObjectFactory"/>
		/// (may not be <see langword="null"/>). The object can immediately
		/// call methods on the factory.
		/// </value>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// In case of initialization errors.
		/// </exception>
		/// <seealso cref="Spring.Objects.Factory.IObjectFactoryAware.ObjectFactory"/>
		public IObjectFactory ObjectFactory
		{
			set
			{
				// the call to set this property occurs 'after' all properties have been set...
				_objectFactory = value;
				if (StringUtils.IsNullOrEmpty(_targetObjectName))
				{
					throw new ArgumentException(
						"The 'TargetObjectName' property is required.");
				}
				if (!_objectFactory.ContainsObject(_targetObjectName))
				{
					throw new NoSuchObjectDefinitionException(
						_targetObjectName, _objectFactory.ToString());
				}
			}
		}
	}
}
