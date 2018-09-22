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

using System;
using System.Web;
using System.Web.UI;

using FakeItEasy;

using NUnit.Framework;

using Spring.TestSupport;
using Spring.Validation;
using Spring.Web.Support;

namespace Spring.Web.UI
{
    /// <summary>
    ///
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class UserControlTests : TestWebContextTests
    {
        [Test]
        public void SetResultSelectsCorrectResult()
        {
            TestUserControl uc = new TestUserControl();
            Result theResult = A.Fake<Result>();

            uc.Results.Add("theResult", theResult);
            uc.SetResult("theResult");

        }

        [Test]
        public void SetResultBubblesUpHierarchyUntilFirstMatch()
        {
            TestUserControl c1 = new TestUserControl();
            Control c11 = new Control();
            c1.Controls.Add(c11);
            TestUserControl c111 = new TestUserControl(c11);
            Result theResult = A.Fake<Result>();

            c1.Results.Add("theResult", theResult);
            c111.SetResult("theResult");

            // context is the control, that contains matching Result
            A.CallTo(() => theResult.Navigate(c1)).MustHaveHappened();
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

        [Test]
        public void NoNullModelPersistenceMediumAllowed()
        {
            TestUserControl tuc = new TestUserControl();
            Assert.Throws<ArgumentNullException>(() => tuc.ModelPersistenceMedium = null);
        }

        [Test]
        public void StoresAndLoadsModelUsingModelPersistenceMedium()
        {
            TestUserControl tuc = new TestUserControl();
            tuc.ModelPersistenceMedium = new DictionaryModelPersistenceMedium();
            tuc.SaveModelToPersistenceMedium(this);
            Assert.AreEqual(this, tuc.LoadModelFromPersistenceMedium());
        }
    }
}