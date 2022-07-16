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

using System.Runtime.Serialization;

namespace Spring.Validation
{
    /// <summary>
    /// Thrown by the validation advice if the method parameters validation fails.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [Serializable]
    public class ValidationException : Exception
    {
        private readonly IValidationErrors errors;

        /// <summary>
        /// Creates a new instance of the ValidationException class.
        /// </summary>
        public ValidationException()
        {
        }

        /// <summary>
        /// Creates a new instance of the ValidationException class with
        /// specified validation errors.
        /// </summary>
        /// <param name="errors">
        /// Validation errors.
        /// </param>
        public ValidationException(IValidationErrors errors)
        {
            this.errors = errors;
        }

        /// <summary>
        /// Creates a new instance of the ValidationException class with the
        /// specified message.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        public ValidationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the ValidationException class with the
        /// specified message and validation errors.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="errors">
        /// Validation errors.
        /// </param>
        public ValidationException(string message, IValidationErrors errors)
            : base(message)
        {
            this.errors = errors;
        }

        /// <summary>
        /// Creates a new instance of the ValidationException class with the
        /// specified message and root cause.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public ValidationException(string message, Exception rootCause)
            : base(message, rootCause)
        {
        }

        /// <summary>
        /// Creates a new instance of the ValidationException class with the
        /// specified message, root cause and validation errors.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        /// <param name="errors">
        /// Validation errors.
        /// </param>
        public ValidationException(string message, Exception rootCause, IValidationErrors errors)
            : base(message, rootCause)
        {
            this.errors = errors;
        }

        /// <summary>
        /// Creates a new instance of the ValidationException class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        protected ValidationException (
            SerializationInfo info, StreamingContext context)
            : base (info, context)
        {
            this.errors = (IValidationErrors) info.GetValue("errors", typeof (IValidationErrors));
        }

        /// <summary>
        /// Implements object serialization.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("errors", this.errors);
        }

        /// <summary>
        /// Gets validation errors.
        /// </summary>
        /// <value>Validation errors.</value>
        public IValidationErrors ValidationErrors
        {
            get { return errors; }
        }
    }
}
