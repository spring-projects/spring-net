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

using System.Text;
using System.Web.UI;

namespace Spring.Web.UI.Validation
{
    /// <summary>
    /// Implementation of <see cref="IValidationErrorsRenderer"/> that
    /// displays an error image to let user know there is an error, and
    /// tooltip to display actual error messages.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This renderer's behavior is similar to Windows Forms error provider.
    /// </para>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class IconValidationErrorsRenderer : AbstractValidationErrorsRenderer
    {
        private string iconSrc;

        /// <summary>
        /// Gets or sets the name of the image file to use as an error icon.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Image name should be relative to the value of the <see cref="Spring.Web.UI.Page.ImagesRoot"/>
        /// property, and should not use leading path separator.
        /// </para>
        /// </remarks>
        /// <value>The name of the image file to use as an error icon.</value>
        public string IconSrc
        {
            get { return this.iconSrc; }
            set { this.iconSrc = value; }
        }

        /// <summary>
        /// Renders validation errors using specified <see cref="HtmlTextWriter"/>.
        /// </summary>
        /// <param name="page">Web form instance.</param>
        /// <param name="writer">An HTML writer to use.</param>
        /// <param name="errors">The list of validation errors.</param>
        public override void RenderErrors(Page page, HtmlTextWriter writer, IList<string> errors)
        {
            if (errors != null && errors.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                string separator = "";
                foreach (string error in errors)
                {
                    sb.Append(separator).Append(error);
                    separator = "\n";
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Src, page.ImagesRoot + "/" + IconSrc);
                writer.AddAttribute(HtmlTextWriterAttribute.Title, sb.ToString());

                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
            }
        }
    }
}
