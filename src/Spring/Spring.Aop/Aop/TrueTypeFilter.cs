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

using System.Runtime.Serialization;
using System.Security.Permissions;

#endregion

namespace Spring.Aop
{
	/// <summary>
	/// Canonical <see cref="Spring.Aop.ITypeFilter"/> instances.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Only one canonical <see cref="Spring.Aop.ITypeFilter"/> instance is
	/// provided out of the box. The <see cref="TrueTypeFilter.True"/>
	/// <see cref="Spring.Aop.ITypeFilter"/> matches all classes.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	[Serializable]
	public sealed class TrueTypeFilter : ITypeFilter, ISerializable
	{
		/// <summary>
		/// Canonical <see cref="Spring.Aop.ITypeFilter"/> instance that
		/// matches all classes.
		/// </summary>
		public static readonly ITypeFilter True = new TrueTypeFilter();

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="TrueTypeFilter"/> class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is a utility class, and as such has no publicly visible
		/// constructors.
		/// </p>
		/// </remarks>
		private TrueTypeFilter()
		{
		}

		/// <summary>
		/// Should the pointcut apply to the supplied
		/// <see cref="System.Type"/>?
		/// </summary>
		/// <param name="type">
		/// The candidate <see cref="System.Type"/>.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the advice should apply to the supplied
		/// <paramref name="type"/>
		/// </returns>
		/// <seealso cref="Spring.Aop.ITypeFilter.Matches(Type)"/>
		public bool Matches(Type type)
		{
			return true;
		}

		/// <summary>
		/// A <see cref="System.String"/> that represents the current
		/// <see cref="Spring.Aop.ITypeFilter"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents the current
		/// <see cref="Spring.Aop.ITypeFilter"/>.
		/// </returns>
		public override string ToString()
		{
			return "TrueTypeFilter.True";
		}

		/// <inheritdoc />
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.SetType(typeof (TrueTypeFilterObjectReference));
		}

		[Serializable]
		private sealed class TrueTypeFilterObjectReference : IObjectReference
		{
			public object GetRealObject(StreamingContext context)
			{
				return TrueTypeFilter.True;
			}
		}
	}
}
