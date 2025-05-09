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

using Spring.Web.UI.Validation;

namespace Spring.Web.UI.Controls;

/// <summary>
/// This control should be used to display field-level validation errors.
/// </summary>
/// <author>Aleksandar Seovic</author>
/// <author>Jonathan Allenby</author>
public class ValidationError : AbstractValidationControl
{
    /// <summary>
    /// Create the default <see cref="SpanValidationErrorsRenderer"/>
    /// for this ValidationControl if none is configured.
    /// </summary>
    protected override IValidationErrorsRenderer CreateValidationErrorsRenderer()
    {
        return new SpanValidationErrorsRenderer();
    }
}
