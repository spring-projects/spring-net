#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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
using Spring.Context;
using Spring.Context.Support;

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
        private IApplicationContext context;
        private CacheAspect cacheAspect;
        private ICache cache;

        [SetUp]
        public void SetUp()
        {
            cache = new NonExpiringCache();

            context = new XmlApplicationContext();
            ((IConfigurableApplicationContext) context).ObjectFactory.RegisterSingleton("inventors", cache);

            cacheAspect = new CacheAspect();
            cacheAspect.ApplicationContext = context;
        }

        [Test]
        public void TestCaching()
        {
            ProxyFactory pf = new ProxyFactory(new InventorStore());
            pf.AddAdvisors(cacheAspect);

            IInventorStore store = (IInventorStore) pf.GetProxy();

            Assert.AreEqual(0, cache.Count);
            
            IList inventors = store.GetAll();
            Assert.AreEqual(2, cache.Count);

            store.Delete((Inventor) inventors[0]);
            Assert.AreEqual(1, cache.Count);

            Inventor tesla = store.Load("Nikola Tesla");
            Assert.AreEqual(2, cache.Count);

            store.Save(tesla);
            Assert.AreEqual(2, cache.Count);
            Assert.AreEqual("Serbian", ((Inventor)cache.Get("Nikola Tesla")).Nationality);

            store.DeleteAll();
            Assert.AreEqual(0, cache.Count);
        }
    }

    #region Inner Class : CacheParameterTarget

    public interface IInventorStore
    {
        IList GetAll();
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

    #endregion
}