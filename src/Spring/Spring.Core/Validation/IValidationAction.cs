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

using Spring.Validation.Actions;

namespace Spring.Validation
{
    /// <summary>
    /// An action that should be executed after validator is evaluated.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This interface allows us to define the actions that should be executed
    /// after validation in a generic fashion.
    /// </p>
    /// <p>
    /// For example, addition of error messages to validation errors collection
    /// is performed by one specific implementation of this interface, <see cref="ErrorMessageAction"/>.
    /// </p>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    public interface IValidationAction
    {
        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="isValid">Whether associated validator is valid or not.</param>
        /// <param name="validationContext">Validation context.</param>
        /// <param name="contextParams">Additional context parameters.</param>
        /// <param name="errors">Validation errors container.</param>
        void Execute(bool isValid, object validationContext, IDictionary<string, object> contextParams, IValidationErrors errors);
    }
}
