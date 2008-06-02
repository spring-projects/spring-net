#region License

/*
 * Copyright © 2002-2008 the original author or authors.
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
using System.Web;
using NUnit.Framework;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class AbstractHandlerFactoryTests : AbstractHandlerFactory
    {
        private class Type1 {}
       
        [Test]
        public void FindWebObjectDefinition()
        {
            NamedObjectDefinition nod;

            nod = Find("/path/o1.ext", "/path/o1.ext");
            Assert.AreEqual( typeof(Type1), nod.ObjectDefinition.ObjectType);
            Assert.AreEqual( "/path/o1.ext", nod.Name);

            nod = Find("/path/o1.ext", "/o1.ext");
            Assert.IsNull(nod);

            nod = Find("/path/o1.ext", "/path/o1");
            Assert.IsNull(nod);

            nod = Find("/path/o1.ext", "o1.ext");
            Assert.AreEqual(typeof(Type1), nod.ObjectDefinition.ObjectType);
            Assert.AreEqual("o1.ext", nod.Name);

            nod = Find("/path/o1.ext", "o1");
            Assert.AreEqual(typeof(Type1), nod.ObjectDefinition.ObjectType);
            Assert.AreEqual("o1", nod.Name);
        }

        private NamedObjectDefinition Find(string url, string objectName)
        {
            DefaultListableObjectFactory of = new DefaultListableObjectFactory();
            RootObjectDefinition rod = new RootObjectDefinition(typeof(Type1));
            of.RegisterObjectDefinition(objectName, rod);

            return FindWebObjectDefinition( url, of );
        }

        #region AbstractHandlerFactory implementations

        public override IHttpHandler GetHandler(HttpContext context, string requestType, string url,
                                                string pathTranslated)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}