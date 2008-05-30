#region Licence

/*
 * Copyright © 2002-2005 the original author or authors.
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

using System;
using System.Runtime.Serialization;

#endregion

namespace SpringAir.Service
{
    /// <summary>
    /// Thrown in response to a failure to confirm a
    /// <see cref="SpringAir.Domain.Reservation"/>.
    /// </summary>
    /// <remarks>
    /// <p>
    /// This could be for any number of reasons: the backend database is down, the
    /// reservation has been rejected, etc.
    /// </p>
    /// </remarks>
    /// <author>Rick Evans</author>
    /// <version>$Id: CannotConfirmReservationException.cs,v 1.1 2005/10/02 00:06:29 springboy Exp $</version>
    [Serializable]
    public class CannotConfirmReservationException : ApplicationException
    {
        /// <summary>
        /// Creates a new instance of the
        /// <see cref="SpringAir.Service.CannotConfirmReservationException"/> class.
        /// </summary>
        public CannotConfirmReservationException()
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="SpringAir.Service.CannotConfirmReservationException"/> class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        public CannotConfirmReservationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="SpringAir.Service.CannotConfirmReservationException"/> class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public CannotConfirmReservationException(string message, Exception rootCause)
            : base(message, rootCause)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="SpringAir.Service.CannotConfirmReservationException"/> class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        protected CannotConfirmReservationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}