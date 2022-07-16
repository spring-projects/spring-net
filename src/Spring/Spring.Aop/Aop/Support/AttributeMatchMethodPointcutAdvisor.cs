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

using System.Runtime.Serialization;

using AopAlliance.Aop;
using Spring.Core;

#endregion

namespace Spring.Aop.Support
{
	/// <summary>
	/// Convenient class for attribute-match method pointcuts that hold an Interceptor,
	/// making them an Advisor.
	/// </summary>
    /// <author>Bruno Baia</author>
    [Serializable]
    public class AttributeMatchMethodPointcutAdvisor
		: AttributeMatchMethodPointcut, IPointcutAdvisor, IOrdered, ISerializable
	{
		private int _order = Int32.MaxValue;
		private IAdvice _advice;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="AttributeMatchMethodPointcutAdvisor"/> class.
		/// </summary>
		public AttributeMatchMethodPointcutAdvisor()
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="AttributeMatchMethodPointcutAdvisor"/> class
		/// for the supplied <paramref name="advice"/>.
		/// </summary>
		/// <param name="advice">the advice to apply if the pointcut matches</param>
		public AttributeMatchMethodPointcutAdvisor(IAdvice advice)
		{
			this._advice = advice;
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="AttributeMatchMethodPointcutAdvisor"/> class
		/// for the supplied <paramref name="advice"/>.
		/// </summary>
        /// <param name="attribute">
        /// The <see cref="System.Attribute"/> to match.
        /// </param>
        /// <param name="inherit">
        /// Flag that controls whether or not the inheritance tree of the
        /// method to be included in the search for the <see cref="Attribute"/>?
        /// </param>
        /// <param name="advice">the advice to apply if the pointcut matches</param>
        public AttributeMatchMethodPointcutAdvisor(Type attribute, bool inherit, IAdvice advice)
            :base(attribute, inherit)
		{
			this._advice = advice;
		}

	    /// <inheritdoc />
	    private AttributeMatchMethodPointcutAdvisor(SerializationInfo info, StreamingContext context)
	        : base(info, context)
	    {
	        Order = info.GetInt32("Order");
	        Advice = (IAdvice) info.GetValue("Advice", typeof(IAdvice));
	    }

	    /// <inheritdoc />
	    public override void GetObjectData(SerializationInfo info, StreamingContext context)
	    {
            base.GetObjectData(info, context);
	        info.AddValue("Order", Order);
	        info.AddValue("Advice", Advice);
	    }

		/// <summary>
		/// Is this advice associated with a particular instance?
		/// </summary>
		/// <value>
		/// <see langword="true"/> if this advice is associated with a
		/// particular instance.
		/// </value>
		/// <exception cref="System.NotSupportedException">
		/// Always; this property is not yet supported.
		/// </exception>
		public virtual bool IsPerInstance
		{
			get
			{
				throw new NotSupportedException(
					"The 'IsPerInstance' property of the IAdvisor interface is " +
					"not yet supported in Spring.NET.");
			}
		}

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
		/// Return the advice part of this advisor.
		/// </summary>
		/// <returns>
		/// The advice that should apply if the pointcut matches.
		/// </returns>
		/// <see cref="Spring.Aop.IAdvisor.Advice"/>
		public virtual IAdvice Advice
		{
			get { return this._advice; }
			set { this._advice = value; }
		}

		/// <summary>
		/// The <see cref="Spring.Aop.IPointcut"/> that drives this advisor.
		/// </summary>
		public virtual IPointcut Pointcut
		{
			get { return this; }
		}
	}
}
