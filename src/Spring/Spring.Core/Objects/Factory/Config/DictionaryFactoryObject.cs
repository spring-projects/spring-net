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
	/// Simple factory for shared <see cref="System.Collections.IDictionary"/> instances.
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <author>Simon White (.NET)</author>
    [Serializable]
    public class DictionaryFactoryObject : AbstractFactoryObject
	{
		private IDictionary _sourceDictionary;
		private Type _targetDictionaryType = typeof (Hashtable);

		/// <summary>
		/// Set the source <see cref="System.Collections.IDictionary"/>.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This value will be used to populate the <see cref="System.Collections.IDictionary"/>
		/// returned by this factory.
		/// </p>
		/// </remarks>
		public IDictionary SourceDictionary
		{
			set { this._sourceDictionary = value; }
		}

		/// <summary>
		/// Set the <see cref="System.Type"/> of the <see cref="System.Collections.IDictionary"/>
		/// implementation to use.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The default is the <see cref="System.Collections.Hashtable"/> <see cref="System.Type"/>.
		/// </p>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">
		/// If the <c>value</c> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// If the <c>value</c> is an <see langword="abstract"/> <see cref="System.Type"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// If the <c>value</c> is an interface.
		/// </exception>
		public Type TargetDictionaryType
		{
			set
			{
				AssertUtils.ArgumentNotNull(value, "value");
				if (!typeof (IDictionary).IsAssignableFrom(value))
				{
					throw new ArgumentException(
						string.Format(CultureInfo.InvariantCulture,
						              "The Type passed to the TargetDictionaryType property must implement the '{0}' interface.",
						              ObjectType.FullName));
				}
				if (value.IsInterface)
				{
					throw new ArgumentException(
						string.Format(CultureInfo.InvariantCulture,
						"The Type passed to the TargetDictionaryType property cannot be an interface; it must be a concrete class that implements the '{0}' interface.",
						ObjectType.FullName));
				}
				if (value.IsAbstract)
				{
					throw new ArgumentException(
						string.Format(CultureInfo.InvariantCulture,
						"The Type passed to the TargetDictionaryType property cannot be abstract (MustInherit in VisualBasic.NET); it must be a concrete class that implements the '{0}' interface.",
						ObjectType.FullName));
				}
				this._targetDictionaryType = value;
			}
		}

		/// <summary>
		/// The <see cref="System.Type"/> of objects created by this factory.
		/// </summary>
		/// <value>
		/// Always returns the <see cref="System.Collections.IDictionary"/> <see cref="System.Type"/>.
		/// </value>
		public override Type ObjectType
		{
			get { return typeof (IDictionary); }
		}

		/// <summary>
		/// Constructs a new instance of the target dictionary.
		/// </summary>
		/// <returns>The new <see cref="System.Collections.IDictionary"/> instance.</returns>
		protected override object CreateInstance()
		{
			if (this._sourceDictionary == null)
			{
				throw new ArgumentException("The 'SourceDictionary' property cannot be null (Nothing in Visual Basic.NET).");
			}
			IDictionary result = (IDictionary) ObjectUtils.InstantiateType(this._targetDictionaryType);
			foreach (DictionaryEntry de in _sourceDictionary)
			{
				result[de.Key] = de.Value;
			}
			return result;
		}
	}
}
