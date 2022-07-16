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

using System.Collections;
using System.Reflection;

namespace Spring.Objects.Factory.Support
{
	/// <summary>
	/// A collection (with set semantics) of method overrides, determining which, if any,
	/// methods on a managed object the Spring.NET IoC container will override at runtime.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Rick Evans</author>
    [Serializable]
    public class MethodOverrides : IEnumerable<MethodOverride>
	{
		private HashSet<MethodOverride> _overrides;

		private HashSet<string> _overloadedMethodNames;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.Support.MethodOverrides"/> class.
		/// </summary>
		public MethodOverrides()
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.Support.MethodOverrides"/> class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Deep copy constructoe.
		/// </p>
		/// </remarks>
		/// <param name="other">
		/// The instance supplying initial overrides for this new instance.
		/// </param>
		public MethodOverrides(MethodOverrides other)
		{
			AddAll(other);
		}

		/// <summary>
		/// Returns true if this instance contains no overrides.
		/// </summary>
		public bool IsEmpty => _overrides== null || _overrides.Count == 0;

		/// <summary>
		/// Copy all given method overrides into this object.
		/// </summary>
		/// <param name="other">
		/// The overrides to be copied into this object.
		/// </param>
		public void AddAll(MethodOverrides other)
		{
			if (other != null)
			{
				if (other._overrides != null && other._overrides.Count > 0)
				{
					_overrides = _overrides ?? new HashSet<MethodOverride>();
					foreach (var @override in other._overrides)
					{
						_overrides.Add(@override);
					}
				}

				if (other._overloadedMethodNames != null && other._overloadedMethodNames.Count > 0)
				{
					_overloadedMethodNames = _overloadedMethodNames ?? new HashSet<string>();
					foreach (var methodName in other._overloadedMethodNames)
					{
						_overloadedMethodNames.Add(methodName);
					}
				}
			}
		}

		/// <summary>
		/// Adds the supplied <paramref name="theOverride"/> to the overrides contained
		/// within this instance.
		/// </summary>
		/// <param name="theOverride">
		/// The <see cref="Spring.Objects.Factory.Support.MethodOverride"/> to be
		/// added.
		/// </param>
		public void Add(MethodOverride theOverride)
		{
			_overrides = _overrides ?? new HashSet<MethodOverride>();
			_overrides.Add(theOverride);
		}

		/// <summary>
		/// Adds the supplied <paramref name="methodName"/> to the overloaded method names
		/// contained within this instance.
		/// </summary>
		/// <param name="methodName">
		/// The overloaded method name to be added.
		/// </param>
		public void AddOverloadedMethodName(string methodName)
		{
			_overloadedMethodNames = _overloadedMethodNames ?? new HashSet<string>();
			_overloadedMethodNames.Add(methodName);
		}

		/// <summary>
		/// Returns true if the supplied <paramref name="methodName"/> is present within
		/// the overloaded method names contained within this instance.
		/// </summary>
		/// <param name="methodName">
		/// The overloaded method name to be checked.
		/// </param>
		/// <returns>
		/// True if the supplied <paramref name="methodName"/> is present within
		/// the overloaded method names contained within this instance.
		/// </returns>
		public bool IsOverloadedMethodName(string methodName)
		{
			return _overloadedMethodNames != null && _overloadedMethodNames.Contains(methodName);
		}

		/// <summary>
		/// Return the override for the given method, if any.
		/// </summary>
		/// <param name="method">
		/// The method to check for overrides for.
		/// </param>
		/// <returns>
		/// the override for the given method, if any.
		/// </returns>
		public MethodOverride GetOverride(MethodInfo method)
		{
			if (_overrides == null)
			{
				return null;
			}
			foreach (MethodOverride ovr in _overrides)
			{
				if (ovr.Matches(method))
				{
					return ovr;
				}
			}

			return null;
		}

		/// <inheritdoc />
		public IEnumerator<MethodOverride> GetEnumerator()
		{
			return _overrides?.GetEnumerator() ?? Enumerable.Empty<MethodOverride>().GetEnumerator();
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
