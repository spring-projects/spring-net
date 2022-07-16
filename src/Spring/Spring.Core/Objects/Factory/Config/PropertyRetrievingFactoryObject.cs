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

using System.Reflection;
using System.Text;

using Spring.Core;
using Spring.Core.TypeResolution;
using Spring.Util;

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// <see cref="Spring.Objects.Factory.IFactoryObject"/> implementation that
	/// retrieves a <see lang="static"/> or non-static <b>public</b> property value.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Typically used for retrieving <b>public</b> property values.
	/// </p>
	/// </remarks>
	/// <author>Rick Evans (.NET)</author>
    [Serializable]
    public class PropertyRetrievingFactoryObject : AbstractFactoryObject, IInitializingObject
	{
		#region Constructor (s) / Destructor

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.Config.PropertyRetrievingFactoryObject"/> class.
		/// </summary>
		public PropertyRetrievingFactoryObject()
		{
			Arguments = new object[] {};
		}

		#endregion

		#region Properties

		/// <summary>
		/// The <see cref="System.Type.AssemblyQualifiedName"/> of the static property
		/// to be retrieved.
		/// </summary>
		public string StaticProperty
		{
			set
			{
				AssertUtils.ArgumentNotNull(value, "StaticProperty");
                TypeAssemblyHolder info = new TypeAssemblyHolder(value);
				string typeName = info.TypeName;
				int indexWherePropertyStarts = 0;
				do
				{
					try
					{
						indexWherePropertyStarts = typeName.LastIndexOf('.');

						#region Sanity Check

						if (indexWherePropertyStarts == -1
							|| indexWherePropertyStarts == typeName.Length)
						{
							throw new ArgumentException(
								"The value passed to the StaticProperty property must be a fully " +
									"qualified Type plus property name: " +
									"e.g. 'Example.MyExampleClass.MyProperty, MyAssembly'");
						}

						#endregion

						typeName = typeName.Substring(0, indexWherePropertyStarts);
						StringBuilder buffer = new StringBuilder(typeName);
						if (info.IsAssemblyQualified)
						{
                            buffer.Append(TypeAssemblyHolder.TypeAssemblySeparator);
							buffer.Append(info.AssemblyName);
						}
						TargetType = TypeResolutionUtils.ResolveType(buffer.ToString());
					}
					catch (TypeLoadException)
					{
					}
				} while (TargetType == null);
				TargetProperty = info.TypeName.Substring(indexWherePropertyStarts + 1);
			}
		}

		/// <summary>
		/// Arguments for the property invocation.
		/// </summary>
		/// <remarks>
		/// <p>
		/// If this property is not set, or the value passed to the setter invocation
		/// is a null or zero-length array, a property with no arguments is assumed.
		/// </p>
		/// </remarks>
		public object[] Arguments
		{
			get { return _arguments; }
			set
			{
				if (value != null)
				{
					this._arguments = value;
				}
			}
		}

		/// <summary>
		/// The name of the property the value of which is to be retrieved.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Refers to either a <see lang="static"/> property or a non-static property,
		/// depending on a target object being set.
		/// </p>
		/// </remarks>
		public string TargetProperty
		{
			get { return _targetProperty; }
			set { _targetProperty = value; }
		}

		/// <summary>
		/// The object instance on which the property is defined.
		/// </summary>
		public object TargetObject
		{
			get { return _targetObject; }
			set
			{
				_targetObject = value;
				_targetObjectWrapper = new ObjectWrapper(_targetObject);
			}
		}

		/// <summary>
		/// The <see cref="System.Type"/> on which the property is defined.
		/// </summary>
		public Type TargetType
		{
			get { return _targetType; }
			set { _targetType = value; }
		}

		/// <summary>
		/// Return the type of object that this
		/// <see cref="Spring.Objects.Factory.IFactoryObject"/> creates, or
		/// <see lang="null"/> if not known in advance.
		/// </summary>
		public override Type ObjectType
		{
			get { return (Property == null) ? null : Property.PropertyType; }
		}

		private PropertyInfo Property
		{
			get { return _property; }
			set { _property = value; }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Invoked by an <see cref="Spring.Objects.Factory.IObjectFactory"/>
		/// after it has set all object properties supplied
		/// (and satisfied <see cref="Spring.Objects.Factory.IObjectFactoryAware"/>
		/// and ApplicationContextAware).
		/// </summary>
		/// <exception cref="System.Exception">
		/// In the event of misconfiguration (such as failure to set an essential
		/// property) or if initialization fails.
		/// </exception>
		public override void AfterPropertiesSet()
		{
			if (TargetType == null
				&& TargetObject == null)
			{
				throw new ArgumentException("One of the TargetType or TargetObject properties must be set.");
			}
			if (TargetProperty == null)
			{
				throw new ArgumentException("The TargetProperty property is required.");
			}
			Type targetType = null;
			BindingFlags propertyFlags = BindingFlags.Public | BindingFlags.IgnoreCase;
			if (TargetObject == null)
			{
				// a static property...
				propertyFlags |= BindingFlags.Static;
				targetType = TargetType;
				if (TargetProperty.IndexOf(".") == -1)
				{
					Property = targetType.GetProperty(TargetProperty, propertyFlags);
				}
				else
				{
					// $�%#@! a nested static property... recurse to the end property
					string property = TargetProperty;
					int propertyIndex = property.IndexOf(".");
					string startProperty = property.Substring(0, propertyIndex);
					Property = targetType.GetProperty(startProperty, propertyFlags);
					TargetObject = Property.GetValue(null, new object[] {});
					TargetProperty = property.Substring(propertyIndex + 1);
					AfterPropertiesSet();
				}
			}
			else
			{
				// an instance property...
				propertyFlags |= BindingFlags.Instance;
				targetType = TargetObject.GetType();

				// using the object wrapper does nested property lookup
				Property = _targetObjectWrapper.GetPropertyInfo(TargetProperty);
			}
			if (Property == null)
			{
				throw new InvalidPropertyException(targetType, TargetProperty);
			}
			if (!Property.CanRead)
			{
				throw new NotWritablePropertyException(TargetProperty, targetType);
			}
			base.AfterPropertiesSet();
		}

		/// <summary>
		/// Template method that subclasses must override to construct the object
		/// returned by this factory.
		/// </summary>
		/// <exception cref="System.Exception">
		/// If an exception occured during object creation.
		/// </exception>
		/// <returns>The object returned by this factory.</returns>
		protected override object CreateInstance()
		{
			object instance = null;
			object target = null;
			if (TargetObject != null)
			{
				target = TargetObject;
			}
			try
			{
				if (Arguments.Length == 0 && target != null)
				{
					// using object wrapper supports nested property lookup...
					instance = _targetObjectWrapper.GetPropertyValue(_targetProperty);
				}
				else
				{
					instance = Property.GetValue(target, Arguments);
				}
			}
			catch (Exception ex)
			{
				throw new FatalObjectException("Error reading property value.", ex);
			}
			return instance;
		}

		#endregion

		#region Fields

		private string _targetProperty;
		private ObjectWrapper _targetObjectWrapper;
		private object _targetObject;
		private Type _targetType;
		private PropertyInfo _property;
		private object[] _arguments;

		#endregion
	}
}
