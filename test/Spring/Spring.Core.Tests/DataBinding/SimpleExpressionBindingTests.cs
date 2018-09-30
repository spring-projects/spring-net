#region License

/*
 * Copyright 2002-2010 the original author or authors.
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
    [TestFixture]
    public class SimpleExpressionBindingTests
    {
        private class BindToNullable_TestEntity
        {
            private Nullable<short> sortOrder;
            public Nullable<short> SortOrder { get { return sortOrder; } set { sortOrder = value; } }
        }

#if !NETCOREAPP
        [Test(Description="http://jira.springframework.org/browse/SPRNET-996")]
        public void BindToNullable()
        {
            System.Web.UI.WebControls.TextBox textBox = new System.Web.UI.WebControls.TextBox();
            textBox.Text = string.Empty;
            BindToNullable_TestEntity entity = new BindToNullable_TestEntity();
            new SimpleExpressionBinding("Text", "SortOrder").BindSourceToTarget(textBox, entity, null);
            Assert.IsNull(entity.SortOrder);
        }
#endif

        [Test]
        public void WithNullMessageId()
        {
            Assert.Throws<ArgumentNullException>(() => new SimpleExpressionBinding("exp", "exp").SetErrorMessage(null, "errors"));
        }

        [Test]
        public void WithEmptyMessageId()
        {
            Assert.Throws<ArgumentNullException>(() => new SimpleExpressionBinding("exp", "exp").SetErrorMessage("", "errors"));
        }

        [Test]
        public void WithWhitespaceMessageId()
        {
            Assert.Throws<ArgumentNullException>(() => new SimpleExpressionBinding("exp", "exp").SetErrorMessage("\t   ", "errors"));
        }

        [Test]
        public void WithNullProviders()
        {
            Assert.Throws<ArgumentException>(() => new SimpleExpressionBinding("exp", "exp").SetErrorMessage("error", null));
        }

        [Test]
        public void WithEmptyProviders()
        {
            Assert.Throws<ArgumentException>(() => new SimpleExpressionBinding("exp", "exp").SetErrorMessage("error", new string[0]));
        }
    }
}