#region License

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

#endregion

using System;
using System.Runtime.Serialization;
using NUnit.Framework;
using Spring.Aop.Framework;
using Spring.Dao.Support;
using Spring.Stereotype;

#pragma warning disable SYSLIB0051

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

        private IndexOutOfRangeException doNotTranslate = new IndexOutOfRangeException();

        private PersistenceException persistenceException = new PersistenceException();

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void NoTranslationNeeded()
        {
            RepositoryInterfaceImpl target = new RepositoryInterfaceImpl();
            IRepositoryInterface ri = CreateProxy(target);

            ri.Throws();

            target.Behavior = persistenceException;

            try
            {
                ri.Throws();
                Assert.Fail();
            } catch (Exception ex)
            {
                Assert.AreSame(persistenceException, ex);
            }
        }

        [Test]
        public void TranslationNotNeededForTheseExceptions()
        {
            RepositoryInterfaceImpl target = new StereotypedRepositoryInterfaceImpl();
            IRepositoryInterface ri = CreateProxy(target);

            ri.Throws();

            target.Behavior = doNotTranslate;
            try
            {
                ri.Throws();
                Assert.Fail();
            } catch (Exception ex)
            {
                Assert.AreSame(doNotTranslate, ex);
            }
        }

        [Test]
        public void TranslationNeededForTheseExceptions()
        {
            DoTestTranslationNeededForTheseExceptions((new StereotypedRepositoryInterfaceImpl()));
        }

        [Test]
        public void TranslationNeededForTheseExceptionsOnSuperclass()
        {
            DoTestTranslationNeededForTheseExceptions((new MyStereotypedRepositoryInterfaceImpl()));            
        }

        [Test]
        public void TranslationNeededForTheseExceptionsOnInterface()
        {
            DoTestTranslationNeededForTheseExceptions((new MyInterfaceStereotypedRepositoryInterfaceImpl()));
        }

        [Test]
        public void TranslationNeededForTheseExceptionsOnInheritedInterface()
        {
            DoTestTranslationNeededForTheseExceptions((new MyInterfaceInheritedStereotypedRepositoryInterfaceImpl()));
        }

        private void DoTestTranslationNeededForTheseExceptions(RepositoryInterfaceImpl target)
        {
            IRepositoryInterface ri = CreateProxy(target);
            target.Behavior = persistenceException;
            try
            {
                ri.Throws();
                Assert.Fail();
            } catch (DataAccessException ex)
            {
                //Expected
                Assert.AreSame(persistenceException, ex.InnerException);
            } catch (PersistenceException)
            {
                Assert.Fail("Should have been translated");
            }
        }


        private IRepositoryInterface CreateProxy(RepositoryInterfaceImpl target)
        {
            MapPersistenceExceptionTranslator mpet = new MapPersistenceExceptionTranslator();
            mpet.AddTranslation(persistenceException, new InvalidDataAccessApiUsageException("", persistenceException));
            ProxyFactory pf = new ProxyFactory(target);
            pf.AddInterface(typeof(IRepositoryInterface));
            AddPersistenceExceptionTranslation(pf, mpet);
            return (IRepositoryInterface) pf.GetProxy();
        }

        protected virtual void AddPersistenceExceptionTranslation(ProxyFactory pf, IPersistenceExceptionTranslator pet)
        {
            pf.AddAdvisor(new PersistenceExceptionTranslationAdvisor(pet, typeof(RepositoryAttribute)));
        }
    }





    [Repository]
    public class StereotypedRepositoryInterfaceImpl : RepositoryInterfaceImpl
    {
        // Extends above class just to add repository annotation
    }

    public class MyStereotypedRepositoryInterfaceImpl : StereotypedRepositoryInterfaceImpl {
	}

    [Repository]
    public interface IStereotypedInterface
    {
        
    }
    public class MyInterfaceStereotypedRepositoryInterfaceImpl : RepositoryInterfaceImpl, IStereotypedInterface
    {
    }

    public interface IStereotypedInheritingInterface : IStereotypedInterface {
	}

    public class MyInterfaceInheritedStereotypedRepositoryInterfaceImpl : RepositoryInterfaceImpl, IStereotypedInheritingInterface {
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

    public interface IRepositoryInterface
    {
        void Throws();
    }

    [Serializable]
    public class PersistenceException : ApplicationException
    {
        #region Constructor (s) / Destructor

        public PersistenceException()
        {
        }

        public PersistenceException(string message)
            : base(message)
        {
        }

        public PersistenceException(string message, Exception rootCause)
            : base(message, rootCause)
        {
        }

        protected PersistenceException(
            SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion
    }

}
