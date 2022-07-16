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

#region Imports

using System.Collections.Specialized;
using System.Web.UI;
using System.Web.UI.WebControls;
using Spring.Util;

#endregion

namespace Spring.Web.UI.Controls
{
    /// <summary>
    /// Displays a pop-up DHTML calendar.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Credit: this control uses a slightly modified version of the
    /// <a href="http://www.dynarch.com/projects/calendar/">Dynarch.com DHTML Calendar</a>,
    /// written by Mihai Bazon.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    [ValidationProperty("SelectedDate")]
    public class Calendar : WebControl, IPostBackDataHandler
    {
        private const string AllowEditingViewStateKey = "AllowEditing";
        private const string DateFormatViewStateKey = "Format";
        private const string SelectedDateViewStateKey = "SelectedDate";
        private static readonly object EventDateChanged = new object();

        private string skin;

        /// <summary>
        /// Registers necessary scripts and stylesheet.
        /// </summary>
        /// <param name="e">
        /// An <see cref="System.EventArgs"/> object that contains the event data.
        /// </param>
        protected override void OnPreRender(EventArgs e)
        {
            if (skin != null)
            {
                Page.RegisterStyleFile("CalendarStyle", WebUtils.CreateAbsolutePath(Page.ScriptsRoot, "Calendar/calendar-" + skin + ".css"));
            }
            Page.RegisterHeadScriptFile("Calendar", WebUtils.CreateAbsolutePath(Page.ScriptsRoot, "Calendar/calendar.js"));
            Page.RegisterHeadScriptFile("CalendarLanguage", WebUtils.CreateAbsolutePath(Page.ScriptsRoot, "Calendar/lang/calendar-" + Page.UserCulture.Name + ".js"));
            Page.RegisterHeadScriptFile("CalendarSetup", WebUtils.CreateAbsolutePath(Page.ScriptsRoot, "Calendar/calendar-setup.js"));
        }

        /// <summary>
        /// Gets a reference to the <see cref="Spring.Web.UI.Page"/> instance that contains the
        /// server control.
        /// </summary>
        /// <value>
        /// A reference to the <see cref="Spring.Web.UI.Page"/> instance that contains the
        /// server control.
        /// </value>
        private new Page Page
        {
            get { return base.Page as Page; }
        }

        /// <summary>
        /// The selected date.
        /// </summary>
        /// <value>The selected date.</value>
        public DateTime SelectedDate
        {
            get
            {
                if (this.ViewState[SelectedDateViewStateKey] == null)
                {
                    return DateTime.Now;
                }
                return (DateTime)this.ViewState[SelectedDateViewStateKey];
            }
            set { this.ViewState[SelectedDateViewStateKey] = value; }
        }

        /// <summary>
        /// The date format that is to be used.
        /// </summary>
        /// <value>A valid date format string.</value>
        public string Format
        {
            get
            {
                if (this.ViewState[DateFormatViewStateKey] == null)
                {
                    return Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern;
                }
                return (string)this.ViewState[DateFormatViewStateKey];
            }
            set { this.ViewState[DateFormatViewStateKey] = value; }
        }

        /// <summary>
        /// Is direct editing of the date allowed?
        /// </summary>
        /// <value>
        /// <see lang="true"/> if direct editing of the date is allowed?</value>
        public bool AllowEditing
        {
            get
            {
                if (this.ViewState[AllowEditingViewStateKey] == null)
                {
                    return true;
                }
                return (bool)this.ViewState[AllowEditingViewStateKey];
            }
            set { this.ViewState[AllowEditingViewStateKey] = value; }
        }

        /// <summary>
        /// The (CSS) style.
        /// </summary>
        /// <value>The style for the calendar.</value>
        public string Skin
        {
            get { return skin; }
            set { skin = value; }
        }

        /// <summary>
        /// Renders a hidden input field that stores the value for the radio button group.
        /// </summary>
        /// <param name="writer">
        /// <see cref="System.Web.UI.HtmlTextWriter"/> to use for rendering.
        /// </param>
        protected override void Render(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");

            writer.RenderBeginTag(HtmlTextWriterTag.Table);
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            RenderTextBox(writer);

            writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            if (Enabled)
            {
                RenderButton(writer);
            }

            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();

            RenderSetupScript(writer);
        }

        private void RenderTextBox(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID);
            writer.AddAttribute(HtmlTextWriterAttribute.Value, (SelectedDate != DateTime.MinValue ? SelectedDate.ToString(Format) : ""));

            if (!AllowEditing)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.ReadOnly, "readonly");
            }

            base.AddAttributesToRender(writer);


            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
        }

        private void RenderButton(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID + "_button");
            writer.AddAttribute(HtmlTextWriterAttribute.Src, WebUtils.CreateAbsolutePath(Page.ScriptsRoot, "Calendar/img.gif"));
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");

            writer.RenderBeginTag(HtmlTextWriterTag.Img);
            writer.RenderEndTag();
        }

        private void RenderSetupScript(HtmlTextWriter writer)
        {
            writer.AddAttribute("type", "text/javascript");

            writer.RenderBeginTag(HtmlTextWriterTag.Script);
            writer.WriteLine("Calendar.setup({");
            writer.WriteLine("    inputField : \"" + ClientID + "\",");
            writer.WriteLine("    button     : \"" + ClientID + "_button\"");
            writer.WriteLine("});");
            writer.RenderEndTag();
        }

        #region IPostBackDataHandler Members

        /// <summary>
        /// Raises the <c>SelectionChanged</c> event.
        /// </summary>
        public void RaisePostDataChangedEvent()
        {
            OnDateChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Loads postback data into the control.
        /// </summary>
        /// <param name="postDataKey">
        /// The key that should be used to retrieve data.
        /// </param>
        /// <param name="postCollection">The postback data collection.</param>
        /// <returns><see lang="true"/> if data has changed.</returns>
        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            DateTime dateValue;
            string dateString = postCollection[postDataKey];
            if (StringUtils.HasText(dateString))
            {
                try
                {
                    dateValue = DateTime.Parse(dateString);
                }
                catch (FormatException)
                {
                    dateValue = DateTime.MinValue;
                }
            }
            else
            {
                dateValue = DateTime.MinValue;
            }
            bool changed = dateValue != this.SelectedDate;
            if (changed)
            {
                this.ViewState[SelectedDateViewStateKey] = dateValue;
            }
            return changed;
        }

        #endregion

        /// <summary>
        /// The method that is called on postback if the date has changed.
        /// </summary>
        /// <param name="e">
        /// The event argument (empty and unused).
        /// </param>
        protected virtual void OnDateChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler)base.Events[EventDateChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Occurs when the value of the radio button group changes between postbacks to the server.
        /// </summary>
        public event EventHandler DateChanged
        {
            add { base.Events.AddHandler(EventDateChanged, value); }
            remove { base.Events.RemoveHandler(EventDateChanged, value); }
        }
    }
}
