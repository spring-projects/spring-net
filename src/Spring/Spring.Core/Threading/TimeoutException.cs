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

using System.Runtime.Serialization;
using System.Threading;


/*
Originally written by Doug Lea and released into the public domain.
This may be used for any purposes whatsoever without acknowledgment.
Thanks for the assistance and support of Sun Microsystems Labs,
and everyone contributing, testing, and using this code.
*/
namespace Spring.Threading
{
	/// <summary> Thrown by synchronization classes that report
	/// timeouts via exceptions. The exception is treated
	/// as a form (subclass) of InterruptedException. This both
	/// simplifies handling, and conceptually reflects the fact that
	/// timed-out operations are artificially interrupted by timers.
	///
	/// </summary>
	[Serializable]
	public class TimeoutException : ThreadInterruptedException
	{
		/// <summary> The approximate time that the operation lasted before
		/// this timeout exception was thrown.
		///
		/// </summary>
		private readonly long _duration;
		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Threading.TimeoutException"/> class.
		/// </summary>
		public TimeoutException( ) : base(){}
		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Threading.TimeoutException"/> class with the
		/// specified message.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		public TimeoutException( string message ) : base( message ){}
		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Threading.TimeoutException"/> class with the
		/// specified message.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		/// <param name="innerException">
		/// The root exception that is being wrapped.
		/// </param>
		public TimeoutException( string message, Exception innerException ) : base( message, innerException ){}
		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Threading.TimeoutException"/> class.
		/// </summary>
		/// <param name="info">
		/// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
		/// that holds the serialized object data about the exception being thrown.
		/// </param>
		/// <param name="context">
		/// The <see cref="System.Runtime.Serialization.StreamingContext"/>
		/// that contains contextual information about the source or destination.
		/// </param>
		protected TimeoutException( SerializationInfo info, StreamingContext context )
			: base( info, context )
		{
			_duration = info.GetInt32( "duration" );
		}
		/// <summary>
		/// Override of GetObjectData to allow for private serialization
		/// </summary>
		/// <param name="info">serialization info</param>
		/// <param name="context">streaming context</param>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue( "duration", _duration );
			base.GetObjectData( info, context );
		}
		/// <summary> Constructs a TimeoutException with given duration value.
		///
		/// </summary>
		public TimeoutException( long time )
		{
			_duration = time;
		}
		/// <summary> Constructs a TimeoutException with the
		/// specified duration value and detail message.
		/// </summary>
		public TimeoutException( long time, String message ) : base( message )
		{
			_duration = time;
		}
		/// <summary>
		/// Gets the approximate time that the operation lasted before
		/// this timeout exception was thrown.
		/// </summary>
		public long Duration
		{
			get
			{
				return _duration;
			}
		}
	}
}
