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
using System.Runtime.Serialization;
using System.Security.Permissions;

#endregion

namespace Spring.Aop
{
	/// <summary>
	/// Canonical <see cref="Spring.Aop.IMethodMatcher"/> that matches
	/// <b>all</b> methods.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	[Serializable]
	public sealed class TrueMethodMatcher : IMethodMatcher, ISerializable
	{
		/// <summary>
		/// Canonical instance that matches <b>all</b> methods.
		/// </summary>
		/// <remarks>
		/// <p>
		/// It is not dynamic.
		/// </p>
		/// </remarks>
		public static readonly IMethodMatcher True = new TrueMethodMatcher();

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="TrueMethodMatcher"/> class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is a utility class, and as such has no publicly visible
		/// constructors.
		/// </p>
		/// </remarks>
		private TrueMethodMatcher()
		{
		}

		/// <summary>
		/// Is this <see cref="Spring.Aop.IMethodMatcher"/> dynamic?
		/// </summary>
		/// <value>
		/// <see langword="true"/> if this
		/// <see cref="Spring.Aop.IMethodMatcher"/> is dynamic.
		/// </value>
		/// <seealso cref="Spring.Aop.IMethodMatcher.IsRuntime"/>
		public bool IsRuntime
		{
			get { return false; }
		}

		/// <summary>
		/// Does the supplied <paramref name="method"/> satisfy this matcher?
		/// Perform static checking. If this returns false, or if the isRuntime() method
		/// returns false, no runtime check will be made.
		/// </summary>
		/// <param name="method">The candidate method.</param>
		/// <param name="targetType">
		/// The target class (may be <see langword="null"/>, in which case the
		/// candidate class must be taken to be the <paramref name="method"/>'s
		/// declaring class).
		/// </param>
		/// <returns>
		/// <see langword="true"/> if this this method matches statically.
		/// </returns>
		/// <seealso cref="Spring.Aop.IMethodMatcher.Matches(MethodInfo, Type)"/>
		public bool Matches(MethodInfo method, Type targetType)
		{
			return true;
		}

		/// <summary>
		/// Is there a runtime (dynamic) match for the supplied
		/// <paramref name="method"/>?
		/// </summary>
		/// <param name="method">The candidate method.</param>
		/// <param name="targetType">The target class.</param>
		/// <param name="args">The arguments to the method</param>
		/// <returns>
		/// <see langword="true"/> if there is a runtime match.</returns>
		/// <seealso cref="Spring.Aop.IMethodMatcher.Matches(MethodInfo, Type, object[])"/>
		public bool Matches(MethodInfo method, Type targetType, object[] args)
		{
			// should never be invoked if IsRuntime is false
			return true;
		}

		/// <summary>
		/// A <see cref="System.String"/> that represents the current
		/// <see cref="Spring.Aop.IMethodMatcher"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents the current
		/// <see cref="Spring.Aop.IMethodMatcher"/>.
		/// </returns>
		public override string ToString()
		{
			return "TrueMethodMatcher.True";
		}

		/// <inheritdoc />
		[SecurityPermission (SecurityAction.Demand,SerializationFormatter=true)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.SetType(typeof (TrueMethodMatcherObjectReference));
		}

		[Serializable]
		private sealed class TrueMethodMatcherObjectReference : IObjectReference
		{
			public object GetRealObject(StreamingContext context)
			{
				return TrueMethodMatcher.True;
			}
		}
	}
}
