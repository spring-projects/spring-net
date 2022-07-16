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

using System.Web.UI.WebControls;

#endregion

namespace Spring.Web.UI.Controls
{
	/// <summary>
	///	Adds the <see cref="SelectedValues"/> property to the framework's <see cref="System.Web.UI.WebControls.CheckBoxList"/> control.
	/// </summary>
	/// <remarks>
	/// When using Spring.Web's DataBinding, you should use this control for easier binding declaration.
	/// </remarks>
	/// <author>Erich Eichinger</author>
	public class CheckBoxList : System.Web.UI.WebControls.CheckBoxList
	{
		/// <summary>
		/// Gets or Sets the list of selected values and checks the <see cref="CheckBox"/>es accordingly
		/// </summary>
		public string[] SelectedValues
		{
			get
			{
				List<string> vals = new List<string>();
				foreach( ListItem item in this.Items )
				{
					if (item.Selected) vals.Add(item.Value);
				}
				return vals.ToArray();
			}
			set
			{
				if (value == null || value.Length == 0)
				{
					this.ClearSelection();
				}
				else
				{
                    List<string> vals = new List<string>();
                    foreach (ListItem item in this.Items)
					{
						item.Selected = (vals.Contains(item.Value));
					}
				}
			}
		}
	}
}
