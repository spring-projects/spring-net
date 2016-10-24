#region License

/*
 * Copyright © 2010-2011 the original author or authors.
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
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

using Spring.Context.Support;
using Spring.Example.Scannable;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Xml;

namespace Spring.Context.Attributes
{

    public class SimpleScanTests
    {
        private IApplicationContext _applicationContext;

        [SetUp]
        public void Setup()
        {
            _applicationContext = new XmlApplicationContext(ReadOnlyXmlTestResource.GetFilePath("SimpleScanTest.xml", GetType()));
        }
        
        //[Test]
        public void FooService()
        {

            IFooService fooService = GetObject<IFooService>();

        }

        public T GetObject<T>()
        {
            return (T)DoGetInstance(typeof(T), null);
        }
        public T GetObject<T>(string name)
        {
            return (T)DoGetInstance(typeof(T), name);
        }

        protected object DoGetInstance(Type serviceType, string key)
        {
            if (key == null)
            {
                IEnumerator it = DoGetAllInstances(serviceType).GetEnumerator();
                if (it.MoveNext())
                {
                    return it.Current;
                }
                throw new ObjectCreationException(string.Format("no services of type '{0}' defined", serviceType.FullName));
            }
            return _applicationContext.GetObject(key, serviceType);
        }

        /// <summary>
        /// Resolves service instances by type.
        /// </summary>
        /// <param name="serviceType">Type of service requested.</param>
        /// <returns>
        /// Sequence of service instance objects matching the <paramref name="serviceType"/>.
        /// </returns>
        protected IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            foreach (string objectName in _applicationContext.GetObjectNamesForType(serviceType))
            {
                yield return _applicationContext.GetObject(objectName);
            }
        }

    }
}