#region License

/*
 * Copyright 2002-2007 the original author or authors.
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

namespace Spring.Testing.NUnit
{
    /// <summary>
    /// This is 
    /// </summary>
    /// <remarks>
    ///
    /// </remarks>
    /// <author>Mark Pollack</author>
    /// <version>$Id: BaseTestAppContextTests.cs,v 1.1 2007/08/20 18:38:11 markpollack Exp $</version>
    public class BaseTestAppContextTests : AbstractTransactionalDbProviderSpringContextTests
    {

        protected override string[] ConfigLocations
        {
            get { return new string[] {"assembly://Spring.Testing.NUnit.Tests/Spring.Testing.NUnit/TestApplicationContext.xml"}; }
        }
    }
}