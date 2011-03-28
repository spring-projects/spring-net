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
#if !NET_1_0
#region Imports

using NUnit.Framework;
using Spring.Aop.Framework;
using Spring.Dao.Support;
using Spring.Objects.Factory.Support;
using Spring.Stereotype;
using Spring.Util;

#endregion

namespace Spring.Dao.Attributes
{
    /// <summary>
    /// Tests for standalone usage of a PersistenceExceptionTranslationInterceptor,
    /// as explicit advice bean in a BeanFactory rather than applied as part of
    /// as explicit advice bean in a BeanFactory rather than applied as part of
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <author>Mark Pollack  (.NET)</author>
    public class PersistenceExceptionTranslationInterceptorTests : PersistenceExceptionTranslationAdvisorTests
    {

        protected override void AddPersistenceExceptionTranslation(ProxyFactory pf, IPersistenceExceptionTranslator pet)
        {
            if (AttributeUtils.FindAttribute(pf.TargetType, typeof(RepositoryAttribute)) != null)
            {
                DefaultListableObjectFactory of = new DefaultListableObjectFactory();
                of.RegisterObjectDefinition("peti", new RootObjectDefinition(typeof(PersistenceExceptionTranslationInterceptor)));
                of.RegisterSingleton("pet", pet);
                pf.AddAdvice((PersistenceExceptionTranslationInterceptor) of.GetObject("peti"));

            }
        }
    }
}
#endif