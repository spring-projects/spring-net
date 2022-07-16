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

namespace Spring.Aop.Target
{
	/// <summary>
	/// The <see cref="Spring.Aop.ITargetSource"/> to be used
	/// when there is no target object, and behavior is supplied by the
	/// advisors.
	/// </summary>
	/// <remarks>
	/// <p>
	/// This class is exposed as a singleton.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	[Serializable]
	public sealed class EmptyTargetSource : ITargetSource, ISerializable
	{
		/// <summary>
		/// The <see cref="Spring.Aop.ITargetSource"/> to be used
		/// when there is no target object, and behavior is supplied by the
		/// advisors.
		/// </summary>
		public static readonly ITargetSource Empty = new EmptyTargetSource();

		private object _dummyTarget = new object();

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="EmptyTargetSource"/> class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is a utility class, and as such has no publicly visible constructors.
		/// </p>
		/// </remarks>
		private EmptyTargetSource()
		{
		}

		/// <summary>
		/// The <see cref="System.Type"/> of the target object.
		/// </summary>
		public Type TargetType
		{
			get { return typeof(object); }
		}

		/// <summary>
		/// Is the target source static?
		/// </summary>
		/// <remarks>
		/// <p>
		/// The <see cref="Spring.Aop.Target.EmptyTargetSource.Empty"/>
		/// instance is static, and this always returns <see langword="true"/>.
		/// </p>
		/// </remarks>
		/// <value>
		/// <see langword="true"/> if the target source is static.
		/// </value>
		public bool IsStatic
		{
			get { return true; }
		}

		/// <summary>
		/// Returns the target object.
		/// </summary>
		/// <returns>The target object.</returns>
		/// <exception cref="System.Exception">
		/// If unable to obtain the target object.
		/// </exception>
		public object GetTarget()
		{
			return _dummyTarget;
		}

		/// <summary>
		/// Releases the target object.
		/// </summary>
		/// <remarks>
		/// <note type="implementnotes">
		/// This is a no-op operation in this implementation.
		/// </note>
		/// </remarks>
		/// <param name="target">The target object to release.</param>
		public void ReleaseTarget(object target)
		{
		}

		/// <summary>
		/// A <see cref="System.String"/> that represents the current
		/// <see cref="Spring.Aop.ITargetSource"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents the current
		/// <see cref="Spring.Aop.ITargetSource"/>.
		/// </returns>
		public override string ToString()
		{
			return "EmptyTargetSource: no target";
		}

		/// <inheritdoc />
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.SetType(typeof (EmptyTargetSourceObjectReference));
		}

		[Serializable]
		private sealed class EmptyTargetSourceObjectReference : IObjectReference
		{
			public object GetRealObject(StreamingContext context)
			{
				return EmptyTargetSource.Empty;
			}
		}
	}
}
