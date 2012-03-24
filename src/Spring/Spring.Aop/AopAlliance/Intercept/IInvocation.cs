#region License

/*
 * All the source code provided by AOP Alliance is Public Domain.
 * 
 *      http://aopalliance.sourceforge.net/
 * 
 */

#endregion

namespace AopAlliance.Intercept
{
	/// <summary>
	/// Represents an invocation in the program.
	/// </summary>
	/// <remarks>
	/// <p>
	/// An invocation is a joinpoint and can be intercepted by an interceptor.
	/// Typical examples would be a constructor invocation and a method call.
	/// </p>
	/// </remarks>
	public interface IInvocation : IJoinpoint
	{
        /// <summary>
        /// Gets the arguments to an invocation.
        /// </summary>
		/// <remarks>
		/// <p>
		/// It is of course possible to change element values within this array
		/// to change the arguments to an intercepted invocation.
		/// </p>
		/// </remarks>
        /// <value>
        /// The arguments to an invocation.
        /// </value>
        object[] Arguments { get; }
	}
}
