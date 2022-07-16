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

using System.Reflection;

#endregion

namespace Spring.Util
{
	/// <summary>
	/// Discovers the attributes of a <see cref="System.Delegate"/>
	/// <see cref="System.Type"/> and provides access to the
	/// <see cref="System.Delegate"/> <see cref="System.Type"/>s metadata.
	/// </summary>
	/// <author>Rick Evans</author>
	public sealed class DelegateInfo
	{
		#region Constants

		/// <summary>
		/// The method name associated with a delegate invocation.
		/// </summary>
		private const string InvocationMethod = "Invoke";

		#endregion

		#region Constructor (s) / Destructor

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Util.DelegateInfo"/> class.
		/// </summary>
		/// <param name="eventMeta">
		/// The event used to extract the delegate <see cref="System.Type"/>
		/// from.
		/// </param>
		/// <exception cref="System.NullReferenceException">
		/// if the supplied <paramref name="eventMeta"/> is
		/// <see langword="null"/>.
		/// </exception>
		public DelegateInfo(EventInfo eventMeta) : this(eventMeta.EventHandlerType)
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Util.DelegateInfo"/> class.
		/// </summary>
		/// <param name="type">
		/// The delegate <see cref="System.Type"/>.
		/// </param>
		/// <exception cref="System.ArgumentException">
		/// If the supplied <see cref="System.Type"/> is not a subclass of the
		/// <see cref="System.Delegate"/> class, or is <see langword="null"/>.
		/// </exception>
		public DelegateInfo(Type type)
		{
			DelegateType = type;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The <see cref="System.Type"/> of the delegate.
		/// </summary>
		private Type DelegateType
		{
			get { return _delegateType; }
			set
			{
				if (!DelegateInfo.IsDelegate(value))
				{
					throw new ArgumentException("Not a delegate Type");
				}
				_delegateType = value;
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Checks to see if the method encapsulated by the supplied method
		/// metadata is compatible with the method signature associated with
		/// this delegate type.
		/// </summary>
		/// <param name="method">The method to be checked.</param>
		/// <returns>
		/// <see langword="true"/> if the method signature is compatible with
		/// the signature of this delegate; <see langword="false"/> if not, or
		/// if the supplied <paramref name="method"/> parameter is
		/// <see langword="null"/>.
		/// </returns>
		public bool IsSignatureCompatible(MethodInfo method)
		{
			if (method != null &&
				method.ReturnType.Equals(GetReturnType()))
			{
				ParameterInfo[] methodParameters =
					method.GetParameters();
				Type[] delegateParameters = GetParameterTypes();
				if (methodParameters.Length == delegateParameters.Length)
				{
					for (int i = 0; i < methodParameters.Length; ++i)
					{
						if (!methodParameters[i].ParameterType.
							Equals(delegateParameters[i]))
						{
							return false;
						}
					}
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Gets the <see cref="System.Type"/>s of the parameters of the
		/// method signature associated with this delegate type.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This method will never return <see langword="null"/>; the returned
		/// <see cref="System.Type"/> array may be empty, but it most certainly
		/// will not be <see langword="null"/>.
		/// </p>
		/// </remarks>
		/// <returns>
		/// A <see cref="System.Type"/> array of the parameter
		/// <see cref="System.Type"/>s; or the <see cref="System.Type.EmptyTypes"/>
		/// array if the method signature has no parameters.
		/// </returns>
		public Type[] GetParameterTypes()
		{
			ParameterInfo[] parameters = GetMethod().GetParameters();
			if (parameters != null
				&& parameters.Length > 0)
			{
				Type[] types = new Type[parameters.Length];
				for (int i = 0; i < parameters.Length; ++i)
				{
					types[i] = parameters[i].ParameterType;
				}
				return types;
			}
			return Type.EmptyTypes;
		}

		/// <summary>
		/// Gets the return <see cref="System.Type"/> of the
		/// method signature associated with this delegate type.
		/// </summary>
		/// <returns>The return <see cref="System.Type"/>.</returns>
		public Type GetReturnType()
		{
			return GetMethod().ReturnType;
		}

		/// <summary>
		/// Gets the metadata about the method signature associated
		/// with this delegate type.
		/// </summary>
		/// <returns>
		/// The metadata about the method signature associated
		/// with this delegate type.
		/// </returns>
		public MethodInfo GetMethod()
		{
			return DelegateType.GetMethod(DelegateInfo.InvocationMethod);
		}

		/// <summary>
		/// Determines whether the supplied <see cref="System.Type"/>
		/// is a <see cref="System.Delegate"/> type.
		/// </summary>
		/// <param name="type">
		/// The <see cref="System.Type"/> to be checked.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the supplied <see cref="System.Type"/>
		/// is a <see cref="System.Delegate"/> <see cref="System.Type"/>;
		/// <see langword="false"/> if not or the supplied
		/// <paramref name="type"/> is <see langword="null"/>.
		/// </returns>
		public static bool IsDelegate(Type type)
		{
			return type == null ?
				false :
				type.IsSubclassOf(typeof (Delegate));
		}

		/// <summary>
		/// Checks if the signature of the supplied <paramref name="handlerMethod"/>
		/// is compatible with the signature expected by the supplied
		/// <paramref name="eventMeta"/>.
		/// </summary>
		/// <param name="eventMeta">The event to be checked against.</param>
		/// <param name="handlerMethod">
		/// The method signature to check for compatibility.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the signature of the supplied
		/// <paramref name="handlerMethod"/> is compatible with the signature
		/// expected by the supplied <paramref name="eventMeta"/>;
		/// <see langword="false"/> if not or either of the supplied
		/// parameters is <see langword="null"/>.
		/// </returns>
		/// <seealso cref="Spring.Util.DelegateInfo.IsSignatureCompatible(MethodInfo)"/>
		public static bool IsSignatureCompatible(
			EventInfo eventMeta, MethodInfo handlerMethod)
		{
			bool compatible = false;
			if (eventMeta != null
				&& DelegateInfo.IsDelegate(eventMeta.EventHandlerType))
			{
				compatible = new DelegateInfo(eventMeta.EventHandlerType)
					.IsSignatureCompatible(handlerMethod);
			}
			return compatible;
		}

		#endregion

		#region Fields

		private Type _delegateType;

		#endregion
	}
}
