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

using FakeItEasy;

using NHibernate;
using NUnit.Framework;

using Spring.Context.Support;
using Spring.Objects.Factory.Config;

#endregion

namespace Spring.Data.NHibernate.Support
{
    /// <summary>
    /// Tests <see cref="ConfigSectionSessionScopeSettings"/> behaviour.
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class ConfigSectionSessionScopeSettingsTests
    {
        [Test]
        public void CanCreateWithDefaults()
        {
            string SESSIONFACTORY_OBJECTNAME = ConfigSectionSessionScopeSettings.DEFAULT_SESSION_FACTORY_OBJECT_NAME;

            // setup expected values
            ISessionFactory expectedSessionFactory = A.Fake<ISessionFactory>();
            IInterceptor expectedEntityInterceptor = null;
            bool expectedSingleSession = SessionScopeSettings.SINGLESESSION_DEFAULT;
            FlushMode expectedDefaultFlushMode = SessionScopeSettings.FLUSHMODE_DEFAULT;

            // create and register context
            StaticApplicationContext appCtx = new StaticApplicationContext();
            appCtx.Name = AbstractApplicationContext.DefaultRootContextName;
            appCtx.ObjectFactory.RegisterSingleton(SESSIONFACTORY_OBJECTNAME, expectedSessionFactory);
            ContextRegistry.Clear();
            ContextRegistry.RegisterContext(appCtx);

            ConfigSectionSessionScopeSettings settings = new ConfigSectionSessionScopeSettings(this.GetType(), (IVariableSource)null);

            Assert.AreEqual(expectedSessionFactory, settings.SessionFactory);
            Assert.AreEqual(expectedEntityInterceptor, settings.EntityInterceptor);
            Assert.AreEqual(expectedSingleSession, settings.SingleSession);
            Assert.AreEqual(expectedDefaultFlushMode, settings.DefaultFlushMode);
        }

        [Test]
        public void CanCreateFromExplicitConfiguration()
        {
            string SESSIONFACTORY_OBJECTNAME = "SessionFactory";
            string ENTITYINTERCEPTOR_OBJECTNAME = "EntityInterceptor";

            ISessionFactory expectedSessionFactory =  A.Fake<ISessionFactory>();
            IInterceptor expectedEntityInterceptor = A.Fake<IInterceptor>();
            bool expectedSingleSession = false;
            FlushMode expectedDefaultFlushMode = FlushMode.Auto;

            // create and register context
            StaticApplicationContext appCtx = new StaticApplicationContext();
            appCtx.Name = AbstractApplicationContext.DefaultRootContextName;
            appCtx.ObjectFactory.RegisterSingleton(SESSIONFACTORY_OBJECTNAME, expectedSessionFactory);
            appCtx.ObjectFactory.RegisterSingleton(ENTITYINTERCEPTOR_OBJECTNAME, expectedEntityInterceptor);
            ContextRegistry.Clear();
            ContextRegistry.RegisterContext(appCtx);

            // simulate config section
            string thisTypeName = this.GetType().FullName;
            DictionaryVariableSource variableSource = new DictionaryVariableSource()
                .Add(thisTypeName + ".SessionFactoryObjectName", SESSIONFACTORY_OBJECTNAME)
                .Add(thisTypeName + ".EntityInterceptorObjectName", ENTITYINTERCEPTOR_OBJECTNAME)
                .Add(thisTypeName + ".SingleSession", expectedSingleSession.ToString().ToLower() ) // case insensitive!
                .Add(thisTypeName + ".DefaultFlushMode", expectedDefaultFlushMode.ToString().ToLower() ) // case insensitive!
                ;


            ConfigSectionSessionScopeSettings settings = new ConfigSectionSessionScopeSettings(this.GetType(), variableSource);

            Assert.AreEqual( expectedSessionFactory, settings.SessionFactory );
            Assert.AreEqual( expectedEntityInterceptor, settings.EntityInterceptor );
            Assert.AreEqual( expectedSingleSession, settings.SingleSession );
            Assert.AreEqual( expectedDefaultFlushMode, settings.DefaultFlushMode );
        }
    }
}