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

using System.Collections;
using System.Globalization;

using Spring.Util;

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Simple factory for shared <see cref="System.Collections.IList"/> instances.
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <author>Simon White (.NET)</author>
    [Serializable]
    public class ListFactoryObject : AbstractFactoryObject
	{
		private IList _sourceList;
		private Type _targetListType = typeof (ArrayList);

		#region Properties

		/// <summary>
		/// Set the source <see cref="System.Collections.IList"/>.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This value will be used to populate the <see cref="System.Collections.IList"/>
		/// returned by this factory.
		/// </p>
		/// </remarks>
		public IList SourceList
		{
			set { this._sourceList = value; }
		}

		/// <summary>
		/// Set the <see cref="System.Type"/> of the <see cref="System.Collections.IList"/>
		/// implementation to use.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The default is the <see cref="System.Collections.ArrayList"/> <see cref="System.Type"/>.
		/// </p>
		/// </remarks>
		public Type TargetListType
		{
			set
			{
				AssertUtils.ArgumentNotNull(value, "value");
				if (!typeof (IList).IsAssignableFrom(value))
				{
					throw new ArgumentException(
						string.Format(CultureInfo.InvariantCulture,
						              "The Type passed to the TargetListType property must implement the '{0}' interface.",
						              ObjectType.FullName));
				}
				if (value.IsInterface)
				{
					throw new ArgumentException(
						string.Format(CultureInfo.InvariantCulture,
						              "The Type passed to the TargetListType property cannot be an interface; it must be a concrete class that implements the '{0}' interface.",
						              ObjectType.FullName));
				}
				if (value.IsAbstract)
				{
					throw new ArgumentException(
						string.Format(CultureInfo.InvariantCulture,
						              "The Type passed to the TargetListType property cannot be abstract (MustInherit in VisualBasic.NET); it must be a concrete class that implements the '{0}' interface.",
						              ObjectType.FullName));
				}
				this._targetListType = value;
			}
		}

		/// <summary>
		/// The <see cref="System.Type"/> of objects created by this factory.
		/// </summary>
		/// <value>
		/// Always returns the <see cref="System.Collections.IList"/> <see cref="System.Type"/>.
		/// </value>
		public override Type ObjectType
		{
			get { return typeof (IList); }
		}

		#endregion

		/// <summary>
		/// Constructs a new instance of the target dictionary.
		/// </summary>
		/// <returns>The new <see cref="System.Collections.IList"/> instance.</returns>
		protected override object CreateInstance()
		{
			if (this._sourceList == null)
			{
				throw new ArgumentException("The 'SourceList' property cannot be null (Nothing in Visual Basic.NET).");
			}
			IList result = (IList) ObjectUtils.InstantiateType(this._targetListType);
			foreach (object obj in this._sourceList)
			{
				result.Add(obj);
			}
			return result;
		}
	}
}
