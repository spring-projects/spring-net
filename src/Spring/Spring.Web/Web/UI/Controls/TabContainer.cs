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

using System.Web.UI;
using System.Web.UI.WebControls;

#endregion

namespace Spring.Web.UI.Controls
{
	/// <summary>
	/// This control is responsible for rendering tabs.
	/// </summary>
	/// <remarks>
	/// By default, this TabContainer implementation uses <see cref="LinkButton"/> controls to
	/// render tabs. Override <see cref="CreateTab"/> to change this behaviour.
	/// </remarks>
	/// <author>Erich Eichinger</author>
	public class TabContainer : Control
	{
		/// <summary>
		/// Represents the command name of the tab to be selected.
		/// </summary>
		public static readonly string SelectTabCommandName = "SelectTab";

		/// <summary>
		/// Key into the eventhandler table.
		/// </summary>
		private static readonly object _eventClick = new object();

		/// <summary>
		/// Occurs, when a tab control is clicked.
		/// </summary>
		public event TabCommandEventHandler Click
		{
			add { base.Events.AddHandler(_eventClick, value); }
			remove { base.Events.RemoveHandler(_eventClick, value); }
		}

		/// <summary>
		/// Catches <see cref="CommandEventArgs"/> with name '<see cref="SelectTabCommandName"/>' and
		/// raises the <see cref="Click"/> event.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="args">contains event information.</param>
		/// <returns></returns>
		protected override bool OnBubbleEvent(object source, EventArgs args)
		{
			CommandEventArgs cea = args as CommandEventArgs;
			if (cea != null && cea.CommandName == SelectTabCommandName)
			{
				this.OnTabClickedCommand(source, (CommandEventArgs) args);
				return true;
			}

			return base.OnBubbleEvent(source, args);
		}

		/// <summary>
		/// Raises the <see cref="Click"/> event.
		/// </summary>
		protected virtual void OnTabClickedCommand(object sender, CommandEventArgs args)
		{
			TabCommandEventHandler handler = (TabCommandEventHandler) base.Events[_eventClick];
			if (handler != null)
			{
				handler(sender, new TabCommandEventArgs(args.CommandName, Int32.Parse((string) args.CommandArgument)));
			}
		}

		/// <summary>
		/// Creates a new tab for the specified <c>view</c> within the given <c>container</c>.
		/// </summary>
		/// <param name="container">The <see cref="TabularMultiView"/> containing the <c>view</c></param>
		/// <param name="view">The <see cref="TabularView"/> for which a new tab is to be created.</param>
		/// <param name="index">The index of the tab to be created.</param>
		/// <remarks>
		/// By default, <see cref="LinkButton"/> controls are used for rendering tabs. Override this method to
		/// change this behaviour.
		/// </remarks>
		protected internal virtual void CreateTab(TabularMultiView container, TabularView view, int index)
		{
			LinkButton btnTab = new LinkButton();
			btnTab.Text = view.TabName;
			btnTab.CommandName = "SelectTab";
			btnTab.CommandArgument = "" + index;
			btnTab.ID = "Tab" + index;
			btnTab.ToolTip = view.TabToolTip;
			btnTab.Enabled = !view.Active;

			WebControl span = new WebControl(HtmlTextWriterTag.Span);
			span.CssClass = (view.Active) ? container.TabularMenuSelectedItemCSS : container.TabularMenuItemCSS;
			span.Controls.Add(btnTab);
			this.Controls.Add(span);
		}
	}
}
