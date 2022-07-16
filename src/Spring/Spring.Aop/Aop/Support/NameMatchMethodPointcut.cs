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
using Spring.Util;

#endregion

namespace Spring.Aop.Support
{
	/// <summary>
	/// Pointcut object for simple method name matches, useful as an alternative to pure
	/// regular expression based patterns.
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <author>Aleksandar Seovic (.NET)</author>
    [Serializable]
    public class NameMatchMethodPointcut : StaticMethodMatcherPointcut
	{
		private string[] _mappedNames = new string[0];

		/// <summary>
		/// Convenience property when we have only a single method name
		/// to match.
		/// </summary>
		/// <remarks>
		/// <note type="caution">
		/// Use either this property or the
		/// <see cref="Spring.Aop.Support.NameMatchMethodPointcut.MappedNames"/> property,
		/// not both.
		/// </note>
		/// </remarks>
		public virtual string MappedName
		{
			set { MappedNames = new string[] {value}; }
		}

		/// <summary>
		/// Set the method names defining methods to match.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Matching will be the union of all these; if any match, the pointcut matches.
		/// </p>
		/// </remarks>
		public virtual string[] MappedNames
		{
			set { this._mappedNames = value; }
		}

		/// <summary>
		/// Does the <see cref="System.Reflection.MemberInfo.Name"/> of the supplied
		/// <paramref name="method"/> matches any of the mapped names?
		/// </summary>
		/// <param name="method">
		/// The <see cref="System.Reflection.MethodBase"/> to check.
		/// </param>
		/// <param name="targetType">
		/// The <see cref="System.Type"/> of the target class.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the name of the supplied
		/// <paramref name="method"/> matches one of the mapped names.
		/// </returns>
		public override bool Matches(MethodInfo method, Type targetType)
		{
			for (int i = 0; i < this._mappedNames.Length; i++)
			{
				string mappedName = this._mappedNames[i];
				if (mappedName.Equals(method.Name) || IsMatch(method.Name, mappedName))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
        /// Does the supplied <paramref name="methodName"/> match the supplied <paramref name="mappedName"/>?
		/// </summary>
		/// <remarks>
		/// <p>
		/// The default implementation checks for "xxx*", "*xxx" and "*xxx*" matches,
	    /// as well as direct equality. Can be overridden in subclasses.
		/// </p>
		/// </remarks>
		/// <param name="methodName">
		/// The method name of the class.
		/// </param>
		/// <param name="mappedName">
		/// The name in the descriptor.
		/// </param>
		/// <returns>
		/// <b>True</b> if the names match.
		/// </returns>
		protected virtual bool IsMatch(string methodName, string mappedName)
		{
            return PatternMatchUtils.SimpleMatch(mappedName, methodName);
		}
	}
}
