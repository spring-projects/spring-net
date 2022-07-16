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

using System.Collections;
using Spring.Data.Support;
using Spring.Util;

namespace Spring.Dao.Support
{
    /// <summary>
    /// Implementation of PersistenceExceptionTranslator that supports chaining,
    /// allowing the addition of PersistenceExceptionTranslator instances in order.
    /// Returns <code>non-null</code> on the first (if any) match.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public class ChainedPersistenceExceptionTranslator : IPersistenceExceptionTranslator
    {
        private readonly ArrayList translatorList = new ArrayList(4);

        /// <summary>
        /// Adds the translator to the translator list.
        /// </summary>
        /// <param name="translator">The translator.</param>
        public void AddTranslator(IPersistenceExceptionTranslator translator)
        {
            AssertUtils.ArgumentNotNull(translator, "PersistenceExceptionTranslator must not be null");
            this.translatorList.Add(translator);
        }


        /// <summary>
        /// Gets all registered IPersistenceExceptionTranslator as an array.
        /// </summary>
        /// <value>The IPersistenceExceptionTranslators.</value>
        public IPersistenceExceptionTranslator[] Translators
        {
            get
            {
                return (IPersistenceExceptionTranslator[]) translatorList.ToArray(typeof (IPersistenceExceptionTranslator));
            }
        }

        /// <summary>
        /// Translate the given exception thrown by a persistence framework to a
        /// corresponding exception from Spring's generic DataAccessException hierarchy,
        /// if possible.
        /// </summary>
        /// <param name="ex">The exception thrown.</param>
        /// <returns>
        /// the corresponding DataAccessException (or <code>null</code> if the
        /// exception could not be translated, as in this case it may result from
        /// user code rather than an actual persistence problem)
        /// </returns>
        /// <remarks>
        /// 	<para>
        /// Do not translate exceptions that are not understand by this translator:
        /// for example, if coming from another persistence framework, or resulting
        /// from user code and unrelated to persistence.
        /// </para>
        /// 	<para>
        /// Of particular importance is the correct translation to <see cref="DataIntegrityViolationException"/>
        /// for example on constraint violation.  Implementations may use Spring ADO.NET Framework's
        /// sophisticated exception translation to provide further information in the event of SQLException as a root cause.
        /// </para>
        /// </remarks>
        /// <seealso cref="DataIntegrityViolationException"/>
        /// <seealso cref="ErrorCodeExceptionTranslator"/>
        public DataAccessException TranslateExceptionIfPossible(Exception ex)
        {
            DataAccessException translatedDex = null;
            foreach (IPersistenceExceptionTranslator pet in translatorList)
            {
                translatedDex = pet.TranslateExceptionIfPossible(ex);
            }
            return translatedDex;
        }


    }
}
