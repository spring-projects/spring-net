#region License

/*
 * Copyright © 2002-2007 the original author or authors.
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
using NUnit.Framework;
using Spring.Stereotype;

#endregion

namespace Spring.Dao.Attributes
{
    /// <summary>
    /// This class contains tests for 
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <version>$Id:$</version>
    [TestFixture]
    public class PersistenceExceptionTranslationAdvisorTests
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
#if !NET_1_0
    [Repository]
    public class StereotypedRepositoryInterfaceImpl : RepositoryInterfaceImpl
    {
        // Extends above class just to add repository annotation
    }


    public class RepositoryInterfaceImpl : IRepositoryInterface
    {
        private Exception ex;


        public Exception Behavior
        {
            set { ex = value; }
        }

        public void Throws()
        {
            if (ex != null)
            {
                throw ex;
            }
        }
    }
#endif

    public interface IRepositoryInterface
    {
        void Throws();
    }

}