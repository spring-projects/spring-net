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
using System.Globalization;
using System.Threading;
using System.Web;

using FakeItEasy;

using NUnit.Framework;

using Spring.Globalization.Resolvers;
using Spring.Objects;
using Spring.TestSupport;
using Spring.Validation;
using Spring.Web.Support;

namespace Spring.Web.UI
{
    /// <summary>
    /// Unit tests for the Page class.
    /// </summary>
    /// <author>Goran Milosavljevic</author>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class PageTests : TestWebContextTests
    {
        class MockValidator : System.Web.UI.WebControls.BaseValidator
        {
            public bool WasCalled = false;

            protected override bool ControlPropertiesValid()
            {
                return true;
            }

            protected override bool EvaluateIsValid()
            {
                WasCalled = true;
                return true;
            }
        }

        [Test]
        public void NoSharedStateAtConstruction()
        {
            Page page = new Page();            
            Assert.IsNull(page.SharedState);
        }

        [Test]
        public void CallsBaseValidateMethod()
        {
            Page child;
            MockValidator mockValidator;

            child = new Page();
            mockValidator = new MockValidator();
            mockValidator.ValidationGroup = "theValidationGroup";
            child.Validators.Add( mockValidator );

            child.Validate(mockValidator.ValidationGroup);
            Assert.IsTrue(mockValidator.WasCalled);

            child = new Page();
            mockValidator = new MockValidator();
            child.Validators.Add( mockValidator );

            child.Validate();
            Assert.IsTrue(mockValidator.WasCalled);
        }

        [Test]
        public void Validate()
        {           
            Page page = new TestPage(HttpContext.Current);
            IValidator[] validators = new IValidator[] {new RequiredValidator("Name", null), new ConditionValidator("Loan == 0", "Age > 21")};
            Contact contact = new Contact("Goran", 24, 0);
            bool result = page.Validate(contact, validators);
            Assert.IsTrue(result);

            contact = new Contact(null, 24, 0);
            result = page.Validate(contact, validators);
            Assert.IsFalse(result);

            contact = new Contact("Goran", 24, 1);
            result = page.Validate(contact, validators);
            Assert.IsFalse(result);
        }

        [Test]
        public void DefaultsToDefaultWebCultureResolver()
        {
            TestPage page = new TestPage();
            Assert.AreEqual( typeof(DefaultWebCultureResolver), page.CultureResolver.GetType() );            
        }

        [Test]
        public void AllowsNeutralUserCulture()
        {
            TestPage page = new TestPage();
            // DefaultWebCultureResolver does not allow culture to be set
            page.CultureResolver = new DefaultCultureResolver();
            page.UserCulture = new CultureInfo("de");
            
            page.InitializeCulture();
            Assert.AreEqual( page.UserCulture, Thread.CurrentThread.CurrentUICulture );
            Assert.AreEqual( CultureInfo.CreateSpecificCulture(page.UserCulture.Name), Thread.CurrentThread.CurrentCulture);
        }

        [Test]
        public void SetResultThrowsVerboseExceptionOnUnknownResultName()
        {
            string RESULTNAME = "nonexistant result";

            TestPage page = new TestPage();
            try
            {
                page.SetResult(RESULTNAME);
                Assert.Fail();
            }
            catch(ArgumentException ae)
            {
                string expected = string.Format("No mapping found for the specified destination '{0}'.", RESULTNAME);
                string msg = ae.Message.Substring(0, expected.Length);
                Assert.AreEqual(expected, msg);
            }
        }

        [Test]
        public void SetResultSelectsCorrectResult()
        {
            TestPage page = new TestPage();

            Result theResult = A.Fake<Result>();

            page.Results.Add( "theResult", theResult );
            page.SetResult("theResult");

            A.CallTo(() => theResult.Navigate(page)).MustHaveHappened();
        }

        [Test]
        public void NoNullModelPersistenceMediumAllowed()
        {
            TestPage tuc = new TestPage();
            Assert.Throws<ArgumentNullException>(() => tuc.ModelPersistenceMedium = null);
        }

        [Test]
        public void StoresAndLoadsModelUsingModelPersistenceMedium()
        {
            TestPage tuc = new TestPage();
            tuc.ModelPersistenceMedium = new DictionaryModelPersistenceMedium();
            tuc.SaveModelToPersistenceMedium( this );
            Assert.AreEqual(this, tuc.LoadModelFromPersistenceMedium());
        }
    }
}
