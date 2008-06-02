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
	/// Represents a generic runtime joinpoint (in the AOP terminology).
	/// </summary>
	/// <remarks>
	/// <p>
	/// A runtime joinpoint is an <i>event</i> that occurs on a static
	/// joinpoint (i.e. a location in a program). For instance, an
	/// invocation is the runtime joinpoint on a method (static joinpoint).
	/// The static part of a given joinpoint can be generically retrieved
	/// using the <see cref="AopAlliance.Intercept.IJoinpoint.StaticPart"/>
	/// property.
	/// </p>
	/// <p>
	/// In the context of an interception framework, a runtime joinpoint
	/// is then the reification of an access to an accessible object (a
	/// method, a constructor, a field), i.e. the static part of the
	/// joinpoint. It is passed to the interceptors that are installed on
	/// the static joinpoint.
	/// </p>
	/// </remarks>
	/// <seealso cref="AopAlliance.Intercept.IInterceptor"/>
	public interface IJoinpoint
	{
		/// <summary>
		/// Gets the static part of this joinpoint.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The static part is an accessible object on which a chain of
		/// interceptors are installed.
		/// </p>
		/// </remarks>
		/// <value>
		/// The static part of this joinpoint.
		/// </value>
		MemberInfo StaticPart { get; }

		/// <summary>
		/// Gets the object that holds the current joinpoint's static part.
		/// </summary>
		/// <remarks>
		/// <p>
		/// For instance, the target object for a method invocation.
		/// </p>
		/// </remarks>
		/// <value>
		/// The object that holds the current joinpoint's static part.
		/// </value>
		object This { get; }

		/// <summary>
		/// Proceeds to the next interceptor in the chain.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The implementation and semantics of this method depend on the
		/// actual joinpoint type. Consult the derived interfaces of this
		/// interface for specifics.
		/// </p>
		/// </remarks>
		/// <returns>
		/// Consult the derived interfaces of this interface for specifics.
		/// </returns>
		/// <exception cref="System.Exception">
		/// If any of the interceptors at the joinpoint throws an exception.
		/// </exception>
		object Proceed();
	}
}