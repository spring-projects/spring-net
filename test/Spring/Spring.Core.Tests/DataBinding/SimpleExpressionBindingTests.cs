#region License

/*
 * Copyright 2002-2004 the original author or authors.
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

using NUnit.Framework;

namespace Spring.DataBinding
{
    /// <summary>
    /// Test cases for the SimpleExpressionBinding class.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    /// <version>$Id: SimpleExpressionBindingTests.cs,v 1.1 2006/11/21 05:47:00 aseovic Exp $</version>
    [TestFixture]
    public class SimpleExpressionBindingTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WithNullMesageId()
        {
            new SimpleExpressionBinding("exp", "exp").SetErrorMessage(null, "errors");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WithEmptyMesageId()
        {
            new SimpleExpressionBinding("exp", "exp").SetErrorMessage("", "errors");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WithWhitespaceMesageId()
        {
            new SimpleExpressionBinding("exp", "exp").SetErrorMessage("\t   ", "errors");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void WithNullProviders()
        {
            new SimpleExpressionBinding("exp", "exp").SetErrorMessage("error", null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void WithEmptyProviders()
        {
            new SimpleExpressionBinding("exp", "exp").SetErrorMessage("error", new string[0]);
        }

    }
}