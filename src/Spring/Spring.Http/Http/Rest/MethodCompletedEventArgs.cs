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
    public class MethodCompletedEventArgs<T> : AsyncCompletedEventArgs where T : class
    {
        private T response;

        public T Response
        {
            get
            {
                // Raise an exception if the operation failed or 
                // was canceled.
                base.RaiseExceptionIfNecessary();

                // If the operation was successful, return the 
                // property value.
                return response;
            }
        }

        public MethodCompletedEventArgs(T response, Exception exception, bool cancelled, object userState)
            : base(exception, cancelled, userState)
        {
            this.response = response;
        }
    }
}
