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

namespace Spring.Context.Events
{
	/// <summary>
	/// Simple listener that logs application events to the console.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Intended for use during debugging only.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Griffin Caprio (.NET)</author>
	/// <seealso cref="Spring.Context.IApplicationEventListener"/>
	public sealed class ConsoleListener : IApplicationEventListener
	{
		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Context.Events.ConsoleListener"/> class.
		/// </summary>
		public ConsoleListener()
		{
		}

		/// <summary>
		/// Handle an application event.
		/// </summary>
		/// <param name="sender">
		/// The source of the event.
		/// </param>
		/// <param name="e">
		/// The event that is to be handled.
		/// </param>
		public void HandleApplicationEvent(object sender, ApplicationEventArgs e)
		{
			Console.WriteLine("Source      : " + sender);
			Console.WriteLine("Event fired : " + e.TimeStamp);
		}
	}
}
