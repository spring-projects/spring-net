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

using System.Collections.Specialized;
using System.Reflection;
using System.Text;
using Spring.Util;

namespace Spring.Objects.Factory.Support
{
	/// <summary>
	/// Represents the replacement of a method on a managed object by the IoC
	/// container.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Note that this mechanism is <i>not</i> intended as a generic means of
	/// inserting crosscutting code: use AOP for that.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Rick Evans (.NET)</author>
    [Serializable]
    public sealed class ReplacedMethodOverride : MethodOverride
	{
		private readonly string methodReplacerObjectName;
		private StringCollection typeIdentifiers = new StringCollection();

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="ReplacedMethodOverride"/> class.
		/// </summary>
		/// <param name="methodName">
		/// The name of the method that is to be overridden.
		/// </param>
		/// <param name="methodReplacerObjectName">
		/// The object name of the <see cref="Spring.Objects.Factory.Support.IMethodReplacer"/>
		/// instance in the surrounding IoC container.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// If either of the supplied arguments is <see langword="null"/> or
		/// contains only whitespace character(s).
		/// </exception>
		public ReplacedMethodOverride(string methodName, string methodReplacerObjectName)
			: base(methodName)
		{
			AssertUtils.ArgumentHasText(methodReplacerObjectName, "methodReplacerObjectName");
			this.methodReplacerObjectName = methodReplacerObjectName;
		}

		/// <summary>
		/// Add a fragment of a <see cref="System.Type"/> instance's <see cref="System.Type.FullName"/>
		/// such as <c>'Exception</c> or <c>System.Excep</c> to identify an argument
		/// <see cref="System.Type"/> for a dependency injected method.
		/// </summary>
		/// <param name="identifier">
		/// A (sub) string of a <see cref="System.Type"/> instance's <see cref="System.Type.FullName"/>.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="identifier"/> is <see langword="null"/> or
		/// contains only whitespace character(s).
		/// </exception>
		/// <seeaso cref="Spring.Objects.Factory.Support.MethodOverride.Matches(MethodInfo)"/>
		public void AddTypeIdentifier(string identifier)
		{
			AssertUtils.ArgumentHasText(identifier, "identifier");
			this.typeIdentifiers.Add(identifier);
		}

		/// <summary>
		/// The object name of the <see cref="Spring.Objects.Factory.Support.IMethodReplacer"/>
		/// instance in the surrounding IoC container.
		/// </summary>
		public string MethodReplacerObjectName
		{
			get { return methodReplacerObjectName; }
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
			if (MethodName != method.Name && !(method.Name.EndsWith(MethodName)))
			{
				// can't ever match...
				return false;
			}
			if (!IsOverloaded)
			{
				// no need to worry about overloading...
				return true;
			}
			// if we get to here, we need to insist on precise argument matching...
			ParameterInfo[] parameters = method.GetParameters();
			if (this.typeIdentifiers.Count != parameters.Length)
			{
				return false;
			}
			for (int i = 0; i < typeIdentifiers.Count; ++i)
			{
				string identifier = this.typeIdentifiers[i];
				ParameterInfo parameter = parameters[i];
				if (parameter.ParameterType.FullName.IndexOf(identifier) == -1)
				{
					// this parameter cannot match, so neither can the whole override...
					return false;
				}
			}
			return true;
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
				.Append(MethodName).Append("'; will call object '").Append(this.methodReplacerObjectName)
				.Append("'.").ToString();
		}
	}
}
