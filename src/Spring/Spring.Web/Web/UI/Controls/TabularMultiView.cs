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

using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

#endregion

namespace Spring.Web.UI.Controls
{
	/// <summary>
	/// The <see cref="TabularMultiView"/> control allows you to build ASP.NET Web pages that present
	/// the user with content arranged in tabular form.
	/// </summary>
	/// <author>Erich Eichinger</author>
	[ToolboxData("<{0}:TabularMultiView runat=\"server\"></{0}:TabularMultiView>")]
	[ParseChildren(false)]
	public class TabularMultiView : WebControl
	{
		#region Style Properties

		private string m_MenuStyle = "TabMenu";
		private string m_BodyStyle = "TabBody";
		private string m_TabItemStyle = "TabItem";
		private string m_TabSelectedItemStyle = "TabSelectedItem";

		/// <summary>
		/// Set the style class of the panel containing the Tabs.
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue(typeof(string), "TabMenu")]
		public string TabularMenuCSS
		{
			get { return m_MenuStyle; }
			set { m_MenuStyle = value; }
		}

		/// <summary>
		/// Set the style class of each Tab item.
		/// </summary>
		[Bindable(true), DefaultValue(typeof(string), "TabItem"), Category("Appearance")]
		public string TabularMenuItemCSS
		{
			get { return (m_TabItemStyle); }
			set { m_TabItemStyle = value; }
		}

		/// <summary>
		/// Set the style class of the currently selected Tab item.
		/// </summary>
		[Bindable(true), DefaultValue(typeof(string), "TabSelectedItem"), Category("Appearance")]
		public string TabularMenuSelectedItemCSS
		{
			get { return m_TabSelectedItemStyle; }
			set { m_TabSelectedItemStyle = value; }
		}

		/// <summary>
		/// Set the style class of the panel containing all <see cref="TabularView"/> controls.
		/// </summary>
		[Bindable(true), Category("Appearance"), DefaultValue(typeof(string), "TabBody")]
		public string TabularBodyCSS
		{
			get { return m_BodyStyle; }
			set { m_BodyStyle = value; }
		}

		#endregion

		#region Public Members

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public TabularMultiView()
			: this(HtmlTextWriterTag.Div)
		{
		}

		/// <summary>
		/// Initializes a new instance with the given container tag to be used for rendering.
		/// </summary>
		protected TabularMultiView(HtmlTextWriterTag containerTag)
			: base(containerTag)
		{
		}

		/// <summary>
		/// Gets or sets the index of the active View control within a <see cref="TabularMultiView"/> control.
		/// </summary>
		public int ActiveTabIndex
		{
			get
			{
				if (!_controlStateInitialized)
				{
					return _activeViewIndexCached;
				}
				else
				{
					return _multiView.ActiveViewIndex;
				}
			}
			set
			{
				if (!_controlStateInitialized)
				{
					_activeViewIndexCached = value;
				}
				else
				{
					_multiView.ActiveViewIndex = value;
				}
			}
		}

		/// <summary>
		/// Occurs, if the active tab has changed.
		/// </summary>
		public event EventHandler ActiveTabChanged
		{
			add { base.Events.AddHandler(_eventActiveTabChanged, value); }
			remove { base.Events.RemoveHandler(_eventActiveTabChanged, value); }
		}

		#endregion Public Members

		#region Customizable Members

		/// <summary>
		/// Create the container for tab items.
		/// </summary>
		protected virtual TabContainer CreateTabContainer()
		{
			return new TabContainer();
		}

		/// <summary>
		/// Creates TabContainer and MultiView
		/// </summary>
		protected virtual Control CreateContent(TabContainer menu, MultiView body)
		{
			Control content = new Control();

			WebControl menuPanel = new WebControl(HtmlTextWriterTag.Div);
			menuPanel.CssClass = TabularMenuCSS;
			menuPanel.Controls.Add(menu);
			content.Controls.Add(menuPanel);

			WebControl bodyPanel = new WebControl(HtmlTextWriterTag.Div);
			bodyPanel.CssClass = TabularBodyCSS;
			bodyPanel.Controls.Add(body);
			content.Controls.Add(bodyPanel);

			return content;
		}

		#endregion

		#region Fields

		private static readonly object _eventActiveTabChanged = new object();

		private int _activeViewIndexCached = -1;
		private bool _controlStateInitialized = false;

		private TabContainer _tabContainer;
		private MultiView _multiView;

		/// <summary>
		/// keeps parsed views until multiView is created
		/// </summary>
		private ArrayList _parsedViews = new ArrayList();

		#endregion

		/// <summary>
		/// Initialize this control.
		/// </summary>
		protected override void OnInit(EventArgs e)
		{
			BuildControlTree();

			base.OnInit(e);
		}

		private void BuildControlTree()
		{
			_controlStateInitialized = true;

			EnsureChildControls();
		}

		/// <summary>
		/// Creates child controls.
		/// </summary>
		protected override void CreateChildControls()
		{
			// create menu tabstrip
			_tabContainer = CreateTabContainer();
			_tabContainer.Click += OnSelectTabCommand;

			// create multiview container
			_multiView = new MultiView();
			_multiView.ActiveViewChanged += OnActiveViewChanged;

			// add views previously parsed
			for (int i = 0; i < _parsedViews.Count; i++) _multiView.Controls.Add((Control) _parsedViews[i]);
			_parsedViews = null;

			// select defined view
			if (_activeViewIndexCached != -1)
			{
				_multiView.ActiveViewIndex = _activeViewIndexCached;
				_activeViewIndexCached = -1;
			}

			// create content pane
			Control content = CreateContent(_tabContainer, _multiView);
			Controls.Add(content);

			RebuildTabs();
		}

		/// <summary>
		/// Adds the element to the collection of child controls.
		/// </summary>
		protected override void AddParsedSubObject(object obj)
		{
			// remember parsed views for later
			if (obj is TabularView)
			{
				_parsedViews.Add(obj);
			}
			else if (!(obj is LiteralControl))
			{
				throw new HttpException(
					string.Format("TabularMultiView_cannot_have_children_of_type '{0}'", obj.GetType().FullName));
			}
		}

		/// <summary>
		/// Called if ActiveViewIndex is changed
		/// </summary>
		private void RebuildTabs()
		{
			_tabContainer.Controls.Clear();

			ViewCollection views = _multiView.Views;
			for (int i = 0; i < views.Count; i++)
			{
				TabularView view = (TabularView) views[i];
				_tabContainer.CreateTab(this, view, i);
			}
		}

		private void OnSelectTabCommand(object sender, TabCommandEventArgs e)
		{
			int selectedIndex = e.TabIndex;
			if (selectedIndex != _multiView.ActiveViewIndex)
			{
				// will trigger OnActiveViewChanged
				_multiView.ActiveViewIndex = selectedIndex;
				RebuildTabs();
			}
		}

		private void OnActiveViewChanged(object sender, EventArgs e)
		{
			EventHandler handler = (EventHandler) base.Events[_eventActiveTabChanged];
			if (handler != null)
			{
				handler(this, e);
			}
		}
	}
}
