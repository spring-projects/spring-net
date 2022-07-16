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

namespace Spring.Messaging.Nms.Core
{
    /// <summary>
    /// Exception handler for exceptions from the messaging infrastrcture.
    /// </summary>
    /// <author>Mark Pollack</author>
    public interface IExceptionListener
    {
        /// <summary>
        /// Called when there is an exception in message processing.
        /// </summary>
        /// <param name="exception">The exception.</param>
        void OnException(Exception exception);
    }
}
