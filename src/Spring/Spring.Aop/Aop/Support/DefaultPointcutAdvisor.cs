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

#endregion

namespace Spring.Aop.Support
{
	/// <summary>
	/// Convenient pointcut-driven advisor implementation.
	/// </summary>
	/// <remarks>
	/// <p>
	/// This is the most commonly used <see cref="Spring.Aop.IAdvisor"/> implementation.
	/// It can be used with any pointcut and advice type, except for introductions.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	[Serializable]
    public class DefaultPointcutAdvisor : AbstractGenericPointcutAdvisor
	{
		private IPointcut pointcut = TruePointcut.True;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Aop.Support.DefaultPointcutAdvisor"/> class.
		/// </summary>
		public DefaultPointcutAdvisor()
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Aop.Support.DefaultPointcutAdvisor"/>
		/// class for the supplied <paramref name="advice"/>,
		/// </summary>
		/// <param name="advice">
		/// The advice to use.
		/// </param>
		public DefaultPointcutAdvisor(IAdvice advice)
			: this(TruePointcut.True, advice)
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Aop.Support.DefaultPointcutAdvisor"/>
		/// class for the supplied <paramref name="advice"/> and
		/// <paramref name="pointcut"/>.
		/// </summary>
		/// <param name="advice">
		/// The advice to use.
		/// </param>
		/// <param name="pointcut">
		/// The pointcut to use.
		/// </param>
		public DefaultPointcutAdvisor(IPointcut pointcut, IAdvice advice)
		{
			this.pointcut = pointcut;
			Advice = advice;
		}

		/// <summary>
		/// The <see cref="Spring.Aop.IPointcut"/> that drives this advisor.
		/// </summary>
		public override IPointcut Pointcut
		{
			get { return pointcut; }
            set { pointcut = value;}
		}


        ///<summary>
        /// 2 <see cref="DefaultPointcutAdvisor"/>s are considered equal, if
        /// a) their pointcuts are equal
        /// b) their advices are equal
        ///</summary>
        public override bool Equals(object obj)
        {
            return base.Equals(obj as DefaultPointcutAdvisor);
        }

        /// <summary>
        /// Calculates a unique hashcode based on advice + pointcut
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

	    /// <summary>
		/// Returns a <see cref="System.String"/> that represents the current
		/// <see cref="System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> representation of this advisor.
		/// </returns>
		public override string ToString()
		{
			return GetType().Name + ": pointcut=[" + pointcut + "] advice=[" + Advice + "]";
		}
	}
}
