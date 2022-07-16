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
    /// This interface should be implemented by all validation errors renderers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Validation errors renderers are used to decouple rendering behavior from the
    /// validation errors controls such as <see cref="ValidationError"/> and
    /// <see cref="ValidationSummary"/>.
    /// </para>
    /// <para>
    /// This allows users to change how validation errors are rendered by simply pluggin in
    /// appropriate renderer implementation into the validation errors controls using
    /// Spring.NET dependency injection.
    /// </para>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public interface IValidationErrorsRenderer
    {
        /// <summary>
        /// Renders validation errors using specified <see cref="HtmlTextWriter"/>.
        /// </summary>
        /// <param name="page">Web form instance.</param>
        /// <param name="writer">An HTML writer to use.</param>
        /// <param name="errors">The list of validation errors.</param>
        void RenderErrors(Page page, HtmlTextWriter writer, IList<string> errors);
    }
}
