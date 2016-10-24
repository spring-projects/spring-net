#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

#region Imports

#endregion

using System.Web.UI.WebControls;

namespace Spring.Web.UI.Controls
{
	/// <summary>
	/// Provides information about a command raised from a <see cref="TabContainer"/>
	/// </summary>
	/// <author>Erich Eichinger</author>
	public class TabCommandEventArgs : CommandEventArgs
	{
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		/// <param name="commandName">The name of the command raised by a <see cref="TabContainer"/>.</param>
		/// <param name="tabIndex">The index of the tab that raised this event.</param>
		public TabCommandEventArgs(string commandName, int tabIndex) : base(commandName, tabIndex)
		{
		}

		/// <summary>
		/// Returns the index of the tab that raised this command.
		/// </summary>
		public int TabIndex
		{
			get { return (int) base.CommandArgument; }
		}
	}
}