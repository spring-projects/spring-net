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

using System.Runtime.Serialization;

namespace Spring.Dao;

/// <summary>
/// RDta access exception thrown when a resource fails temporarily and the operation can be
/// retried.
/// </summary>
/// <author>Thomas Risberg</author>
/// <author>Mark Pollack (.NET)</author>
[Serializable]
public class TransientDataAccessResourceException : TransientDataAccessException
{
    /// <summary>
    /// Creates a new instance of the
    /// <see cref="Spring.Dao.TransientDataAccessResourceException"/> class.
    /// </summary>
    public TransientDataAccessResourceException() : base("No Exception Message") { }

    /// <summary>
    /// Creates a new instance of the
    /// <see cref="Spring.Dao.TransientDataAccessResourceException"/> class.
    /// </summary>
    /// <param name="message">
    /// A message about the exception.
    /// </param>
    public TransientDataAccessResourceException(string message) : base(message) { }

    /// <summary>
    /// Creates a new instance of the
    /// <see cref="Spring.Dao.TransientDataAccessResourceException"/> class.
    /// </summary>
    /// <param name="message">
    /// A message about the exception.
    /// </param>
    /// <param name="rootCause">
    /// The root exception (from the underlying data access API, such as ADO.NET).
    /// </param>
    public TransientDataAccessResourceException(string message, Exception rootCause)
        : base(message, rootCause)
    {
    }

    /// <inheritdoc />
    protected TransientDataAccessResourceException(
        SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
