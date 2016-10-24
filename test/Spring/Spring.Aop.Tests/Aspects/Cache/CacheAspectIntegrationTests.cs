#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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
using System.Collections.Specialized;
using NUnit.Framework;
using Spring.Aop.Framework;
using Spring.Caching;
using Spring.Context.Support;
using Spring.Objects.Factory;

#endregion

namespace Spring.Aspects.Cache
{
    /// <summary>
    /// Unit tests for the CacheParameterAdvice class.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [TestFixture]
    public sealed class CacheAspectIntegrationTests
    {
        private GenericApplicationContext context;
        private CacheAspect cacheAspect;

        [SetUp]
        public void SetUp()
        {
            context = new GenericApplicationContext();

            cacheAspect = new CacheAspect();
            cacheAspect.ApplicationContext = context;
        }

        [Test]
        public void TestCaching()
        {
            ICache cache = new NonExpiringCache();
            context.ObjectFactory.RegisterSingleton("inventors", cache);

            ProxyFactory pf = new ProxyFactory(new InventorStore());
            pf.AddAdvisors(cacheAspect);

            IInventorStore store = (IInventorStore)pf.GetProxy();

            Assert.AreEqual(0, cache.Count);

            IList inventors = store.GetAll();
            Assert.AreEqual(2, cache.Count);

            store.Delete((Inventor)inventors[0]);
            Assert.AreEqual(1, cache.Count);

            Inventor tesla = store.Load("Nikola Tesla");
            Assert.AreEqual(2, cache.Count);

            store.Save(tesla);
            Assert.AreEqual(2, cache.Count);
            Assert.AreEqual("Serbian", ((Inventor)cache.Get("Nikola Tesla")).Nationality);

            store.DeleteAll();
            Assert.AreEqual(0, cache.Count);
        }

        [Test]
        public void TestCacheResultDoesNotReturnInvalidType()
        {
            ICache cache = new NonExpiringCache();
            context.ObjectFactory.RegisterSingleton("inventors", cache);

            ProxyFactory pf = new ProxyFactory(new InventorStore());
            pf.AddAdvisors(cacheAspect);

            IInventorStore store = (IInventorStore)pf.GetProxy();

            cache.Insert("Nikola Tesla", "WrongTypeForMethodSignature");

            Inventor value = store.Load("Nikola Tesla");

            Assert.That(value, Is.AssignableTo(typeof(Inventor)), "CacheAspect returned a cached type that is incompatible with the method signature return type");
        }

        /// <summary>
        /// http://jira.springframework.org/browse/SPRNET-1226
        /// </summary>
        [Test]
        public void NoCacheKeySpecified()
        {
            ICache cache = new NonExpiringCache();
            context.ObjectFactory.RegisterSingleton("inventors", cache);

            ProxyFactory pf = new ProxyFactory(new InventorStore());
            pf.AddAdvisors(cacheAspect);

            IInventorStore store = (IInventorStore)pf.GetProxy();
            Assert.Throws<ArgumentNullException>(() => store.GetAllNoCacheKey());
        }

        [Test]
        public void CacheDoesNotExist()
        {
            //ICache cache = new NonExpiringCache();
            //context.ObjectFactory.RegisterSingleton("losers", cache);

            ProxyFactory pf = new ProxyFactory(new InventorStore());
            pf.AddAdvisors(cacheAspect);

            IInventorStore store = (IInventorStore)pf.GetProxy();
            Assert.Throws<NoSuchObjectDefinitionException>(() => store.GetAll());
        }

        [Test]
        public void CacheDoesNotImplementICache()
        {
            context.ObjectFactory.RegisterSingleton("inventors", new Object());

            ProxyFactory pf = new ProxyFactory(new InventorStore());
            pf.AddAdvisors(cacheAspect);

            IInventorStore store = (IInventorStore)pf.GetProxy();
            Assert.Throws<ArgumentException>(() => store.GetAll());
        }

        [Test(Description = "http://jira.springframework.org/browse/SPRNET-959")]
        public void UseMethodInfoForKeyGeneration()
        {
            ICache cache = new NonExpiringCache();
            context.ObjectFactory.RegisterSingleton("defaultCache", cache);

            ProxyFactory pf = new ProxyFactory(new GenericDao<string, int>());
            pf.AddAdvisors(cacheAspect);

            IGenericDao<string, int> dao = (IGenericDao<string, int>)pf.GetProxy();

            Assert.AreEqual(0, cache.Count);

            dao.Load(1);
            Assert.AreEqual(1, cache.Count);

            // actually, it should be null, because default(string) = null
            // but it returns the NullValue marker created by the CacheResultAttribute
            Assert.IsNotNull(cache.Get("String_1"));
        }
    }

    #region Inner Class : CacheParameterTarget

    public interface IInventorStore
    {
        IList GetAll();
        IList GetAllNoCacheKey();
        Inventor Load(string name);
        void Save(Inventor inventor);
        void Delete(Inventor inventor);
        void DeleteAll();
    }

    public sealed class InventorStore : IInventorStore
    {
        private IDictionary inventors = new ListDictionary();

        public InventorStore()
        {
            Inventor tesla = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), null);
            Inventor pupin = new Inventor("Mihajlo Pupin", new DateTime(1854, 10, 9), null);
            inventors.Add("Nikola Tesla", tesla);
            inventors.Add("Mihajlo Pupin", pupin);
        }

        [CacheResultItems("inventors", "Name")]
        public IList GetAll()
        {
            return new ArrayList(inventors.Values);
        }


        [CacheResult(CacheName = "inventors")]
        public IList GetAllNoCacheKey()
        {
            return new ArrayList(inventors.Values);
        }

        [CacheResult("inventors", "#name")]
        public Inventor Load(string name)
        {
            return (Inventor) inventors[name];
        }

        public void Save([CacheParameter("inventors", "Name")] Inventor inventor)
        {
            inventor.Nationality = "Serbian";
        }

        [InvalidateCache("inventors", Keys = "#inventor.Name")]
        public void Delete(Inventor inventor)
        {
        }

        [InvalidateCache("inventors")]
        public void DeleteAll()
        {
        }
    }

    public interface IGenericDao<T, IdT>
    {
        T Load(IdT id);
    }

    public sealed class GenericDao<T, IdT> : IGenericDao<T, IdT>
    {
        [CacheResult("defaultCache", "#Load.ReturnType.Name + '_' + #id")]
        public T Load(IdT id)
        {
            return default(T);
        }
    }

    #endregion
}