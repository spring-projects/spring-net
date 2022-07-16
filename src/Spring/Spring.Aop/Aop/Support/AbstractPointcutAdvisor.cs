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

using AopAlliance.Aop;
using Spring.Core;

namespace Spring.Aop.Support
{
    /// <summary>
    /// Abstract base class for <see cref="IPointcutAdvisor"/> implementations.
    /// </summary>
    /// <remarks>
    /// Can be subclassed for returning a specific pointcut/advice or a freely configurable pointcut/advice.
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    [Serializable]
    public abstract class AbstractPointcutAdvisor : IPointcutAdvisor, IOrdered
    {
        private int _order = Int32.MaxValue;

        /// <summary>
        /// Returns this <see cref="Spring.Aop.IAdvisor"/>s order in the
        /// interception chain.
        /// </summary>
        /// <returns>
        /// This <see cref="Spring.Aop.IAdvisor"/>s order in the
        /// interception chain.
        /// </returns>
        public virtual int Order
        {
            get { return this._order; }
            set { this._order = value; }
        }

        /// <summary>
        /// Return the advice part of this aspect.
        /// </summary>
        /// <remarks>
        /// <p>
        /// An advice may be an interceptor, a throws advice, before advice,
        /// introduction etc.
        /// </p>
        /// </remarks>
        /// <returns>
        /// The advice that should apply if the pointcut matches.
        /// </returns>
        public abstract IAdvice Advice { get; set; }

        /// <summary>
        /// Is this advice associated with a particular instance?
        /// </summary>
        /// <remarks>
        /// <p>
        /// Not supported for dynamic advisors.
        /// </p>
        /// </remarks>
        /// <value>
        /// <see langword="true"/> if this advice is associated with a
        /// particular instance.
        /// </value>
        /// <exception cref="System.NotSupportedException">Always.</exception>
        /// <see cref="Spring.Aop.IAdvisor.IsPerInstance"/>
        public virtual bool IsPerInstance
        {
            get
            {
                throw new NotSupportedException(
                    "The 'IsPerInstance' property of the IAdvisor interface " +
                        "is not yet supported in Spring.NET.");
            }
        }

        /// <summary>
        /// The <see cref="Spring.Aop.IPointcut"/> that drives this advisor.
        /// </summary>
        public abstract IPointcut Pointcut { get; set; }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/>
        /// is equal to the current <see cref="System.Object"/>.
        /// </summary>
        /// <param name="o">The advisor to compare with.</param>
        /// <returns>
        /// <see langword="true"/> if this instance is equal to the
        /// specified <see cref="System.Object"/>.
        /// </returns>
        public override bool Equals(object o)
        {
            if (!(o is AbstractPointcutAdvisor))
            {
                return false;
            }

            if (ReferenceEquals(this, o))
            {
                return true;
            }

            AbstractPointcutAdvisor otherAdvisor = (AbstractPointcutAdvisor)o;
            if (this.Order != otherAdvisor.Order)
            {
                return false;
            }
            if (otherAdvisor.Advice == null && otherAdvisor.Pointcut == null)
            {
                return (this.Advice == null && this.Pointcut == null);
            }
            else if (otherAdvisor.Advice == null)
            {
                return (Advice == null && otherAdvisor.Pointcut.Equals(this.Pointcut));
            }
            else if (otherAdvisor.Pointcut == null)
            {
                return (this.Pointcut == null && otherAdvisor.Advice.Equals(this.Advice));
            }
            else
            {
                return otherAdvisor.Advice.Equals(this.Advice) && otherAdvisor.Pointcut.Equals(this.Pointcut);
            }
        }

        /// <summary>
        /// Serves as a hash function for a particular type, suitable for use
        /// in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return 0 // (SPRNET-847) base.GetHashCode()
                + 13 * (Pointcut == null ? 0 : Pointcut.GetHashCode())
                + 27 * (Advice == null ? 0 : Advice.GetHashCode())
                + 31 * Order.GetHashCode();
        }
    }
}
