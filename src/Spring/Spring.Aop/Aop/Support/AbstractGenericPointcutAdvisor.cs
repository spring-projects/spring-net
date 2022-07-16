#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

using AopAlliance.Aop;

namespace Spring.Aop.Support
{
    /// <summary>
    /// Abstract PointcutAdvisor that allows for any Advice to be configured.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    [Serializable]
    public abstract class AbstractGenericPointcutAdvisor : AbstractPointcutAdvisor
    {
        private IAdvice advice;

        /// <summary>
        /// Return the advice part of this advisor.
        /// </summary>
        /// <returns>
        /// The advice that should apply if the pointcut matches.
        /// </returns>
        /// <see cref="Spring.Aop.IAdvisor.Advice"/>
        public override IAdvice Advice
        {
            get { return this.advice; }
            set { this.advice = value; }
        }

        ///<summary>
        /// 2 <see cref="AbstractGenericPointcutAdvisor"/>s are considered equals, if
        /// a) their pointcuts are equal
        /// b) their advices are equal
        ///</summary>
        public override bool Equals(object obj)
        {
            return base.Equals(obj as AbstractGenericPointcutAdvisor);
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
            return GetType().Name + ": advice=[" + Advice + "]";
        }
    }
}
