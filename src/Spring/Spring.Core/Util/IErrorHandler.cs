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

namespace Spring.Util
{
    /// <summary>
    /// A strategy for handling errors.  This is especially useful for handling
    /// errors that occur during asynchronous execution as in such cases it may not be
    /// possible to throw the error to the original caller.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Mark Pollack (.NET)</author>
    public interface IErrorHandler
    {
        /// <summary>
        /// Handles the error.
        /// </summary>
        /// <param name="exception">The exception.</param>
        void HandleError(Exception exception);

    }

}
