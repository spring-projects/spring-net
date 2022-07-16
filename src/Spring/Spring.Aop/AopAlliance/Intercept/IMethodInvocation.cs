#region License

/*
 * All the source code provided by AOP Alliance is Public Domain.
 *
 *      http://aopalliance.sourceforge.net/
 *
 */

#endregion

#region Imports

using System.Reflection;

#endregion

namespace AopAlliance.Intercept
{
	/// <summary>
	/// Description of an invocation to a method, given to an interceptor
	/// upon method-call.
	/// </summary>
	/// <remarks>
	/// <p>
	/// A method invocation is a joinpoint and can be intercepted by a method
	/// interceptor.
	/// </p>
	/// </remarks>
	/// <seealso cref="AopAlliance.Intercept.IMethodInterceptor"/>
	public interface IMethodInvocation : IInvocation
	{
		/// <summary>
		/// Gets the method invocation that is to be invoked.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This property is a friendly implementation of the
		/// <see cref="AopAlliance.Intercept.IJoinpoint.StaticPart"/> property.
		/// It should be used in preference to the
		/// <see cref="AopAlliance.Intercept.IJoinpoint.StaticPart"/> property
		/// because it provides immediate access to the underlying method
		/// without the need to resort to a cast.
		/// </p>
		/// </remarks>
		/// <value>
		/// The method invocation that is to be invoked.
		/// </value>
		MethodInfo Method { get; }

        /// <summary>
        /// Gets the proxy object for the invocation.
        /// </summary>
        /// <value>
        /// The proxy object for this method invocation.
        /// </value>
        object Proxy { get; }

        /// <summary>
        /// Gets the target object for the invocation.
        /// </summary>
        /// <value>
        /// The target object for this method invocation.
        /// </value>
        object Target { get; }

        /// <summary>
        /// Gets the type of the target object.
        /// </summary>
        /// <value>
        /// The type of the target object.
        /// </value>
        Type TargetType { get; }

	}
}
