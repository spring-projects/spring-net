#region License

/*
 * Copyright © 2002-2009 the original author or authors.
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
using Spring.Context;
using Spring.Context.Support;
using Spring.Core.TypeResolution;
using Spring.Expressions;
using Spring.Messaging.Ems.Common;
using Spring.Messaging.Ems.Jndi;
using Spring.Messaging.Ems.Listener;
using Spring.Objects.Factory.Xml;
using Spring.Testing.NUnit;
using TIBCO.EMS;

#endregion

namespace Spring.Messaging.Ems.Core
{
    [TestFixture]
    public class EmsTemplateTests : AbstractDependencyInjectionSpringContextTests
    {
        protected IConnectionFactory emsConnectionFactory;

        protected IConnectionFactory connectionFactory;

        //This is the 'raw' TIBCO type
        protected ConnectionFactory jndiEmsConnectionFactory;

        protected IConnectionFactory cachingJndiConnectionFactory;

        /// <summary>
        /// Default constructor for EmsTemplateTests.
        /// </summary>
        public EmsTemplateTests()
        {
            this.PopulateProtectedVariables = true;
        }



        [Test]
        public void SendAndReceive()
        {
            Assert.NotNull(emsConnectionFactory);
            Assert.NotNull(connectionFactory);
            Assert.NotNull(jndiEmsConnectionFactory);       
            Assert.NotNull(cachingJndiConnectionFactory);
        }

        #region Overrides of AbstractDependencyInjectionSpringContextTests

        /// <summary>
        /// Subclasses must implement this property to return the locations of their
        /// config files. A plain path will be treated as a file system location.
        /// </summary>
        /// <value>An array of config locations</value>
        protected override string[] ConfigLocations
        {
            get { return new string[] { "assembly://Spring.Messaging.Ems.Integration.Tests/Spring.Messaging.Ems.Core/EmsTemplateTests.xml" }; }
        }

        #endregion
    }
}