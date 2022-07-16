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

using System.Reflection;

using Spring.Util;

namespace Spring.Objects.Factory.Support
{
	/// <summary>
	/// An <see cref="Spring.Objects.Factory.Support.IMethodReplacer"/>
	/// implementation that provides some convenience support for
	/// derived classes.
	/// </summary>
	/// <remarks>
	/// <p>
	/// This class is reserved for internal use within the framework; it is
	/// not intended to be used by application developers using Spring.NET.
	/// </p>
	/// </remarks>
	/// <author>Rick Evans</author>
	public abstract class AbstractMethodReplacer : IMethodReplacer
	{
		private IConfigurableObjectDefinition objectDefinition;
		private IObjectFactory objectFactory;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="ArgumentNullException"/>
		/// class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is an <see lang="abstract"/> class, and as such has no
		/// publicly visible constructors.
		/// </p>
		/// </remarks>
		/// <param name="objectDefinition">
		/// The object definition that is the target of the method replacement.
		/// </param>
		/// <param name="objectFactory">
		/// The enclosing IoC container with which the above
		/// <paramref name="objectDefinition"/> is associated.
		/// </param>
		/// <exception cref="AbstractMethodReplacer">
		/// If either of the supplied arguments is <see langword="null"/>.
		/// </exception>
		protected AbstractMethodReplacer(
            IConfigurableObjectDefinition objectDefinition, IObjectFactory objectFactory)
		{
			AssertUtils.ArgumentNotNull(objectDefinition, "objectDefinition");
			AssertUtils.ArgumentNotNull(objectFactory, "objectFactory");
			this.objectDefinition = objectDefinition;
			this.objectFactory = objectFactory;
		}

		/// <summary>
		/// Is <see lang="abstract"/>; derived classes must supply an implementation.
		/// </summary>
		/// <param name="target">
		/// The instance whose <paramref name="method"/> is to be
		/// (re)implemented.
		/// </param>
		/// <param name="method">
		/// The method that is to be (re)implemented.
		/// </param>
		/// <param name="arguments">The target method's arguments.</param>
		/// <returns>The result of the object lookup.</returns>
		public abstract object Implement(object target, MethodInfo method, object[] arguments);

		/// <summary>
		/// Helper method for subclasses to retrieve the appropriate
		/// <see cref="Spring.Objects.Factory.Support.MethodOverride"/> for the
		/// supplied <paramref name="method"/>.
		/// </summary>
		/// <param name="method">
		/// The <see cref="System.Reflection.MethodInfo"/> to use to retrieve
		/// the appropriate
		/// <see cref="Spring.Objects.Factory.Support.MethodOverride"/>.
		/// </param>
		/// <returns>
		/// The appropriate
		/// <see cref="Spring.Objects.Factory.Support.MethodOverride"/>.
		/// </returns>
		protected MethodOverride GetOverride(MethodInfo method)
		{
			return this.objectDefinition.MethodOverrides.GetOverride(method);
		}

		/// <summary>
		/// Helper method for subclasses to lookup an object from an enclosing
		/// IoC container.
		/// </summary>
		/// <param name="objectName">
		/// The name of the object that is to be looked up.
		/// </param>
		/// <returns>
		/// The named object.
		/// </returns>
		protected object GetObject(string objectName)
		{
			return this.objectFactory.GetObject(objectName);
		}
	}
}
