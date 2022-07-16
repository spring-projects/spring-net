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

using System.ComponentModel;

using Spring.Core.TypeConversion;
using Spring.Util;

namespace Spring.Objects.Support
{
	/// <summary>
	/// Specialisation of the <see cref="MethodInvoker"/> class that tries
	/// to convert the given arguments for the actual target method via an
	/// appropriate <see cref="Spring.Objects.IObjectWrapper"/> implementation.
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans</author>
	/// <seealso cref="MethodInvoker"/>
	public class ArgumentConvertingMethodInvoker : MethodInvoker
	{
		#region Constructor (s) / Destructor

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Support.ArgumentConvertingMethodInvoker"/> class.
		/// </summary>
		public ArgumentConvertingMethodInvoker()
		{
		}

		#endregion

		#region Properties

		private ObjectWrapper Wrapper
		{
			get { return _wrapper; }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Prepare the specified method.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The method can be invoked any number of times afterwards.
		/// </p>
		/// </remarks>
		/// <exception cref="System.ArgumentException">
		/// If all required properties are not set.
		/// </exception>
		/// <exception cref="System.MissingMethodException">
		/// If the specified method could not be found.
		/// </exception>
		public override void Prepare()
		{
			base.Prepare();
			// try to convert the arguments for the chosen method
		    Type[] requiredTypes = ReflectionUtils.GetParameterTypes(GetPreparedMethod());
			object[] arguments = PreparedArguments;
			object[] convertedArguments = new object[arguments.Length];
			for (int i = 0; i < arguments.Length; ++i)
			{
				convertedArguments[i] = TypeConversionUtils.ConvertValueIfNecessary(requiredTypes[i], arguments[i], null);
			}
			PreparedArguments = convertedArguments;
		}

		/// <summary>
		/// Register the given custom <see cref="System.ComponentModel.TypeConverter"/>
		/// for all properties of the given <see cref="System.Type"/>.
		/// </summary>
		/// <param name="requiredType">
		/// The <see cref="System.Type"/> of property.
		/// </param>
		/// <param name="typeConverter">
		/// The <see cref="System.ComponentModel.TypeConverter"/> to register.
		/// </param>
		public virtual void RegisterCustomConverter(
			Type requiredType, TypeConverter typeConverter)
		{
			TypeConverterRegistry.RegisterConverter(requiredType, typeConverter);
		}

		#endregion

		#region Fields

		private readonly ObjectWrapper _wrapper = new ObjectWrapper();

		#endregion
	}
}
