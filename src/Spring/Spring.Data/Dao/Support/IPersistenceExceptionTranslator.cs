

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

using Spring.Data.Support;

namespace Spring.Dao.Support
{
    /// <summary>
    /// Interface implemented by Spring integrations with data access technologies
    /// that throw exceptions.
    /// </summary>
    /// <remarks>
    /// This allows consistent usage of combined exception translation functionality,
    /// without forcing a single translator to understand every single possible type
    /// of exception.
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Mark Pollack (.NET)</author>
    public interface IPersistenceExceptionTranslator
    {
        /// <summary>
        /// Translate the given exception thrown by a persistence framework to a
        /// corresponding exception from Spring's generic DataAccessException hierarchy,
        /// if possible.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Do not translate exceptions that are not understand by this translator:
        /// for example, if coming from another persistence framework, or resulting
        /// from user code and unrelated to persistence.
        /// </para>
        /// <para>
        /// Of particular importance is the correct translation to <see cref="DataIntegrityViolationException"/>
        /// for example on constraint violation.  Implementations may use Spring ADO.NET Framework's
        /// sophisticated exception translation to provide further information in the event of SQLException as a root cause.
        /// </para>
        /// </remarks>
        /// <param name="ex">The exception thrown.</param>
        /// <returns>the corresponding DataAccessException (or <code>null</code> if the
        /// exception could not be translated, as in this case it may result from
        /// user code rather than an actual persistence problem)
        /// </returns>
        /// <seealso cref="DataIntegrityViolationException"/>
        /// <seealso cref="ErrorCodeExceptionTranslator"/>
        /// <author>Rod Johnson</author>
        /// <author>Juergen Hoeller</author>
        /// <author>Mark Pollack (.NET)</author>
        DataAccessException TranslateExceptionIfPossible(Exception ex);
    }
}
