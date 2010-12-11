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

using System.ComponentModel;
using System.Web.UI;

namespace Spring.Web.UI.Controls
{
    /// <summary>
    /// Represents ContentPlaceHolder control that can be used to define placeholders
    /// within the master page.
    /// </summary>
    /// <remarks>
    /// Any content defined within this control will be treated as a default content 
    /// for the placeholder and will be rendered unless the child page overrides it
    /// by defining matching <see cref="Spring.Web.UI.Controls.Content"/> control.
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
#if NET_2_0
	public class ContentPlaceHolder : System.Web.UI.WebControls.ContentPlaceHolder
	{}
#else
    [Designer("System.Web.UI.Design.ReadWriteControlDesigner, System.Design")]
    [PersistChildren(true)]
    [ParseChildren(false)]
    public class ContentPlaceHolder : Control
    {
        private Content content;

        /// <summary>
        /// Association to content control defined in the child page
        /// </summary>
        public Content Content
        {
            get { return content; }
            set { content = value; }
        }

        /// <summary>
        /// Renders either its own (default) content or content defined if the associated
        /// <see cref="Spring.Web.UI.Controls.Content"/> control.
        /// </summary>
        /// <param name="writer">HtmlTextWriter that should be used for output</param>
        protected override void Render(HtmlTextWriter writer)
        {
            if (content == null)
            {
                base.Render(writer);
            }
            else
            {
                content.RenderControl(writer);
            }
        }
    }
#endif	
}