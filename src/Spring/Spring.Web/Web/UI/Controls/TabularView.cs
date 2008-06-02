#region License

/*
 * Copyright 2002-2007 the original author or authors.
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

using System.Reflection;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

#endregion

namespace Spring.Web.UI.Controls
{
	/// <summary>
	/// Represents a control that acts as a container for a group of controls within a <see cref="TabularMultiView"/> control. 
	/// </summary>
	/// <author>Erich Eichinger</author>
	[ToolboxData("<{0}:TabularView runat=\"server\"></{0}:TabularView>")]
	public class TabularView : View
	{
#if NET_2_0
		private static readonly PropertyInfo s_fiActive =
			typeof(View).GetProperty("Active", BindingFlags.Instance | BindingFlags.NonPublic);

		/// <summary>
		/// Indicates if this view is currently active.
		/// </summary>
		public bool Active
		{
			get { return (bool) s_fiActive.GetValue(this, null); }
		}
#endif
		private string m_TabToolTip;

		/// <summary>
		/// Get or Set the name of the tab item associated with this view.
		/// </summary>
		[Description("Name of the tab.")
			, NotifyParentProperty(true)
			, DefaultValue("Tab")
			, Category("Behavior")]
		public string TabName
		{
			get
			{
				string str = (string) ViewState["m_TabName"];
				return str;
			}
			set { ViewState["m_TabName"] = value; }
		}

		/// <summary>
		/// Get or Set the tooltip text of the tab item associated with this view.
		/// </summary>
		[Description("Tooltip of the tab.")
			, NotifyParentProperty(true)
			, DefaultValue("")
			, Category("Behavior")]
		public string TabToolTip
		{
			get { return m_TabToolTip; }
			set { m_TabToolTip = value; }
		}
	}
}