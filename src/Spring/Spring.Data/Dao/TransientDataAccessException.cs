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

using System.Runtime.Serialization;

namespace Spring.Dao;

/// <summary>
/// Root of the hierarchy of data access exception that are considered transient -
/// where a previously failed operation might be able to succeed when the operation
/// is retried without any intervention by application-level functionality.
/// </summary>
/// <author>Thomas Risberg</author>
/// <author>Mark Pollack (.NET)</author>
[Serializable]
public abstract class TransientDataAccessException : DataAccessException
{
    /// <summary>
    /// Creates a new instance of the
    /// <see cref="Spring.Dao.TransientDataAccessException"/> class.
    /// </summary>
    public TransientDataAccessException() : base("No Exception Message") { }

    /// <summary>
    /// Creates a new instance of the
    /// <see cref="Spring.Dao.TransientDataAccessException"/> class.
    /// </summary>
    /// <param name="message">
    /// A message about the exception.
    /// </param>
    public TransientDataAccessException(string message) : base(message) { }

    /// <summary>
    /// Creates a new instance of the
    /// <see cref="Spring.Dao.TransientDataAccessException"/> class.
    /// </summary>
    /// <param name="message">
    /// A message about the exception.
    /// </param>
    /// <param name="rootCause">
    /// The root exception (from the underlying data access API, such as ADO.NET).
    /// </param>
    public TransientDataAccessException(string message, Exception rootCause)
        : base(message, rootCause)
    {
    }

    /// <inheritdoc />
    protected TransientDataAccessException(
        SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
