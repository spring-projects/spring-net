#region License

/*
 * All the source code provided by AOP Alliance is Public Domain.
 * 
 *      http://aopalliance.sourceforge.net/
 * 
 */

#endregion

#region Imports

using AopAlliance.Aop;

#endregion

namespace AopAlliance.Intercept
{
	/// <summary>
    /// Represents a generic interceptor.
    /// </summary>
	/// <remarks>
	/// <p>
	/// A generic interceptor can intercept runtime events that occur within a
	/// base program. Those events are materialized by (reified in) joinpoints.
	/// Runtime joinpoints can be invocations, field access, exceptions, etc.
	/// </p>
	/// <p>
	/// This interface is not used directly. Use the various derived interfaces
	/// to intercept specific events.
	/// </p>
	/// </remarks>
    /// <seealso cref="AopAlliance.Intercept.IJoinpoint"/>
	public interface IInterceptor : IAdvice
	{
	}
}
