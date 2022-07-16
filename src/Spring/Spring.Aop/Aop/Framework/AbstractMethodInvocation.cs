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

using System.Collections;
using System.Reflection;
using System.Text;

using AopAlliance.Intercept;
using Spring.Util;

#endregion

namespace Spring.Aop.Framework
{
	/// <summary>
	/// Convenience base class for <see cref="AopAlliance.Intercept.IMethodInvocation"/>
	/// implementations.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Subclasses can override the
	/// <see cref="Spring.Aop.Framework.AbstractMethodInvocation.InvokeJoinpoint()"/>
	/// method to change this behavior, so this is a useful/ base class for
	/// <see cref="AopAlliance.Intercept.IMethodInvocation"/> implementations.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	/// <author>Rick Evans (.NET)</author>
    /// <author>Bruno Baia (.NET)</author>
	[Serializable]
    public abstract class AbstractMethodInvocation : IMethodInvocation
	{
		/// <summary>
		/// The arguments (if any = may be <see lang="null"/>) to the method
		/// that is to be invoked.
		/// </summary>
		private object[] arguments;

		/// <summary>
		/// The target object that the method is to be invoked on.
		/// </summary>
		private readonly object target;

		/// <summary>
        /// The AOP proxy for the target object.
        /// </summary>
		private object proxy;

        /// <summary>
        /// The method invocation that is to be invoked.
        /// </summary>
        private MethodInfo method;

		/// <summary>
		/// The list of <see cref="AopAlliance.Intercept.IMethodInterceptor"/> and
		/// <cref see="Spring.Aop.Framework.InterceptorAndDynamicMethodMatcher"/>
		/// that need dynamic checks.
		/// </summary>
		private IList interceptors;

        /// <summary>
        /// The declaring type of the method that is to be invoked.
        /// </summary>
        private Type targetType;

        /// <summary>
        /// The index from 0 of the current interceptor we're invoking.
        /// </summary>
        private int currentInterceptorIndex;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Aop.Framework.AbstractMethodInvocation"/> class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is an abstract class, and as such exposes no publicly visible
		/// constructors.
		/// </p>
		/// <p>
		/// <note type="implementnotes">
		/// The <paramref name="interceptors"/>	list can also contain any
		/// <see cref="Spring.Aop.Framework.InterceptorAndDynamicMethodMatcher"/>s
		/// that need evaluation at runtime.
		/// <see cref="Spring.Aop.IMethodMatcher"/>s included in an
		/// <see cref="Spring.Aop.Framework.InterceptorAndDynamicMethodMatcher"/>
		/// must already have been found to have matched as far as was possible
		/// <b>statically</b>. Passing an array might be about 10% faster, but
		/// would complicate the code, and it would work only for static
		/// pointcuts.
		/// </note>
		/// </p>
		/// </remarks>
		/// <param name="proxy">The AOP proxy.</param>
		/// <param name="target">The target object.</param>
        /// <param name="method">the target method.</param>
		/// <param name="arguments">The target method's arguments.</param>
		/// <param name="targetType">
		/// The <see cref="System.Type"/> of the target object.</param>
		/// <param name="interceptors">
		/// The list of interceptors that are to be applied. May be
		/// <cref lang="null"/>.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// If the <paramref name="target"/> is <see lang="null"/>.
		/// </exception>
		protected AbstractMethodInvocation(object proxy, object target,
            MethodInfo method, object[] arguments, Type targetType, IList interceptors)
		{
		    // EE: There is not necessarily always a target - e.g. for DynamicEntities
            // moved this check to InvokeJoinpoint()
            AssertUtils.ArgumentNotNull(method, "method");

		    this.proxy = proxy;
			this.target = target;
            this.method = method;
			this.targetType = targetType;
			this.arguments = arguments;
			this.interceptors = interceptors;
		}

		/// <summary>
		/// Gets the method invocation that is to be invoked.
		/// </summary>
		/// <remarks>
		/// <p>
		/// May or may not correspond with a method invoked on an underlying
		/// implementation of that interface.
		/// </p>
		/// </remarks>
		/// <see cref="AopAlliance.Intercept.IMethodInvocation.Method"/>
		public virtual MethodInfo Method
		{
		    get { return method; }
		    protected set { method = value; }
		}

	    /// <summary>
		/// Gets the static part of this joinpoint.
		/// </summary>
		/// <value>
		/// The proxied member's information.
		/// </value>
		/// <see cref="AopAlliance.Intercept.IJoinpoint.StaticPart"/>
		public virtual MemberInfo StaticPart
		{
			get { return Method; }
		}

		/// <summary>
		/// Gets the proxy that this interception was made through.
		/// </summary>
		/// <value>
		/// The proxy that this interception was made through.
		/// </value>
		public virtual object Proxy
		{
			get { return this.proxy; }
            protected set { this.proxy = value; }
		}
	    /// <summary>
	    /// Gets the target object for the invocation.
	    /// </summary>
	    /// <value>
	    /// The target object for this method invocation.
	    /// </value>
	    public virtual object Target
	    {
            get { return this.target; }
	    }
	    /// <summary>
	    /// Gets the type of the target object.
	    /// </summary>
	    /// <value>
	    /// The type of the target object.
	    /// </value>
	    public virtual Type TargetType
	    {
	        get { return this.targetType; }
	        protected set { this.targetType = value; }
	    }

	    /// <summary>
		/// Gets and sets the arguments (if any - may be <cref lang="null"/>)
		/// to the method that is to be invoked.
		/// </summary>
		/// <value>
		/// The arguments (if any - may be <cref lang="null"/>) to the
		/// method that is to be invoked.
		/// </value>
		/// <see cref="AopAlliance.Intercept.IInvocation.Arguments"/>
		public virtual object[] Arguments
		{
			get { return this.arguments; }
			set { this.arguments = value; }
		}

		/// <summary>
		/// The list of method interceptors.
		/// </summary>
		/// <remarks>
		/// <p>
		/// May be <see lang="null"/>.
		/// </p>
		/// </remarks>
		public virtual IList Interceptors
		{
			get { return this.interceptors; }
			set { this.interceptors = value; }
		}

		/// <summary>
		/// Gets the target object.
		/// </summary>
		public virtual object This
		{
			get { return this.target; }
		}

        /// <summary>
        /// The index from 0 of the current interceptor we're invoking.
        /// </summary>
	    protected int CurrentInterceptorIndex
	    {
	        get { return currentInterceptorIndex; }
	        set { currentInterceptorIndex = value; }
	    }

	    /// <summary>
		/// Proceeds to the next interceptor in the chain.
		/// </summary>
		/// <returns>
		/// The return value of the method invocation.
		/// </returns>
		/// <exception cref="System.Exception">
		/// If any of the interceptors at the joinpoint throws an exception.
		/// </exception>
		/// <see cref="AopAlliance.Intercept.IJoinpoint.Proceed"/>
		public virtual object Proceed()
		{
            if (this.interceptors == null ||
                this.currentInterceptorIndex == this.interceptors.Count)
            {
                AssertJoinpoint();
                return InvokeJoinpoint();
            }
            object interceptor = this.interceptors[this.currentInterceptorIndex];
            InterceptorAndDynamicMethodMatcher dynamicMatcher
                = interceptor as InterceptorAndDynamicMethodMatcher;
            IMethodInvocation nextInvocation = PrepareMethodInvocationForProceed(this);
            if (dynamicMatcher != null)
            {
                // evaluate dynamic method matcher here: static part will already have
                // been evaluated and found to match...
                if (dynamicMatcher.MethodMatcher.Matches(
                    nextInvocation.Method, nextInvocation.TargetType, nextInvocation.Arguments))
                {
                    return dynamicMatcher.Interceptor.Invoke(nextInvocation);
                }
                else
                {
                    // dynamic match failed; skip this interceptor and invoke the next in the chain...
                    return nextInvocation.Proceed();
                }
            }
            else
            {
                // it's an interceptor so we just invoke it: the pointcut will have
                // been evaluated statically before this object was constructed...
                return ((IMethodInterceptor)interceptor).Invoke(nextInvocation);
            }
		}

        /// <summary>
        /// Retrieves a new <see cref="AopAlliance.Intercept.IMethodInvocation"/> instance
        /// for the next Proceed method call.
        /// </summary>
        /// <param name="invocation">
        /// The current <see cref="AopAlliance.Intercept.IMethodInvocation"/> instance.
        /// </param>
        /// <returns>
        /// The new <see cref="AopAlliance.Intercept.IMethodInvocation"/> instance to use.
        /// </returns>
        /// <see cref="Spring.Aop.Framework.ReflectiveMethodInvocation.PrepareMethodInvocationForProceed"/>
        protected abstract IMethodInvocation PrepareMethodInvocationForProceed(
            IMethodInvocation invocation);

        /// <summary>
        /// Performs sanity checks, whether the actual joinpoint may be invoked
        /// </summary>
        /// <remarks>
        /// By default checks that the underlying target is not null and the called method is implemented
        /// by the target's type.
        /// </remarks>
        /// <exception cref="ArgumentNullException">if <see cref="target"/> is <c>null</c>.</exception>
        /// <exception cref="NotSupportedException">if the <see cref="target"/> 's type does not implement <see cref="method"/>.</exception>
        protected virtual void AssertJoinpoint()
        {
            AssertUtils.ArgumentNotNull(target, "target");
//            if (this.method != null
//                && !this.method.DeclaringType.IsAssignableFrom(target.GetType()))
//            {
//                // This means the target type doesn't implement the interface.
//                // Since no interceptor has handled the call, we throw a sensible exception here.
//                throw new NotSupportedException(string.Format("Interface method '{0}.{1}()' was not handled by any interceptor and the underlying target type '{2}' does not implement this method.", method.DeclaringType.FullName, method.Name, target.GetType().FullName));
//            }
        }

		/// <summary>
		/// Invokes the joinpoint.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Subclasses can override this to use custom invocation.
		/// </p>
		/// </remarks>
		/// <returns>
		/// The return value of the invocation of the joinpoint.
		/// </returns>
		/// <exception cref="System.Exception">
		/// If invoking the joinpoint resulted in an exception.
		/// </exception>
        /// <see cref="Spring.Aop.Framework.ReflectiveMethodInvocation.InvokeJoinpoint"/>
        protected abstract object InvokeJoinpoint();

		/// <summary>
		/// A <see cref="System.String"/> that represents the current
		/// invocation.
		/// </summary>
		/// <remarks>
		/// <p>
		/// <note type="implementnotes">
		/// Does <b>not</b> invoke <see cref="System.Object.ToString()"/> on the
		/// <see cref="Spring.Aop.Framework.AbstractMethodInvocation.This"/> target
		/// object, as that too may be proxied.
		/// </note>
		/// </p>
		/// </remarks>
		/// <returns>
		/// A <see cref="System.String"/> that represents the current invocation.
		/// </returns>
		public override string ToString()
		{
			StringBuilder buffer = new StringBuilder("Invocation: method '");
			buffer.Append(Method.Name).Append("', ").Append("arguments ");
            buffer.Append(Arguments != null ? StringUtils.CollectionToCommaDelimitedString(Arguments) : "[none]");
			buffer.Append("; ");
			if (Target == null)
			{
				buffer.Append("target is null.");
			}
			else
			{
				buffer.Append("target is of Type [").Append(TargetType.FullName).Append(']');
			}
			return buffer.ToString();
		}
	}
}
