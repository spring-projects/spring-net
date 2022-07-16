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

using System.Collections;
using Spring.Expressions;
using Spring.Util;

namespace Spring.Validation.Actions
{
    /// <summary>
    /// Implementation of <see cref="IValidationAction"/> that adds error message
    /// to the validation errors container.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class ErrorMessageAction : BaseValidationAction
    {
        private string messageId;
        private IExpression[] messageParams;
        private string[] providers;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorMessageAction"/> class.
        /// </summary>
        /// <param name="messageId">Error message resource identifier.</param>
        /// <param name="providers">Names of the error providers this message should be added to.</param>
        public ErrorMessageAction(string messageId, params string[] providers)
        {
            AssertUtils.ArgumentHasText(messageId, "messageId");
            if (providers == null || providers.Length == 0)
            {
                throw new ArgumentException("At least one error provider has to be specified.", "providers");
            }

            this.messageId = messageId;
            this.providers = providers;
        }

        /// <summary>
        /// Sets the expressions that should be resolved to error message parameters.
        /// </summary>
        /// <value>The expressions that should be resolved to error message parameters.</value>
        public IExpression[] Parameters
        {
            set { messageParams = value; }
        }

        /// <summary>
        /// Called when associated validator is invalid.
        /// </summary>
        /// <param name="validationContext">Validation context.</param>
        /// <param name="contextParams">Additional context parameters.</param>
        /// <param name="errors">Validation errors container.</param>
        protected override void OnInvalid(object validationContext, IDictionary<string, object> contextParams, IValidationErrors errors)
        {
            ErrorMessage error = CreateErrorMessage(validationContext, contextParams);
            foreach (string provider in this.providers)
            {
                errors.AddError(provider.Trim(), error);
            }
        }

        /// <summary>
        /// Resolves the error message.
        /// </summary>
        /// <param name="validationContext">Validation context to resolve message parameters against.</param>
        /// <param name="contextParams">Additional context parameters.</param>
        /// <returns>Resolved error message</returns>
        private ErrorMessage CreateErrorMessage(object validationContext, IDictionary<string, object> contextParams)
        {
            if (messageParams != null && messageParams.Length > 0)
            {
                object[] parameters = ResolveMessageParameters(messageParams, validationContext, contextParams);
                return new ErrorMessage(messageId, parameters);
            }
            else
            {
                return new ErrorMessage(messageId, null);
            }
        }

        /// <summary>
        /// Resolves the message parameters.
        /// </summary>
        /// <param name="messageParams">List of parameters to resolve.</param>
        /// <param name="validationContext">Validation context to resolve parameters against.</param>
        /// <param name="contextParams">Additional context parameters.</param>
        /// <returns>Resolved message parameters.</returns>
        private object[] ResolveMessageParameters(IList messageParams, object validationContext, IDictionary<string, object> contextParams)
        {
            object[] parameters = new object[messageParams.Count];
            for (int i = 0; i < messageParams.Count; i++)
            {
                parameters[i] = ((IExpression) messageParams[i]).GetValue(validationContext, contextParams);
            }

            return parameters;
        }


    }
}
