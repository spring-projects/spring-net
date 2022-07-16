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
	/// Returns a value that is an
	/// <see cref="Spring.Objects.Factory.IGenericObjectFactory"/> that
	/// returns an object from an
	/// <see cref="Spring.Objects.Factory.IObjectFactory"/>.
	/// </summary>
	/// <remarks>
	/// <p>
	/// The primary motivation of this class is to avoid having a client object
	/// directly calling the
	/// <see cref="Spring.Objects.Factory.IObjectFactory.GetObject(string)"/>
	/// method to get a prototype object out of an
	/// <see cref="Spring.Objects.Factory.IObjectFactory"/>, which would be a
	/// violation of the inversion of control principle. With the use of this
	/// class, the client object can be fed an
	/// <see cref="Spring.Objects.Factory.IObjectFactory"/> as a property
	/// that directly returns one target <b>prototype</b> object.
	/// </p>
	/// <p>
	/// The object referred to by the value of the
	/// <see cref="Spring.Objects.Factory.Config.ObjectFactoryCreatingFactoryObject.TargetObjectName"/>
	/// property does not have to be a prototype object, but there is little
	/// to no point in using this class in conjunction with a singleton object.
	/// </p>
	/// </remarks>
	/// <example>
	/// <p>
	/// The following XML configuration snippet illustrates the use of this
	/// class...
	/// </p>
	/// <code escaped="true">
	/// <objects>
	///   <!-- prototype object since we have state -->
	///   <object id="MyService" type="A.B.C.MyService" singleton="false"/>
	///
	///   <object id="MyServiceFactory"
	///		type="Spring.Objects.Factory.Config.ObjectFactoryCreatingFactoryObject">
	///     <property name="TargetObjectName"><idref local="MyService"/></property>
	///   </object>
	///
	///   <object id="MyClientObject" type="A.B.C.MyClientObject">
	///     <property name="MyServiceFactory" ref="MyServiceFactory"/>
	///   </object>
	/// </objects>
	/// </code>
	/// </example>
	/// <author>Colin Sampaleanu</author>
	/// <author>Simon White (.NET)</author>
    [Serializable]
    public class ObjectFactoryCreatingFactoryObject
		: AbstractFactoryObject, IObjectFactoryAware
	{
		private string _targetObjectName;
		private IObjectFactory _objectFactory;

		#region Properties

		/// <summary>
		/// Sets the name of the target object.
		/// </summary>
		public string TargetObjectName
		{
			set { _targetObjectName = value; }
		}

		/// <summary>
		/// The target factory that will be used to perform the lookup
		/// of the object referred to by the <see cref="TargetObjectName"/>
		/// property.
		/// </summary>
		/// <value>
		/// The owning <see cref="Spring.Objects.Factory.IObjectFactory"/>
		/// (will never be <see langword="null"/>).
		/// </value>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// In case of initialization errors.
		/// </exception>
		/// <seealso cref="Spring.Objects.Factory.IObjectFactoryAware.ObjectFactory"/>
		public IObjectFactory ObjectFactory
		{
			set { this._objectFactory = value; }
		}

		/// <summary>
		/// The <see cref="System.Type"/> of object created by this factory.
		/// </summary>
		public override Type ObjectType
		{
			get { return typeof (IGenericObjectFactory); }
		}

		#endregion

		/// <summary>
		/// Returns an instance of the object factory.
		/// </summary>
		/// <returns>The object factory.</returns>
		protected override object CreateInstance()
		{
			return new GenericObjectFactory(this);
		}

		/// <summary>
		/// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
		/// after it has set all supplied object properties.
		/// </summary>
		/// <exception cref="System.Exception">
		/// In the event of misconfiguration (such as failure to set an essential
		/// property) or if initialization fails.
		/// </exception>
		/// <seealso cref="Spring.Objects.Factory.Config.AbstractFactoryObject.AfterPropertiesSet"/>
		public override void AfterPropertiesSet()
		{
			if(StringUtils.IsNullOrEmpty(_targetObjectName))
			{
				throw new ArgumentException("The 'TargetObjectName' property must have a value.");
			}
			base.AfterPropertiesSet ();
		}

		private sealed class GenericObjectFactory : IGenericObjectFactory
		{
			private ObjectFactoryCreatingFactoryObject _enclosing;

			/// <summary>
			/// Creates a new instance of the GenericObjectFactory class.
			/// </summary>
			/// <param name="enclosing">
			/// The enclosing
			/// <see cref="Spring.Objects.Factory.Config.ObjectFactoryCreatingFactoryObject"/>.
			/// </param>
			public GenericObjectFactory(
				ObjectFactoryCreatingFactoryObject enclosing)
			{
				_enclosing = enclosing;
			}

			/// <summary>
			/// Returns the object created by the enclosed object factory.
			/// </summary>
			/// <returns>The created object.</returns>
			public object GetObject()
			{
				return _enclosing._objectFactory.GetObject(_enclosing._targetObjectName);
			}
		}
	}
}
