#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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

using System;
using Spring.Util;

#endregion

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Holder for a typed <see cref="System.String"/> value.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Can be added to object definitions to explicitly specify
	/// a target type for a <see cref="System.String"/> value,
	/// for example for collection
	/// elements.
	/// </p>
	/// <p>
	/// This holder just stores the <see cref="System.String"/> value and the target
	/// <see cref="System.Type"/>. The actual conversion will be performed by
	/// the surrounding object factory.
	/// </p>
	/// </remarks>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans (.NET)</author>
	[Serializable]
	public class TypedStringValue
	{
		#region Constructor (s) / Destructor

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.Config.TypedStringValue"/>
		/// class.
		/// </summary>
		public TypedStringValue()
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="TypedStringValue"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public TypedStringValue(string value)
        {
            Value = value;
        }

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.Config.TypedStringValue"/>
		/// class.
		/// </summary>
		/// <param name="value">
		/// The value that is to be converted.
		/// </param>
		/// <param name="targetType">
		/// The <see cref="System.Type"/> to convert to.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="targetType"/> is
		/// <see langword="null"/>.
		/// </exception>
		public TypedStringValue(string value, Type targetType)
		{
			Value = value;
			TargetType = targetType;
		}

		#endregion

		/// <summary>
		/// The value that is to be converted. 
		/// </summary>
		/// <remarks>
		/// <p>
		/// Obviously if the
		/// <see cref="Spring.Objects.Factory.Config.TypedStringValue.TargetType"/>
		/// is the <see cref="System.String"/> <see cref="System.Type"/>, no conversion
		/// will actually be performed.
		/// </p>
		/// </remarks>
		public string Value
		{
		    get { return theValue; }
		    set { this.theValue = value; }
		}

	    /// <summary>
	    /// The <see cref="System.Type"/> to convert to.
	    /// </summary>
	    /// <exception cref="System.ArgumentNullException">
	    /// If the setter is supplied with a <see langword="null"/> value.
	    /// </exception>
	    public Type TargetType
	    {
	        get { return targetType; }
	        set
	        {
	            AssertUtils.ArgumentNotNull(value, "TargetType");
	            targetType = value;
	        }
	    }

        /// <summary>
        /// Gets a value indicating whether this instance has target type.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has target type; otherwise, <c>false</c>.
        /// </value>
	    public bool HasTargetType
	    {
	        get { return targetType != null; }
	    }

	    private string theValue;
	    private Type targetType;
	}

}