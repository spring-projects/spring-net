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
	/// A description of an invocation to a constuctor, given to an interceptor
	/// upon constructor-call.
	/// </summary>
	/// <remarks>
	/// <p>
	/// A constructor invocation is a joinpoint and can be intercepted by a
	/// constructor interceptor.
	/// </p>
	/// </remarks>
	/// <seealso cref="AopAlliance.Intercept.IConstructorInterceptor"/>
	public interface IConstructorInvocation : IInvocation
	{
		/// <summary>
		/// Gets the constructor invocation that is to be invoked.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This property is a friendly implementation of the
		/// <see cref="AopAlliance.Intercept.IJoinpoint.StaticPart"/> property.
		/// It should be used in preference to the
		/// <see cref="AopAlliance.Intercept.IJoinpoint.StaticPart"/> property
		/// because it provides immediate access to the underlying constructor
		/// without the need to resort to a cast.
		/// </p>
		/// </remarks>
		/// <value>
		/// The constructor invocation that is to be invoked.
		/// </value>
		ConstructorInfo Constructor { get; }
	}
}