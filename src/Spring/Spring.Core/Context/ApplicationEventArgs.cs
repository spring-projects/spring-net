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

namespace Spring.Context
{
	/// <summary>
	/// Encapsulates the data associated with an event raised by an
	/// <see cref="Spring.Context.IApplicationContext"/>.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Mark Pollack (.NET)</author>
	/// <author>Griffin Caprio (.NET)</author>
	/// <seealso cref="Spring.Context.IApplicationEventListener"/>
	[Serializable]
	public class ApplicationEventArgs : EventArgs
	{
		private const long TicksAtEpoch = 621355968000000000;
		private DateTime _timestamp;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Context.ApplicationEventArgs"/> class.
		/// </summary>
		public ApplicationEventArgs()
		{
			_timestamp = DateTime.Now;
		}

		/// <summary>
		/// The date and time when the event occured.
		/// </summary>
		/// <value>
		/// The date and time when the event occured.
		/// </value>
		public DateTime TimeStamp
		{
			get { return _timestamp; }
		}

		/// <summary>
		/// The system time in milliseconds when the event happened.
		/// </summary>
		/// <value>
		/// The system time in milliseconds when the event happened.
		/// </value>
		public long EventTimeMilliseconds 
		{
			get { return ( _timestamp.Ticks - TicksAtEpoch ) / 10000; }
		}
	}
}
