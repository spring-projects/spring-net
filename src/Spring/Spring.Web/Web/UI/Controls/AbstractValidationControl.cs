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

using System.Web.UI;
using Spring.Context;
using Spring.Util;
using Spring.Validation;
using Spring.Web.UI.Validation;

namespace Spring.Web.UI.Controls
{
    /// <summary>
    /// Provides common functionality to all validation renderer controls.
    /// </summary>
    /// <author>Erich Eichinger</author>
    public abstract class AbstractValidationControl : Control
    {
        private string _provider;
        private string _validationContainerName;
        private IValidationErrorsRenderer renderer;
        private IValidationErrors _validationErrors;
        private IMessageSource _messageSource;

        /// <summary>
        /// Set a particular message source to be used for
        /// resolving error messages to display texts.
        /// </summary>
        /// <remarks>
        /// If not set, the control will probe the control hierarchy
        /// for containing controls implementing <see cref="IValidationContainer"/>
        /// and use the container's <see cref="IValidationContainer.MessageSource"/>.
        /// </remarks>
        public IMessageSource MessageSource
        {
            get { return _messageSource; }
            set { _messageSource = value; }
        }

        /// <summary>
        /// Allows to set a particular instance of the validation errors
        /// collection to render.
        /// </summary>
        /// <remarks>
        /// If not set, the control will probe the control hierarchy for
        /// containing controls implementing <see cref="IValidationContainer"/>
        /// and use the container's <see cref="IValidationContainer.ValidationErrors"/>
        /// </remarks>
        public IValidationErrors ValidationErrors
        {
            get { return _validationErrors; }
            set { _validationErrors = value; }
        }

        /// <summary>
        /// If set, <see cref="FindValidationContainer"/> will resolve to the named control specified
        /// by this property. The behavior of name resolution is identical to
        /// <see cref="System.Web.UI.WebControls.BaseValidator.ControlToValidate"/>, except that if the name
        /// starts with "::", the resolution will start at the page level instead of relative to this
        /// control
        /// </summary>
        public string ValidationContainerName
        {
            get { return _validationContainerName; }
            set { _validationContainerName = value; }
        }

        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        /// <value>The provider.</value>
        public virtual string Provider
        {
            get
            {
                if (this._provider == null)
                {
                    this._provider = this.ID;
                    if (this._provider == null)
                    {
                        this._provider = string.Empty;
                    }
                }
                return this._provider;
            }
            set
            {
                AssertUtils.ArgumentNotNull(value, "Provider");
                this._provider = value;
            }
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
                    AssertUtils.ArgumentNotNull(this.renderer, "Renderer", "CreateValidationErrorsRenderer must not return null");
                }
                return this.renderer;
            }
            set
            {
                AssertUtils.ArgumentNotNull(value, "Renderer");
                this.renderer = value;
            }
        }

        /// <summary>
        /// Create the default <see cref="IValidationErrorsRenderer"/>
        /// for this ValidationControl if none is configured.
        /// </summary>
        protected abstract IValidationErrorsRenderer CreateValidationErrorsRenderer();

        /// <summary>
        /// Gets the MessageSource to be used for resolve error messages
        /// </summary>
        /// <remarks>
        /// By default, returns <see cref="FindValidationContainer"/>'s MessageSource.
        /// </remarks>
        /// <returns>the <see cref="IMessageSource"/> to resolve message texts. May be <c>null</c></returns>
        protected virtual IMessageSource ResolveMessageSource()
        {
            IMessageSource messageSource = this.MessageSource;
            if (messageSource == null)
            {
                IValidationContainer validationContainer = FindValidationContainer();
                messageSource = (validationContainer == null)
                                ? null
                                : validationContainer.MessageSource;
            }
            return messageSource;
        }

        /// <summary>
        /// Gets the list of validation errors to render
        /// </summary>
        /// <returns>the <see cref="IValidationErrors"/> to render. May be <c>null</c></returns>
        protected virtual IValidationErrors ResolveValidationErrors()
        {
            IValidationErrors validationErrors = this.ValidationErrors;

            if (validationErrors == null)
            {
                IValidationContainer container = this.FindValidationContainer();
                if (container != null)
                {
                    validationErrors = container.ValidationErrors;
                }
            }
            return validationErrors;
        }

        /// <summary>
        /// Gets the <see cref="IValidationContainer"/>, who's <see cref="IValidationContainer.ValidationErrors"/>
        /// shall be rendered by this control.
        /// </summary>
        /// <remarks>
        /// First, it tries to resolve the specified <see cref="ValidationContainerName"/>, if any. If no explicit name
        /// is set, will probe the control hierarchy for controls implementing <see cref="IValidationContainer"/>.
        /// </remarks>
        protected virtual IValidationContainer FindValidationContainer()
        {
            // is an explicit container specified?
            if (ValidationContainerName != null && ValidationContainerName.Length > 0)
            {
                Control start = this.NamingContainer;
                string containerName = this.ValidationContainerName;
                // shall we do a global search?
                if (containerName.StartsWith("::"))
                {
                    containerName = containerName.Substring(2);
                    start = this.Page;
                }
                IValidationContainer container = start as IValidationContainer;
                if (containerName.Length > 0)
                {
                    container = start.FindControl(containerName) as IValidationContainer;
                }
                if (container == null)
                {
                    throw new ArgumentException(
                        string.Format(
                            "Validation Container Control specified by {0} does not exist or does not implement IValidationContainer",
                            this.ValidationContainerName));
                }
                return container;
            }

            for (Control parent = this.Parent; parent != null; parent = parent.Parent)
            {
                IValidationContainer container = parent as IValidationContainer;
                if (container != null
                    && container.ValidationErrors != null)
                {
                    return container;
                }
            }
            return null;
        }

        /// <summary>
        /// Resolves the list of validation errors either explicitely specified using
        /// <see cref="ValidationErrors"/> or obtained from the containing <see cref="IValidationContainer"/>
        /// resolved by <see cref="FindValidationContainer"/> to a list
        /// of <see cref="string"/> elements containing the error messages to be rendered.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The list of validation errors may either be explicitely specified using <see cref="ValidationErrors"/>
        /// or will automatically be obtained from the containing <see cref="IValidationContainer"/> resolved by
        /// <see cref="FindValidationContainer"/>.
        /// </para>
        /// <para>
        /// Error Messages are resolved using either an explicitely specified <see cref="MessageSource"/> or the
        /// <see cref="IMessageSource"/> obtained from the validation container.
        /// </para>
        /// </remarks>
        /// <returns>a list containing <see cref="string"/> elements. May return <c>null</c></returns>
        protected virtual IList<string> ResolveErrorMessages()
        {
            IList<string> errorMessages;

            // good catch - idea & patch from Roberto Paterlini
            if (DesignMode)
            {
                errorMessages = new string[] { GetType().Name + ":" + ID };
                return errorMessages;
            }

            IValidationErrors validationErrors = ResolveValidationErrors();
            if (validationErrors == null)
            {
                return null;
            }
            IMessageSource messageSource = this.ResolveMessageSource();

            errorMessages = validationErrors.GetResolvedErrors(this.Provider, messageSource);
            return errorMessages;
        }

        /// <summary>
        /// Renders error messages using the specified <see cref="Renderer"/>.
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            IList<string> errorMessages = ResolveErrorMessages();

            Renderer.RenderErrors(Page as Page, writer, errorMessages);
        }
    }
}
