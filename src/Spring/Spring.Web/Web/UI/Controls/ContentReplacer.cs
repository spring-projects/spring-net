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

using System.Reflection;
using System.Web.UI;
using Common.Logging;

namespace Spring.Web.UI.Controls
{
    /// <summary>
    /// Represents Content control that can be used to populate or override placeholders
    /// anywhere within the page.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Any content defined within this control will override default content
    /// in the matching control specified by <see cref="ContentPlaceHolderID"/> anywhere
    /// within the page.
    /// </para>
    /// <para>
    /// In contrast to <see cref="Content"/> control, ContentReplacer can replace the content of
    /// any control within the current page - it is not limited to replacing ContentPlaceholders on master pages.
    /// </para>
    /// <para>
    /// This technique is useful if you want to group e.g. rendering navigation elements on 1 ascx control, but your
    /// design requires navigation elements to be distributed across different places within the HTML code.
    /// </para>
    /// </remarks>
    /// <author>Erich Eichinger</author>
    public class ContentReplacer : Control
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ContentReplacer));

        private string contentPlaceHolderID;

        /// <summary>
        /// Specifies the unique id of the control, who's content is to be replaced.
        /// </summary>
        public string ContentPlaceHolderID
        {
            get { return contentPlaceHolderID; }
            set { contentPlaceHolderID = value; }
        }

        /// <summary>
        /// Overriden to correctly redirect rendermethod calls.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

			// if our container is not visible, don't replace placeholder's content
			if (!Visible) return;

            //log.Debug(string.Format("OnPreRender Content['{0}']", this.contentPlaceHolderID));
            Control ctlRoot = Page.Master != null ? Page.Master : (Control) Page;

            Control ctl = ctlRoot.FindControl(this.contentPlaceHolderID);
            if (ctl != null)
            {
                log.Debug(string.Format("OnPreRender Content['{0}'] found placeholder - replacing RenderMethod",this.contentPlaceHolderID));

                RenderMethod myRenderMethod = GetRenderMethod();
                //log.Debug(string.Format("OnPreRender Content['{0}'] renderMethod found={1}", this.contentPlaceHolderID,(myRenderMethod != null ? "true" : "false")));

                // prevent content control from rendering itself
                this.SetRenderMethodDelegate(new RenderMethod(RenderNothing));
                if (myRenderMethod == null)
                {
                    myRenderMethod = new RenderMethod(RenderChildControls);
                }
                // instead replace placeholder's rendermethod to render this control's content
                ctl.SetRenderMethodDelegate(myRenderMethod);
            }
            else
            {
                throw new ArgumentException(string.Format("No ContentPlaceHolder with id '{0}' defined on this page.", this.contentPlaceHolderID));
            }
        }

        /// <summary>
        /// Renders child controls
        /// </summary>
        private void RenderChildControls(HtmlTextWriter output, Control container)
        {
            if (this.HasControls())
            {
                foreach (Control ctl in this.Controls)
                {
                    ctl.RenderControl(output);
                }
            }
        }

        /// <summary>
        /// Render nothing.
        /// </summary>
        private void RenderNothing(HtmlTextWriter output, Control container)
        {
            // do nothing
        }

        private static readonly PropertyInfo piRareFieldsEnsured =
            typeof(Control).GetProperty("RareFieldsEnsured", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo fiRenderMethod =
            typeof(Control).GetNestedType("ControlRareFields",BindingFlags.NonPublic).GetField("RenderMethod");

        private RenderMethod GetRenderMethod()
        {
            object o = piRareFieldsEnsured.GetValue(this, null);
            RenderMethod myRenderMethod = (RenderMethod) fiRenderMethod.GetValue(o);
            return myRenderMethod;
        }
    }
}
