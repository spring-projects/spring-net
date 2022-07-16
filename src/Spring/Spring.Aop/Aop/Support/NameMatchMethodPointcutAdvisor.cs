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

using AopAlliance.Aop;

using Spring.Util;

#endregion

namespace Spring.Aop.Support
{
	/// <summary> 
	/// Convenient class for name-match method pointcuts that hold an Interceptor,
	/// making them an Advisor.
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	/// <author>Mark Pollack (.NET)</author>
    [Serializable]
    public class NameMatchMethodPointcutAdvisor : AbstractGenericPointcutAdvisor
	{

        private NameMatchMethodPointcut pointcut = new NameMatchMethodPointcut();

        #region Constructor(s)

        /// <summary>
		/// Creates a new instance of the
		/// <see cref="NameMatchMethodPointcutAdvisor"/> class.
		/// </summary>
		public NameMatchMethodPointcutAdvisor()
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="NameMatchMethodPointcutAdvisor"/> class
		/// for the supplied <paramref name="advice"/>.
		/// </summary>
		/// <param name="advice"></param>
		public NameMatchMethodPointcutAdvisor(IAdvice advice)
		{
			Advice = advice;
        }

        #endregion


        #region Properties 
        /// <summary>
        /// The <see cref="Spring.Aop.ITypeFilter"/> for this pointcut.
        /// </summary>
        /// <remarks>Default is </remarks>
        /// <value>
        /// The current <see cref="Spring.Aop.ITypeFilter"/>.
        /// </value>
        public ITypeFilter TypeFilter {
            set
            {
                pointcut.TypeFilter = value;
            }
        }

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
	    public string MappedName
	    {
            set { pointcut.MappedName = value; }
	    }

	    /// <summary>
	    /// Set the method names defining methods to match.
	    /// </summary>
	    /// <remarks>
	    /// <p>
	    /// Matching will be the union of all these; if any match, the pointcut matches.
	    /// </p>
	    /// </remarks>
	    public string[] MappedNames
	    {
	        set { pointcut.MappedNames = value; }
	    }

		/// <summary>
		/// The <see cref="Spring.Aop.IPointcut"/> that drives this advisor.
		/// </summary>
		public override IPointcut Pointcut
		{
            get { return pointcut; }
            set
            {
                AssertUtils.AssertArgumentType(value, "pointcut", typeof(NameMatchMethodPointcut),
                              "Pointcut most be compatible with type NameMatchMethodPointcut");
                pointcut = value as NameMatchMethodPointcut;
            }
        }

        #endregion
    }
}
