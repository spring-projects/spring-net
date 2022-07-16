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

using System.Globalization;
using Spring.Collections;
using Spring.Util;

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Simple factory object for shared <see cref="Spring.Collections.ISet"/> instances.
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <author>Simon White (.NET)</author>
    [Serializable]
    public class SetFactoryObject : AbstractFactoryObject
	{
		private ISet _sourceSet;
		private Type _targetSetType = typeof (HybridSet);

		/// <summary>
		/// Set the source <see cref="Spring.Collections.ISet"/>.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This value will be used to populate the <see cref="Spring.Collections.ISet"/>
		/// returned by this factory.
		/// </p>
		/// </remarks>
		public ISet SourceSet
		{
			set { this._sourceSet = value; }
		}

		/// <summary>
		/// Set the <see cref="System.Type"/> of the <see cref="Spring.Collections.ISet"/>
		/// implementation to use.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The default is the <see cref="Spring.Collections.HybridSet"/> <see cref="System.Type"/>.
		/// </p>
		/// </remarks>
		public Type TargetSetType
		{
			set
			{
				AssertUtils.ArgumentNotNull(value, "value");
				if (!typeof (ISet).IsAssignableFrom(value))
				{
					throw new ArgumentException(
						string.Format(CultureInfo.InvariantCulture,
						              "The Type passed to the TargetSetType property must implement the '{0}' interface.",
						              typeof (ISet).FullName));
				}
				if (value.IsInterface)
				{
					throw new ArgumentException(
						string.Format(CultureInfo.InvariantCulture,
						"The Type passed to the TargetSetType property cannot be an interface; it must be a concrete class that implements the '{0}' interface.",
						ObjectType.FullName));
				}
				if (value.IsAbstract)
				{
					throw new ArgumentException(
						string.Format(CultureInfo.InvariantCulture,
						"The Type passed to the TargetSetType property cannot be abstract (MustInherit in VisualBasic.NET); it must be a concrete class that implements the '{0}' interface.",
						ObjectType.FullName));
				}
				this._targetSetType = value;
			}
		}

		/// <summary>
		/// The <see cref="System.Type"/> of objects created by this factory.
		/// </summary>
		/// <value>
		/// Always returns the <see cref="Spring.Collections.ISet"/> <see cref="System.Type"/>.
		/// </value>
		public override Type ObjectType
		{
			get { return typeof (ISet); }
		}

		/// <summary>
		/// Constructs a new instance of the target set.
		/// </summary>
		/// <returns>The new <see cref="Spring.Collections.ISet"/> instance.</returns>
		protected override object CreateInstance()
		{
			if (this._sourceSet == null)
			{
				throw new ArgumentException("The 'SourceSet' property cannot be null (Nothing in Visual Basic.NET).");
			}
			Set result = (Set) ObjectUtils.InstantiateType(this._targetSetType);
			result.AddAll(this._sourceSet);
			return result;
		}
	}
}
