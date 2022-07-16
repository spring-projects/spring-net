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

using System.Collections.Specialized;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Spring.Web.UI.Controls
{
    /// <summary>
    /// Groups radio button controls and returns the value of the selected control
    /// </summary>
    /// <remarks>
    /// This control alows radio buttons to be data-bound to a data model of the page.
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    [ParseChildren(false)]
    public class RadioButtonGroup : WebControl, IPostBackDataHandler
    {
        private static readonly object EventSelectionChanged = new object();

        private List<Control> options = new List<Control>();

		/// <summary>
		/// Overloaded to track addition of contained <see cref="RadioButton"/> controls.
		/// </summary>
		protected override void AddedControl(Control control, int index)
		{
			if (control is RadioButton)
			{
				RadioButton option = (RadioButton)control;
				option.GroupName = this.ID + "Group";
				option.AutoPostBack = this.AutoPostBack;
				if(option.Attributes["value"] == null) option.Attributes["value"] = option.ID;
				option.Checked = (0 == string.Compare(this.Value, option.Attributes["value"]));
				options.Add(control);
			}
			base.AddedControl (control, index);
		}

		/// <summary>
		/// Overloaded to track removal of a RadioButton
		/// </summary>
		/// <param name="control"></param>
		protected override void RemovedControl(Control control)
		{
			int index = -1;
			for(int i=0;i<options.Count;i++)
			{
				if (control == options[i]) index = i;
			}
			if (index > -1) options.RemoveAt(index);

			base.RemovedControl (control);
		}

        /// <summary>
        /// Gets or sets the ID of the selected radio button.
        /// </summary>
        /// <value>ID of the selected radio button.</value>
        public string Value
        {
            get { return (string) this.ViewState["value"]; }
            set
            {
                this.ViewState["value"] = value;
                foreach(RadioButton option in this.options)
                {
                    option.Checked = (0==string.Compare(value, option.Attributes["value"]));
                }
            }
        }

        /// <summary>
        /// Gets or sets whether form should be posted back on every selection change
        /// within the radio group.
        /// </summary>
        [DefaultValue(false)]
        public virtual bool AutoPostBack
        {
            get
            {
                object autoPostBack = this.ViewState["AutoPostBack"];
                if (autoPostBack != null)
                {
                    return (bool) autoPostBack;
                }
                return false;
            }
            set
            {
                this.ViewState["AutoPostBack"] = value;
                foreach(RadioButton option in this.options)
                {
                    option.AutoPostBack = value;
                }
            }
        }

		/// <summary>
		/// Registers the RadioButtonGroup for PostBack.
		/// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (this.Visible)
            {
                Page.RegisterRequiresPostBack(this);
            }
        }

        /// <summary>
        /// Renders only children of this control.
        /// </summary>
        /// <param name="writer">HtmlTextWriter to use for rendering.</param>
        protected override void Render(HtmlTextWriter writer)
        {
            this.RenderChildren(writer);
        }

        #region IPostBackDataHandler Members

        /// <summary>
        /// Loads postback data into the control.
        /// </summary>
        /// <param name="postDataKey">Key that should be used to retrieve data.</param>
        /// <param name="postCollection">Postback data collection.</param>
        /// <returns>True if data has changed, false otherwise.</returns>
        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            string newValue = postCollection[postDataKey+"Group"];

            if (0 != string.Compare(newValue,this.Value))
            {
                this.ViewState["value"] = newValue;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Raises SelectionChanged event.
        /// </summary>
        public void RaisePostDataChangedEvent()
        {
            OnSelectionChanged(EventArgs.Empty);
        }

        #endregion

        /// <summary>
        /// Method that is called on postback if selected radio button has changed.
        /// </summary>
        /// <param name="e">Empty event argument.</param>
        protected virtual void OnSelectionChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[RadioButtonGroup.EventSelectionChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Occurs when the value of the radio button group changes between postbacks to the server.
        /// </summary>
        public event EventHandler SelectionChanged
        {
            add { base.Events.AddHandler(RadioButtonGroup.EventSelectionChanged, value); }
            remove { base.Events.RemoveHandler(RadioButtonGroup.EventSelectionChanged, value); }
        }


    }
}
