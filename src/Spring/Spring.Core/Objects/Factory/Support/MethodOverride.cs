#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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
using System.Reflection;
using Spring.Util;

#endregion

namespace Spring.Objects.Factory.Support
{
	/// <summary>
	/// Represents the override of a method on a managed object by the IoC container.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Note that the override mechanism is <i>not</i> intended as a generic means of
	/// inserting crosscutting code: use AOP for that.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Rick Evans (.NET)</author>
    [Serializable]
    public abstract class MethodOverride
	{
		private readonly string methodName;
		private bool isOverloaded = true;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.Support.MethodOverride"/> class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is an <see langword="abstract"/> class, and as such exposes no
		/// public constructors.
		/// </p>
		/// </remarks>
		/// <param name="methodName">
		/// The name of the method that is to be overridden.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="methodName"/> is <see langword="null"/> or
		/// contains only whitespace character(s).
		/// </exception>
		protected MethodOverride(string methodName)
		{
			AssertUtils.ArgumentHasText(methodName, "methodName");
			this.methodName = methodName.Trim();
		}

		/// <summary>
		/// The name of the method that is to be overridden.
		/// </summary>
		public string MethodName
		{
			get { return methodName; }
		}

		/// <summary>
		/// Is the method that is ot be injected
		/// (<see cref="Spring.Objects.Factory.Support.MethodOverride.MethodName"/>)
		/// to be considered as overloaded?
		/// </summary>
		/// <remarks>
		/// <p>
		/// If <see lang="true"/> (the default), then argument type matching
		/// will be performed (because one would not want to override the wrong
		/// method).
		/// </p>
		/// <p>
		/// Setting the value of this property to <see lang="false"/> can be used
		/// to optimize runtime performance (ever so slightly).
		/// </p>
		/// </remarks>
		public bool IsOverloaded
		{
			get { return isOverloaded; }
			set { isOverloaded = value; }
		}

		/// <summary>
		/// Does this <see cref="Spring.Objects.Factory.Support.MethodOverride"/>
		/// match the supplied <paramref name="method"/>?
		/// </summary>
		/// <remarks>
		/// <p>
		/// By 'match' one means does this particular
		/// <see cref="Spring.Objects.Factory.Support.MethodOverride"/>
		/// instance apply to the supplied <paramref name="method"/>?
		/// </p>
		/// <p>
		/// This allows for argument list checking as well as method name checking.
		/// </p>
		/// </remarks>
		/// <param name="method">The method to be checked.</param>
		/// <returns>
		/// <see lang="true"/> if this override matches the supplied
		/// <paramref name="method"/>.
		/// </returns>
		public abstract bool Matches(MethodInfo method);
	}
}
