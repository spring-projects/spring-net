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

using System.Diagnostics;
using System.Reflection;

#endregion

namespace Spring.Core
{
	/// <summary>
	/// Factory class to conceal any default <see cref="Spring.Core.IControlFlow"/> implementation.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Simon White (.NET)</author>
	public abstract class ControlFlowFactory
	{
		/// <summary>
		/// Creates a new instance of the <see cref="Spring.Core.IControlFlow"/>
		/// implementation provided by this factory.
		/// </summary>
		/// <returns>
		/// A new instance of the <see cref="Spring.Core.IControlFlow"/>
		/// implementation provided by this factory.
		/// </returns>
		public static IControlFlow CreateControlFlow()
		{
			return new DefaultControlFlow();
		}

		private class DefaultControlFlow : IControlFlow
		{
			private StackTrace _stackTrace;

			#region Constructor (s) / Destructor

			/// <summary>
			/// Creates a new instance of the
			/// <see cref="Spring.Core.ControlFlowFactory.DefaultControlFlow"/> class.
			/// </summary>
			public DefaultControlFlow()
			{
				_stackTrace = new StackTrace();
			}

			#endregion

			#region IControlFlow Members

			/// <summary>
			/// Detects whether the caller is under the supplied <see cref="System.Type"/>,
			/// according to the current stacktrace.
			/// </summary>
			/// <seealso cref="Spring.Core.IControlFlow.Under(Type)"/>
			bool IControlFlow.Under(Type type)
			{
				return IsMatch(new MethodsDeclaredTypeCriteria(type));
			}

			/// <summary>
			/// Detects whether the caller is under the supplied <see cref="System.Type"/>
			/// and <paramref name="methodName"/>, according to the current stacktrace.
			/// </summary>
			/// <remarks>
			/// <p>
			/// Matches the whole method name.
			/// </p>
			/// </remarks>
			/// <seealso cref="Spring.Core.IControlFlow.Under(Type, string)"/>
			bool IControlFlow.Under(Type type, string methodName)
			{
				ComposedCriteria criteria = new ComposedCriteria();
				criteria.Add(new MethodsDeclaredTypeCriteria(type));
				criteria.Add(new RegularExpressionMethodNameCriteria(methodName));
				return IsMatch(criteria);
			}

			/// <summary>
			/// Does the current stack trace contain the supplied <paramref name="token"/>?
			/// </summary>
			/// <remarks>
			/// <p>
			/// This leaves it up to the caller to decide what matches, but is obviously less of
			/// an abstraction because the caller must know the exact format of the underlying
			/// stack trace.
			/// </p>
			/// </remarks>
			/// <seealso cref="Spring.Core.IControlFlow.UnderToken(string)"/>
			bool IControlFlow.UnderToken(string token)
			{
				return _stackTrace.ToString().IndexOf(token) != -1;
			}

			private bool IsMatch(ICriteria criteria)
			{
				for (int i = 0; i < _stackTrace.FrameCount; i++)
				{
					MethodBase method = _stackTrace.GetFrame(i).GetMethod();
					if(criteria.IsSatisfied(method))
					{
						return true;
					}
				}
				return false;
			}

			private sealed class MethodsDeclaredTypeCriteria : ICriteria
			{
				public MethodsDeclaredTypeCriteria(Type typeToMatch)
				{
					_typeToMatch = typeToMatch;
				}

				public bool IsSatisfied(object datum)
				{
					MethodBase method = datum as MethodBase;
					if(method != null && method.DeclaringType != null)
					{
						return method.DeclaringType.Equals(_typeToMatch);
					}
					return false;
				}

				private Type _typeToMatch;
			}

			#endregion
		}
	}
}
