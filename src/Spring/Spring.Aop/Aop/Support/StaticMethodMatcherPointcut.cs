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

namespace Spring.Aop.Support
{
	/// <summary>
	/// Convenient superclass when one wants to force subclasses to
	/// implement the <see cref="Spring.Aop.IMethodMatcher"/> interface
	/// but subclasses will still want to be pointcuts.
	/// </summary>
	/// <remarks>
	/// <p>
	/// The <see cref="Spring.Aop.Support.StaticMethodMatcherPointcut.TypeFilter"/>
	/// property can be overriden to customize <see cref="System.Type"/> filter
	/// behavior as well.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	/// <author>Mark Pollack (.NET)</author>
	[Serializable]
	public abstract class StaticMethodMatcherPointcut : StaticMethodMatcher, IPointcut
	{
	    private ITypeFilter typeFilter = TrueTypeFilter.True;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Aop.Support.AbstractRegularExpressionMethodPointcut"/>
		/// class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is an abstract class, and as such has no publicly
		/// visible constructors.
		/// </p>
		/// </remarks>
		protected StaticMethodMatcherPointcut()
		{
		}

		/// <summary>
		/// The <see cref="Spring.Aop.ITypeFilter"/> for this pointcut.
		/// </summary>
		/// <value>
		/// The current <see cref="Spring.Aop.ITypeFilter"/>.
		/// </value>
		public virtual ITypeFilter TypeFilter
		{
            get { return typeFilter; }
            set { typeFilter = value;}
		}

		/// <summary>
		/// The <see cref="Spring.Aop.IMethodMatcher"/> for this pointcut.
		/// </summary>
		/// <value>
		/// The current <see cref="Spring.Aop.IMethodMatcher"/>.
		/// </value>
		public virtual IMethodMatcher MethodMatcher
		{
			get { return this; }
		}
	}
}
