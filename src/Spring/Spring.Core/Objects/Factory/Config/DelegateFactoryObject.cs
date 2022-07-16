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
	/// <see cref="Spring.Objects.Factory.IFactoryObject"/> implementation that
	/// creates delegates.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Supports the creation of <see cref="System.Delegate"/>s for both
	/// instance and <see langword="static"/> methods.
	/// </p>
	/// </remarks>
	/// <author>Rick Evans</author>
    [Serializable]
    public class DelegateFactoryObject : AbstractFactoryObject
	{
		/// <summary>
		/// Callback method called once all factory properties have been set.
		/// </summary>
		/// <exception cref="System.Exception">
		/// In the event of misconfiguration (such as failure to set an essential
		/// property) or if initialization fails.
		/// </exception>
		/// <seealso cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
		public override void AfterPropertiesSet()
		{
			if (DelegateType == null)
			{
				throw new ArgumentException(
					"The 'DelegateType' property is required.");
			}
			if (!typeof (Delegate).IsAssignableFrom(DelegateType))
			{
				throw new ArgumentException(
					"The 'DelegateType' property must (obviously) be a Type derived from [System.Delegate].");
			}
			if (TargetType == null && TargetObject == null)
			{
				throw new ArgumentException(
					"Exactly one of either the 'TargetType' or 'TargetObject' properties is required.");
			}
			if (TargetType != null && TargetObject != null)
			{
				throw new ArgumentException(
					"Exactly one of either the 'TargetType' or 'TargetObject' properties is required (not both).");
			}
			if (StringUtils.IsNullOrEmpty(MethodName))
			{
				throw new ArgumentException(
					"The 'MethodName' property is required.");
			}
			base.AfterPropertiesSet();
		}

		/// <summary>
		/// Creates the delegate.
		/// </summary>
		/// <exception cref="System.Exception">
		/// If an exception occured during object creation.
		/// </exception>
		/// <returns>The object returned by this factory.</returns>
		/// <seealso cref="Spring.Objects.Factory.Config.AbstractFactoryObject.CreateInstance()"/>
		protected override object CreateInstance()
		{
			Delegate instance = null;
			if (TargetType != null)
			{
				instance = Delegate.CreateDelegate(DelegateType, TargetType, MethodName);
			}
			else
			{
				instance = Delegate.CreateDelegate(DelegateType, TargetObject, MethodName);
			}
			return instance;
		}

		#region Properties

		/// <summary>
		/// The <see cref="System.Type"/> of <see cref="System.Delegate"/>
		/// created by this factory.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Returns the <see cref="System.Delegate"/> <see cref="System.Type"/>
		/// if accessed prior to the <see cref="AfterPropertiesSet"/> method
		/// being called.
		/// </p>
		/// </remarks>
		public override Type ObjectType
		{
			get
			{
				return (DelegateType != null)
				       	? DelegateType
				       	: typeof (Delegate);
			}
		}

		/// <summary>
		/// The <see cref="System.Type"/> of the <see cref="System.Delegate"/>
		/// created by this factory.
		/// </summary>
		public Type DelegateType
		{
			get { return _delegateType; }
			set { _delegateType = value; }
		}

		/// <summary>
		/// The name of the method that is to be invoked by the created
		/// delegate.
		/// </summary>
		public string MethodName
		{
			get { return _methodName; }
			set { _methodName = value; }
		}

		/// <summary>
		/// The target <see cref="System.Type"/> if the <see cref="MethodName"/>
		/// refers to a <see langword="static"/> method.
		/// </summary>
		public Type TargetType
		{
			get { return _targetType; }
			set { _targetType = value; }
		}

		/// <summary>
		/// The target object if the <see cref="MethodName"/>
		/// refers to an instance method.
		/// </summary>
		public object TargetObject
		{
			get { return _targetObject; }
			set { _targetObject = value; }
		}

		#endregion

		#region Fields

		private Type _delegateType;
		private string _methodName;
		private Type _targetType;
		private object _targetObject;

		#endregion
	}
}
