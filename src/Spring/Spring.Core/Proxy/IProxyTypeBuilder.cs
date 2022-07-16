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

using System.Collections;

#endregion

namespace Spring.Proxy
{
	/// <summary>
	/// Describes the operations for a generic proxy type builder that can be
	/// used to create a proxy type for any class.
	/// </summary>
	/// <author>Aleksandar Seovic</author>
	public interface IProxyTypeBuilder
	{
		/// <summary>
        /// Creates the proxy type.
        /// </summary>
        /// <returns>The generated proxy class.</returns>
        Type BuildProxyType();

		/// <summary>
		/// The name of the proxy <see cref="System.Type"/>.
		/// </summary>
		/// <value>The name of the proxy <see cref="IProxyTypeBuilder.TargetType"/>.</value>
		string Name { get; set; }

		/// <summary>
		/// The <see cref="System.Type"/> of the target object.
		/// </summary>
		Type TargetType { get; set; }

		/// <summary>
		/// The <see cref="object"/> of the class that the proxy must
		/// inherit from.
		/// </summary>
		Type BaseType { get; set; }

        /// <summary>
        /// Gets or sets the list of interfaces proxy should implement.
        /// </summary>
        IList<Type> Interfaces { get; set; }

        /// <summary>
        /// Should we proxy target attributes?
        /// </summary>
        /// <remarks>
        /// <see langword="True"/> by default.
        /// Target type attributes, method attributes, method's return type attributes
        /// and method's parameter attributes are copied to the proxy.
        /// </remarks>
        bool ProxyTargetAttributes { get; set; }

		/// <summary>
		/// The list of custom <see cref="System.Attribute"/>s that the proxy
		/// class must be decorated with.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Note that the list is composed of instances of the actual
		/// <see cref="System.Attribute"/>s that are to be applied, not the
		/// <see cref="Attribute"/>s of the <see cref="Type"/>s.
		/// </p>
		/// </remarks>
		/// <example>
		/// <p>
		/// The following code snippets show examples of how to decorate the
		/// the proxied class with one or more <see cref="System.Attribute"/>s.
		/// </p>
		/// <code language="C#">
		/// // get a concrete implementation of an IProxyTypeBuilder...
		/// IProxyTypeBuilder builder = ... ;
		/// builder.TargetType = typeof( ... );
		///
		/// IDictionary typeAtts = new Hashtable();
        /// builder.TypeAttributes = typeAtts;
		///
		/// // applies a single Attribute to the proxied class...
		/// typeAtts = new Attribute[] { new MyCustomAttribute() });
		///
		/// // applies a number of Attributes to the proxied class...
		/// typeAtts = new Attribute[]
		///     {
		///			new MyCustomAttribute(),
		///			new AnotherAttribute(),
		///     });
		/// </code>
		/// </example>
        IList TypeAttributes { get; set; }

		/// <summary>
		/// The custom <see cref="System.Attribute"/>s that the proxy
		/// members must be decorated with.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This dictionary must use simple <see cref="System.String"/>s for keys
		/// (denoting the member names that the attributes are to be applied to),
		/// with the corresponding values being
		/// <see cref="System.Collections.IList"/>s.
		/// </p>
		/// <p>
		/// The key may be wildcarded using the <c>'*'</c> character... if so,
		/// then those proxy members that match against the key will be
		/// decorated with the attendant list of
		/// <see cref="System.Attribute"/>s. This naturally implies that using
		/// the <c>'*'</c> character as a key will result in the attendant list
		/// of <see cref="System.Attribute"/>s being applied to every member of
		/// the proxied class.
		/// </p>
		/// </remarks>
		/// <example>
		/// <p>
		/// The following code snippets show examples of how to decorate the
		/// members of a proxied class with one or more
		/// <see cref="System.Attribute"/>s.
		/// </p>
		/// <code language="C#">
		/// // get a concrete implementation of an IProxyTypeBuilder...
		/// IProxyTypeBuilder builder = ... ;
		/// builder.TargetType = typeof( ... );
		///
		/// IDictionary memAtts = new Hashtable();
		/// builder.MemberAttributes = memAtts;
		///
		/// // applies a single Attribute to all members of the proxied class...
		/// memAtts ["*"] = new Attribute[] { new MyCustomAttribute() });
		///
		/// // applies a number of Attributes to all members of the proxied class...
		/// memAtts ["*"] = new Attribute[]
		///     {
		///			new MyCustomAttribute(),
		///			new AnotherAttribute(),
		///     });
		///
		/// // applies a single Attribute to those members of the proxied class
		/// //     that have identifiers starting with 'Do' ...
		/// memAtts ["Do*"] = new Attribute[] { new MyCustomAttribute() });
		///
		/// // applies a number of Attributes to those members of the proxied class
		/// //     that have identifiers starting with 'Do' ...
		/// memAtts ["Do*"] = new Attribute[]
		///     {
		///			new MyCustomAttribute(),
		///			new AnotherAttribute(),
		///     });
		/// </code>
		/// </example>
		IDictionary MemberAttributes { get; set; }
	}
}
