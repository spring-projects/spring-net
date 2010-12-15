#region License

/*
 * Copyright © 2002-2010 the original author or authors.
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
using System.Collections;
using NUnit.Framework;

#endregion

namespace Spring.Dao.Support
{
    /// <summary>
    /// This class contains tests for 
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <version>$Id:$</version>
    [TestFixture]
    public class DataAccessUtilsTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test()
        {
        }

        
    }

    public class MapPersistenceExceptionTranslator : IPersistenceExceptionTranslator
    {
        #region IPersistenceExceptionTranslator Members

        private IDictionary translations = new Hashtable();

        public void AddTranslation(Exception inException, Exception outException)
        {
            this.translations.Add(inException, outException);
        }
        public DataAccessException TranslateExceptionIfPossible(Exception ex)
        {
            return (DataAccessException)translations[ex];
        }

        #endregion
    }
}