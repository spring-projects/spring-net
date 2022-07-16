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
	/// <see cref="Spring.Objects.Factory.IFactoryObject"/> implementation that
	/// evaluates a property path on a given target object.
	/// </summary>
	/// <remarks>
	/// <p>
	/// The target object can be specified directly or via an object name (see
	/// example below).
	/// </p>
	/// <p>
	/// Please note that the <see cref="Spring.Objects.Factory.Config.PropertyPathFactoryObject"/>
	/// is an <see cref="Spring.Objects.Factory.IFactoryObject"/> implementation, and as such has
	/// to comply with the contract of the <see cref="Spring.Objects.Factory.IFactoryObject"/>
	/// interface; more specifically, this means that the end result of the property lookup path
	/// evaluation cannot be <see lang="null"/> (<see cref="Spring.Objects.Factory.IFactoryObject"/>
	/// implementations are not permitted to return <see lang="null"/>). If the resut of a
	/// property lookup path evaluates to <see lang="null"/>, an exception will be thrown.
	/// </p>
	/// </remarks>
	/// <example>
	/// <code escaped="true">
	/// <!-- this is the target object -->
	/// <object id="foo" type="Whatever.MyClass, MyAssembly" singleton="false">
	///		<!-- a System.String typed property -->
	///		<property name="name" value="Chinua Achebe"/>
	/// </object>
	///
	/// <!--
	///		will result in "Chinua Achebe", which is the value of the 'name' property of the 'foo' object
	///	-->
	/// <object id="consumer"
	///			type="Spring.Objects.Factory.Config.PropertyPathFactoryObject, Spring.Core">
	///		<property name="targetObject" ref="foo"/>
	///		<property name="propertyPath" value="name"/>
	/// </object>
	///
	/// <!--
	///		will result in "Chinua Achebe", which is the value of the 'name' property of the 'foo' object
	///	-->
	/// <object id="foo.name"
	///			type="Spring.Objects.Factory.Config.PropertyPathFactoryObject, Spring.Core"/>
	///
	/// <!--
	///		will result in "Chinua Achebe", which is the value of the 'name' property of the 'foo' object
	/// -->
	/// <object id="consumer"
	///			type="Spring.Objects.Factory.Config.PropertyPathFactoryObject, Spring.Core">
	///		<property name="targetObjectName" value="foo"/>
	///		<property name="propertyPath" value="name"/>
	/// </object>
	///
	/// <!--
	///		will result in (the int value) '13', which is the value of the length property of the 'name'
	///		property of the 'foo' object.
	///
	///		in this case, the first 'part' of the id is taken to be the name of the target object ('foo');
	///		the remainder of the name is taken to be the property lookup path
	///	-->
	/// <object id="foo.name.length"
	///			type="Spring.Objects.Factory.Config.PropertyPathFactoryObject, Spring.Core"/>
	/// </code>
	/// </example>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans (.NET)</author>
    [Serializable]
    public class PropertyPathFactoryObject : IFactoryObject, IObjectNameAware, IObjectFactoryAware
	{
		private IObjectWrapper targetObjectWrapper;
		private string targetObjectName;
		private string propertyPath;
		private Type resultType;
		private string objectName;
		private IObjectFactory objectFactory;

		/// <summary>
		/// The target object that the property path lookup is to be applied to.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This would most likely be an inner object, but can of course be
		/// any object reference.
		/// </p>
		/// </remarks>
		/// <value>
		/// The target object that the property path lookup is to be applied to.
		/// </value>
		/// <seealso cref="Spring.Objects.Factory.Config.PropertyPathFactoryObject.TargetObjectName"/>
		public object TargetObject
		{
			set { this.targetObjectWrapper = new ObjectWrapper(value); }
		}

		/// <summary>
		/// The (object) name of the target object that the property path lookup
		/// is to be applied to.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Please note that any leading or trailing whitespace <b>will</b> be
		/// trimmed from this name prior to resolution. The implication of this is that
		/// one cannot use the <see cref="Spring.Objects.Factory.Config.PropertyPathFactoryObject"/>
		/// class in conjunction with object names that start or end with whitespace.
		/// </p>
		/// </remarks>
		/// <value>
		/// The (object) name of the target object that the property path lookup
		/// is to be applied to.
		/// </value>
		/// <seealso cref="Spring.Objects.Factory.Config.PropertyPathFactoryObject.TargetObject"/>
		public string TargetObjectName
		{
			set
			{
				if (value != null)
				{
					value = value.Trim();
				}
				this.targetObjectName = value;
			}
		}

		/// <summary>
		/// The property (lookup) path to be applied to the target object.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Please note that any leading or trailing whitespace <b>will</b> be
		/// trimmed from this path prior to resolution. Whitespace is not a valid
		/// identifier for property names (in part or whole) in CLS-based languages,
		/// so this is a not unreasonable action. Please also note that whitespace
		/// that is embedded within the property path will be left as-is (which may
		/// or may not result in an error being thrown, depending on the context of
		/// the whitespace).
		/// </p>
		/// </remarks>
		/// <example>
		/// <p>
		/// Examples of such property lookup paths can be seen below; note that
		/// property lookup paths can be nested to an arbitrary level.
		/// </p>
		/// <code escaped="true">
		/// name.length
		/// accountManager.account['the key'].name
		/// accounts[0].name
		/// </code>
		/// </example>
		/// <value>
		/// The property (lookup) path to be applied to the target object.
		/// </value>
		public string PropertyPath
		{
			set
			{
				if (value != null)
				{
					value = value.Trim();
				}
				this.propertyPath = value;
			}
		}

		/// <summary>
		/// The 'expected' <see cref="System.Type"/> of the result from evaluating the
		/// property path.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is not necessary for directly specified target objects, or
		/// singleton target objects, where the <see cref="System.Type"/> can
		/// be determined via reflection. Just specify this in case of a
		/// prototype target, provided that you need matching by type (for
		/// example, for autowiring).
		/// </p>
		/// <p>
		/// It is permissable to set the value of this property to
		/// <see lang="null"/> (which in any case is the default value).
		/// </p>
		/// </remarks>
		/// <value>
		/// The 'expected' <see cref="System.Type"/> of the result from evaluating the
		/// property path.
		/// </value>
		public Type ResultType
		{
			set { this.resultType = value; }
		}

		/// <summary>
		/// Return an instance (possibly shared or independent) of the object
		/// managed by this factory.
		/// </summary>
		/// <returns>
		/// An instance (possibly shared or independent) of the object managed by
		/// this factory.
		/// </returns>
		/// <see cref="Spring.Objects.Factory.IFactoryObject.GetObject()"/>
		public object GetObject()
		{
			IObjectWrapper target = this.targetObjectWrapper;
			if (target == null)
			{
				// fetch the prototype object object...
				target = new ObjectWrapper(this.objectFactory[this.targetObjectName]);
			}
			object value = target.GetPropertyValue(this.propertyPath);
			if (value == null)
			{
				throw new FatalObjectException("PropertyPathFactoryObject is not allowed to return null, " +
					"but property value for path '" + this.propertyPath + "' is null.");
			}
			return value;
		}

		/// <summary>
		/// Return the <see cref="System.Type"/> of object that this
		/// <see cref="Spring.Objects.Factory.IFactoryObject"/> creates, or
		/// <see langword="null"/> if not known in advance.
		/// </summary>
		/// <see cref="Spring.Objects.Factory.IFactoryObject.ObjectType"/>
		public Type ObjectType
		{
			get { return this.resultType; }
		}

		/// <summary>
		/// Is the object managed by this factory a singleton or a prototype?
		/// </summary>
		/// <see cref="Spring.Objects.Factory.IFactoryObject.IsSingleton"/>
		public bool IsSingleton
		{
			get { return false; }
		}

		/// <summary>
		/// Set the name of the object in the object factory that created this object.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The object name of this
		/// <see cref="Spring.Objects.Factory.Config.PropertyPathFactoryObject"/>
		/// will be interpreted as "objectName.property" pattern, if neither the
		/// <see cref="Spring.Objects.Factory.Config.PropertyPathFactoryObject.TargetObjectName"/>
		/// <see cref="Spring.Objects.Factory.Config.PropertyPathFactoryObject.TargetObject"/>
		/// have been supplied (set).
		/// </p>
		/// <p>
		/// This allows for concise object definitions with just an id or name.
		/// </p>
		/// </remarks>
		/// <value>
		/// The name of the object in the factory.
		/// </value>
		public string ObjectName
		{
			set { this.objectName = value; }
		}

		/// <summary>
		/// Callback that supplies the owning factory to an object instance.
		/// </summary>
		/// <value>
		/// Owning <see cref="IInitializingObject.AfterPropertiesSet"/>
		/// (may not be <see langword="null"/>). The object can immediately
		/// call methods on the factory.
		/// </value>
		/// <exception cref="IInitializingObject">
		/// In case of initialization errors.
		/// </exception>
		public IObjectFactory ObjectFactory
		{
			set
			{
				this.objectFactory = value;
				if (this.targetObjectWrapper != null && this.targetObjectName != null)
				{
					throw new ArgumentException("Only one of the TargetObjectName or TargetObject properties can be set, not both.");
				}
				if (this.targetObjectWrapper == null && this.targetObjectName == null)
				{
					if (this.propertyPath != null)
					{
						throw new ArgumentException(
							"Specify TargetObject or TargetObjectName property in combination with PropertyPath.");
					}
					// no other properties specified: check object name...
					string strippedObjectname = this.objectName.Trim();
					int dotIndex = strippedObjectname.IndexOf('.');
					if (dotIndex <= 0)
					{
						throw new ArgumentException(
							"Neither TargetObject nor TargetObjectName specified, and PropertyPathFactoryObject " +
								"object name '" + this.objectName + "' does not follow 'objectName.property' syntax.");
					}
					this.targetObjectName = strippedObjectname.Substring(0, dotIndex);
					this.propertyPath = strippedObjectname.Substring(dotIndex + 1);
				}
				else if (this.propertyPath == null)
				{
					throw new ArgumentException("The 'PropertyPath' property has not been set.");
				}
				if (this.targetObjectWrapper == null
					&& this.objectFactory.IsSingleton(this.targetObjectName))
				{
					// eagerly fetch singleton target object, and determine result type...
					this.targetObjectWrapper
						= new ObjectWrapper(this.objectFactory.GetObject(this.targetObjectName));
					this.resultType = this.targetObjectWrapper.GetPropertyType(this.propertyPath);
				}
			}
		}
	}
}
