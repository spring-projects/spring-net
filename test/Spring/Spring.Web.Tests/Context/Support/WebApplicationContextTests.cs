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

using NUnit.Framework;
using NUnitAspEx;
using Spring.Objects.Factory;
using Spring.TestSupport;
using Spring.Threading;

#endregion

namespace Spring.Context.Support
{
    /// <summary>
    ///
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class WebApplicationContextTests : WebFormTestCase
    {
        public WebApplicationContextTests()
            : base("/Test", "/Spring/Context/Support/WebApplicationContextTests/")
        {}

        [Test]
        public void CanAccessContextFromNonWebThread()
        {
            Host.Execute(new TestAction(CanAccessContextFromNonWebThreadImpl));
        }

        public static void CanAccessContextFromNonWebThreadImpl()
        {
            IApplicationContext ctx;

            ctx = WebApplicationContext.GetRootContext();

            AsyncTestMethod testMethod = new AsyncTestMethod(1, new AsyncTestMethod.TestMethod(DoBackgroundWork), ctx);
            testMethod.Start();
            testMethod.AssertNoException();
        }

        private static object DoBackgroundWork(object[] args)
        {
            IApplicationContext ctx = (IApplicationContext) args[0];

            object o;
            o = ctx.GetObject("singletonObject"); Assert.IsNotNull(o);
            o = ctx.GetObject("prototypeObject"); Assert.IsNotNull(o);
            o = ctx.GetObject("applicationScopedObject"); Assert.IsNotNull(o);
            try
            {
                o = ctx.GetObject("requestScopedObject"); Assert.IsNotNull(o);
                Assert.Fail("shouldn't be allowed");
            }
            catch(ObjectCreationException oce1)
            {
                Assert.IsTrue(-1 < oce1.Message.IndexOf("'request' scoped web singleton object 'requestScopedObject' requires a valid Request"));
            }

            try
            {
                o = ctx.GetObject("sessionScopedObject"); Assert.IsNotNull(o);
                Assert.Fail("shouldn't be allowed");
            }
            catch (ObjectCreationException oce1)
            {
                Assert.IsTrue(-1 < oce1.Message.IndexOf("'session' scoped web singleton object 'sessionScopedObject' requires a valid Session"));
            }

            return null;
        }

    }
}