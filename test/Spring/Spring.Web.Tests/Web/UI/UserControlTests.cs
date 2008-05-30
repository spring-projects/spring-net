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

using System.Web;
using System.Web.UI;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.TestSupport;
using Spring.Validation;
using Spring.Web.Support;

#endregion

namespace Spring.Web.UI
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Erich Eichinger</author>
    /// <version>$Id: UserControlTests.cs,v 1.2 2008/03/15 11:19:39 oakinger Exp $</version>
    [TestFixture]
    public class UserControlTests : TestWebContextTests
    {
        public class TestUserControl : UserControl
        {
            public TestUserControl()
            {
            }

            public TestUserControl(Control parent)
            {
                parent.Controls.Add(this);
            }

            public new void SetResult(string name)
            {
                base.SetResult(name);
            }
        }

        [Test]
        public void SetResultSelectsCorrectResult()
        {
            MockRepository mocks = new MockRepository();
            TestUserControl uc = new TestUserControl();
            Result theResult = (Result)mocks.CreateMock(typeof(Result));

            using (mocks.Ordered())
            {
                theResult.Navigate(uc);
            }
            mocks.ReplayAll();

            uc.Results.Add("theResult", theResult);
            uc.SetResult("theResult");
            mocks.VerifyAll();
        }

        [Test]
        public void SetResultBubblesUpHierarchyUntilFirstMatch()
        {
            MockRepository mocks = new MockRepository();
            TestUserControl c1 = new TestUserControl();
            Control c11 = new Control(); c1.Controls.Add(c11);
            TestUserControl c111 = new TestUserControl(c11);
            Result theResult = (Result)mocks.CreateMock(typeof(Result));

            using (mocks.Ordered())
            {
                theResult.Navigate(c1);
            }
            mocks.ReplayAll();

            c1.Results.Add("theResult", theResult);
            c111.SetResult("theResult");
            mocks.VerifyAll();
        }

        [Test]
        public void ValidateSetsDefaultVariables()
        {
            TestPage page = new TestPage(HttpContext.Current);
            TestUserControl c1 = new TestUserControl();
            page.Controls.Add(c1);

            ConditionValidator v1 = new ConditionValidator("#page == #this.Page", null);
            ConditionValidator v2 = new ConditionValidator("#usercontrol == #this", null);

            Assert.IsTrue(c1.Validate(c1, v1, v2));
        }
    }
}