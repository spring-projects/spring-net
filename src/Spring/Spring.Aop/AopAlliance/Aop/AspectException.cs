#region License

/*
 * All the source code provided by AOP Alliance is Public Domain.
 * 
 *      http://aopalliance.sourceforge.net/
 * 
 */

#endregion

#region Imports

using System;
using System.Runtime.Serialization;

#endregion

namespace AopAlliance.Aop
{
	/// <summary>
	/// Superclass for all AOP infrastructure exceptions.
	/// </summary>
	/// <author>Aleksandar Seovic</author>
	[Serializable]
	public class AspectException : Exception
	{
		/// <summary>
		/// Creates a new instance of the
		/// <see cref="AopAlliance.Aop.AspectException"/> class.
		/// </summary>
		public AspectException()
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="AopAlliance.Aop.AspectException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		public AspectException(string message) : base(message)
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="AopAlliance.Aop.AspectException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		/// <param name="innerException">
		/// The root exception that is being wrapped.
		/// </param>
		public AspectException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="AopAlliance.Aop.AspectException"/> class.
		/// </summary>
		/// <param name="info">
		/// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
		/// that holds the serialized object data about the exception being thrown.
		/// </param>
		/// <param name="context">
		/// The <see cref="System.Runtime.Serialization.StreamingContext"/>
		/// that contains contextual information about the source or destination.
		/// </param>
		protected AspectException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}