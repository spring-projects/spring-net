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

namespace Spring.Validation
{
    /// <summary>
    /// An object that can validate application-specific objects.
    /// </summary>
    /// <remarks>
    /// <p>
    /// The primary motivation for this interface is to enable validation to be
    /// decoupled from the (user) interface and placed in business objects.
    /// </p>
    /// <p>
    /// Application developers writing their own custom
    /// <see cref="Spring.Validation.IValidator"/> implementations will
    /// typically not implement this interface directly. In most cases, custom
    /// validators woud be better served deriving from the
    /// <see lang="abstract"/> <see cref="BaseValidator"/> class, with the
    /// custom validation ligic being implemented in an override of the
    /// <see lang="abstract"/>
    /// <see cref="BaseValidator.Validate(object, IValidationErrors)"/>
    /// template method.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    /// <seealso cref="BaseValidator"/>
    public interface IValidator
    {
        /// <summary>
        /// Validates the specified object.
        /// </summary>
        /// <param name="validationContext">The object to validate.</param>
        /// <param name="errors">
        /// The <see cref="ValidationErrors"/> instance to add any error
        /// messages to in the case of validation failure.
        /// </param>
        /// <returns>
        /// <see lang="true"/> if validation was successful.
        /// </returns>
        bool Validate(object validationContext, IValidationErrors errors);

        /// <summary>
        /// Validates the specified object.
        /// </summary>
        /// <param name="validationContext">The object to validate.</param>
        /// <param name="contextParams">Additional context parameters.</param>
        /// <param name="errors">
        ///   The <see cref="ValidationErrors"/> instance to add any error
        ///   messages to in the case of validation failure.
        /// </param>
        /// <returns>
        /// <see lang="true"/> if validation was successful.
        /// </returns>
        bool Validate(object validationContext, IDictionary<string, object> contextParams, IValidationErrors errors);

    }
}
