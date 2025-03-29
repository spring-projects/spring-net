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

using System.Data;

namespace Spring.Data.Generic;

/// <summary>
/// Generic callback interface for code that operates on a
/// IDbCommand.
/// </summary>
/// <typeparam name="T">The return type from executing the
/// callback</typeparam>
/// <remarks>
/// <p>Allows you to execute any number of operations
/// on a single IDbCommand, for example a single ExecuteScalar
/// call or repeated execute calls with varying parameters.
/// </p>
/// <p>Used internally by AdoTemplate, but also useful for
/// application code.  Note that the passed in IDbCommand
/// has been created by the framework and will have its
/// Connection property set and the Transaction property
/// set based on the transaction context.</p>
/// <p>As an alternative to using this interface you can use
/// the delegate version which is particularly nice when using
/// anonymous delegates for writing more terse code and having
/// easy access to the variables in the calling class.</p>
/// <p>See <see cref="ICommandCallback"/> for a version that has the
/// base class DbCommand as the callback argument.</p>
/// </remarks>
/// <author>Mark Pollack</author>
public interface IDbCommandCallback<T>
{
    /// <summary>
    /// Called by AdoTemplate.Execute with an active ADO.NET IDbCommand.
    /// The calling code does not need to care about closing the
    /// command or the connection, or
    /// about handling transactions:  this will all be handled by
    /// Spring's AdoTemplate
    /// </summary>
    /// <param name="command">An active IDbCommand instance</param>
    /// <returns>The result object</returns>
    T DoInCommand(IDbCommand command);
}
