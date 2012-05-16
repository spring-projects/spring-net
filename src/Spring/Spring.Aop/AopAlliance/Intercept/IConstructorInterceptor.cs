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
	/// Intercepts the construction of a new object.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Such interceptions are nested "on top" of the target.
	/// </p>
	/// </remarks>
	public interface IConstructorInterceptor : IInterceptor
	{
		/// <summary>
		/// Implement this method to perform extra treatments before and after
		/// the consruction of a new object.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Polite implementations would certainly like to invoke
		/// <see cref="AopAlliance.Intercept.IJoinpoint.Proceed"/>. 
		/// </p>
		/// </remarks>
		/// <param name="invocation">
		/// The constructor invocation that is being intercepted.
		/// </param>
		/// <returns>
		/// The newly created object, which is also the result of the call to
		/// <see cref="AopAlliance.Intercept.IJoinpoint.Proceed"/>, and might be
		/// replaced by the interceptor.
		/// </returns>
		/// <exception cref="System.Exception">
		/// If any of the interceptors in the chain or the target object itself
		/// throws an exception.
		/// </exception>
		object Construct(IConstructorInvocation invocation);
	}
}