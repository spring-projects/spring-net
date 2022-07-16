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

using System.Threading;

namespace Spring.Threading
{
	/// <summary>
	/// Support to account for differences between java nad .NET:
	/// <ul>
	/// </ul>
	/// </summary>
	public class Utils
	{
		private Utils()
		{
		}

		/// <summary>
		/// .NET threads have not a method to check if they have been interrupted.
		/// Moreover, differently from java threads, when entering <c>lock</c>ed
		/// blocks, Monitor, Sleep, SpinWait and so on, a <see cref="ThreadInterruptedException"/>
		/// will be raised by the runtime.
		/// <p/>Spring.Threading classes usually call this method before entering a lock block, to mirror java code
		/// <p>Usually this is non issue because the same exception will be raised entering the monitor
		/// associated with the lock (<seealso href="http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpref/html/frlrfsystemthreadingmonitorclassentertopic.asp"/>)
		/// </p>
		/// </summary>
		/// <exception cref="ThreadInterruptedException">if the thread has been interrupted</exception>
		public static void FailFastIfInterrupted()
		{
			Thread.Sleep(0);
		}

		/// <summary>
		/// Placeholder for <c>java.lang.System.currentTimeMillis</c>
		/// </summary>
		/// <returns>The current machine time in milliseconds</returns>
		public static long CurrentTimeMillis
		{
			get { return ToTimeMillis(DateTime.Now); }
		}

		/// <summary>
		/// Normalize the given <see cref="System.DateTime"/> so that
		/// is is comparable with <see cref="Utils.CurrentTimeMillis"/>.
		/// </summary>
		/// <param name="date">Date.</param>
		/// <returns></returns>
		public static long ToTimeMillis(DateTime date)
		{
			// may be also new TimeSpan(DateTime.UtcNow.Ticks).TotalMilliseconds;
			// see http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dv_jlca/html/vberrjavalangsystemcurrenttimemillis.asp
			return (date.Ticks - 621355968000000000)/10000;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="one"></param>
		/// <param name="another"></param>
		/// <returns>the difference between millisecodns of the first and second date</returns>
		public static long DeltaTimeMillis(DateTime one, DateTime another)
		{
			return (one.Ticks - another.Ticks)/10000;
		}

		/// <summary>
		/// Returns the number of nanoseconds for the current value of <see cref="System.DateTime.Now"/>
		/// </summary>
		/// <returns>Current number of nanoseconds</returns>
		public static long CurrentNanoSeconds()
		{
			return (DateTime.Now.Ticks - 621355968000000000)/10000*1000000;
		}

		/// <summary>
		/// Returns the number of nano seconds represented by the <paramref name="timeSpan"/>
		/// </summary>
		/// <param name="timeSpan"><see cref="System.TimeSpan"/> to use</param>
		/// <returns>Number of nano seconds for <parmref name="timeSpan"/></returns>
		public static long TimeSpanNanoSeconds(TimeSpan timeSpan)
		{
			return (timeSpan.Ticks - 621355968000000000)/10000*1000000;
		}
		/// <summary>
		/// Returns a <see cref="System.TimeSpan"/> representing the number of nanoseconds passed in via <paramref name="nanoSeconds"/>.
		/// </summary>
		/// <param name="nanoSeconds">Number of nanoseconds.</param>
		/// <returns><see cref="System.TimeSpan"/> representing the number of nanoseconds passed in.</returns>
		public static TimeSpan NanoSecondsTimeSpan(long nanoSeconds )
		{
			int milliseconds = Convert.ToInt32(nanoSeconds / 1000000);
			return new TimeSpan(0,0,0,0, milliseconds);
		}

		/// <summary>
		/// Has been interrupted this thread
		/// </summary>
		public static bool ThreadInterrupted
		{
			get
			{
				bool interrupted = false;
				try
				{
					Utils.FailFastIfInterrupted();
				}
				catch (ThreadInterruptedException)
				{
					Thread.CurrentThread.Interrupt();
					interrupted = true;
				}
				return interrupted;
			}
		}
	}
}
