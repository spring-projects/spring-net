#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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
using System.Runtime.Serialization;
using System.Security.Permissions;
using AopAlliance.Aop;
using Spring.Core;
using Spring.Objects;
using Spring.Util;

#endregion

namespace Spring.Aop.Support
{
	/// <summary>
	/// Abstract base regular expression pointcut object.
	/// </summary>
	/// <remarks>
	/// <p>
	/// The regular expressions must be a match. For example, the
	/// <code>.*Get.*</code> pattern will match <c>Com.Mycom.Foo.GetBar()</c>, and
	/// <code>Get.*</code> will not.
	/// </p>
	/// <p>
	/// This base class is serializable. Subclasses should decorate all
	/// fields with the <see cref="System.NonSerializedAttribute"/> - the
	/// <see cref="AbstractRegularExpressionMethodPointcut.InitPatternRepresentation"/>
	/// method in this class will be invoked again on the client side on deserialization.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Juergen Hoeller</author>
	/// <author>Simon White (.NET)</author>
	[Serializable]
	public abstract class AbstractRegularExpressionMethodPointcut
		: StaticMethodMatcherPointcut, ITypeFilter, ISerializable
	{
		[NonSerialized]
		private object[] _patterns = ObjectUtils.EmptyObjects;

		#region Constructors

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="AbstractRegularExpressionMethodPointcut"/>
		/// class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is an abstract class, and as such has no publicly
		/// visible constructors.
		/// </p>
		/// </remarks>
		protected AbstractRegularExpressionMethodPointcut()
		{
		}

		/// <summary>
		/// Creates a new instance of the <see cref="AbstractRegularExpressionMethodPointcut"/>
		/// class.
		/// </summary>
		/// <param name="info">
		/// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
		/// that holds the serialized object data about the exception being thrown.
		/// </param>
		/// <param name="context">
		/// The <see cref="System.Runtime.Serialization.StreamingContext"/>
		/// that contains contextual information about the source or destination.
		/// </param>
		/// <exception cref="AopAlliance.Aop.AspectException">
		/// If an error was encountered during the deserialization process.
		/// </exception>
		protected AbstractRegularExpressionMethodPointcut(
			SerializationInfo info, StreamingContext context)
		{
			_patterns = (object[]) info.GetValue("Patterns", typeof(object[]));
            if (_patterns == null)
            {
                _patterns = ObjectUtils.EmptyObjects;
            }
        }

	    /// <summary>
	    /// Overridden to ensure proper initialization
	    /// </summary>
	    protected override void OnDeserialization(object sender)
        {
            base.OnDeserialization(sender);
            try
            {
                InitPatternRepresentation(_patterns);
            }
            catch (Exception ex)
            {
                throw new AspectException(
                    "Failed to deserialize AOP regular expression pointcut: " + ex.Message);
            }
        }

		#endregion

		#region Properties

		/// <summary>
		/// The <see cref="Spring.Aop.ITypeFilter"/> for this pointcut.
		/// </summary>
		/// <value>
		/// The current <see cref="Spring.Aop.ITypeFilter"/>.
		/// </value>
		public override ITypeFilter TypeFilter
		{
			get { return this; }
		}

		/// <summary>
		/// Convenience property for setting a single pattern.
		/// </summary>
		/// <remarks>
		/// Use this property or Patterns, not both.
		/// </remarks>
		public virtual object Pattern
		{
			get { return (_patterns.Length > 0 ? _patterns[0] : null); }
			set
			{
			    AssertUtils.ArgumentNotNull(value, "Pattern");
			    this.Patterns = new object[] {value};
			}
		}

		/// <summary>
		/// The regular expressions defining methods to match.
		/// </summary>
		/// <remarks>
		/// Matching will be the union of all these; if any match,
		/// the pointcut matches.
		/// </remarks>
		public virtual object[] Patterns
		{
			get { return _patterns; }
			set
			{
                AssertUtils.ArgumentNotNull(value, "Patterns");
                this._patterns = value;
				InitPatternRepresentation(this.Patterns);
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Populates a <see cref="System.Runtime.Serialization.SerializationInfo"/> with
		/// the data needed to serialize the target object.
		/// </summary>
		/// <param name="info">
		/// The <see cref="System.Runtime.Serialization.SerializationInfo"/> to populate
		/// with data.
		/// </param>
		/// <param name="context">
		/// The destination (see <see cref="System.Runtime.Serialization.StreamingContext"/>)
		/// for this serialization.
		/// </param>
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Patterns", _patterns);
		}

		/// <summary>
		/// Subclasses must implement this to initialize regular expression pointcuts.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Can be invoked multiple times.
		/// </p>
		/// <p>
		/// This method will be invoked from the <see cref="Patterns"/> property,
		/// and also on deserialization.
		/// </p>
		/// </remarks>
		/// <param name="patterns">
		/// The patterns to initialize.
		/// </param>
		/// <exception cref="System.ArgumentException">
		/// In the case of an invalid pattern.
		/// </exception>
		protected abstract void InitPatternRepresentation(object[] patterns);

		/// <summary>
		/// Does the pattern at the supplied <paramref name="patternIndex"/>
		/// match this <paramref name="pattern"/>?
		/// </summary>
		/// <param name="pattern">The pattern to match</param>
		/// <param name="patternIndex">The index of pattern.</param>
		/// <returns>
		/// <see langword="true"/> if there is a match.
		/// </returns>
		protected abstract bool Matches(string pattern, int patternIndex);

		/// <summary>
		/// Does the supplied <paramref name="method"/> satisfy this matcher?
		/// </summary>
		/// <remarks>
		/// <p>
		/// Try to match the regular expression against the fully qualified name
		/// of the method's declaring <see cref="System.Type"/>, plus the name of
		/// the supplied <paramref name="method"/>.
		/// </p>
		/// <p>
		/// Note that the declaring <see cref="System.Type"/> is that
		/// <see cref="System.Type"/> that originally declared
		/// the method, not necessarily the <see cref="System.Type"/> that is
		/// currently exposing it. For example, <see cref="System.Object.Equals(object)"/>
		/// matches any subclass of <see cref="System.Object"/>'s
		/// <see cref="System.Object.Equals(object)"/> method.
		/// </p>
		/// </remarks>
		/// <param name="method">The candidate method.</param>
		/// <param name="targetType">
		/// The target <see cref="System.Type"/> (may be <see langword="null"/>,
		/// in which case the candidate <see cref="System.Type"/> must be taken
		/// to be the <paramref name="method"/>'s declaring class).
		/// </param>
		/// <returns>
		/// <see langword="true"/> if this this method matches statically.
		/// </returns>
		public override bool Matches(MethodInfo method, Type targetType)
		{
			string patt = method.DeclaringType.FullName + "." + method.Name;
			for (int i = 0; i < this.Patterns.Length; ++i)
			{
				bool matched = Matches(patt, i);
				if (matched)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Should the pointcut apply to the supplied
		/// <see cref="System.Type"/>?
		/// </summary>
		/// <remarks>
		/// <p>
		/// In this instance, simply returns <see langword="true"/>.
		/// </p>
		/// </remarks>
		/// <param name="type">
		/// The candidate <see cref="System.Type"/>.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the advice should apply to the supplied
		/// <paramref name="type"/>
		/// </returns>
		public bool Matches(Type type)
		{
			return true;
		}

		#endregion
	}
}