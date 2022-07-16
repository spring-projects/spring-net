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

namespace Spring.Web.UI.Validation
{
    /// <summary>
    /// This class provides common members for all validation errors renderers.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public abstract class AbstractValidationErrorsRenderer : IValidationErrorsRenderer
    {
        private string cssClass;

        /// <summary>
        /// Gets or sets the name of the CSS class that should be used.
        /// </summary>
        /// <value>
        /// The name of the CSS class that should be used
        /// </value>
        public string CssClass
        {
            get { return this.cssClass; }
            set { this.cssClass = value; }
        }

        /// <summary>
        /// Renders validation errors using specified <see cref="HtmlTextWriter"/>.
        /// </summary>
        /// <param name="page">Web form instance.</param>
        /// <param name="writer">An HTML writer to use.</param>
        /// <param name="errors">The list of validation errors.</param>
        public abstract void RenderErrors(Page page, HtmlTextWriter writer, IList<string> errors);
    }
}
