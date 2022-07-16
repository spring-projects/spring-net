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

using System.Reflection;
using System.Text;
using Spring.Util;

namespace Spring.Objects.Factory.Support
{
	/// <summary>
	/// Represents an override of a method that looks up an object in the same IoC context.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Methods eligible for lookup override must not have arguments.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Rick Evans (.NET)</author>
    [Serializable]
    public sealed class LookupMethodOverride : MethodOverride
	{
		private readonly string objectName;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.Support.LookupMethodOverride"/> class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Methods eligible for lookup override must not have arguments.
		/// </p>
		/// </remarks>
		/// <param name="methodName">
		/// The name of the method that is to be overridden.
		/// </param>
		/// <param name="objectName">
		/// The name of the object in the current IoC context that the
		/// dependency injected method must return.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// If either of the supplied arguments is <see langword="null"/> or
		/// contains only whitespace character(s).
		/// </exception>
		public LookupMethodOverride(string methodName, string objectName)
			: base(methodName)
		{
			AssertUtils.ArgumentHasText(objectName, "objectName");
			this.objectName = objectName;
		}

		/// <summary>
		/// The name of the object in the current IoC context that the
		/// dependency injected method must return.
		/// </summary>
		public string ObjectName
		{
			get { return objectName; }
		}

		/// <summary>
		/// Does this <see cref="Spring.Objects.Factory.Support.MethodOverride"/>
		/// match the supplied <paramref name="method"/>?
		/// </summary>
		/// <param name="method">The method to be checked.</param>
		/// <returns>
		/// <see lang="true"/> if this override matches the supplied <paramref name="method"/>.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="method"/> is <see langword="null"/>.
		/// </exception>
		public override bool Matches(MethodInfo method)
		{
			AssertUtils.ArgumentNotNull(method, "method");
			return MethodName == method.Name || method.Name.EndsWith(MethodName);
		}

		/// <summary>
		/// A <see cref="System.String"/> that represents the current
		/// <see cref="System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents the current
		/// <see cref="System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			return new StringBuilder(GetType().Name).Append(" for method '")
				.Append(MethodName).Append("'; will return object '").Append(this.objectName)
				.Append("'.").ToString();
		}
	}
}
