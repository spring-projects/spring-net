#region License

/*
 * Copyright 2002-2011 the original author or authors.
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

using System;
using System.ComponentModel;

namespace Spring.Http.Rest
{
    // TODO: Rename this to RestOperationCompletedEventArgs or RestAsyncCompletedEventArgs ?

    /// <summary>
    /// Provides data when an asynchronous REST operation completes.
    /// </summary>
    /// <typeparam name="T">The type of the response value.</typeparam>
    /// <see cref="IRestAsyncOperations"/>
    /// <see cref="RestTemplate"/>
    public class MethodCompletedEventArgs<T> : AsyncCompletedEventArgs where T : class
    {
        private T response;

        /// <summary>
        /// Gest the response of the REST operation.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">If the operation was canceled.</exception>
        /// <exception cref="System.Reflection.TargetInvocationException">If the operation failed.</exception>
        public T Response
        {
            get
            {
                // Raise an exception if the operation failed or was canceled.
                base.RaiseExceptionIfNecessary();

                // If the operation was successful, return the value.
                return response;
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="MethodCompletedEventArgs{T}"/>.
        /// </summary>
        /// <param name="response">The response of the REST operation.</param>
        /// <param name="exception">Any error that occurred during the asynchronous operation.</param>
        /// <param name="cancelled">A value indicating whether the asynchronous operation was canceled.</param>
        /// <param name="userState">The optional user-supplied state object.</param>
        public MethodCompletedEventArgs(T response, Exception exception, bool cancelled, object userState)
            : base(exception, cancelled, userState)
        {
            this.response = response;
        }
    }
}
