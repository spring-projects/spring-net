#region License

/*
 * Copyright © 2002-2008 the original author or authors.
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
using System.Web.UI;
using Spring.Context;
using Spring.Web.UI.Validation;

#endregion

namespace Spring.Web.UI.Controls
{
    /// <summary>
    /// Provides common functionality to all validation renderer controls.
    /// </summary>
    /// <author>Erich Eichinger</author>
    public abstract class AbstractValidationControl : Control
    {
        private string provider;
        private IValidationErrorsRenderer renderer;

        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        /// <value>The provider.</value>
        public string Provider
        {
            get
            {
                if (this.provider == null)
                {
                    return this.ID;
                }
                return this.provider;
            }
            set { this.provider = value; }
        }

        /// <summary>
        /// Gets or sets the validation errors renderer to use. 
        /// </summary>
        /// <remarks>
        /// If not explicitly specified, defaults to <see cref="SpanValidationErrorsRenderer"/>.
        /// </remarks>
        /// <value>The validation errors renderer to use.</value>
        public IValidationErrorsRenderer Renderer
        {
            get
            {
                if (this.renderer == null)
                {
                    this.renderer = CreateValidationErrorsRenderer();
                }
                return this.renderer;
            }
            set { this.renderer = value; }
        }

        /// <summary>
        /// Gets the MessageSource to be used for resolve error messages
        /// </summary>
        /// <remarks>
        /// By default, returns <see cref="ValidationContainer"/>'s MessageSource.
        /// </remarks>
        protected virtual IMessageSource MessageSource
        {
            get { return ValidationContainer.MessageSource; }
        }

        /// <summary>
        /// Create the default <see cref="IValidationErrorsRenderer"/> 
        /// for this ValidationControl if none is configured.
        /// </summary>
        protected abstract IValidationErrorsRenderer CreateValidationErrorsRenderer();

        /// <summary>
        /// Gets the <see cref="IValidationContainer"/>, who's <see cref="IValidationContainer.ValidationErrors"/> 
        /// shall be rendered by this control.
        /// </summary>
        protected virtual IValidationContainer ValidationContainer
        {
            get
            {
                for(Control parent=this.Parent; parent!=null; parent=parent.Parent)
                {
                    IValidationContainer container = parent as IValidationContainer;
                    if (container != null)
                    {
                        return container;
                    }                    
                }
                return null;
            }
        }

        /// <summary>
        /// Resolves the <see cref="ValidationContainer"/>'s list of validation errors to a list
        /// of <see cref="string"/> elements containing the error messages to be rendered.
        /// </summary>
        /// <returns>a list containing <see cref="string"/> elements</returns>
        protected virtual IList ResolveErrorMessages()
        {
            IList errorMessages;
            errorMessages = this.ValidationContainer.ValidationErrors.GetResolvedErrors(this.Provider, this.MessageSource);
            return errorMessages;
        }

        /// <summary>
        /// Renders error messages using the specified <see cref="Renderer"/>.
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            IList errorMessages = ResolveErrorMessages();
            Renderer.RenderErrors(Page as Spring.Web.UI.Page, writer, errorMessages);   
        }
    }
}