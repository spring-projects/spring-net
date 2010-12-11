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

using System;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;

#endregion

namespace Spring.Web.UI.Controls
{
#if !NET_2_0
	/// <summary>
	/// Represents a control that acts as a container for a group of controls within a <see cref="MultiView"/> control.
	/// </summary>
	[ParseChildren(false, null)
		, ToolboxData("<{0}:View runat=\"server\"></{0}:View>")
		, AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)
		, AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
	public class View : Control
	{
		private bool _active;
		private static readonly object _eventActivate = new object();
		private static readonly object _eventDeactivate = new object();

		/// <summary>
		/// Occurs when the current View control becomes the active view.
		/// </summary>
		public event EventHandler Activate
		{
			add { base.Events.AddHandler(_eventActivate, value); }
			remove { base.Events.RemoveHandler(_eventActivate, value); }
		}

		/// <summary>
		/// Occurs when the current active View control becomes inactive.
		/// </summary>
		public event EventHandler Deactivate
		{
			add { base.Events.AddHandler(_eventDeactivate, value); }
			remove { base.Events.RemoveHandler(_eventDeactivate, value); }
		}

		/// <summary>
		/// Raises the <see cref="Activate"/> event of the View control.
		/// </summary>
		protected internal virtual void OnActivate(EventArgs e)
		{
			EventHandler handler = (EventHandler) base.Events[_eventActivate];
			if (handler != null)
			{
				handler(this, e);
			}
		}

		/// <summary>
		/// Raises the <see cref="Deactivate"/> event of the View control. 
		/// </summary>
		protected internal virtual void OnDeactivate(EventArgs e)
		{
			EventHandler handler = (EventHandler) base.Events[_eventDeactivate];
			if (handler != null)
			{
				handler(this, e);
			}
		}

		/// <summary>
		/// Gets a value that indicates, if this control has been instantiated by a designer.
		/// </summary>
		protected bool DesignMode
		{
			get { return Context == null; }
		}

		/// <summary>
		/// Gets or Sets a value that indicates, if this control is currently active.
		/// </summary>
		internal bool Active
		{
			get { return _active; }
			set
			{
				_active = value;
				base.Visible = value;
			}
		}

		/// <summary>
		/// Gets a value that indicates, whether this view will be rendered as UI.
		/// </summary>
		public override bool Visible
		{
			get
			{
				if (Parent == null)
				{
					return Active;
				}
				if (Active)
				{
					return Parent.Visible;
				}
				return false;
			}
			set
			{
				if (!DesignMode)
				{
					throw new InvalidOperationException("View_CannotSetVisible");
				}
			}
		}
	}
#endif // !NET_2_0
}