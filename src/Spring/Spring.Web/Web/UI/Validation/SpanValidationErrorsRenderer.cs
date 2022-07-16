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

using System.Web.UI;

using Spring.Web.UI.Controls;

namespace Spring.Web.UI.Validation
{
    /// <summary>
    /// Implementation of <see cref="IValidationErrorsRenderer"/> that renders
    /// validation errors within a <c>span</c> element, using breaks between the
    /// errors.
    /// </summary>
    /// <remarks>
    /// This renderer's behavior is consistent with standard ASP.NET behavior of
    /// the control validators, and is used as the default renderer for Spring.NET
    /// <see cref="ValidationError"/> control.
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public class SpanValidationErrorsRenderer : AbstractValidationErrorsRenderer
    {
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
                if (CssClass != null)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClass);
                }

                writer.RenderBeginTag(HtmlTextWriterTag.Span);

                foreach (string error in errors)
                {
                    writer.Write(error);
                    writer.Write(" ");
                }
                writer.RenderEndTag();

            }
        }
    }
}
