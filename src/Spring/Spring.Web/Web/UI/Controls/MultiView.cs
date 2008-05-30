#region License

/*
 * Copyright 2002-2004 the original author or authors.
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

using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

#endregion

namespace Spring.Web.UI.Controls
{
#if !NET_2_0
	/// <summary>
	/// This controls provides MultiView introduced with ASP.NET 2.0 for NET 1.1 Platform
	/// </summary>
	/// <author>Erich Eichinger</author>
	/// <version>$Id: MultiView.cs,v 1.1 2007/07/24 13:33:27 oakinger Exp $</version>
	[
		DefaultEvent("ActiveViewChanged")
			, ToolboxData("<{0}:MultiView runat=\"server\"></{0}:MultiView>")
			, ParseChildren(false, null)
			, AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)
			, AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
	public class MultiView : Control
	{
		#region Constants

		/// <summary>
		/// Represents the command name associated with the next View control to display in a MultiView control. This field is read-only.
		/// </summary>
		public static readonly string NextViewCommandName = "NextView";
		/// <summary>
		/// Represents the command name associated with the previous View control to display in a MultiView control. This field is read-only.
		/// </summary>
		public static readonly string PreviousViewCommandName = "PrevView";
		/// <summary>
		/// Represents the command name associated with changing the active View control in a MultiView control, based on a specified View id. This field is read-only.
		/// </summary>
		public static readonly string SwitchViewByIDCommandName = "SwitchViewByID";
		/// <summary>
		/// Represents the command name associated with changing the active View control in a MultiView control based on a specified View index. This field is read-only.
		/// </summary>
		public static readonly string SwitchViewByIndexCommandName = "SwitchViewByIndex";

		#endregion

		#region Fields

		private static readonly FieldInfo s_fiControlState =
			typeof(Control).GetField("_controlState", BindingFlags.Instance | BindingFlags.NonPublic);


		private int _activeViewIndex = -1;
		private int _cachedActiveViewIndex = -1;
		private bool _controlStateApplied;
		private static readonly object _eventActiveViewChanged = new object();
		private bool _ignoreBubbleEvents;

		#endregion

		#region Construction

		/// <summary>
		/// Initializes a new instance of the <see cref="MultiView"/> class.
		/// </summary>
		public MultiView()
		{
			// intentionally left empty
		}

		#endregion

		#region Events

		/// <summary>
		/// Occurs when the active View control of a MultiView control changes between posts to the server.
		/// </summary>
		public event EventHandler ActiveViewChanged
		{
			add { base.Events.AddHandler(_eventActiveViewChanged, value); }
			remove { base.Events.RemoveHandler(_eventActiveViewChanged, value); }
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the index of the active View control within a MultiView control.
		/// </summary>
		[DefaultValue(-1)]
		public virtual int ActiveViewIndex
		{
			get
			{
				if (_cachedActiveViewIndex > -1)
				{
					return _cachedActiveViewIndex;
				}
				return _activeViewIndex;
			}
			set
			{
				if (value < -1)
				{
					throw new ArgumentOutOfRangeException("value",
						string.Format(
						"MultiView_ActiveViewIndex_less_than_minus_one",
						new object[] {value}));
				}
				if ((Views.Count == 0) && (ControlState < ControlState.ChildrenInitialized))
				{
					_cachedActiveViewIndex = value;
				}
				else
				{
					if (value >= Views.Count)
					{
						throw new ArgumentOutOfRangeException("value",
							string.Format(
							"MultiView_ActiveViewIndex_equal_or_greater_than_count",
							new object[] {value, Views.Count}));
					}
					int num = (_cachedActiveViewIndex != -1) ? -1 : _activeViewIndex;
					_activeViewIndex = value;
					_cachedActiveViewIndex = -1;
					if (((num != value) && (num != -1)) && (num < Views.Count))
					{
						Views[num].Active = false;
						if (ShouldTriggerViewEvent)
						{
							Views[num].OnDeactivate(EventArgs.Empty);
						}
					}
					if (((num != value) && (Views.Count != 0)) && (value != -1))
					{
						Views[value].Active = true;
						if (ShouldTriggerViewEvent)
						{
							Views[value].OnActivate(EventArgs.Empty);
							OnActiveViewChanged(EventArgs.Empty);
						}
					}
				}
			}
		}


		/// <summary>
		/// Gets the collection of View controls in the MultiView control.
		/// </summary>
		[
		PersistenceMode(PersistenceMode.InnerDefaultProperty)
		, Browsable(false)
		]
		public virtual ViewCollection Views
		{
			get { return (ViewCollection) Controls; }
		}

		/// <summary>
		/// Return the current lifecycle state of this control.
		/// </summary>
		private ControlState ControlState
		{
			get { return (ControlState) s_fiControlState.GetValue(this); }
		}

		#endregion

		/// <summary>
		/// Sets the specified View control to the active view within a MultiView control.
		/// </summary>
		/// <param name="view">A View control to set as the active view within a MultiView control.</param>
		public void SetActiveView(View view)
		{
			int index = Views.IndexOf(view);
			if (index < 0)
			{
				throw new HttpException(
					string.Format("MultiView_view_not_found", new object[] {(view == null) ? "null" : view.ID, ID}));
			}
			ActiveViewIndex = index;
		}

		/// <summary>
		/// Returns the current active View control within a MultiView control. 
		/// </summary>
		/// <returns>A View control that represents the active view within a MultiView control.</returns>
		public View GetActiveView()
		{
			int activeViewIndex = ActiveViewIndex;
			if (activeViewIndex >= Views.Count)
			{
				throw new Exception("MultiView_ActiveViewIndex_out_of_range");
			}
			if (activeViewIndex < 0)
			{
				return null;
			}
			View view = Views[activeViewIndex];
			if (!view.Active)
			{
				UpdateActiveView(activeViewIndex);
			}
			return view;
		}

		/// <summary>
		/// Mark this control to ignore <see cref="OnBubbleEvent"/>
		/// </summary>
		internal void IgnoreBubbleEvents()
		{
			_ignoreBubbleEvents = true;
		}

		/// <summary>
		/// Adds the element to the collection of child controls.
		/// </summary>
		protected override void AddParsedSubObject(object obj)
		{
			if (obj is View)
			{
				Controls.Add((Control) obj);
			}
			else if (!(obj is LiteralControl))
			{
				throw new HttpException(
					string.Format("MultiView_cannot_have_children_of_type", new object[] {obj.GetType().Name}));
			}
		}

		/// <summary>
		/// Creates a new <see cref="ViewCollection"/> instance.
		/// </summary>
		/// <returns></returns>
		protected override ControlCollection CreateControlCollection()
		{
			return new ViewCollection(this);
		}

		/// <summary>
		/// Restores view-state information from a previous page request.
		/// </summary>
		protected override void LoadViewState(object savedState)
		{
			base.LoadViewState(savedState);
			ActiveViewIndex = (int) ViewState["_activeViewIndex"];
			_controlStateApplied = true;
		}

		/// <summary>
		/// Saves view-state information to the page response.
		/// </summary>
		protected override object SaveViewState()
		{
			ViewState["_activeViewIndex"] = ActiveViewIndex;
			return base.SaveViewState();
		}

		/// <summary>
		/// Raises the <see cref="ActiveViewChanged"/> event.
		/// </summary>
		protected virtual void OnActiveViewChanged(EventArgs e)
		{
			EventHandler handler = (EventHandler) base.Events[_eventActiveViewChanged];
			if (handler != null)
			{
				handler(this, e);
			}
		}

		/// <summary>
		/// Captures know commands.
		/// </summary>
		protected override bool OnBubbleEvent(object source, EventArgs e)
		{
			if (!_ignoreBubbleEvents && (e is CommandEventArgs))
			{
				CommandEventArgs args = (CommandEventArgs) e;
				string commandName = args.CommandName;
				if (commandName == NextViewCommandName)
				{
					if (ActiveViewIndex < (Views.Count - 1))
					{
						ActiveViewIndex++;
					}
					else
					{
						ActiveViewIndex = -1;
					}
					return true;
				}
				if (commandName == PreviousViewCommandName)
				{
					if (ActiveViewIndex > -1)
					{
						ActiveViewIndex--;
					}
					return true;
				}
				if (commandName == SwitchViewByIDCommandName)
				{
					View view = FindControl((string) args.CommandArgument) as View;
					if ((view == null) || (view.Parent != this))
					{
						throw new HttpException(
							string.Format("MultiView_invalid_view_id",
							              new object[]
							              	{ID, (string) args.CommandArgument, SwitchViewByIDCommandName}));
					}
					SetActiveView(view);
					return true;
				}
				if (commandName == SwitchViewByIndexCommandName)
				{
					int num;
					try
					{
						num = int.Parse((string) args.CommandArgument, CultureInfo.InvariantCulture);
					}
					catch (FormatException)
					{
						throw new FormatException(
							string.Format("MultiView_invalid_view_index_format",
							              new object[] {(string) args.CommandArgument, SwitchViewByIndexCommandName}));
					}
					ActiveViewIndex = num;
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Initialize this control.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			if (_cachedActiveViewIndex > -1)
			{
				ActiveViewIndex = _cachedActiveViewIndex;
				_cachedActiveViewIndex = -1;
				GetActiveView();
			}
		}

		/// <summary>
		/// Handle removal of a view.
		/// </summary>
		/// <param name="ctl"></param>
		protected override void RemovedControl(Control ctl)
		{
			if (((View) ctl).Active && (ActiveViewIndex < Views.Count))
			{
				GetActiveView();
			}
			base.RemovedControl(ctl);
		}

		/// <summary>
		/// Render the currently active view.
		/// </summary>
		/// <param name="writer"></param>
		protected override void Render(HtmlTextWriter writer)
		{
			View activeView = GetActiveView();
			if (activeView != null)
			{
				activeView.RenderControl(writer);
			}
		}

		/// <summary>
		/// Switches from the currently active view to the specified view.
		/// </summary>
		/// <param name="activeViewIndex"></param>
		private void UpdateActiveView(int activeViewIndex)
		{
			for (int i = 0; i < Views.Count; i++)
			{
				View view = Views[i];
				if (i == activeViewIndex)
				{
					view.Active = true;
					if (ShouldTriggerViewEvent)
					{
						view.OnActivate(EventArgs.Empty);
					}
				}
				else if (view.Active)
				{
					view.Active = false;
					if (ShouldTriggerViewEvent)
					{
						view.OnDeactivate(EventArgs.Empty);
					}
				}
			}
		}

		/// <summary>
		/// Indicates if view events are ready to be raised.
		/// </summary>
		private bool ShouldTriggerViewEvent
		{
			get
			{
				if (_controlStateApplied)
				{
					return true;
				}
				if (this.Page != null)
				{
					return !this.Page.IsPostBack;
				}
				return false;
			}
		}
	}
#endif // !NET_2_0
}